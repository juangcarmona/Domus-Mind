import { useState, useEffect, useRef, useMemo } from "react";
import { weekApi } from "../../agenda-today/api/weekApi";
import type { WeeklyGridResponse, DayTypeSummary } from "../../agenda-today/types";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { normalizeCellItems } from "../../agenda-today/utils/calendarEntry";
import { sortEntries } from "../../agenda-today/utils/todayPanelHelpers";
import { DAY_ORDER, toIsoDate } from "../../agenda-today/utils/dateUtils";

/**
 * Fetches all weekly grids needed to populate a month calendar view
 * and computes per-day item-type summaries scoped to a single member.
 *
 * If memberId is null/undefined the summary falls back to the full household
 * (shared cells + all members), but the normal agenda use-case always provides
 * a memberId so the calendar reflects only that person.
 */
export function useAgendaMonthCache(
  familyId: string,
  memberId: string | null | undefined,
  monthAnchor: string,
  firstDayOfWeek: string | null,
  active: boolean,
) {
  const [cache, setCache] = useState<Record<string, WeeklyGridResponse>>({});
  const requested = useRef<Set<string>>(new Set());

  // Reset when household or member changes.
  useEffect(() => {
    setCache({});
    requested.current.clear();
  }, [familyId, memberId]);

  useEffect(() => {
    if (!active || !familyId) return;

    const anchor = new Date(monthAnchor + "T00:00:00");
    const year = anchor.getFullYear();
    const month = anchor.getMonth();
    const firstDayIdx = Math.max(0, DAY_ORDER.indexOf((firstDayOfWeek ?? "monday").toLowerCase()));
    const firstOfMonth = new Date(year, month, 1);
    let startPad = firstOfMonth.getDay() - firstDayIdx;
    if (startPad < 0) startPad += 7;
    const firstVisible = new Date(year, month, 1 - startPad);
    const lastOfMonth = new Date(year, month + 1, 0);

    const weekStarts: string[] = [];
    let cursor = new Date(firstVisible);
    while (cursor <= lastOfMonth) {
      weekStarts.push(toIsoDate(cursor));
      cursor = new Date(cursor.getFullYear(), cursor.getMonth(), cursor.getDate() + 7);
    }

    const missing = weekStarts.filter((ws) => !requested.current.has(ws));
    if (missing.length === 0) return;

    missing.forEach((ws) => requested.current.add(ws));

    Promise.all(missing.map((ws) => weekApi.getWeeklyGrid(familyId, ws)))
      .then((grids) => {
        setCache((prev) => {
          const next = { ...prev };
          grids.forEach((g, i) => { next[missing[i]] = g; });
          return next;
        });
      })
      .catch(() => {
        missing.forEach((ws) => requested.current.delete(ws));
      });
  }, [active, monthAnchor, familyId, firstDayOfWeek]);

  /** Per-day summary scoped to the given memberId (or household-wide if null). */
  const daySummary = useMemo((): Record<string, DayTypeSummary> => {
    const summary: Record<string, DayTypeSummary> = {};
    if (Object.keys(cache).length === 0) return summary;

    for (const weekGrid of Object.values(cache)) {
      let cells;
      if (memberId) {
        // Member-scoped: only that member's cells.
        const row = weekGrid.members?.find((m) => m.memberId === memberId);
        cells = row?.cells ?? [];
      } else {
        // Shared/collective row only — not all members combined.
        cells = weekGrid.sharedCells ?? [];
      }

      for (const cell of cells) {
        const dayKey = cell.date.slice(0, 10);
        if (!summary[dayKey]) summary[dayKey] = { events: 0, tasks: 0, routines: 0, listItems: 0 };
        summary[dayKey].events += cell.events?.length ?? 0;
        summary[dayKey].tasks += cell.tasks?.length ?? 0;
        summary[dayKey].routines += cell.routines?.length ?? 0;
        summary[dayKey].listItems += cell.listItems?.length ?? 0;
      }
    }
    return summary;
  }, [cache, memberId]);

  /**
   * The highest-priority non-completed entry for each day, keyed by YYYY-MM-DD.
   * Uses normalizeCellItems + sortEntries so ordering matches Day/Week views.
   */
  const dayTopEntry = useMemo((): Record<string, CalendarEntry> => {
    const result: Record<string, CalendarEntry> = {};
    if (Object.keys(cache).length === 0) return result;

    for (const weekGrid of Object.values(cache)) {
      let cells;
      if (memberId) {
        const row = weekGrid.members?.find((m) => m.memberId === memberId);
        cells = row?.cells ?? [];
      } else {
        // Shared/collective row only.
        cells = weekGrid.sharedCells ?? [];
      }

      for (const cell of cells) {
        const dayKey = cell.date.slice(0, 10);
        if (result[dayKey]) continue; // already have a top entry for this day
        const sorted = sortEntries(normalizeCellItems(cell));
        const top = sorted.find((e) => e.displayType !== "completed");
        if (top) result[dayKey] = top;
      }
    }
    return result;
  }, [cache, memberId]);

  return { daySummary, dayTopEntry };
}

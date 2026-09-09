import { useState, useEffect, useRef, useMemo } from "react";
import { weekApi } from "../api/weekApi";
import type { WeeklyGridResponse, DayTypeSummary } from "../types";
import { DAY_ORDER, toIsoDate } from "../utils/dateUtils";

export function useMonthGridCache(
  familyId: string,
  monthAnchor: string,
  firstDayOfWeek: string | null,
  active: boolean,
) {
  const [monthGridCache, setMonthGridCache] = useState<Record<string, WeeklyGridResponse>>({});
  const requestedMonthWeeks = useRef<Set<string>>(new Set());

  // Reset cache when the household changes
  useEffect(() => {
    setMonthGridCache({});
    requestedMonthWeeks.current.clear();
  }, [familyId]);

  // Fetch all weeks visible in the month calendar
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

    const missing = weekStarts.filter((ws) => !requestedMonthWeeks.current.has(ws));
    if (missing.length === 0) return;

    missing.forEach((ws) => requestedMonthWeeks.current.add(ws));

    Promise.all(missing.map((ws) => weekApi.getWeeklyGrid(familyId, ws)))
      .then((grids) => {
        setMonthGridCache((prev) => {
          const next = { ...prev };
          grids.forEach((g, i) => { next[missing[i]] = g; });
          return next;
        });
      })
      .catch(() => {
        missing.forEach((ws) => requestedMonthWeeks.current.delete(ws));
      });
  }, [active, monthAnchor, familyId, firstDayOfWeek]);

  const monthDaySummary = useMemo(() => {
    const summary: Record<string, DayTypeSummary> = {};
    if (Object.keys(monthGridCache).length === 0) return summary;
    for (const weekGrid of Object.values(monthGridCache)) {
      const allCells = [
        ...(weekGrid.sharedCells ?? []),
        ...((weekGrid.members ?? []).flatMap((m) => m.cells)),
      ];
      for (const cell of allCells) {
        const dayKey = cell.date.slice(0, 10);
        if (!summary[dayKey]) summary[dayKey] = { events: 0, tasks: 0, routines: 0, listItems: 0 };
        summary[dayKey].events += cell.events?.length ?? 0;
        summary[dayKey].tasks += cell.tasks?.length ?? 0;
        summary[dayKey].routines += cell.routines?.length ?? 0;
        summary[dayKey].listItems += cell.listItems?.length ?? 0;
      }
    }
    return summary;
  }, [monthGridCache]);

  return { monthDaySummary };
}

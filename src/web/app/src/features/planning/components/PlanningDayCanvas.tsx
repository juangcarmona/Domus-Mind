import React, { useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../today/types";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import {
  buildSharedEntries,
  buildMemberEntries,
  sortEntries,
} from "../../today/utils/todayPanelHelpers";
import { toIsoDate } from "../../today/utils/dateUtils";
import { HourTimeline } from "../../agenda/components/HourTimeline";

/**
 * Pixel height of one 30-minute slot.
 * Must equal CSS `--ht-slot-h` (1.5rem × 16px base font size).
 */
const SLOT_H_PX = 24;

interface PlanningDayCanvasProps {
  grid: WeeklyGridResponse | null;
  selectedDate: string; // ISO YYYY-MM-DD
  loading: boolean;
  error: string | null;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
  /**
   * Called when empty slot background is clicked.
   * Receives "HH:MM" for create-at-time intent.
   */
  onSlotClick?: (time: string) => void;
}

/**
 * Household-level day view for Planning.
 *
 * Combines shared entries + all member entries for the selected date
 * into a single HourTimeline. Deduplicates by entry id so events that
 * appear in both the shared row and a member row are shown once.
 *
 * This is intentionally different from MemberDayView (single-member)
 * and SharedDayView (shared-only). Planning shows the full household picture.
 */
export function PlanningDayCanvas({
  grid,
  selectedDate,
  loading,
  error,
  onItemClick,
  onSlotClick,
}: PlanningDayCanvasProps) {
  const { t } = useTranslation("agenda");
  const panelRef = useRef<HTMLElement>(null);

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  const now = new Date();
  const nowMinutes = isToday ? now.getHours() * 60 + now.getMinutes() : undefined;

  // Scroll to intelligent position on date change
  useEffect(() => {
    if (!panelRef.current) return;
    let targetSlot: number;
    if (isToday) {
      const n = new Date();
      const currentSlot = n.getHours() * 2 + (n.getMinutes() >= 30 ? 1 : 0);
      targetSlot = Math.max(0, currentSlot - 2);
    } else {
      targetSlot = 14; // 07:00 anchor for non-today dates
    }
    panelRef.current.scrollTop = targetSlot * SLOT_H_PX;
  }, [selectedDate]); // eslint-disable-line react-hooks/exhaustive-deps

  if (loading) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }
  if (error) {
    return <p className="error-msg">{error}</p>;
  }

  // Combine all entries from shared + every member cell for the selected date.
  // Deduplicate by id so events with multiple participants aren't shown twice.
  const allEntries: CalendarEntry[] = [];
  const seenIds = new Set<string>();

  function addEntry(entry: CalendarEntry) {
    if (!seenIds.has(entry.id)) {
      seenIds.add(entry.id);
      allEntries.push(entry);
    }
  }

  if (grid) {
    for (const e of buildSharedEntries(grid.sharedCells, selectedDate)) {
      addEntry(e);
    }
    for (const member of grid.members ?? []) {
      for (const e of buildMemberEntries(member, selectedDate)) {
        addEntry(e);
      }
    }
  }

  const sorted = sortEntries(allEntries);
  const timedEntries = sorted.filter((e) => e.time !== null);

  return (
    <div className="planning-day-canvas">
      <section
        ref={panelRef as React.RefObject<HTMLElement>}
        className="planning-day-timeline-panel"
        aria-label={t("day.timeline")}
      >
        <HourTimeline
          timedEntries={timedEntries}
          isToday={isToday}
          nowMinutes={nowMinutes}
          onItemClick={onItemClick}
          onSlotClick={onSlotClick}
        />
      </section>
    </div>
  );
}

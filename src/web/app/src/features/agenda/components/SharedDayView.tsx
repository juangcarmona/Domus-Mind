import React, { useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { WeeklyGridCell } from "../../agenda-today/types";
import { buildSharedEntries } from "../../agenda-today/utils/todayPanelHelpers";
import { toIsoDate } from "../../agenda-today/utils/dateUtils";
import { HourTimeline } from "./HourTimeline";

/**
 * Pixel height of one 30-minute slot.
 * Must equal CSS `--ht-slot-h` (1.5rem × 16px base font size).
 */
const SLOT_H_PX = 24;

interface SharedDayViewProps {
  sharedCells: WeeklyGridCell[];
  selectedDate: string; // ISO YYYY-MM-DD
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
  onSlotClick?: (time: string) => void;
}

/**
 * Day-focused shared agenda view — shows the hourly timeline for shared/collective items.
 * Mirrors MemberDayView but sources entries from sharedCells instead of a member row.
 */
export function SharedDayView({
  sharedCells,
  selectedDate,
  onItemClick,
  onSlotClick,
}: SharedDayViewProps) {
  const { t } = useTranslation("agenda");

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  const now = new Date();
  const nowMinutes = isToday ? now.getHours() * 60 + now.getMinutes() : undefined;

  const panelRef = useRef<HTMLElement>(null);

  useEffect(() => {
    if (!panelRef.current) return;
    let targetSlot: number;
    if (isToday) {
      const n = new Date();
      const currentSlot = n.getHours() * 2 + (n.getMinutes() >= 30 ? 1 : 0);
      targetSlot = Math.max(0, currentSlot - 2);
    } else {
      targetSlot = 14; // 07:00
    }
    panelRef.current.scrollTop = targetSlot * SLOT_H_PX;
  }, [selectedDate]); // eslint-disable-line react-hooks/exhaustive-deps

  const allEntries = buildSharedEntries(sharedCells, selectedDate);
  const timedEntries = allEntries.filter((e) => e.time !== null);

  return (
    <div className="member-day-view">
      <section
        ref={panelRef as React.RefObject<HTMLElement>}
        className="mday-timeline-panel"
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

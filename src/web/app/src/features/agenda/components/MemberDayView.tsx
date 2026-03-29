import React, { useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { WeeklyGridMember } from "../../today/types";
import { buildMemberEntries } from "../../today/utils/todayPanelHelpers";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { toIsoDate } from "../../today/utils/dateUtils";
import { HourTimeline } from "./HourTimeline";

/**
 * Pixel height of one 30-minute slot.
 * Must equal CSS `--ht-slot-h` (1.5rem × 16px base font size).
 */
const SLOT_H_PX = 24;

interface MemberDayViewProps {
  member: WeeklyGridMember;
  selectedDate: string; // ISO YYYY-MM-DD
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
  /**
   * Called when an empty timeline slot is clicked.
   * Receives "HH:MM" (the :00 or :30 slot start time).
   */
  onSlotClick?: (time: string) => void;
}

/**
 * Day-focused member agenda view — shows the hourly timeline only.
 * Untimed entries (backlog) are surfaced via SelectedDateCard in the page sidebar.
 */
export function MemberDayView({ member, selectedDate, onItemClick, onSlotClick }: MemberDayViewProps) {
  const { t } = useTranslation("agenda");

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  const now = new Date();
  const nowMinutes = isToday ? now.getHours() * 60 + now.getMinutes() : undefined;

  const panelRef = useRef<HTMLElement>(null);

  // Scroll the timeline to an intelligent start position whenever the date changes.
  // For today: show ~1 hour before the current time.
  // For other dates: open at 07:00 (a sensible morning anchor).
  useEffect(() => {
    if (!panelRef.current) return;
    let targetSlot: number;
    if (isToday) {
      const n = new Date();
      const currentSlot = n.getHours() * 2 + (n.getMinutes() >= 30 ? 1 : 0);
      targetSlot = Math.max(0, currentSlot - 2);
    } else {
      targetSlot = 14; // 07:00 — slot 7*2
    }
    panelRef.current.scrollTop = targetSlot * SLOT_H_PX;
  }, [selectedDate]); // eslint-disable-line react-hooks/exhaustive-deps

  const allEntries: CalendarEntry[] = buildMemberEntries(member, selectedDate);
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

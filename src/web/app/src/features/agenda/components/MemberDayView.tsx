import { useTranslation } from "react-i18next";
import type { WeeklyGridMember } from "../../today/types";
import { buildMemberEntries } from "../../today/utils/todayPanelHelpers";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { HourTimeline } from "./HourTimeline";

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

  const allEntries: CalendarEntry[] = buildMemberEntries(member, selectedDate);
  const timedEntries = allEntries.filter((e) => e.time !== null);

  return (
    <div className="member-day-view">
      <section className="mday-timeline-panel" aria-label={t("day.timeline")}>
        <HourTimeline
          timedEntries={timedEntries}
          onItemClick={onItemClick}
          onSlotClick={onSlotClick}
        />
      </section>
    </div>
  );
}

import { useTranslation } from "react-i18next";
import type { WeeklyGridCell, WeeklyGridMember } from "../../agenda-today/types";
import { buildMemberEntries, sortEntries } from "../../agenda-today/utils/todayPanelHelpers";
import { MemberSelectedDaySummary } from "./MemberSelectedDaySummary";
import { MemberSelectedDayUntimedSection } from "./MemberSelectedDayUntimedSection";
import { MemberSelectedDayTimedSection } from "./MemberSelectedDayTimedSection";

interface MemberDayViewProps {
  member: WeeklyGridMember;
  selectedDate: string; // ISO YYYY-MM-DD
  sharedCell?: WeeklyGridCell | null;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
  /**
   * Called when an empty timeline slot is clicked.
   * Receives "HH:MM" (the :00 or :30 slot start time).
   */
  onSlotClick?: (time: string) => void;
}

/**
 * Day-focused member agenda view.
 *
 * Shows the selected day's full content in reading order:
 *   1. Summary stat strip
 *   2. Untimed entries (overdue first, then unscheduled, completed last)
 *   3. Hourly timeline for timed entries (only when timed entries exist)
 *
 * No longer timeline-only — untimed state is surfaced inline, not pushed to a sidebar.
 */
export function MemberDayView({ member, selectedDate, sharedCell, onItemClick, onSlotClick }: MemberDayViewProps) {
  const { t } = useTranslation("agenda");

  const allEntries = sortEntries(buildMemberEntries(member, selectedDate, sharedCell));
  const untimedEntries = allEntries.filter((e) => e.time === null);
  const timedEntries   = allEntries.filter((e) => e.time !== null);

  return (
    <div className="member-day-view">
      <div className="mday-sections">
        <MemberSelectedDaySummary entries={allEntries} />

        <MemberSelectedDayUntimedSection
          entries={untimedEntries}
          onItemClick={onItemClick}
        />

        <MemberSelectedDayTimedSection
          timedEntries={timedEntries}
          selectedDate={selectedDate}
          onItemClick={onItemClick}
          onSlotClick={onSlotClick}
        />

        {allEntries.length === 0 && (
          <span className="mday-empty">{t("day.nothingScheduled")}</span>
        )}
      </div>
    </div>
  );
}

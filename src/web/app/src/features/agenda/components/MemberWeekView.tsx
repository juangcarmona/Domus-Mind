import { useTranslation } from "react-i18next";
import type { WeeklyGridMember } from "../../today/types";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { buildMemberEntries } from "../../today/utils/todayPanelHelpers";
import { CalendarEntryItem } from "../../today/components/shared/CalendarEntryItem";

interface MemberWeekViewProps {
  member: WeeklyGridMember;
  /** ISO YYYY-MM-DD — any day in the target week. */
  selectedDate: string;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

/**
 * Week-level member agenda view.
 *
 * V1: renders a simple day-by-day list of entries for the week window
 * that is already present in the member's grid cells.
 * Full week grid layout is deferred.
 */
export function MemberWeekView({ member, selectedDate, onItemClick }: MemberWeekViewProps) {
  const { t, i18n } = useTranslation("agenda");

  // The member cells already cover the full week from the loaded grid.
  const days = member.cells.map((cell) => cell.date.slice(0, 10));

  if (days.length === 0) {
    return (
      <div className="member-week-view">
        <span className="mday-empty">{t("week.empty")}</span>
      </div>
    );
  }

  return (
    <div className="member-week-view">
      {days.map((day) => {
        const entries: CalendarEntry[] = buildMemberEntries(member, day);
        const label = new Date(day + "T00:00:00").toLocaleDateString(i18n.language, {
          weekday: "short",
          day: "numeric",
          month: "short",
        });
        const isSelected = day === selectedDate;

        return (
          <div
            key={day}
            className={`mweek-day${isSelected ? " mweek-day--selected" : ""}`}
          >
            <div className="mweek-day-label">{label}</div>
            {entries.length === 0 ? (
              <span className="mday-empty">{t("day.nothingScheduled")}</span>
            ) : (
              <div className="mday-entry-list">
                {entries.map((entry) => (
                  <CalendarEntryItem
                    key={entry.id}
                    entry={entry}
                    onClick={() => onItemClick(entry.sourceType, entry.id)}
                  />
                ))}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}

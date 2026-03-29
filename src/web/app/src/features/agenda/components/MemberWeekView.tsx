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
  /** Called when the user taps a day header to drill into the Day view. */
  onDayClick?: (date: string) => void;
}

/**
 * Week-level member agenda view.
 *
 * Renders a compact day-per-row list. Each day header is clickable (onDayClick)
 * to navigate into the Day view for that day.
 */
export function MemberWeekView({ member, selectedDate, onItemClick, onDayClick }: MemberWeekViewProps) {
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
        const hasItems = entries.length > 0;

        return (
          <div
            key={day}
            className={`mweek-day${isSelected ? " mweek-day--selected" : ""}${!hasItems ? " mweek-day--empty" : ""}`}
          >
            <button
              className="mweek-day-header"
              type="button"
              onClick={() => onDayClick?.(day)}
              aria-label={label}
              aria-pressed={isSelected}
            >
              <span className="mweek-day-label">{label}</span>
              {hasItems && (
                <span className="mweek-day-count">{entries.length}</span>
              )}
            </button>
            {hasItems ? (
              <div className="mday-entry-list mweek-entry-list">
                {entries.map((entry) => (
                  <CalendarEntryItem
                    key={entry.id}
                    entry={entry}
                    onClick={() => onItemClick(entry.sourceType, entry.id)}
                  />
                ))}
              </div>
            ) : (
              <span className="mday-empty mweek-empty">{t("day.nothingScheduled")}</span>
            )}
          </div>
        );
      })}
    </div>
  );
}


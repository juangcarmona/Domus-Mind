import { useTranslation } from "react-i18next";
import type { WeeklyGridCell } from "../../agenda-today/types";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { buildSharedEntries } from "../../agenda-today/utils/todayPanelHelpers";
import { CalendarEntryItem } from "../../agenda-today/components/shared/CalendarEntryItem";

interface SharedWeekViewProps {
  sharedCells: WeeklyGridCell[];
  /** ISO YYYY-MM-DD — any day in the target week. */
  selectedDate: string;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
  /** Called when the user taps a day header to drill into the Day view. */
  onDayClick?: (date: string) => void;
}

/**
 * Week-level shared agenda view.
 *
 * Mirrors MemberWeekView but sources entries from sharedCells.
 * Only collective/shared items appear here — member items are excluded.
 */
export function SharedWeekView({
  sharedCells,
  selectedDate,
  onItemClick,
  onDayClick,
}: SharedWeekViewProps) {
  const { t, i18n } = useTranslation("agenda");

  const days = [...sharedCells]
    .map((cell) => cell.date.slice(0, 10))
    .sort();

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
        const entries: CalendarEntry[] = buildSharedEntries(sharedCells, day);
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

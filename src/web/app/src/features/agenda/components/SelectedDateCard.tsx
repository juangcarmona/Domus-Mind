import { useTranslation } from "react-i18next";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { CalendarEntryItem } from "../../agenda-today/components/shared/CalendarEntryItem";

interface SelectedDateCardProps {
  /** Untimed entries for the selected date (entry.time === null). */
  entries: CalendarEntry[];
  /** "Today" or a formatted date string, used as the card heading. */
  dateLabel: string;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

/**
 * Sidebar card showing untimed entries (backlog) for the selected date.
 *
 * Renders two groups when present:
 *  - Overdue: items past their due date
 *  - Everything else: unscheduled / no-time items (no group label when overdue is absent)
 */
export function SelectedDateCard({ entries, dateLabel, onItemClick }: SelectedDateCardProps) {
  const { t } = useTranslation("agenda");
  const overdue: CalendarEntry[] = [];
  const rest: CalendarEntry[] = [];

  for (const e of entries) {
    if (e.displayType === "overdue" || e.isOverdue) {
      overdue.push(e);
    } else {
      rest.push(e);
    }
  }

  // Don't render the card when there's nothing to show.
  if (entries.length === 0) {
    return null;
  }

  return (
    <div className="agenda-date-card">
      <div className="agenda-date-card__heading">{dateLabel}</div>

      {overdue.length > 0 && (
        <div className="agenda-date-card__group">
          <div className="agenda-date-card__group-label agenda-date-card__group-label--overdue">
            {t("day.overdue")}
          </div>
          <div className="agenda-date-card__entry-list">
            {overdue.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
          </div>
        </div>
      )}

      {rest.length > 0 && (
        <div className="agenda-date-card__group">
          {overdue.length > 0 && (
            <div className="agenda-date-card__group-label">{t("day.unscheduled")}</div>
          )}
          <div className="agenda-date-card__entry-list">
            {rest.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

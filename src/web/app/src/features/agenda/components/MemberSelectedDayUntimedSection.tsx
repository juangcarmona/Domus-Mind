import { useTranslation } from "react-i18next";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { CalendarEntryItem } from "../../agenda-today/components/shared/CalendarEntryItem";

interface MemberSelectedDayUntimedSectionProps {
  /** All untimed entries for the selected day (time === null). */
  entries: CalendarEntry[];
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

/**
 * Renders untimed entries for the selected day in three ordered groups:
 *  1. Overdue — items past their due date
 *  2. Unscheduled — pending items with no time
 *  3. Completed — de-emphasised, shown last
 *
 * Never passes entries into HourTimeline; this section is strictly for time === null items.
 */
export function MemberSelectedDayUntimedSection({
  entries,
  onItemClick,
}: MemberSelectedDayUntimedSectionProps) {
  const { t } = useTranslation("agenda");

  if (entries.length === 0) return null;

  const overdue   = entries.filter((e) => e.isOverdue && !e.isCompleted);
  const rest      = entries.filter((e) => !e.isOverdue && !e.isCompleted);
  const completed = entries.filter((e) => e.isCompleted);

  return (
    <div className="mday-section mday-section--untimed">
      {overdue.length > 0 && (
        <div className="mday-section-group">
          <div className="mday-section-title mday-section-title--overdue">
            {t("day.overdue", "Overdue")}
          </div>
          <div className="mday-untimed-list">
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
        <div className="mday-section-group">
          {overdue.length > 0 && (
            <div className="mday-section-title">
              {t("day.unscheduled", "Unscheduled")}
            </div>
          )}
          <div className="mday-untimed-list">
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

      {completed.length > 0 && (
        <div className="mday-section-group mday-completed-group">
          <div className="mday-section-title mday-section-title--completed">
            {t("day.summary.done", "{{count}} done", { count: completed.length })}
          </div>
          <div className="mday-untimed-list mday-untimed-list--completed">
            {completed.map((entry) => (
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

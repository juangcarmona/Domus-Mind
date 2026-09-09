import { useTranslation } from "react-i18next";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";

interface MemberSelectedDaySummaryProps {
  entries: CalendarEntry[];
}

/**
 * Compact stat strip for the selected day — shows overdue / unscheduled / timed / done counts.
 * Summary-only: no rows, no heavy chrome.
 */
export function MemberSelectedDaySummary({ entries }: MemberSelectedDaySummaryProps) {
  const { t } = useTranslation("agenda");

  if (entries.length === 0) return null;

  const overdue = entries.filter((e) => e.isOverdue && !e.isCompleted).length;
  const untimed = entries.filter((e) => e.time === null && !e.isCompleted && !e.isOverdue).length;
  const timed   = entries.filter((e) => e.time !== null && !e.isCompleted).length;
  const done    = entries.filter((e) => e.isCompleted).length;

  const hasAny = overdue + untimed + timed + done > 0;
  if (!hasAny) return null;

  return (
    <div className="mday-summary">
      {overdue > 0 && (
        <span className="mday-summary-stat mday-summary-stat--overdue">
          {t("day.summary.overdue", "{{count}} overdue", { count: overdue })}
        </span>
      )}
      {untimed > 0 && (
        <span className="mday-summary-stat">
          {t("day.summary.untimed", "{{count}} unscheduled", { count: untimed })}
        </span>
      )}
      {timed > 0 && (
        <span className="mday-summary-stat">
          {t("day.summary.timed", "{{count}} timed", { count: timed })}
        </span>
      )}
      {done > 0 && (
        <span className="mday-summary-stat mday-summary-stat--done">
          {t("day.summary.done", "{{count}} done", { count: done })}
        </span>
      )}
    </div>
  );
}

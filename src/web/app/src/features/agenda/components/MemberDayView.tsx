import { useTranslation } from "react-i18next";
import type { WeeklyGridMember } from "../../today/types";
import {
  buildMemberEntries,
  sortEntries,
} from "../../today/utils/todayPanelHelpers";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { CalendarEntryItem } from "../../today/components/shared/CalendarEntryItem";
import { HourTimeline } from "./HourTimeline";

interface MemberDayViewProps {
  member: WeeklyGridMember;
  selectedDate: string; // ISO YYYY-MM-DD
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

/**
 * Day-focused member agenda view.
 *
 * Structure:
 *   1. Backlog — entries without an assigned time (overdue tasks, untimed tasks,
 *      routines without time). These must always be shown; they cannot fall through.
 *   2. Timeline — hourly vertical timeline for timed entries (events, timed routines).
 *
 * Completed items remain visible with lower emphasis in both sections.
 * The split is based solely on whether entry.time is set.
 */
export function MemberDayView({ member, selectedDate, onItemClick }: MemberDayViewProps) {
  const { t } = useTranslation("agenda");

  // Full sorted entry list for the selected date (includes within-week overdue).
  const allEntries: CalendarEntry[] = buildMemberEntries(member, selectedDate);

  // Split into timed vs backlog.
  const timedEntries: CalendarEntry[] = [];
  const backlogEntries: CalendarEntry[] = [];

  for (const entry of allEntries) {
    if (entry.time !== null) {
      timedEntries.push(entry);
    } else {
      backlogEntries.push(entry);
    }
  }

  // Active vs completed in backlog for visual separation.
  const activeBacklog = backlogEntries.filter((e) => e.displayType !== "completed");
  const completedBacklog = backlogEntries.filter((e) => e.displayType === "completed");

  return (
    <div className="member-day-view">
      {/* ---- Backlog: untimed items ---- */}
      <section className="mday-backlog" aria-label={t("day.backlog")}>
        <div className="mday-section-label">{t("day.backlog")}</div>

        {activeBacklog.length === 0 && completedBacklog.length === 0 ? (
          <span className="mday-empty">{t("day.noBacklogItems")}</span>
        ) : (
          <div className="mday-entry-list">
            {sortEntries(activeBacklog).map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
            {completedBacklog.length > 0 && (
              <div className="mday-completed-group" aria-label={t("day.completedSection")}>
                {sortEntries(completedBacklog).map((entry) => (
                  <CalendarEntryItem
                    key={entry.id}
                    entry={entry}
                    onClick={() => onItemClick(entry.sourceType, entry.id)}
                  />
                ))}
              </div>
            )}
          </div>
        )}
      </section>

      {/* ---- Timeline: timed items ---- */}
      <section className="mday-timeline" aria-label={t("day.timeline")}>
        <div className="mday-section-label">{t("day.timeline")}</div>

        {timedEntries.length === 0 ? (
          <span className="mday-empty">{t("day.nothingScheduled")}</span>
        ) : (
          <HourTimeline timedEntries={timedEntries} onItemClick={onItemClick} />
        )}
      </section>
    </div>
  );
}

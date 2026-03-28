import { useTranslation } from "react-i18next";
import type { WeeklyGridMember } from "../../today/types";
import { buildMemberEntries } from "../../today/utils/todayPanelHelpers";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { CalendarEntryItem } from "../../today/components/shared/CalendarEntryItem";
import { HourTimeline } from "./HourTimeline";

interface MemberDayViewProps {
  member: WeeklyGridMember;
  selectedDate: string; // ISO YYYY-MM-DD
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
  /**
   * Called when an empty hour slot is clicked.
   * Plumbed for future "create at time" flow. Hour is 0–23.
   * If not provided the slot is still visually clickable (cursor change) but no-ops.
   */
  onHourClick?: (hour: number) => void;
}

// ----------------------------------------------------------------
// Backlog grouping — structured so per-type groups can be added later
// without rewriting the split logic.
// ----------------------------------------------------------------

interface BacklogGroups {
  overdue: CalendarEntry[];
  unscheduled: CalendarEntry[];
  completed: CalendarEntry[];
}

function groupBacklog(entries: CalendarEntry[]): BacklogGroups {
  const overdue: CalendarEntry[] = [];
  const unscheduled: CalendarEntry[] = [];
  const completed: CalendarEntry[] = [];

  for (const e of entries) {
    if (e.displayType === "completed") {
      completed.push(e);
    } else if (e.displayType === "overdue" || e.isOverdue) {
      overdue.push(e);
    } else {
      unscheduled.push(e);
    }
  }

  return { overdue, unscheduled, completed };
}

// ----------------------------------------------------------------

/**
 * Day-focused member agenda view.
 *
 * Layout:
 *   Desktop (≥680px): backlog panel on the left, timeline on the right.
 *   Mobile: backlog above, timeline below.
 *
 * Backlog is always rendered; timeline always shows the hourly grid even
 * when empty. This ensures the surface reads as a credible calendar.
 */
export function MemberDayView({ member, selectedDate, onItemClick, onHourClick }: MemberDayViewProps) {
  const { t } = useTranslation("agenda");

  // Full sorted entry list for the selected date (includes within-week overdue).
  const allEntries: CalendarEntry[] = buildMemberEntries(member, selectedDate);

  // Split into timed (have time) vs backlog (no time).
  const timedEntries: CalendarEntry[] = [];
  const backlogEntries: CalendarEntry[] = [];

  for (const entry of allEntries) {
    if (entry.time !== null) {
      timedEntries.push(entry);
    } else {
      backlogEntries.push(entry);
    }
  }

  const { overdue, unscheduled, completed } = groupBacklog(backlogEntries);
  const hasBacklogContent = overdue.length > 0 || unscheduled.length > 0 || completed.length > 0;

  return (
    <div className="member-day-view">
      {/* Two-panel layout: backlog + timeline */}
      <div className="mday-layout">

        {/* ---- Left / top: Backlog ---- */}
        <section className="mday-backlog-panel" aria-label={t("day.backlog")}>
          <div className="mday-panel-heading">{t("day.backlog")}</div>

          {!hasBacklogContent ? (
            <p className="mday-empty">{t("day.noBacklogItems")}</p>
          ) : (
            <>
              {overdue.length > 0 && (
                <div className="mday-backlog-group">
                  <div className="mday-group-label mday-group-label--overdue">
                    {t("day.overdue")}
                  </div>
                  <div className="mday-entry-list">
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

              {unscheduled.length > 0 && (
                <div className="mday-backlog-group">
                  {overdue.length > 0 && (
                    <div className="mday-group-label">{t("day.unscheduled")}</div>
                  )}
                  <div className="mday-entry-list">
                    {unscheduled.map((entry) => (
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
                <div
                  className="mday-backlog-group mday-backlog-group--completed"
                  aria-label={t("day.completedSection")}
                >
                  <div className="mday-group-label">{t("day.completedSection")}</div>
                  <div className="mday-entry-list">
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
            </>
          )}
        </section>

        {/* ---- Right / bottom: Hourly timeline ---- */}
        {/* Always rendered so the surface reads as a calendar, even on empty days. */}
        <section className="mday-timeline-panel" aria-label={t("day.timeline")}>
          <div className="mday-panel-heading">{t("day.timeline")}</div>
          <HourTimeline
            timedEntries={timedEntries}
            onItemClick={onItemClick}
            onHourClick={onHourClick}
          />
        </section>

      </div>
    </div>
  );
}

import { useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import type {
  EnrichedTimelineResponse,
  EnrichedTimelineEntry,
} from "../../../api/domusmindApi";

interface CoordinationTimelineProps {
  data: EnrichedTimelineResponse | null;
  loading: boolean;
  error: string | null;
  selectedDate: string;
  onSelectDay: (date: string) => void;
}

const GROUP_ORDER = [
  "Overdue",
  "Today",
  "Tomorrow",
  "ThisWeek",
  "Later",
  "Undated",
] as const;

type GroupKey = (typeof GROUP_ORDER)[number];

function entryTypeClass(type: string): string {
  if (type === "CalendarEvent") return "event";
  if (type === "Task") return "task";
  return "routine";
}

function CoordinationTimelineEntry({ entry }: { entry: EnrichedTimelineEntry }) {
  const isDone = entry.status === "Completed" || entry.status === "Cancelled";
  return (
    <div
      className={`coord-tl-entry${isDone ? " coord-tl-entry--done" : ""}${entry.isOverdue ? " coord-tl-entry--overdue" : ""}`}
    >
      <span className={`entry-type-dot ${entryTypeClass(entry.entryType)}`} />
      <span className="coord-tl-entry-title">{entry.title}</span>
      {entry.status && (
        <span className={`entry-status-badge ${entry.status.toLowerCase()}`}>
          {entry.status.toLowerCase()}
        </span>
      )}
    </div>
  );
}

function groupLabel(
  groupKey: string,
  t: (key: string) => string,
): string {
  const key = `groups.${groupKey}` as never;
  return t(key) ?? groupKey;
}

export function CoordinationTimeline({
  data,
  loading,
  error,
  selectedDate,
  onSelectDay,
}: CoordinationTimelineProps) {
  const { t: tTl } = useTranslation("timeline");
  const { t } = useTranslation("coordination");
  const trackRef = useRef<HTMLDivElement>(null);
  const todayGroupRef = useRef<HTMLDivElement>(null);

  // Scroll "Today" group into view on mount / when data changes
  useEffect(() => {
    if (todayGroupRef.current && trackRef.current) {
      const track = trackRef.current;
      const group = todayGroupRef.current;
      const offset =
        group.offsetLeft - track.clientWidth / 2 + group.offsetWidth / 2;
      track.scrollLeft = Math.max(0, offset);
    }
  }, [data]);

  if (loading) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }
  if (error) {
    return <p className="error-msg">{error}</p>;
  }
  if (!data || data.totalEntries === 0) {
    return (
      <div className="empty-state">
        <p>{t("timeline.empty")}</p>
      </div>
    );
  }

  // Build an ordered group map
  const groupMap = new Map(data.groups.map((g) => [g.groupKey, g]));

  // Determine which date corresponds to "Today" group for click handling
  const todayIso = new Date().toISOString().slice(0, 10);

  function groupDateForClick(key: GroupKey): string | null {
    if (key === "Today") return todayIso;
    if (key === "Tomorrow") {
      const d = new Date();
      d.setDate(d.getDate() + 1);
      return d.toISOString().slice(0, 10);
    }
    return null;
  }

  return (
    <div className="coord-timeline-wrap">
      <p className="coord-tl-hint">{t("timeline.scrollHint")}</p>
      <div className="coord-tl-track" ref={trackRef}>
        {GROUP_ORDER.map((key) => {
          const group = groupMap.get(key);
          if (!group || group.entries.length === 0) return null;

          const clickDate = groupDateForClick(key);
          const isToday = key === "Today";
          const isActive = clickDate === selectedDate;

          return (
            <div
              key={key}
              ref={isToday ? todayGroupRef : undefined}
              className={[
                "coord-tl-group",
                isToday ? "coord-tl-group--today" : "",
                isActive ? "coord-tl-group--active" : "",
                key === "Overdue" ? "coord-tl-group--overdue" : "",
              ]
                .filter(Boolean)
                .join(" ")}
            >
              <div
                className="coord-tl-group-header"
                onClick={clickDate ? () => onSelectDay(clickDate) : undefined}
                role={clickDate ? "button" : undefined}
                tabIndex={clickDate ? 0 : undefined}
                onKeyDown={
                  clickDate
                    ? (e) => {
                        if (e.key === "Enter" || e.key === " ") {
                          e.preventDefault();
                          onSelectDay(clickDate);
                        }
                      }
                    : undefined
                }
              >
                <span className="coord-tl-group-label">
                  {groupLabel(key, tTl as (k: string) => string)}
                </span>
                <span className="coord-tl-group-count">{group.entries.length}</span>
              </div>
              <div className="coord-tl-group-entries">
                {group.entries.map((entry) => (
                  <CoordinationTimelineEntry key={entry.entryId} entry={entry} />
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

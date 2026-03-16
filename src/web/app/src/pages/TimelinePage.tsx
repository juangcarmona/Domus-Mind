import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { fetchTimeline } from "../store/timelineSlice";
import { completeTask, cancelTask } from "../store/tasksSlice";
import type { EnrichedTimelineEntry } from "../api/domusmindApi";

function formatDate(iso: string | null, locale: string): string {
  if (!iso) return "";
  const d = new Date(iso);
  return new Intl.DateTimeFormat(locale, { hour: "2-digit", minute: "2-digit" }).format(d);
}

function entryTypeDot(type: string) {
  if (type === "CalendarEvent") return "event";
  if (type === "Task") return "task";
  return "routine";
}

function StatusBadge({ status }: { status: string }) {
  const cls = status.toLowerCase();
  return (
    <span className={`entry-status-badge ${cls}`}>
      {status.toLowerCase()}
    </span>
  );
}

function TimelineEntry({
  entry,
  memberName,
  locale,
  labelPlan,
  labelChore,
  labelRoutine,
  labelUnassigned,
  labelComplete,
  labelCancel,
  onComplete,
  onCancel,
}: {
  entry: EnrichedTimelineEntry;
  memberName: string | null;
  locale: string;
  labelPlan: string;
  labelChore: string;
  labelRoutine: string;
  labelUnassigned: string;
  labelComplete: string;
  labelCancel: string;
  onComplete: (id: string) => void;
  onCancel: (id: string) => void;
}) {
  const isTask = entry.entryType === "Task";
  const isDone =
    entry.status === "Completed" || entry.status === "Cancelled";
  const typeLabel =
    entry.entryType === "CalendarEvent"
      ? labelPlan
      : entry.entryType === "Task"
        ? labelChore
        : labelRoutine;

  return (
    <div
      className={`entry-card ${entry.isOverdue ? "overdue" : entry.priority === "High" ? "high-priority" : ""}`}
    >
      <div className={`entry-type-dot ${entryTypeDot(entry.entryType)}`} />
      <div className="entry-body">
        <div className="entry-title">{entry.title}</div>
        <div className="entry-meta">
          <span>{typeLabel}</span>
          {entry.effectiveDate && (
            <span> · {formatDate(entry.effectiveDate, locale)}</span>
          )}
          {memberName && <span> · {memberName}</span>}
          {entry.isUnassigned && isTask && (
            <span style={{ color: "var(--accent)" }}> · {labelUnassigned}</span>
          )}
        </div>
      </div>
      <div className="entry-actions">
        <StatusBadge status={entry.status} />
        {isTask && !isDone && (
          <>
            <button
              className="btn btn-sm"
              onClick={() => onComplete(entry.entryId)}
              title={labelComplete}
              style={{ padding: "0.15rem 0.5rem" }}
            >
              ✓
            </button>
            <button
              className="btn btn-ghost btn-sm"
              onClick={() => onCancel(entry.entryId)}
              title={labelCancel}
              style={{ padding: "0.15rem 0.45rem" }}
            >
              ✕
            </button>
          </>
        )}
      </div>
    </div>
  );
}

export function TimelinePage() {
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { data, status, error } = useAppSelector((s) => s.timeline);
  const [filterType, setFilterType] = useState<string>("");
  const { t, i18n } = useTranslation();
  const locale = i18n.language;

  const familyId = family?.familyId;

  const memberMap = Object.fromEntries(
    members.map((m) => [m.memberId, m.name]),
  );

  function load() {
    if (!familyId) return;
    dispatch(
      fetchTimeline({
        familyId,
        types: filterType || undefined,
      }),
    );
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId, filterType]);

  async function handleComplete(taskId: string) {
    await dispatch(completeTask(taskId));
    load();
  }

  async function handleCancel(taskId: string) {
    await dispatch(cancelTask(taskId));
    load();
  }

  if (!familyId) return null;

  return (
    <div className="timeline-page">
      <div className="page-header">
        <h1>
          {data
            ? (() => {
                const today = data.groups.find((g) => g.groupKey === "Today");
                return today && today.entries.length > 0
                  ? `${t("timeline.groups.Today")} — ${today.entries.length}`
                  : t("timeline.title");
              })()
            : t("timeline.title")}
        </h1>
        <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
          <select
            className="form-control"
            style={{ width: "auto" }}
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
          >
            <option value="">{t("timeline.filter.all")}</option>
            <option value="CalendarEvent">{t("timeline.filter.plans")}</option>
            <option value="Task">{t("timeline.filter.chores")}</option>
            <option value="Routine">{t("timeline.filter.routines")}</option>
          </select>
          <button className="btn btn-ghost btn-sm" onClick={load} title={t("timeline.refresh")}>
            ↻
          </button>
        </div>
      </div>

      {status === "loading" && (
        <div className="loading-wrap">{t("common.loading")}</div>
      )}

      {status === "error" && (
        <div className="card">
          <p className="error-msg">{error}</p>
          <button className="btn btn-ghost btn-sm" onClick={load}>
            Retry
          </button>
        </div>
      )}

      {status === "success" && data && (
        <>
          {data.totalEntries === 0 && (
            <div className="empty-state">
              <p>{t("timeline.empty")}</p>
            </div>
          )}

          {data.groups.map((group) => (
            <section key={group.groupKey} className="timeline-section">
              <div className="timeline-section-header">
                <h2>
                  {t(`timeline.groups.${group.groupKey}` as never, group.groupKey)}
                </h2>
                <span
                  className={`timeline-badge ${group.groupKey === "Overdue" ? "overdue" : ""}`}
                >
                  {group.entries.length}
                </span>
              </div>
              <div className="entry-list">
                {group.entries.map((entry) => (
                  <TimelineEntry
                    key={entry.entryId}
                    entry={entry}
                    memberName={
                      entry.assigneeId
                        ? (memberMap[entry.assigneeId] ?? null)
                        : null
                    }
                    locale={locale}
                    labelPlan={t("nav.plans")}
                    labelChore={t("nav.chores")}
                    labelRoutine={t("timeline.filter.routines")}
                    labelUnassigned={t("tasks.unassigned")}
                    labelComplete={t("timeline.actions.complete")}
                    labelCancel={t("timeline.actions.cancel")}
                    onComplete={handleComplete}
                    onCancel={handleCancel}
                  />
                ))}
              </div>
            </section>
          ))}
        </>
      )}
    </div>
  );
}

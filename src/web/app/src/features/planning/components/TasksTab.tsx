import { useTranslation } from "react-i18next";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import { EntityCard } from "../../../components/EntityCard";
import type { EnrichedTimelineEntry } from "../../../api/domusmindApi";

type TaskGroupKey = "Overdue" | "Today" | "Tomorrow" | "ThisWeek" | "Later" | "Undated";
const ACTIVE_GROUP_ORDER: TaskGroupKey[] = ["Overdue", "Today", "Tomorrow", "ThisWeek", "Later", "Undated"];

interface Props {
  activeTasks: EnrichedTimelineEntry[];
  tasksLoading: boolean;
  /** null = not yet loaded; load triggered by onLoadHistory */
  taskHistory: EnrichedTimelineEntry[] | null;
  taskHistoryLoading: boolean;
  memberMap: Record<string, string>;
  onEdit: (taskId: string) => void;
  onAssign: (task: EnrichedTimelineEntry) => void;
  onComplete: (taskId: string) => void;
  onCancel: (taskId: string) => void;
  onLoadHistory: () => void;
}

function TaskRow({
  task,
  memberMap,
  onEdit,
  onAssign,
  onComplete,
  onCancel,
}: {
  task: EnrichedTimelineEntry;
  memberMap: Record<string, string>;
  onEdit: (id: string) => void;
  onAssign: (task: EnrichedTimelineEntry) => void;
  onComplete: (id: string) => void;
  onCancel: (id: string) => void;
}) {
  const { t } = useTranslation("tasks");
  const { t: tCommon } = useTranslation("common");
  const { formatDate } = useDateFormatter();

  const dateLabel = task.effectiveDate ? formatDate(task.effectiveDate) : null;
  const assigneeLabel = task.assigneeId && memberMap[task.assigneeId]
    ? memberMap[task.assigneeId]
    : task.isUnassigned
      ? t("unassigned")
      : null;
  const subtitle = [dateLabel, assigneeLabel].filter(Boolean).join(" · ");

  return (
    <EntityCard
      title={task.title}
      subtitle={subtitle || undefined}
      accentColor={task.color}
      isOverdue={task.isOverdue}
      onClick={() => onEdit(task.entryId)}
      actions={
        <>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={(e) => { e.stopPropagation(); onAssign(task); }}
            title={t("assignTitle")}
          >
            {t("assign")}
          </button>
          <button
            type="button"
            className="btn btn-sm"
            onClick={(e) => { e.stopPropagation(); onComplete(task.entryId); }}
            title={t("markDoneTitle")}
          >
            ✓ {t("done")}
          </button>
          <button
            type="button"
            className="btn btn-ghost btn-sm planning-cancel-btn"
            onClick={(e) => { e.stopPropagation(); onCancel(task.entryId); }}
            title={tCommon("cancel")}
          >
            ✕
          </button>
        </>
      }
    />
  );
}

export function TasksTab({
  activeTasks,
  tasksLoading,
  taskHistory,
  taskHistoryLoading,
  memberMap,
  onEdit,
  onAssign,
  onComplete,
  onCancel,
  onLoadHistory,
}: Props) {
  const { t } = useTranslation("tasks");
  const { t: tCommon } = useTranslation("common");

  if (tasksLoading) {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  const groupLabels: Record<TaskGroupKey, string> = {
    Overdue: t("groupOverdue"),
    Today: t("groupToday"),
    Tomorrow: t("groupTomorrow"),
    ThisWeek: t("groupThisWeek"),
    Later: t("groupLater"),
    Undated: t("groupUndated"),
  };

  // Bucket tasks by their API-provided group key
  const buckets = Object.fromEntries(
    ACTIVE_GROUP_ORDER.map((k) => [k, [] as EnrichedTimelineEntry[]])
  ) as Record<TaskGroupKey, EnrichedTimelineEntry[]>;

  for (const task of activeTasks) {
    const key = (ACTIVE_GROUP_ORDER.includes(task.group as TaskGroupKey)
      ? task.group
      : "Later") as TaskGroupKey;
    buckets[key].push(task);
  }

  const hasAnyActive = activeTasks.length > 0;

  return (
    <section>
      {!hasAnyActive ? (
        <div className="empty-state">
          <p>{t("empty")}</p>
        </div>
      ) : (
        ACTIVE_GROUP_ORDER.map((groupKey) => {
          const tasks = buckets[groupKey];
          if (tasks.length === 0) return null;

          const isOverdueGroup = groupKey === "Overdue";
          const isUndatedGroup = groupKey === "Undated";

          return (
            <div
              key={groupKey}
              className={`planning-group${isUndatedGroup ? " planning-group--backlog" : ""}`}
            >
              <div className={`planning-group-header${isOverdueGroup ? " planning-group-header--danger" : ""}`}>
                {groupLabels[groupKey]}
              </div>
              <div className="item-list">
                {tasks.map((task) => (
                  <TaskRow
                    key={task.entryId}
                    task={task}
                    memberMap={memberMap}
                    onEdit={onEdit}
                    onAssign={onAssign}
                    onComplete={onComplete}
                    onCancel={onCancel}
                  />
                ))}
              </div>
            </div>
          );
        })
      )}

      {/* History affordance: show trigger only until history has been loaded */}
      {taskHistory === null && (
        <div className="planning-history-trigger">
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={onLoadHistory}
            disabled={taskHistoryLoading}
          >
            {taskHistoryLoading ? tCommon("loading") : t("showHistory")}
          </button>
        </div>
      )}

      {taskHistory !== null && (
        <div className="planning-history-section">
          <div className="planning-history-label">{t("history")}</div>
          {taskHistory.length === 0 ? (
            <p className="planning-history-empty">{t("noHistory")}</p>
          ) : (
            <div className="item-list">
              {taskHistory.map((task) => (
                <EntityCard
                  key={task.entryId}
                  title={task.title}
                  titleStrike={task.status === "Completed"}
                  subtitle={task.status.toLowerCase()}
                  accentColor={task.color}
                  dimmed
                  onClick={() => onEdit(task.entryId)}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </section>
  );
}

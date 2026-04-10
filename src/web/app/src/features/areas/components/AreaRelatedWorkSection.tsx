import { useTranslation } from "react-i18next";
import { EntityCard } from "../../../components/EntityCard";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import type { EnrichedTimelineEntry, FamilyTimelineEventItem, RoutineListItem } from "../../../api/domusmindApi";
import { formatRoutineAssigned, formatRoutineDays } from "../../planning/utils/routineFormatters";

interface AreaRelatedWorkSectionProps {
  tasksLoading: boolean;
  linkedTasks: EnrichedTimelineEntry[];
  linkedPlans: FamilyTimelineEventItem[];
  linkedRoutines: RoutineListItem[];
  memberMap: Record<string, string>;
  /** When provided, renders an add button in the section header. */
  onAddClick?: () => void;
  onEdit: (target: { type: "task" | "routine" | "event"; id: string }) => void;
  onCompleteTask: (taskId: string) => void;
  onCancelTask: (taskId: string) => void;
  onPauseRoutine: (routineId: string) => void;
  onResumeRoutine: (routineId: string) => void;
}

export function AreaRelatedWorkSection({
  tasksLoading,
  linkedTasks,
  linkedPlans,
  linkedRoutines,
  memberMap,
  onAddClick,
  onEdit,
  onCompleteTask,
  onCancelTask,
  onPauseRoutine,
  onResumeRoutine,
}: AreaRelatedWorkSectionProps) {
  const { t } = useTranslation("areas");
  const { t: tTasks } = useTranslation("tasks");
  const { t: tPlans } = useTranslation("plans");
  const { t: tRoutines } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");
  const { formatDate, formatDateTime } = useDateFormatter();

  return (
    <div className="area-detail-section">
      <div className="area-detail-section-header">
        <span className="area-detail-section-title">{t("relatedWork")}</span>
        {onAddClick && (
          <button
            type="button"
            className="btn btn-sm"
            style={{ marginLeft: "auto" }}
            onClick={onAddClick}
          >
            + {tCommon("add")}
          </button>
        )}
      </div>

      {tasksLoading ? (
        <div className="loading-wrap area-related-loading">{tCommon("loading")}</div>
      ) : linkedTasks.length > 0 ? (
        <div style={{ marginBottom: "1.25rem" }}>
          <p className="area-related-group-label">{tTasks("title")}</p>
          <div className="item-list">
            {linkedTasks.map((task) => (
              <EntityCard
                key={task.entryId}
                title={task.title}
                subtitle={
                  <>
                    {task.effectiveDate ? formatDate(task.effectiveDate) : tTasks("noDueDate")}
                    {task.assigneeId && memberMap[task.assigneeId]
                      ? ` · ${memberMap[task.assigneeId]}`
                      : task.isUnassigned
                        ? ` · ${tTasks("unassigned")}`
                        : ""}
                    {task.isOverdue && (
                      <span style={{ color: "var(--danger)" }}> · {tTasks("overdue")}</span>
                    )}
                  </>
                }
                accentColor={task.color}
                isOverdue={task.isOverdue}
                onClick={() => onEdit({ type: "task", id: task.entryId })}
                actions={
                  <>
                    <button
                      className="btn btn-sm"
                      title={tTasks("markDoneTitle")}
                      onClick={(e) => {
                        e.stopPropagation();
                        onCompleteTask(task.entryId);
                      }}
                    >
                      ✓ {tTasks("done")}
                    </button>
                    <button
                      className="btn btn-ghost btn-sm"
                      title={tCommon("cancel")}
                      onClick={(e) => {
                        e.stopPropagation();
                        onCancelTask(task.entryId);
                      }}
                    >
                      ✕
                    </button>
                  </>
                }
              />
            ))}
          </div>
        </div>
      ) : null}

      {linkedPlans.length > 0 && (
        <div style={{ marginBottom: "1.25rem" }}>
          <p className="area-related-group-label">{tPlans("title")}</p>
          <div className="item-list">
            {linkedPlans.map((plan) => (
              <EntityCard
                key={plan.calendarEventId}
                title={plan.title}
                titleStrike={plan.status === "Cancelled"}
                subtitle={
                  <>
                    {formatDateTime(plan.startTime)}
                    {plan.endTime && ` → ${formatDateTime(plan.endTime)}`}
                    {plan.participants?.length > 0 && (
                      <span> · {plan.participants.map((p) => p.displayName).join(", ")}</span>
                    )}
                  </>
                }
                accentColor={plan.color}
                onClick={() => onEdit({ type: "event", id: plan.calendarEventId })}
              />
            ))}
          </div>
        </div>
      )}

      {linkedRoutines.length > 0 && (
        <div style={{ marginBottom: "1.25rem" }}>
          <p className="area-related-group-label">{tRoutines("title")}</p>
          <div className="item-list">
            {linkedRoutines.map((routine) => {
              const days = formatRoutineDays(routine, tRoutines);
              const assigned = formatRoutineAssigned(routine, memberMap, tRoutines);
              const statusLine = routine.status === "Paused"
                ? <span style={{ color: "var(--muted)", fontWeight: 600 }}>{tRoutines("paused")}</span>
                : <span style={{ color: "var(--success)", fontWeight: 600 }}>{tRoutines("active")}</span>;
              return (
                <EntityCard
                  key={routine.routineId}
                  title={routine.name}
                  subtitle={
                    <>
                      {tRoutines(`frequency${routine.frequency}` as Parameters<typeof tRoutines>[0])}
                      {days ? ` · ${days}` : ""}
                      {routine.time ? ` · ${routine.time.slice(0, 5)}` : ""}
                      {` · ${assigned}`}
                      <span style={{ display: "block", marginTop: "0.2rem" }}>{statusLine}</span>
                    </>
                  }
                  accentColor={routine.color}
                  onClick={() => onEdit({ type: "routine", id: routine.routineId })}
                  actions={
                    routine.status === "Active" ? (
                      <button
                        className="btn btn-ghost btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          onPauseRoutine(routine.routineId);
                        }}
                      >
                        {tRoutines("pause")}
                      </button>
                    ) : (
                      <button
                        className="btn btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          onResumeRoutine(routine.routineId);
                        }}
                      >
                        {tRoutines("resume")}
                      </button>
                    )
                  }
                />
              );
            })}
          </div>
        </div>
      )}

      {!tasksLoading &&
        linkedTasks.length === 0 &&
        linkedPlans.length === 0 &&
        linkedRoutines.length === 0 && (
          <div className="area-related-empty">
            <p style={{ fontWeight: 500, marginBottom: "0.4rem", color: "var(--text)" }}>
              {t("relatedWorkEmpty")}
            </p>
            <p className="area-related-hint-text">{t("relatedWorkHint")}</p>
          </div>
        )}
    </div>
  );
}

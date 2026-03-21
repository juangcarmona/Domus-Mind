import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchPlans, cancelEvent } from "../../../store/plansSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { fetchRoutines, pauseRoutine, resumeRoutine } from "../../../store/routinesSlice";
import { completeTask, cancelTask, assignTask } from "../../../store/tasksSlice";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import { PlanningAddModal } from "../components/modals/PlanningAddModal";
import { EditEntityModal } from "../../editors/components/EditEntityModal";
import { AssignTaskModal } from "../components/modals/AssignTaskModal";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import type { FamilyTimelineEventItem, EnrichedTimelineEntry, RoutineListItem } from "../../../api/domusmindApi";

type PlanningTab = "routines" | "tasks" | "plans";

const DAY_KEYS = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"] as const;

export function PlanningPage() {
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: planItems, status: plansStatus } = useAppSelector((s) => s.plans);
  const { data: timeline, status: timelineStatus } = useAppSelector((s) => s.timeline);
  const { items: routineItems, status: routinesStatus } = useAppSelector((s) => s.routines);
  const familyId = family?.familyId;

  const { t: tPlans } = useTranslation("plans");
  const { t: tTasks } = useTranslation("tasks");
  const { t: tRoutines } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");
  const { t: tNav } = useTranslation("nav");
  const { formatDate, formatDateTime } = useDateFormatter();

  const [activeTab, setActiveTab] = useState<PlanningTab>("routines");
  const [addModal, setAddModal] = useState<"plan" | "task" | "routine" | "choose" | null>(null);
  const [cancelTarget, setCancelTarget] = useState<FamilyTimelineEventItem | null>(null);
  const [assignTarget, setAssignTarget] = useState<EnrichedTimelineEntry | null>(null);
  const [editTarget, setEditTarget] = useState<{ type: "routine" | "task" | "event"; id: string } | null>(null);

  const memberMap = Object.fromEntries(members.map((m) => [m.memberId, m.name]));

  function loadPlans() {
    if (familyId) dispatch(fetchPlans(familyId));
  }

  function loadTasks() {
    if (familyId) dispatch(fetchTimeline({ familyId, types: "Task" }));
  }

  function loadRoutines() {
    if (familyId && routinesStatus === "idle") dispatch(fetchRoutines(familyId));
  }

  useEffect(() => {
    loadPlans();
    loadTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId]);

  useEffect(() => {
    loadRoutines();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId, routinesStatus]);

  async function handleCancelPlan() {
    if (!cancelTarget || !familyId) return;
    await dispatch(cancelEvent({ eventId: cancelTarget.calendarEventId, familyId }));
    setCancelTarget(null);
  }

  async function handleCompleteTask(taskId: string) {
    await dispatch(completeTask(taskId));
    loadTasks();
  }

  async function handleCancelTask(taskId: string) {
    await dispatch(cancelTask(taskId));
    loadTasks();
  }

  async function handleAssign(taskId: string, memberId: string) {
    await dispatch(assignTask({ taskId, assigneeId: memberId }));
    loadTasks();
  }

  async function handlePauseRoutine(routineId: string) {
    if (!familyId) return;
    await dispatch(pauseRoutine({ routineId, familyId }));
  }

  async function handleResumeRoutine(routineId: string) {
    if (!familyId) return;
    await dispatch(resumeRoutine({ routineId, familyId }));
  }

  function formatRoutineDays(routine: RoutineListItem): string {
    if (routine.frequency === "Weekly" && routine.daysOfWeek.length > 0) {
      return routine.daysOfWeek
        .slice()
        .sort((a, b) => a - b)
        .map((d) => tRoutines(DAY_KEYS[d]))
        .join(", ");
    }
    if (
      (routine.frequency === "Monthly" || routine.frequency === "Yearly") &&
      routine.daysOfMonth.length > 0
    ) {
      return routine.daysOfMonth.join(", ");
    }
    return "";
  }

  function routineAssignedLabel(routine: RoutineListItem): string {
    if (routine.scope === "Members" && routine.targetMemberIds.length > 0) {
      const names = routine.targetMemberIds
        .map((id) => memberMap[id] ?? id)
        .join(", ");
      return names;
    }
    return tRoutines("scopeHousehold");
  }

  if (!familyId) return null;

  const activePlans = planItems.filter((p) => p.status !== "Cancelled");

  const tasks: EnrichedTimelineEntry[] =
    timeline?.groups.flatMap((g) =>
      g.entries.filter((e) => e.entryType === "Task"),
    ) ?? [];

  const activeTasks = tasks.filter(
    (t) => t.status !== "Completed" && t.status !== "Cancelled",
  );
  const doneTasks = tasks.filter(
    (t) => t.status === "Completed" || t.status === "Cancelled",
  );

  const tabs: { key: PlanningTab; label: string }[] = [
    { key: "routines", label: tRoutines("title") },
    { key: "tasks", label: tTasks("title") },
    { key: "plans", label: tPlans("title") },
  ];

  return (
    <div>
      <div className="page-header">
        <h1>{tNav("planning")}</h1>
        <button className="btn" onClick={() => setAddModal("choose")}>
          + {tCommon("add")}
        </button>
      </div>

      {/* ── Tab navigation ── */}
      <div className="settings-tabs" style={{ marginBottom: "1.5rem" }}>
        {tabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            className={`settings-tab${activeTab === tab.key ? " active" : ""}`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* ── Routines tab ── */}
      {activeTab === "routines" && (
        <section>
          {routinesStatus === "loading" && <div className="loading-wrap">{tCommon("loading")}</div>}

          {routinesStatus !== "loading" && routineItems.length === 0 && (
            <div className="empty-state">
              <p>{tRoutines("empty")}</p>
              <p>{tRoutines("emptyHint")}</p>
            </div>
          )}

          {routineItems.length > 0 && (
            <div className="item-list">
              {routineItems.map((routine: RoutineListItem) => {
                const days = formatRoutineDays(routine);
                const assigned = routineAssignedLabel(routine);
                const isGeneratesTasks = routine.kind === "Scheduled";
                return (
                  <div
                    key={routine.routineId}
                    className="item-card"
                    style={{ borderLeft: `3px solid ${routine.color}` }}
                    onClick={() => setEditTarget({ type: "routine", id: routine.routineId })}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        setEditTarget({ type: "routine", id: routine.routineId });
                      }
                    }}
                  >
                    <div className="item-card-body">
                      <div className="item-card-title">{routine.name}</div>
                      <div className="item-card-subtitle">
                        {tRoutines(`frequency${routine.frequency}` as Parameters<typeof tRoutines>[0])}
                        {days ? ` · ${days}` : ""}
                        {routine.time ? ` · ${routine.time.slice(0, 5)}` : ""}
                        {` · ${assigned}`}
                      </div>
                      <div className="item-card-subtitle" style={{ marginTop: "0.2rem" }}>
                        <span style={{ color: routine.status === "Paused" ? "var(--muted)" : "var(--success)", fontWeight: 600 }}>
                          {routine.status === "Paused" ? tRoutines("paused") : tRoutines("active")}
                        </span>
                        <span style={{ color: "var(--muted)" }}>
                          {" · "}
                          {isGeneratesTasks
                            ? `→ ${tRoutines("executionTypeGeneratesTasks")}`
                            : `→ ${tRoutines("executionTypeReminderOnly")}`}
                        </span>
                      </div>
                    </div>
                    <div className="item-card-actions">
                      {routine.status === "Active" ? (
                        <button
                          className="btn btn-ghost btn-sm"
                          onClick={(e) => {
                            e.stopPropagation();
                            handlePauseRoutine(routine.routineId);
                          }}
                        >
                          {tRoutines("pause")}
                        </button>
                      ) : (
                        <button
                          className="btn btn-sm"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleResumeRoutine(routine.routineId);
                          }}
                        >
                          {tRoutines("resume")}
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </section>
      )}

      {/* ── Tasks tab ── */}
      {activeTab === "tasks" && (
        <section>
          {timelineStatus === "loading" && <div className="loading-wrap">{tCommon("loading")}</div>}

          {activeTasks.length === 0 && timelineStatus !== "loading" && (
            <div className="empty-state">
              <p>{tTasks("empty")}</p>
            </div>
          )}

          {activeTasks.length > 0 && (
            <>
              <div style={{ marginBottom: "0.4rem", fontSize: "0.82rem", color: "var(--muted)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.04em" }}>
                {tTasks("active")} ({activeTasks.length})
              </div>
              <div className="item-list" style={{ marginBottom: "1rem" }}>
                {activeTasks.map((task) => (
                  <div
                    key={task.entryId}
                    className={`item-card ${task.isOverdue ? "overdue" : ""}`}
                    style={task.isOverdue ? { borderLeft: "3px solid var(--danger)" } : undefined}
                    onClick={() => setEditTarget({ type: "task", id: task.entryId })}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        setEditTarget({ type: "task", id: task.entryId });
                      }
                    }}
                  >
                    <div className="item-card-body">
                      <div className="item-card-title">{task.title}</div>
                      <div className="item-card-subtitle">
                        {task.effectiveDate ? formatDate(task.effectiveDate) : tTasks("noDueDate")}
                        {task.assigneeId && memberMap[task.assigneeId]
                          ? ` · ${memberMap[task.assigneeId]}`
                          : task.isUnassigned
                            ? ` · ${tTasks("unassigned")}`
                            : ""}
                        {task.isOverdue && (
                          <span style={{ color: "var(--danger)" }}> · {tTasks("overdue")}</span>
                        )}
                      </div>
                    </div>
                    <div className="item-card-actions">
                      <button
                        className="btn btn-ghost btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          setAssignTarget(task);
                        }}
                        title={tTasks("assignTitle")}
                      >
                        {tTasks("assign")}
                      </button>
                      <button
                        className="btn btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleCompleteTask(task.entryId);
                        }}
                        title={tTasks("markDoneTitle")}
                      >
                        ✓ {tTasks("done")}
                      </button>
                      <button
                        className="btn btn-ghost btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleCancelTask(task.entryId);
                        }}
                        title={tCommon("cancel")}
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}

          {doneTasks.length > 0 && (
            <>
              <div style={{ marginBottom: "0.4rem", fontSize: "0.82rem", color: "var(--muted)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.04em" }}>
                {tTasks("completedCancelled")}
              </div>
              <div className="item-list">
                {doneTasks.slice(0, 10).map((task) => (
                  <div
                    key={task.entryId}
                    className="item-card"
                    style={{ opacity: 0.65 }}
                    onClick={() => setEditTarget({ type: "task", id: task.entryId })}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        setEditTarget({ type: "task", id: task.entryId });
                      }
                    }}
                  >
                    <div className="item-card-body">
                      <div
                        className="item-card-title"
                        style={{ textDecoration: task.status === "Completed" ? "line-through" : undefined }}
                      >
                        {task.title}
                      </div>
                      <div className="item-card-subtitle">{task.status.toLowerCase()}</div>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </section>
      )}

      {/* ── Plans tab ── */}
      {activeTab === "plans" && (
        <section>
          {plansStatus === "loading" && <div className="loading-wrap">{tCommon("loading")}</div>}

          {plansStatus === "success" && activePlans.length === 0 && (
            <div className="empty-state">
              <p>{tPlans("noPlans")}</p>
            </div>
          )}

          {activePlans.length > 0 && (
            <div className="item-list">
              {activePlans.map((plan) => (
                <div
                  key={plan.calendarEventId}
                  className="item-card"
                  onClick={() => setEditTarget({ type: "event", id: plan.calendarEventId })}
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      setEditTarget({ type: "event", id: plan.calendarEventId });
                    }
                  }}
                >
                  <div className="item-card-body">
                    <div className="item-card-title">{plan.title}</div>
                    <div className="item-card-subtitle">
                      {formatDateTime(plan.startTime)}
                      {plan.endTime && ` → ${formatDateTime(plan.endTime)}`}
                      {plan.participants?.length > 0 && (
                        <span> · {plan.participants.map((p) => p.displayName).join(", ")}</span>
                      )}
                    </div>
                    <div className="item-card-subtitle" style={{ marginTop: "0.2rem" }}>
                      <span className={`entry-status-badge ${plan.status.toLowerCase()}`}>
                        {plan.status.toLowerCase()}
                      </span>
                    </div>
                  </div>
                  <div className="item-card-actions">
                    {plan.status !== "Cancelled" && (
                      <button
                        className="btn btn-ghost btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          setCancelTarget(plan);
                        }}
                      >
                        {tPlans("cancelEvent")}
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      )}

      {/* Modals */}
      {addModal && (
        <PlanningAddModal
          familyId={familyId}
          members={members}
          initialStep={addModal === "choose" ? "choose" : addModal}
          onClose={() => setAddModal(null)}
          onSuccess={() => {
            setAddModal(null);
            loadPlans();
            loadTasks();
            dispatch(fetchRoutines(familyId));
          }}
        />
      )}

      {assignTarget && (
        <AssignTaskModal
          entry={assignTarget}
          members={members}
          onAssign={handleAssign}
          onClose={() => setAssignTarget(null)}
        />
      )}

      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={async () => {
            setEditTarget(null);
            loadPlans();
            loadTasks();
            await dispatch(fetchRoutines(familyId));
          }}
        />
      )}

      <ConfirmDialog
        isOpen={!!cancelTarget}
        title={tPlans("cancelEvent")}
        message={tPlans("confirmCancel")}
        confirmLabel={tPlans("yes")}
        onConfirm={handleCancelPlan}
        onCancel={() => setCancelTarget(null)}
      />
    </div>
  );
}

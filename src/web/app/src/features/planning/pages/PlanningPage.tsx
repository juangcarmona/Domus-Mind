import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchPlans, cancelEvent } from "../../../store/plansSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { fetchRoutines, pauseRoutine, resumeRoutine } from "../../../store/routinesSlice";
import { completeTask, cancelTask, assignTask } from "../../../store/tasksSlice";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import { PlanningAddModal } from "../../../components/PlanningAddModal";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import type { FamilyTimelineEventItem, EnrichedTimelineEntry, RoutineListItem } from "../../../api/domusmindApi";

function AssignModal({
  entry,
  members,
  onAssign,
  onClose,
}: {
  entry: EnrichedTimelineEntry;
  members: { memberId: string; name: string }[];
  onAssign: (taskId: string, memberId: string) => Promise<void>;
  onClose: () => void;
}) {
  const { t } = useTranslation("tasks");
  const { t: tCommon } = useTranslation("common");
  const [memberId, setMemberId] = useState("");
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!memberId) return;
    setSubmitting(true);
    await onAssign(entry.entryId, memberId);
    setSubmitting(false);
    onClose();
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>{t("assign")} — {entry.title}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="planning-assign-select">{t("assignTo")}</label>
            <select
              id="planning-assign-select"
              className="form-control"
              value={memberId}
              onChange={(e) => setMemberId(e.target.value)}
              required
              autoFocus
            >
              <option value="">{tCommon("selectPerson")}</option>
              {members.map((m) => (
                <option key={m.memberId} value={m.memberId}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {tCommon("cancel")}
            </button>
            <button type="submit" className="btn" disabled={submitting || !memberId}>
              {submitting ? t("assigning") : t("assign")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

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
  const { t: tTimeline } = useTranslation("timeline");
  const { t: tCommon } = useTranslation("common");
  const { t: tNav, i18n } = useTranslation("nav");
  const locale = i18n.language;
  const { formatDate, formatDateTime } = useDateFormatter(locale);

  const [addModal, setAddModal] = useState<"plan" | "task" | "routine" | null>(null);
  const [cancelTarget, setCancelTarget] = useState<FamilyTimelineEventItem | null>(null);
  const [assignTarget, setAssignTarget] = useState<EnrichedTimelineEntry | null>(null);

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

  return (
    <div>
      <div className="page-header">
        <h1>{tNav("planning")}</h1>
        <button className="btn" onClick={() => setAddModal("plan")}>
          + {tCommon("add")}
        </button>
      </div>

      {/* ── Plans section ── */}
      <section style={{ marginBottom: "2.5rem" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "0.75rem" }}>
          <h2 style={{ margin: 0, fontSize: "1.1rem" }}>{tPlans("title")}</h2>
          <button className="btn btn-ghost btn-sm" onClick={() => setAddModal("plan")}>
            + {tPlans("add")}
          </button>
        </div>

        {plansStatus === "loading" && <div className="loading-wrap">{tCommon("loading")}</div>}

        {plansStatus === "success" && activePlans.length === 0 && (
          <div className="empty-state">
            <p>{tPlans("noPlans")}</p>
          </div>
        )}

        {activePlans.length > 0 && (
          <div className="item-list">
            {activePlans.map((plan) => (
              <div key={plan.calendarEventId} className="item-card">
                <div className="item-card-body">
                  <div className="item-card-title">{plan.title}</div>
                  <div className="item-card-subtitle">
                    {formatDateTime(plan.startTime)}
                    {plan.endTime && ` → ${formatDateTime(plan.endTime)}`}
                    {plan.participants?.length > 0 && (
                      <span> · {plan.participants.map((p) => p.displayName).join(", ")}</span>
                    )}
                  </div>
                </div>
                <div className="item-card-actions">
                  <span className={`entry-status-badge ${plan.status.toLowerCase()}`}>
                    {plan.status.toLowerCase()}
                  </span>
                  {plan.status !== "Cancelled" && (
                    <button
                      className="btn btn-ghost btn-sm"
                      onClick={() => setCancelTarget(plan)}
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

      {/* ── Tasks section ── */}
      <section style={{ marginBottom: "2.5rem" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "0.75rem" }}>
          <h2 style={{ margin: 0, fontSize: "1.1rem" }}>{tTasks("title")}</h2>
          <button className="btn btn-ghost btn-sm" onClick={() => setAddModal("task")}>
            + {tTasks("add")}
          </button>
        </div>

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
                >
                  <div className="item-card-body">
                    <div className="item-card-title">{task.title}</div>
                    <div className="item-card-subtitle">
                      {task.effectiveDate ? formatDate(task.effectiveDate) : tTasks("noDueDate")}
                      {task.assigneeId && memberMap[task.assigneeId]
                        ? ` · ${memberMap[task.assigneeId]}`
                        : task.isUnassigned
                          ? ` · ${tTimeline("unassigned")}`
                          : ""}
                      {task.isOverdue && (
                        <span style={{ color: "var(--danger)" }}> · {tTasks("overdue")}</span>
                      )}
                    </div>
                  </div>
                  <div className="item-card-actions">
                    <button
                      className="btn btn-ghost btn-sm"
                      onClick={() => setAssignTarget(task)}
                      title={tTasks("assignTitle")}
                    >
                      {tTasks("assign")}
                    </button>
                    <button
                      className="btn btn-sm"
                      onClick={() => handleCompleteTask(task.entryId)}
                      title={tTasks("markDoneTitle")}
                    >
                      ✓ {tTasks("done")}
                    </button>
                    <button
                      className="btn btn-ghost btn-sm"
                      onClick={() => handleCancelTask(task.entryId)}
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
                <div key={task.entryId} className="item-card" style={{ opacity: 0.65 }}>
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

      {/* ── Routines section ── */}
      <section>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "0.75rem" }}>
          <h2 style={{ margin: 0, fontSize: "1.1rem" }}>{tRoutines("title")}</h2>
          <button className="btn btn-ghost btn-sm" onClick={() => setAddModal("routine")}>
            + {tRoutines("add")}
          </button>
        </div>

        {routinesStatus === "loading" && <div className="loading-wrap">{tCommon("loading")}</div>}

        {routinesStatus !== "loading" && routineItems.length === 0 && (
          <div className="empty-state">
            <p>{tRoutines("empty")}</p>
            <p>{tRoutines("emptyHint")}</p>
          </div>
        )}

        {routineItems.length > 0 && (
          <div className="item-list">
            {routineItems.map((routine: RoutineListItem) => (
              <div key={routine.routineId} className="item-card">
                <div className="item-card-body">
                  <div className="item-card-title">{routine.name}</div>
                  <div className="item-card-subtitle">
                    {routine.frequency}
                    {routine.time ? ` · ${routine.time.slice(0, 5)}` : ""}
                    {" · "}
                    <span style={{ color: routine.status === "Paused" ? "var(--muted)" : "var(--success)" }}>
                      {routine.status === "Paused" ? tRoutines("paused") : tRoutines("active")}
                    </span>
                  </div>
                </div>
                <div className="item-card-actions">
                  {routine.status === "Active" ? (
                    <button
                      className="btn btn-ghost btn-sm"
                      onClick={() => handlePauseRoutine(routine.routineId)}
                    >
                      {tRoutines("pause")}
                    </button>
                  ) : (
                    <button
                      className="btn btn-sm"
                      onClick={() => handleResumeRoutine(routine.routineId)}
                    >
                      {tRoutines("resume")}
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Modals */}
      {addModal && (
        <PlanningAddModal
          familyId={familyId}
          members={members}
          initialStep={addModal}
          onClose={() => setAddModal(null)}
          onSuccess={() => {
            setAddModal(null);
            loadPlans();
            loadTasks();
            if (addModal === "routine") dispatch(fetchRoutines(familyId));
          }}
        />
      )}

      {assignTarget && (
        <AssignModal
          entry={assignTarget}
          members={members}
          onAssign={handleAssign}
          onClose={() => setAssignTarget(null)}
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

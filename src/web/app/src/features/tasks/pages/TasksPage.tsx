import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchTimeline } from "../../../store/timelineSlice";
import { createTask, completeTask, cancelTask, assignTask } from "../../../store/tasksSlice";
import { fetchRoutines, createRoutine, updateRoutine, pauseRoutine, resumeRoutine } from "../../../store/routinesSlice";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import type { EnrichedTimelineEntry, RoutineListItem } from "../../../api/domusmindApi";

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

  async function handleSubmit(e: FormEvent) {
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
            <label htmlFor="assign-select">{t("assignTo")}</label>
            <select
              id="assign-select"
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

export function TasksPage() {
  const { t, i18n } = useTranslation("tasks");
  const { t: tRoutines } = useTranslation("routines");
  const { t: tTimeline } = useTranslation("timeline");
  const { t: tCommon } = useTranslation("common");
  const locale = i18n.language;
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { data: timeline, status: timelineStatus } = useAppSelector((s) => s.timeline);
  const { items: routineItems, status: routinesStatus } = useAppSelector((s) => s.routines);
  const familyId = family?.familyId;
  const { formatDate } = useDateFormatter(locale);

  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [assignTarget, setAssignTarget] = useState<EnrichedTimelineEntry | null>(null);

  // Routine form state
  const [showRoutineForm, setShowRoutineForm] = useState(false);
  const [editingRoutine, setEditingRoutine] = useState<RoutineListItem | null>(null);
  const [routineName, setRoutineName] = useState("");
  const [routineScope, setRoutineScope] = useState("Household");
  const [routineKind, setRoutineKind] = useState("Scheduled");
  const [routineColor, setRoutineColor] = useState("#3B82F6");
  const [routineFrequency, setRoutineFrequency] = useState("Weekly");
  const [routineDaysOfWeek, setRoutineDaysOfWeek] = useState<number[]>([]);
  const [routineDaysOfMonth, setRoutineDaysOfMonth] = useState("");
  const [routineMonthOfYear, setRoutineMonthOfYear] = useState("");
  const [routineTime, setRoutineTime] = useState("");
  const [routineTargetMemberIds, setRoutineTargetMemberIds] = useState<string[]>([]);
  const [routineSubmitting, setRoutineSubmitting] = useState(false);
  const [routineFormError, setRoutineFormError] = useState<string | null>(null);

  const memberMap = Object.fromEntries(members.map((m) => [m.memberId, m.name]));

  function loadTasks() {
    if (familyId) dispatch(fetchTimeline({ familyId, types: "Task" }));
  }

  useEffect(() => {
    loadTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId]);

  useEffect(() => {
    if (familyId && routinesStatus === "idle") {
      dispatch(fetchRoutines(familyId));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId, routinesStatus]);

  const tasks: EnrichedTimelineEntry[] =
    timeline?.groups.flatMap((g) =>
      g.entries.filter((e) => e.entryType === "Task"),
    ) ?? [];

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!familyId || !title.trim()) return;
    setSubmitting(true);
    setFormError(null);
    const result = await dispatch(
      createTask({
        familyId,
        title: title.trim(),
        dueDate: dueDate ? new Date(dueDate).toISOString() : null,
      }),
    );
    setSubmitting(false);
    if (createTask.fulfilled.match(result)) {
      setTitle("");
      setDueDate("");
      setShowForm(false);
      loadTasks();
    } else {
      setFormError(result.payload as string ?? t("createError"));
    }
  }

  async function handleComplete(taskId: string) {
    await dispatch(completeTask(taskId));
    loadTasks();
  }

  async function handleCancel(taskId: string) {
    await dispatch(cancelTask(taskId));
    loadTasks();
  }

  async function handleAssign(taskId: string, memberId: string) {
    await dispatch(assignTask({ taskId, assigneeId: memberId }));
    loadTasks();
  }

  function openCreateRoutine() {
    setEditingRoutine(null);
    setRoutineName("");
    setRoutineScope("Household");
    setRoutineKind("Scheduled");
    setRoutineColor("#3B82F6");
    setRoutineFrequency("Weekly");
    setRoutineDaysOfWeek([]);
    setRoutineDaysOfMonth("");
    setRoutineMonthOfYear("");
    setRoutineTime("");
    setRoutineTargetMemberIds([]);
    setRoutineFormError(null);
    setShowRoutineForm(true);
  }

  function openEditRoutine(routine: RoutineListItem) {
    setEditingRoutine(routine);
    setRoutineName(routine.name);
    setRoutineScope(routine.scope);
    setRoutineKind(routine.kind);
    setRoutineColor(routine.color);
    setRoutineFrequency(routine.frequency);
    setRoutineDaysOfWeek(routine.daysOfWeek ?? []);
    setRoutineDaysOfMonth((routine.daysOfMonth ?? []).join(","));
    setRoutineMonthOfYear(routine.monthOfYear != null ? String(routine.monthOfYear) : "");
    setRoutineTime(routine.time ?? "");
    setRoutineTargetMemberIds(routine.targetMemberIds ?? []);
    setRoutineFormError(null);
    setShowRoutineForm(true);
  }

  function parseDaysOfMonth(raw: string): number[] {
    return raw.split(",")
      .map((s) => parseInt(s.trim(), 10))
      .filter((n) => !isNaN(n) && n >= 1 && n <= 31);
  }

  function buildRoutinePayload() {
    return {
      name: routineName.trim(),
      scope: routineScope,
      kind: routineKind,
      color: routineColor,
      frequency: routineFrequency,
      daysOfWeek: routineFrequency === "Weekly" ? routineDaysOfWeek : [],
      daysOfMonth: routineFrequency !== "Weekly" ? parseDaysOfMonth(routineDaysOfMonth) : [],
      monthOfYear: routineFrequency === "Yearly" && routineMonthOfYear ? parseInt(routineMonthOfYear, 10) : null,
      time: routineTime || null,
      targetMemberIds: routineScope === "Members" ? routineTargetMemberIds : [],
    };
  }

  async function handleRoutineSubmit(e: FormEvent) {
    e.preventDefault();
    if (!familyId || !routineName.trim()) return;
    setRoutineSubmitting(true);
    setRoutineFormError(null);
    const payload = buildRoutinePayload();
    if (editingRoutine) {
      const result = await dispatch(
        updateRoutine({ routineId: editingRoutine.routineId, ...payload }),
      );
      if (updateRoutine.fulfilled.match(result)) {
        setShowRoutineForm(false);
        dispatch(fetchRoutines(familyId));
      } else {
        setRoutineFormError(result.payload as string ?? tRoutines("updateError"));
      }
    } else {
      const result = await dispatch(
        createRoutine({ familyId, ...payload }),
      );
      if (createRoutine.fulfilled.match(result)) {
        setShowRoutineForm(false);
        dispatch(fetchRoutines(familyId));
      } else {
        setRoutineFormError(result.payload as string ?? tRoutines("createError"));
      }
    }
    setRoutineSubmitting(false);
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

  const active = tasks.filter(
    (t) => t.status !== "Completed" && t.status !== "Cancelled",
  );
  const done = tasks.filter(
    (t) => t.status === "Completed" || t.status === "Cancelled",
  );

  return (
    <div>
      <div className="page-header">
        <h1>{t("title")}</h1>
        <button
          className="btn"
          onClick={() => { setShowForm(true); setFormError(null); }}
        >
          {t("add")}
        </button>
      </div>

      {showForm && (
        <div className="card">
          <h2>{t("addHeading")}</h2>
          <form onSubmit={handleCreate}>
            <div className="form-group">
              <label htmlFor="task-title">{t("titleLabel")}</label>
              <input
                id="task-title"
                className="form-control"
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required
                autoFocus
                placeholder={t("titlePlaceholder")}
              />
            </div>
            <div className="form-group">
              <label htmlFor="task-due">{t("dueLabel")}</label>
              <input
                id="task-due"
                className="form-control"
                type="date"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
              />
            </div>
            {formError && <p className="error-msg">{formError}</p>}
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button type="submit" className="btn" disabled={submitting}>
                {submitting ? tCommon("adding") : tCommon("add")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => setShowForm(false)}
              >
                {tCommon("cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {timelineStatus === "loading" && (
        <div className="loading-wrap">{t("loading")}</div>
      )}

      {active.length === 0 && timelineStatus !== "loading" && (
        <div className="empty-state">
          <p>{t("empty")}</p>
          <p>{t("emptyHint")}</p>
        </div>
      )}

      {active.length > 0 && (
        <>
          <div style={{ marginBottom: "0.5rem", fontSize: "0.82rem", color: "var(--muted)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.04em" }}>
            {t("active")} ({active.length})
          </div>
          <div className="item-list" style={{ marginBottom: "1.5rem" }}>
            {active.map((task) => (
              <div
                key={task.entryId}
                className={`item-card ${task.isOverdue ? "overdue" : ""}`}
                style={task.isOverdue ? { borderLeft: "3px solid var(--danger)" } : undefined}
              >
                <div className="item-card-body">
                  <div className="item-card-title">{task.title}</div>
                  <div className="item-card-subtitle">
                    {task.effectiveDate ? formatDate(task.effectiveDate) : t("noDueDate")}
                    {task.assigneeId && memberMap[task.assigneeId]
                      ? ` · ${memberMap[task.assigneeId]}`
                      : task.isUnassigned
                          ? ` · ${tTimeline("unassigned")}`
                        : ""}
                    {task.isOverdue && (
                      <span style={{ color: "var(--danger)" }}> · {t("overdue")}</span>
                    )}
                  </div>
                </div>
                <div className="item-card-actions">
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => setAssignTarget(task)}
                    title={t("assignTitle")}
                  >
                    {t("assign")}
                  </button>
                  <button
                    className="btn btn-sm"
                    onClick={() => handleComplete(task.entryId)}
                    title={t("markDoneTitle")}
                  >
                    ✓ {t("done")}
                  </button>
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => handleCancel(task.entryId)}
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

      {done.length > 0 && (
        <>
          <div style={{ marginBottom: "0.5rem", fontSize: "0.82rem", color: "var(--muted)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.04em" }}>
            {t("completedCancelled")}
          </div>
          <div className="item-list">
            {done.slice(0, 10).map((task) => (
              <div key={task.entryId} className="item-card" style={{ opacity: 0.65 }}>
                <div className="item-card-body">
                  <div className="item-card-title" style={{ textDecoration: task.status === "Completed" ? "line-through" : undefined }}>
                    {task.title}
                  </div>
                  <div className="item-card-subtitle">{task.status.toLowerCase()}</div>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {assignTarget && (
        <AssignModal
          entry={assignTarget}
          members={members}
          onAssign={handleAssign}
          onClose={() => setAssignTarget(null)}
        />
      )}

      {/* ---- Routines section ---- */}
      <div style={{ marginTop: "2.5rem" }}>
        <div className="page-header">
          <h2 style={{ margin: 0 }}>{tRoutines("title")}</h2>
          <button className="btn" onClick={openCreateRoutine}>
            {tRoutines("add")}
          </button>
        </div>

        {showRoutineForm && (
          <div className="card" style={{ marginBottom: "1rem" }}>
            <h3>{editingRoutine ? tRoutines("editHeading") : tRoutines("addHeading")}</h3>
            <form onSubmit={handleRoutineSubmit}>
              <div className="form-group">
                <label htmlFor="routine-name">{tRoutines("nameLabel")}</label>
                <input
                  id="routine-name"
                  className="form-control"
                  type="text"
                  value={routineName}
                  onChange={(e) => setRoutineName(e.target.value)}
                  required
                  autoFocus
                  placeholder={tRoutines("namePlaceholder")}
                />
              </div>
              <div className="form-group">
                <label htmlFor="routine-scope">{tRoutines("scopeLabel")}</label>
                <select
                  id="routine-scope"
                  className="form-control"
                  value={routineScope}
                  onChange={(e) => setRoutineScope(e.target.value)}
                >
                  <option value="Household">{tRoutines("scopeHousehold")}</option>
                  <option value="Members">{tRoutines("scopeMembers")}</option>
                </select>
              </div>
              {routineScope === "Members" && members.length > 0 && (
                <div className="form-group">
                  <label>{tRoutines("targetMembersLabel")}</label>
                  <div style={{ display: "flex", flexWrap: "wrap", gap: "0.5rem" }}>
                    {members.map((m) => (
                      <label key={m.memberId} style={{ display: "flex", alignItems: "center", gap: "0.25rem" }}>
                        <input
                          type="checkbox"
                          checked={routineTargetMemberIds.includes(m.memberId)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setRoutineTargetMemberIds((prev) => [...prev, m.memberId]);
                            } else {
                              setRoutineTargetMemberIds((prev) => prev.filter((id) => id !== m.memberId));
                            }
                          }}
                        />
                        {m.name}
                      </label>
                    ))}
                  </div>
                </div>
              )}
              <div className="form-group">
                <label htmlFor="routine-kind">{tRoutines("kindLabel")}</label>
                <select
                  id="routine-kind"
                  className="form-control"
                  value={routineKind}
                  onChange={(e) => setRoutineKind(e.target.value)}
                >
                  <option value="Scheduled">{tRoutines("kindScheduled")}</option>
                  <option value="Cue">{tRoutines("kindCue")}</option>
                </select>
              </div>
              <div className="form-group">
                <label htmlFor="routine-color">{tRoutines("colorLabel")}</label>
                <input
                  id="routine-color"
                  className="form-control"
                  type="color"
                  value={routineColor}
                  onChange={(e) => setRoutineColor(e.target.value.toUpperCase())}
                />
              </div>
              <div className="form-group">
                <label htmlFor="routine-frequency">{tRoutines("frequencyLabel")}</label>
                <select
                  id="routine-frequency"
                  className="form-control"
                  value={routineFrequency}
                  onChange={(e) => { setRoutineFrequency(e.target.value); setRoutineDaysOfWeek([]); setRoutineDaysOfMonth(""); setRoutineMonthOfYear(""); }}
                >
                  <option value="Weekly">{tRoutines("frequencyWeekly")}</option>
                  <option value="Monthly">{tRoutines("frequencyMonthly")}</option>
                  <option value="Yearly">{tRoutines("frequencyYearly")}</option>
                </select>
              </div>
              {routineFrequency === "Weekly" && (
                <div className="form-group">
                  <label>{tRoutines("daysOfWeekLabel")}</label>
                  <div style={{ display: "flex", flexWrap: "wrap", gap: "0.5rem" }}>
                    {[
                        { value: 0, label: tRoutines("sun") },
                      { value: 1, label: tRoutines("mon") },
                      { value: 2, label: tRoutines("tue") },
                      { value: 3, label: tRoutines("wed") },
                      { value: 4, label: tRoutines("thu") },
                      { value: 5, label: tRoutines("fri") },
                      { value: 6, label: tRoutines("sat") },
                    ].map((d) => (
                      <label key={d.value} style={{ display: "flex", alignItems: "center", gap: "0.25rem" }}>
                        <input
                          type="checkbox"
                          checked={routineDaysOfWeek.includes(d.value)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setRoutineDaysOfWeek((prev) => [...prev, d.value].sort());
                            } else {
                              setRoutineDaysOfWeek((prev) => prev.filter((v) => v !== d.value));
                            }
                          }}
                        />
                        {d.label}
                      </label>
                    ))}
                  </div>
                </div>
              )}
              {(routineFrequency === "Monthly" || routineFrequency === "Yearly") && (
                <div className="form-group">
                  <label htmlFor="routine-days-of-month">{tRoutines("daysOfMonthLabel")}</label>
                  <input
                    id="routine-days-of-month"
                    className="form-control"
                    type="text"
                    value={routineDaysOfMonth}
                    onChange={(e) => setRoutineDaysOfMonth(e.target.value)}
                    placeholder={tRoutines("daysOfMonthPlaceholder")}
                  />
                </div>
              )}
              {routineFrequency === "Yearly" && (
                <div className="form-group">
                  <label htmlFor="routine-month">{tRoutines("monthOfYearLabel")}</label>
                  <select
                    id="routine-month"
                    className="form-control"
                    value={routineMonthOfYear}
                    onChange={(e) => setRoutineMonthOfYear(e.target.value)}
                  >
                    <option value="">{tRoutines("selectMonth")}</option>
                    {Array.from({ length: 12 }, (_, i) => (
                      <option key={i + 1} value={String(i + 1)}>
                        {new Date(2000, i, 1).toLocaleString(undefined, { month: "long" })}
                      </option>
                    ))}
                  </select>
                </div>
              )}
              <div className="form-group">
                <label htmlFor="routine-time">{tRoutines("timeLabel")}</label>
                <input
                  id="routine-time"
                  className="form-control"
                  type="time"
                  value={routineTime}
                  onChange={(e) => setRoutineTime(e.target.value)}
                />
              </div>
              {routineFormError && <p className="error-msg">{routineFormError}</p>}
              <div style={{ display: "flex", gap: "0.5rem" }}>
                <button type="submit" className="btn" disabled={routineSubmitting}>
                  {routineSubmitting ? tCommon("saving") : tRoutines("save")}
                </button>
                <button
                  type="button"
                  className="btn btn-ghost"
                  onClick={() => setShowRoutineForm(false)}
                >
                  {tRoutines("cancel")}
                </button>
              </div>
            </form>
          </div>
        )}

        {routinesStatus === "loading" && (
          <div className="loading-wrap">{tCommon("loading")}</div>
        )}

        {routinesStatus !== "loading" && routineItems.length === 0 && (
          <div className="empty-state">
            <p>{tRoutines("empty")}</p>
            <p>{tRoutines("emptyHint")}</p>
          </div>
        )}

        {routineItems.length > 0 && (
          <div className="item-list">
            {routineItems.map((routine) => (
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
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => openEditRoutine(routine)}
                  >
                    {tRoutines("edit")}
                  </button>
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
      </div>
    </div>
  );
}

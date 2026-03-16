import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchTimeline } from "../../../store/timelineSlice";
import { createTask, completeTask, cancelTask, assignTask } from "../../../store/tasksSlice";
import type { EnrichedTimelineEntry } from "../../../api/domusmindApi";

function formatDate(iso: string | null, locale: string, noDateLabel: string): string {
  if (!iso) return noDateLabel;
  const d = new Date(iso);
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(d);
}

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
  const { t } = useTranslation();
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
        <h2>{t("tasks.assign")} — {entry.title}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="assign-select">{t("tasks.assignTo")}</label>
            <select
              id="assign-select"
              className="form-control"
              value={memberId}
              onChange={(e) => setMemberId(e.target.value)}
              required
              autoFocus
            >
              <option value="">{t("common.selectPerson")}</option>
              {members.map((m) => (
                <option key={m.memberId} value={m.memberId}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {t("common.cancel")}
            </button>
            <button type="submit" className="btn" disabled={submitting || !memberId}>
              {submitting ? t("tasks.assigning") : t("tasks.assign")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export function TasksPage() {
  const { t, i18n } = useTranslation();
  const locale = i18n.language;
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { data: timeline, status: timelineStatus } = useAppSelector((s) => s.timeline);
  const familyId = family?.familyId;

  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [assignTarget, setAssignTarget] = useState<EnrichedTimelineEntry | null>(null);

  const memberMap = Object.fromEntries(members.map((m) => [m.memberId, m.name]));

  function loadTasks() {
    if (familyId) dispatch(fetchTimeline({ familyId, types: "Task" }));
  }

  useEffect(() => {
    loadTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId]);

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
      setFormError(result.payload as string ?? t("tasks.createError"));
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
        <h1>{t("tasks.title")}</h1>
        <button
          className="btn"
          onClick={() => { setShowForm(true); setFormError(null); }}
        >
          {t("tasks.add")}
        </button>
      </div>

      {showForm && (
        <div className="card">
          <h2>{t("tasks.addHeading")}</h2>
          <form onSubmit={handleCreate}>
            <div className="form-group">
              <label htmlFor="task-title">{t("tasks.titleLabel")}</label>
              <input
                id="task-title"
                className="form-control"
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required
                autoFocus
                placeholder={t("tasks.titlePlaceholder")}
              />
            </div>
            <div className="form-group">
              <label htmlFor="task-due">{t("tasks.dueLabel")}</label>
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
                {submitting ? t("common.adding") : t("common.add")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => setShowForm(false)}
              >
                {t("common.cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {timelineStatus === "loading" && (
        <div className="loading-wrap">{t("tasks.loading")}</div>
      )}

      {active.length === 0 && timelineStatus !== "loading" && (
        <div className="empty-state">
          <p>{t("tasks.empty")}</p>
          <p>{t("tasks.emptyHint")}</p>
        </div>
      )}

      {active.length > 0 && (
        <>
          <div style={{ marginBottom: "0.5rem", fontSize: "0.82rem", color: "var(--muted)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.04em" }}>
            {t("tasks.active")} ({active.length})
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
                    {formatDate(task.effectiveDate, locale, t("tasks.noDueDate"))}
                    {task.assigneeId && memberMap[task.assigneeId]
                      ? ` · ${memberMap[task.assigneeId]}`
                      : task.isUnassigned
                        ? ` · ${t("timeline.unassigned")}`
                        : ""}
                    {task.isOverdue && (
                      <span style={{ color: "var(--danger)" }}> · {t("tasks.overdue")}</span>
                    )}
                  </div>
                </div>
                <div className="item-card-actions">
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => setAssignTarget(task)}
                    title={t("tasks.assignTitle")}
                  >
                    {t("tasks.assign")}
                  </button>
                  <button
                    className="btn btn-sm"
                    onClick={() => handleComplete(task.entryId)}
                    title={t("tasks.markDoneTitle")}
                  >
                    ✓ {t("tasks.done")}
                  </button>
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => handleCancel(task.entryId)}
                    title={t("common.cancel")}
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
            {t("tasks.completedCancelled")}
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
    </div>
  );
}

import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { domusmindApi } from "../api/domusmindApi";
import { createTask } from "../store/tasksSlice";
import { useAppDispatch } from "../store/hooks";

interface TaskCrudFormProps {
  mode: "create" | "edit";
  familyId: string;
  taskId?: string;
  initialTitle?: string;
  initialDueDate?: string | null;
  onCancel: () => void;
  onSuccess: () => void | Promise<void>;
}

function toLocalDateInput(value?: string | null): string {
  if (!value) return "";
  return value.slice(0, 10);
}

function toLocalTimeInput(value?: string | null): string {
  if (!value) return "";
  if (!value.includes("T") && !value.includes(":")) return "";
  if (value.includes("T")) {
    const d = new Date(value);
    return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
  }
  return value.slice(0, 5);
}

export function TaskCrudForm({
  mode,
  familyId,
  taskId,
  initialTitle,
  initialDueDate,
  onCancel,
  onSuccess,
}: TaskCrudFormProps) {
  const dispatch = useAppDispatch();
  const { t: tTasks } = useTranslation("tasks");
  const { t: tCommon } = useTranslation("common");

  const [title, setTitle] = useState(initialTitle ?? "");
  const [dueDate, setDueDate] = useState(toLocalDateInput(initialDueDate));
  const [dueTime, setDueTime] = useState(toLocalTimeInput(initialDueDate));
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    if (!title.trim()) {
      setSubmitting(false);
      return;
    }

    if (mode === "create") {
      const result = await dispatch(
        createTask({
          familyId,
          title: title.trim(),
          dueDate: dueDate || null,
          dueTime: dueTime || null,
        }),
      );
      setSubmitting(false);
      if (createTask.fulfilled.match(result)) {
        await Promise.resolve(onSuccess());
      } else {
        setError((result.payload as string) ?? tCommon("failed"));
      }
      return;
    }

    if (!taskId) {
      setSubmitting(false);
      return;
    }

    try {
      await domusmindApi.rescheduleTask(
        taskId,
        dueDate || null,
        dueTime || null,
        title.trim() || null,
      );
      setSubmitting(false);
      await Promise.resolve(onSuccess());
    } catch (err) {
      setError((err as { message?: string }).message ?? tCommon("failed"));
      setSubmitting(false);
    }
  }

  return (
    <>
      <h2>{mode === "create" ? tTasks("addHeading") : tTasks("title")}</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="task-form-title">{tTasks("titleLabel")}</label>
          <input
            id="task-form-title"
            className="form-control"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            autoFocus
            placeholder={tTasks("titlePlaceholder")}
          />
        </div>
        <div className="form-group">
          <label htmlFor="task-form-due">{tTasks("dueLabel")}</label>
          <input
            id="task-form-due"
            className="form-control"
            type="date"
            value={dueDate}
            onChange={(e) => setDueDate(e.target.value)}
          />
        </div>
        <div className="form-group">
          <label htmlFor="task-form-time">{tTasks("timeLabel")}</label>
          <input
            id="task-form-time"
            className="form-control"
            type="time"
            value={dueTime}
            onChange={(e) => setDueTime(e.target.value)}
          />
        </div>
        {error && <p className="error-msg">{error}</p>}
        <div className="modal-footer">
          <button type="button" className="btn btn-ghost" onClick={onCancel}>
            {tCommon("cancel")}
          </button>
          <button type="submit" className="btn" disabled={submitting}>
            {submitting ? tCommon("saving") : mode === "create" ? tCommon("add") : tCommon("save")}
          </button>
        </div>
      </form>
    </>
  );
}

import { useState, useEffect, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { domusmindApi } from "../../../api/domusmindApi";
import { createTask } from "../../../store/tasksSlice";
import { fetchAreas } from "../../../store/areasSlice";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { toLocalDateInput, toLocalTimeInput } from "../utils";
import { DateInput } from "../../../components/DateInput";

interface TaskCrudFormProps {
  mode: "create" | "edit";
  familyId: string;
  taskId?: string;
  initialTitle?: string;
  initialDueDate?: string | null;
  initialColor?: string | null;
  /** Pre-selects the area picker (create mode only). */
  initialAreaId?: string;
  /** Pre-selects the assignee dropdown (create mode only). */
  initialAssigneeId?: string;
  members?: { memberId: string; name: string }[];
  onCancel: () => void;
  onSuccess: () => void | Promise<void>;
}

export function TaskCrudForm({
  mode,
  familyId,
  taskId,
  initialTitle,
  initialDueDate,
  initialColor,
  initialAreaId,
  initialAssigneeId,
  members,
  onCancel,
  onSuccess,
}: TaskCrudFormProps) {
  const dispatch = useAppDispatch();
  const { t: tTasks } = useTranslation("tasks");
  const { t: tCommon } = useTranslation("common");
  const { t: tAreas } = useTranslation("areas");

  const areas = useAppSelector((s) => s.areas.items);
  const areasStatus = useAppSelector((s) => s.areas.status);

  useEffect(() => {
    if (areasStatus === "idle") dispatch(fetchAreas(familyId));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [areasStatus, familyId]);

  const [title, setTitle] = useState(initialTitle ?? "");
  const [dueDate, setDueDate] = useState(toLocalDateInput(initialDueDate));
  const [dueTime, setDueTime] = useState(toLocalTimeInput(initialDueDate));
  const [color, setColor] = useState(
    initialColor ?? "#3B82F6",
  );
  const [selectedAreaId, setSelectedAreaId] = useState(initialAreaId ?? "");
  const [assigneeId, setAssigneeId] = useState<string>(initialAssigneeId ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (initialColor || !selectedAreaId) return;
    const selected = areas.find((a) => a.areaId === selectedAreaId);
    if (selected?.color) setColor(selected.color);
  }, [areas, selectedAreaId, initialColor]);

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
          color,
          areaId: selectedAreaId || null,
        }),
      );
      setSubmitting(false);
      if (createTask.fulfilled.match(result)) {
        if (assigneeId) {
          try {
            await domusmindApi.assignTask((result.payload as { taskId: string }).taskId, { assigneeId });
          } catch (assignErr) {
            console.error("Failed to assign task", assignErr);
          }
        }
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
        color,
      );
      if (assigneeId && assigneeId !== initialAssigneeId) {
        try {
          await domusmindApi.assignTask(taskId, { assigneeId });
        } catch (assignErr) {
          console.error("Failed to assign task", assignErr);
        }
      }
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
        <div className="inline-form">
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="task-form-due">{tTasks("dueLabel")}</label>
            <DateInput
              id="task-form-due"
              className="form-control"
              value={dueDate}
              onChange={setDueDate}
            />
          </div>
          <div className="form-group" style={{ flex: "0 0 7rem" }}>
            <label htmlFor="task-form-time">{tTasks("timeLabel")}</label>
            <input
              id="task-form-time"
              className="form-control"
              type="time"
              value={dueTime}
              onChange={(e) => setDueTime(e.target.value)}
            />
          </div>
        </div>
        {areas.length > 0 && (
          <div className="inline-form">
            <div className="form-group" style={{ flex: 1 }}>
              <label htmlFor="task-form-area">{tAreas("areaLabel")}</label>
              <select
                id="task-form-area"
                className="form-control"
                value={selectedAreaId}
                onChange={(e) => {
                  const id = e.target.value;
                  setSelectedAreaId(id);
                  if (!id) return;
                  const selected = areas.find((a) => a.areaId === id);
                  if (selected?.color) setColor(selected.color);
                }}
              >
                <option value="">{tAreas("noArea")}</option>
                {areas.map((a) => (
                  <option key={a.areaId} value={a.areaId}>{a.name}</option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flex: "0 0 auto" }}>
              <label htmlFor="task-form-color">{tTasks("colorLabel")}</label>
              <input
                id="task-form-color"
                className="form-control editor-color-swatch"
                type="color"
                value={color}
                onChange={(e) => setColor(e.target.value.toUpperCase())}
              />
            </div>
          </div>
        )}
        {areas.length === 0 && (
          <div className="form-group">
            <label htmlFor="task-form-color">{tTasks("colorLabel")}</label>
            <input
              id="task-form-color"
              className="form-control editor-color-swatch"
              type="color"
              value={color}
              onChange={(e) => setColor(e.target.value.toUpperCase())}
            />
          </div>
        )}
        {members && members.length > 0 && (
          <div className="form-group">
            <label htmlFor="task-form-assignee">{tTasks("assignTo")}</label>
            <select
              id="task-form-assignee"
              className="form-control"
              value={assigneeId}
              onChange={(e) => setAssigneeId(e.target.value)}
            >
              <option value="">{tTasks("unassigned")}</option>
              {members.map((m) => (
                <option key={m.memberId} value={m.memberId}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>
        )}
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

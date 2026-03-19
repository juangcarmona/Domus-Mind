import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { domusmindApi } from "../api/domusmindApi";
import { scheduleEvent } from "../store/plansSlice";
import { useAppDispatch } from "../store/hooks";

interface PlanCrudFormProps {
  mode: "create" | "edit";
  familyId: string;
  eventId?: string;
  initialTitle?: string;
  initialStartTime?: string | null;
  initialEndTime?: string | null;
  initialDescription?: string | null;
  onCancel: () => void;
  onSuccess: () => void | Promise<void>;
}

function toLocalDateInput(value?: string | null): string {
  if (!value) return "";
  return value.slice(0, 10);
}

function toLocalTimeInput(value?: string | null): string {
  if (!value) return "";
  if (value.includes("T")) {
    const d = new Date(value);
    return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
  }
  return value.slice(0, 5);
}

export function PlanCrudForm({
  mode,
  familyId,
  eventId,
  initialTitle,
  initialStartTime,
  initialEndTime,
  initialDescription,
  onCancel,
  onSuccess,
}: PlanCrudFormProps) {
  const dispatch = useAppDispatch();
  const { t: tPlans } = useTranslation("plans");
  const { t: tCommon } = useTranslation("common");

  const [title, setTitle] = useState(initialTitle ?? "");
  const [startDate, setStartDate] = useState(toLocalDateInput(initialStartTime));
  const [startTime, setStartTime] = useState(toLocalTimeInput(initialStartTime));
  const [endDate, setEndDate] = useState(toLocalDateInput(initialEndTime));
  const [endTime, setEndTime] = useState(toLocalTimeInput(initialEndTime));
  const [description, setDescription] = useState(initialDescription ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    if (mode === "create") {
      if (!title.trim() || !startDate) {
        setSubmitting(false);
        return;
      }
      const result = await dispatch(
        scheduleEvent({
          familyId,
          title: title.trim(),
          date: startDate,
          time: startTime || undefined,
          endDate: endDate || undefined,
          endTime: endTime || undefined,
          description: description.trim() || undefined,
        }),
      );
      setSubmitting(false);
      if (scheduleEvent.fulfilled.match(result)) {
        await Promise.resolve(onSuccess());
      } else {
        setError((result.payload as string) ?? tCommon("failed"));
      }
      return;
    }

    if (!eventId || !startDate) {
      setSubmitting(false);
      return;
    }

    try {
      await domusmindApi.rescheduleEvent(eventId, {
        date: startDate,
        time: startTime || undefined,
        endDate: endDate || undefined,
        endTime: endTime || undefined,
        title: title.trim(),
        description: description.trim() || null,
      });
      setSubmitting(false);
      await Promise.resolve(onSuccess());
    } catch (err) {
      setError((err as { message?: string }).message ?? tCommon("failed"));
      setSubmitting(false);
    }
  }

  return (
    <>
      <h2>{mode === "create" ? tPlans("add") : tPlans("title")}</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="plan-form-title">{tPlans("form.title")}</label>
          <input
            id="plan-form-title"
            className="form-control"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            autoFocus
          />
        </div>
        <div className="inline-form">
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-start-date">{tPlans("form.startDate")}</label>
            <input
              id="plan-form-start-date"
              className="form-control"
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              required
            />
          </div>
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-start-time">{tPlans("form.startTime")}</label>
            <input
              id="plan-form-start-time"
              className="form-control"
              type="time"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
            />
          </div>
        </div>
        <div className="inline-form">
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-end-date">{tPlans("form.endDate")}</label>
            <input
              id="plan-form-end-date"
              className="form-control"
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
            />
          </div>
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-end-time">{tPlans("form.endTime")}</label>
            <input
              id="plan-form-end-time"
              className="form-control"
              type="time"
              value={endTime}
              onChange={(e) => setEndTime(e.target.value)}
            />
          </div>
        </div>
        <div className="form-group">
          <label htmlFor="plan-form-desc">{tPlans("form.description")}</label>
          <input
            id="plan-form-desc"
            className="form-control"
            type="text"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>
        {error && <p className="error-msg">{error}</p>}
        <div className="modal-footer">
          <button type="button" className="btn btn-ghost" onClick={onCancel}>
            {tCommon("cancel")}
          </button>
          <button type="submit" className="btn" disabled={submitting}>
            {submitting ? tCommon("saving") : mode === "create" ? tPlans("form.save") : tCommon("save")}
          </button>
        </div>
      </form>
    </>
  );
}

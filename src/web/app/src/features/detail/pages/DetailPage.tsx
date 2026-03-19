import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchTimeline } from "../../../store/timelineSlice";
import { fetchRoutines, updateRoutine } from "../../../store/routinesSlice";
import { fetchPlans } from "../../../store/plansSlice";
import { domusmindApi } from "../../../api/domusmindApi";

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

function isDetailType(value: string | undefined): value is "task" | "routine" | "event" {
  return value === "task" || value === "routine" || value === "event";
}

export function DetailPage() {
  const { type: rawType, id } = useParams<{ type: string; id: string }>();
  const type = isDetailType(rawType) ? rawType : null;
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const { t: tCommon } = useTranslation("common");
  const { t: tTasks } = useTranslation("tasks");
  const { t: tRoutines } = useTranslation("routines");
  const { t: tPlans } = useTranslation("plans");

  const familyId = useAppSelector((s) => s.household.family?.familyId ?? "");
  const timeline = useAppSelector((s) => s.timeline.data);
  const timelineStatus = useAppSelector((s) => s.timeline.status);
  const routines = useAppSelector((s) => s.routines.items);
  const routinesStatus = useAppSelector((s) => s.routines.status);
  const plans = useAppSelector((s) => s.plans.items);
  const plansStatus = useAppSelector((s) => s.plans.status);

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!familyId || !type) return;
    if (type === "task" && timelineStatus === "idle") {
      dispatch(fetchTimeline({ familyId, types: "Task" }));
    }
    if (type === "routine" && routinesStatus === "idle") {
      dispatch(fetchRoutines(familyId));
    }
    if (type === "event" && plansStatus === "idle") {
      dispatch(fetchPlans(familyId));
    }
  }, [dispatch, familyId, plansStatus, routinesStatus, timelineStatus, type]);

  const task = useMemo(() => {
    if (type !== "task" || !timeline || !id) return null;
    const all = timeline.groups.flatMap((g) => g.entries);
    return all.find((entry) => entry.entryType === "Task" && entry.entryId === id) ?? null;
  }, [id, timeline, type]);

  const routine = useMemo(() => {
    if (type !== "routine" || !id) return null;
    return routines.find((r) => r.routineId === id) ?? null;
  }, [id, routines, type]);

  const event = useMemo(() => {
    if (type !== "event" || !id) return null;
    return plans.find((p) => p.calendarEventId === id) ?? null;
  }, [id, plans, type]);

  async function handleSaveTask(e: FormEvent) {
    e.preventDefault();
    if (!id) return;
    const form = e.currentTarget as HTMLFormElement;
    const formData = new FormData(form);
    const dueDate = (formData.get("taskDate") as string) || null;
    const dueTime = (formData.get("taskTime") as string) || null;
    setSaving(true);
    setError(null);
    try {
      await domusmindApi.rescheduleTask(id, dueDate, dueTime);
      if (familyId) {
        await dispatch(fetchTimeline({ familyId, types: "Task" }));
      }
    } catch (err) {
      setError((err as { message?: string }).message ?? tCommon("failed"));
      setSaving(false);
      return;
    }
    setSaving(false);
    navigate(-1);
  }

  async function handleSaveRoutine(e: FormEvent) {
    e.preventDefault();
    if (!routine || !id) return;
    const form = e.currentTarget as HTMLFormElement;
    const formData = new FormData(form);
    setSaving(true);
    setError(null);
    const result = await dispatch(
      updateRoutine({
        routineId: id,
        name: ((formData.get("routineName") as string) ?? "").trim(),
        scope: routine.scope,
        kind: routine.kind,
        color: routine.color,
        frequency: routine.frequency,
        daysOfWeek: routine.daysOfWeek,
        daysOfMonth: routine.daysOfMonth,
        monthOfYear: routine.monthOfYear,
        time: (formData.get("routineTime") as string) || null,
        targetMemberIds: routine.targetMemberIds,
      }),
    );
    setSaving(false);
    if (updateRoutine.fulfilled.match(result)) {
      navigate(-1);
    } else {
      setError((result.payload as string) ?? tCommon("failed"));
    }
  }

  async function handleSaveEvent(e: FormEvent) {
    e.preventDefault();
    if (!id) return;
    const form = e.currentTarget as HTMLFormElement;
    const formData = new FormData(form);
    setSaving(true);
    setError(null);
    try {
      await domusmindApi.rescheduleEvent(id, {
        date: (formData.get("eventDate") as string) ?? "",
        time: ((formData.get("eventTime") as string) || undefined),
        endDate: ((formData.get("eventEndDate") as string) || undefined),
        endTime: ((formData.get("eventEndTime") as string) || undefined),
      });
      if (familyId) {
        await dispatch(fetchPlans(familyId));
      }
    } catch (err) {
      setError((err as { message?: string }).message ?? tCommon("failed"));
      setSaving(false);
      return;
    }
    setSaving(false);
    navigate(-1);
  }

  if (!type || !id) {
    return <p className="error-msg">{tCommon("failed")}</p>;
  }

  const notFound =
    (type === "task" && !task && timelineStatus !== "loading") ||
    (type === "routine" && !routine && routinesStatus !== "loading") ||
    (type === "event" && !event && plansStatus !== "loading");

  const loading =
    (type === "task" && timelineStatus === "loading") ||
    (type === "routine" && routinesStatus === "loading") ||
    (type === "event" && plansStatus === "loading");

  return (
    <div className="page-wrap">
      <div className="page-header">
        <h1>
          {type === "task"
            ? tTasks("title")
            : type === "routine"
              ? tRoutines("title")
              : tPlans("title")}
        </h1>
        <button className="btn btn-ghost" type="button" onClick={() => navigate(-1)}>
          {tCommon("back")}
        </button>
      </div>

      <section className="settings-section">
        {type === "task" && task && (
          <form className="card" onSubmit={handleSaveTask}>
            <h2>{task.title}</h2>
            <p className="item-card-subtitle">{task.status.toLowerCase()}</p>
            <div className="inline-form" style={{ marginTop: "0.75rem" }}>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tTasks("dueLabel")}</label>
                <input
                  className="form-control"
                  type="date"
                  name="taskDate"
                  defaultValue={toLocalDateInput(task.effectiveDate)}
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tPlans("form.startTime")}</label>
                <input
                  className="form-control"
                  type="time"
                  name="taskTime"
                  defaultValue={toLocalTimeInput(task.effectiveDate)}
                />
              </div>
            </div>
            {error && <p className="error-msg">{error}</p>}
            <div className="modal-footer">
              <button className="btn" type="submit" disabled={saving}>
                {saving ? tCommon("saving") : tCommon("save")}
              </button>
            </div>
          </form>
        )}

        {type === "routine" && routine && (
          <form className="card" onSubmit={handleSaveRoutine}>
            <h2>{routine.name}</h2>
            <div className="form-group">
              <label>{tRoutines("nameLabel")}</label>
                <input
                  className="form-control"
                  type="text"
                  name="routineName"
                  defaultValue={routine.name}
                  required
                />
              </div>
            <div className="form-group">
              <label>{tRoutines("timeLabel")}</label>
                <input
                  className="form-control"
                  type="time"
                  name="routineTime"
                  defaultValue={toLocalTimeInput(routine.time)}
                />
              </div>
            {error && <p className="error-msg">{error}</p>}
            <div className="modal-footer">
              <button className="btn" type="submit" disabled={saving}>
                {saving ? tCommon("saving") : tRoutines("save")}
              </button>
            </div>
          </form>
        )}

        {type === "event" && event && (
          <form className="card" onSubmit={handleSaveEvent}>
            <h2>{event.title}</h2>
            <div className="inline-form">
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tPlans("form.startDate")}</label>
                <input
                  className="form-control"
                  type="date"
                  name="eventDate"
                  defaultValue={toLocalDateInput(event.startTime)}
                  required
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tPlans("form.startTime")}</label>
                <input
                  className="form-control"
                  type="time"
                  name="eventTime"
                  defaultValue={toLocalTimeInput(event.startTime)}
                />
              </div>
            </div>
            <div className="inline-form">
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tPlans("form.endDate")}</label>
                <input
                  className="form-control"
                  type="date"
                  name="eventEndDate"
                  defaultValue={toLocalDateInput(event.endTime)}
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tPlans("form.endTime")}</label>
                <input
                  className="form-control"
                  type="time"
                  name="eventEndTime"
                  defaultValue={toLocalTimeInput(event.endTime)}
                />
              </div>
            </div>
            {error && <p className="error-msg">{error}</p>}
            <div className="modal-footer">
              <button className="btn" type="submit" disabled={saving}>
                {saving ? tCommon("saving") : tCommon("save")}
              </button>
            </div>
          </form>
        )}

        {notFound && <p className="error-msg">{tCommon("failed")}</p>}
        {loading && <div className="loading-wrap">{tCommon("loading")}</div>}
      </section>
    </div>
  );
}

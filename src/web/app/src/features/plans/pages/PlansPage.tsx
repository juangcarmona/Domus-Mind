import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchPlans, scheduleEvent, cancelEvent } from "../../../store/plansSlice";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import type { FamilyTimelineEventItem } from "../../../api/domusmindApi";

export function PlansPage() {
  const dispatch = useAppDispatch();
  const { family } = useAppSelector((s) => s.household);
  const { items, status, error } = useAppSelector((s) => s.plans);
  const familyId = family?.familyId;
  const { t, i18n } = useTranslation("plans");
  const { t: tCommon } = useTranslation("common");
  const locale = i18n.language;
  const { formatDateTime } = useDateFormatter(locale);

  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState("");
  const [startDate, setStartDate] = useState("");
  const [startTime, setStartTime] = useState("");
  const [endDate, setEndDate] = useState("");
  const [endTime, setEndTime] = useState("");
  const [description, setDescription] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [cancelTarget, setCancelTarget] = useState<FamilyTimelineEventItem | null>(null);

  useEffect(() => {
    if (familyId) dispatch(fetchPlans(familyId));
  }, [familyId, dispatch]);

  async function handleSchedule(e: FormEvent) {
    e.preventDefault();
    if (!familyId || !title.trim() || !startDate) return;
    setSubmitting(true);
    setFormError(null);
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
      setTitle("");
      setStartDate("");
      setStartTime("");
      setEndDate("");
      setEndTime("");
      setDescription("");
      setShowForm(false);
    } else {
      setFormError(result.payload as string ?? "Failed to schedule plan");
    }
  }

  async function handleCancel() {
    if (!cancelTarget || !familyId) return;
    await dispatch(cancelEvent({ eventId: cancelTarget.calendarEventId, familyId }));
    setCancelTarget(null);
  }

  const active = items.filter((i) => i.status !== "Cancelled");

  if (!familyId) return null;

  return (
    <div>
      <div className="page-header">
        <h1>{t("title")}</h1>
        <button
          className="btn"
          onClick={() => { setShowForm(true); setFormError(null); }}
        >
          + {t("add")}
        </button>
      </div>

      {showForm && (
        <div className="card">
          <h2>{t("add")}</h2>
          <form onSubmit={handleSchedule}>
            <div className="form-group">
              <label htmlFor="plan-title">{t("form.title")}</label>
              <input
                id="plan-title"
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
                <label htmlFor="plan-start-date">{t("form.startDate")}</label>
                <input
                  id="plan-start-date"
                  className="form-control"
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  required
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label htmlFor="plan-start-time">{t("form.startTime")}</label>
                <input
                  id="plan-start-time"
                  className="form-control"
                  type="time"
                  value={startTime}
                  onChange={(e) => setStartTime(e.target.value)}
                />
              </div>
            </div>
            <div className="inline-form">
              <div className="form-group" style={{ flex: 1 }}>
                <label htmlFor="plan-end-date">{t("form.endDate")}</label>
                <input
                  id="plan-end-date"
                  className="form-control"
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label htmlFor="plan-end-time">{t("form.endTime")}</label>
                <input
                  id="plan-end-time"
                  className="form-control"
                  type="time"
                  value={endTime}
                  onChange={(e) => setEndTime(e.target.value)}
                />
              </div>
            </div>
            <div className="form-group">
              <label htmlFor="plan-desc">{t("form.description")}</label>
              <input
                id="plan-desc"
                className="form-control"
                type="text"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
            {formError && <p className="error-msg">{formError}</p>}
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button type="submit" className="btn" disabled={submitting}>
                {submitting ? tCommon("saving") : t("form.save")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => setShowForm(false)}
              >
                {t("form.cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {status === "loading" && (
        <div className="loading-wrap">{tCommon("loading")}</div>
      )}
      {status === "error" && <p className="error-msg">{error}</p>}

      {status === "success" && active.length === 0 && (
        <div className="empty-state">
          <p>{t("noPlans")}</p>
        </div>
      )}

      {active.length > 0 && (
        <div className="item-list">
          {active.map((plan) => (
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
                <span
                  className={`entry-status-badge ${plan.status.toLowerCase()}`}
                >
                  {plan.status.toLowerCase()}
                </span>
                {plan.status !== "Cancelled" && (
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => setCancelTarget(plan)}
                  >
                    {t("cancelEvent")}
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={!!cancelTarget}
        title={t("cancelEvent")}
        message={t("confirmCancel")}
        confirmLabel={t("yes")}
        onConfirm={handleCancel}
        onCancel={() => setCancelTarget(null)}
      />
    </div>
  );
}

import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchPlans, scheduleEvent, cancelEvent } from "../../../store/plansSlice";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import type { FamilyTimelineEventItem } from "../../../api/domusmindApi";

function formatDateTime(iso: string, locale: string): string {
  const d = new Date(iso);
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium", timeStyle: "short" }).format(d);
}

export function PlansPage() {
  const dispatch = useAppDispatch();
  const { family } = useAppSelector((s) => s.household);
  const { items, status, error } = useAppSelector((s) => s.plans);
  const familyId = family?.familyId;
  const { t, i18n } = useTranslation();
  const locale = i18n.language;

  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState("");
  const [startTime, setStartTime] = useState("");
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
    if (!familyId || !title.trim() || !startTime) return;
    setSubmitting(true);
    setFormError(null);
    const result = await dispatch(
      scheduleEvent({
        familyId,
        title: title.trim(),
        startTime: new Date(startTime).toISOString(),
        endTime: endTime ? new Date(endTime).toISOString() : undefined,
        description: description.trim() || undefined,
      }),
    );
    setSubmitting(false);
    if (scheduleEvent.fulfilled.match(result)) {
      setTitle("");
      setStartTime("");
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
        <h1>{t("plans.title")}</h1>
        <button
          className="btn"
          onClick={() => { setShowForm(true); setFormError(null); }}
        >
          + {t("plans.add")}
        </button>
      </div>

      {showForm && (
        <div className="card">
          <h2>{t("plans.add")}</h2>
          <form onSubmit={handleSchedule}>
            <div className="form-group">
              <label htmlFor="plan-title">{t("plans.form.title")}</label>
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
                <label htmlFor="plan-start">{t("plans.form.start")}</label>
                <input
                  id="plan-start"
                  className="form-control"
                  type="datetime-local"
                  value={startTime}
                  onChange={(e) => setStartTime(e.target.value)}
                  required
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label htmlFor="plan-end">{t("plans.form.end")}</label>
                <input
                  id="plan-end"
                  className="form-control"
                  type="datetime-local"
                  value={endTime}
                  onChange={(e) => setEndTime(e.target.value)}
                />
              </div>
            </div>
            <div className="form-group">
              <label htmlFor="plan-desc">{t("plans.form.description")}</label>
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
                {submitting ? t("common.saving") : t("plans.form.save")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => setShowForm(false)}
              >
                {t("plans.form.cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {status === "loading" && (
        <div className="loading-wrap">{t("common.loading")}</div>
      )}
      {status === "error" && <p className="error-msg">{error}</p>}

      {status === "success" && active.length === 0 && (
        <div className="empty-state">
          <p>{t("plans.noPlans")}</p>
        </div>
      )}

      {active.length > 0 && (
        <div className="item-list">
          {active.map((plan) => (
            <div key={plan.calendarEventId} className="item-card">
              <div className="item-card-body">
                <div className="item-card-title">{plan.title}</div>
                <div className="item-card-subtitle">
                  {formatDateTime(plan.startTime, locale)}
                  {plan.endTime && ` → ${formatDateTime(plan.endTime, locale)}`}
                  {plan.participantMemberIds.length > 0 && (
                    <span>
                      {" "}
                      · {plan.participantMemberIds.length} participant
                      {plan.participantMemberIds.length > 1 ? "s" : ""}
                    </span>
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
                    {t("plans.cancelEvent")}
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={!!cancelTarget}
        title={t("plans.cancelEvent")}
        message={t("plans.confirmCancel")}
        confirmLabel={t("plans.yes")}
        onConfirm={handleCancel}
        onCancel={() => setCancelTarget(null)}
      />
    </div>
  );
}

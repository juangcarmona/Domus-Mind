import { useTranslation } from "react-i18next";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import type { FamilyTimelineEventItem } from "../../../api/domusmindApi";

interface Props {
  activePlans: FamilyTimelineEventItem[];
  plansStatus: string;
  /** null = not yet loaded; load triggered by onLoadPastPlans */
  pastPlans: FamilyTimelineEventItem[] | null;
  pastPlansLoading: boolean;
  onEdit: (eventId: string) => void;
  onCancelPlan: (plan: FamilyTimelineEventItem) => void;
  onLoadPastPlans: () => void;
}

function PlanCard({
  plan,
  onEdit,
  onCancel,
  dimmed,
}: {
  plan: FamilyTimelineEventItem;
  onEdit: (id: string) => void;
  onCancel?: (plan: FamilyTimelineEventItem) => void;
  dimmed?: boolean;
}) {
  const { t } = useTranslation("plans");
  const { formatDateTime } = useDateFormatter();
  return (
    <div
      className="item-card"
      style={{
        borderLeft: `3px solid ${plan.color}`,
        opacity: dimmed ? 0.6 : undefined,
      }}
      onClick={() => onEdit(plan.calendarEventId)}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onEdit(plan.calendarEventId);
        }
      }}
    >
      <div className="item-card-body">
        <div
          className="item-card-title"
          style={{
            textDecoration: plan.status === "Cancelled" ? "line-through" : undefined,
          }}
        >
          {plan.title}
        </div>
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
      {onCancel && plan.status !== "Cancelled" && (
        <div className="item-card-actions">
          <button
            className="btn btn-ghost btn-sm"
            onClick={(e) => {
              e.stopPropagation();
              onCancel(plan);
            }}
          >
            {t("cancelEvent")}
          </button>
        </div>
      )}
    </div>
  );
}

export function PlansTab({
  activePlans,
  plansStatus,
  pastPlans,
  pastPlansLoading,
  onEdit,
  onCancelPlan,
  onLoadPastPlans,
}: Props) {
  const { t } = useTranslation("plans");
  const { t: tCommon } = useTranslation("common");

  if (plansStatus === "loading") {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  return (
    <section>
      {activePlans.length === 0 ? (
        <div className="empty-state">
          <p>{t("noPlans")}</p>
        </div>
      ) : (
        <div className="item-list">
          {activePlans.map((plan) => (
            <PlanCard
              key={plan.calendarEventId}
              plan={plan}
              onEdit={onEdit}
              onCancel={onCancelPlan}
            />
          ))}
        </div>
      )}

      {/* History affordance: show trigger only until history has been loaded */}
      {pastPlans === null && (
        <div className="planning-history-trigger">
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={onLoadPastPlans}
            disabled={pastPlansLoading}
          >
            {pastPlansLoading ? tCommon("loading") : t("showPastPlans")}
          </button>
        </div>
      )}

      {pastPlans !== null && (
        <div className="planning-history-section">
          <div className="planning-history-label">{t("pastPlans")}</div>
          {pastPlans.length === 0 ? (
            <p className="planning-history-empty">{t("noPastPlans")}</p>
          ) : (
            <div className="item-list">
              {pastPlans.map((plan) => (
                <PlanCard
                  key={plan.calendarEventId}
                  plan={plan}
                  onEdit={onEdit}
                  dimmed
                />
              ))}
            </div>
          )}
        </div>
      )}
    </section>
  );
}

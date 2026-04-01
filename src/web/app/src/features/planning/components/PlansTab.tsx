import { useTranslation } from "react-i18next";
import { useDateFormatter } from "../../../hooks/useDateFormatter";
import { EntityCard } from "../../../components/EntityCard";
import type { FamilyTimelineEventItem } from "../../../api/domusmindApi";

type PlanGroup = "today" | "upcoming" | "later";

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

function addDays(iso: string, n: number): string {
  const d = new Date(iso + "T00:00:00");
  d.setDate(d.getDate() + n);
  return d.toISOString().slice(0, 10);
}

function planGroupOf(plan: FamilyTimelineEventItem, today: string): PlanGroup {
  const d = plan.date ?? "";
  if (d <= today) return "today";
  if (d <= addDays(today, 6)) return "upcoming";
  return "later";
}

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
  today,
  onEdit,
  onCancel,
  dimmed,
}: {
  plan: FamilyTimelineEventItem;
  today: string;
  onEdit: (id: string) => void;
  onCancel?: (plan: FamilyTimelineEventItem) => void;
  dimmed?: boolean;
}) {
  const { t } = useTranslation("plans");
  const { formatDate, formatDateTime } = useDateFormatter();
  const { i18n } = useTranslation();

  // Build a compact date+time label
  let dateLabel: string;
  const planDate = plan.date ?? "";
  const planTime = plan.time ? plan.time.slice(0, 5) : null;

  if (planDate === today) {
    dateLabel = planTime ? `${t("groupToday")} · ${planTime}` : t("groupToday");
  } else if (planDate === addDays(today, 1)) {
    // Tomorrow: show "Tomorrow · HH:MM"
    const d = new Date(planDate + "T00:00:00");
    const dayLabel = new Intl.DateTimeFormat(i18n.language, { weekday: "long" }).format(d);
    dateLabel = planTime ? `${dayLabel} · ${planTime}` : dayLabel;
  } else if (planDate) {
    // This week or later: weekday short + date
    const d = new Date(planDate + "T00:00:00");
    const dayLabel = new Intl.DateTimeFormat(i18n.language, {
      weekday: "short",
      month: "short",
      day: "numeric",
    }).format(d);
    dateLabel = planTime ? `${dayLabel} · ${planTime}` : dayLabel;
  } else {
    // Fallback to ISO datetime
    dateLabel = formatDateTime(plan.startTime);
  }

  // Multi-day end label
  let endLabel = "";
  if (plan.endDate && plan.endDate !== planDate) {
    endLabel = ` → ${formatDate(plan.endDate)}`;
  } else if (plan.endTime && planTime) {
    const endTime = plan.endTimeValue ? plan.endTimeValue.slice(0, 5) : null;
    if (endTime) endLabel = ` → ${endTime}`;
  }

  const participantsLabel =
    plan.participants?.length > 0
      ? plan.participants.map((p) => p.displayName).join(", ")
      : null;

  const subtitle = [dateLabel + endLabel, participantsLabel].filter(Boolean).join(" · ");

  return (
    <EntityCard
      title={plan.title}
      titleStrike={plan.status === "Cancelled"}
      subtitle={subtitle}
      accentColor={plan.color}
      dimmed={dimmed}
      onClick={() => onEdit(plan.calendarEventId)}
      actions={
        onCancel && plan.status !== "Cancelled" ? (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={(e) => { e.stopPropagation(); onCancel(plan); }}
          >
            {t("cancelEvent")}
          </button>
        ) : undefined
      }
    />
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
  const today = todayIso();

  if (plansStatus === "loading") {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  const groupLabels: Record<PlanGroup, string> = {
    today: t("groupToday"),
    upcoming: t("groupUpcoming"),
    later: t("groupLater"),
  };

  const buckets: Record<PlanGroup, FamilyTimelineEventItem[]> = {
    today: [],
    upcoming: [],
    later: [],
  };

  for (const plan of activePlans) {
    buckets[planGroupOf(plan, today)].push(plan);
  }

  const hasAny = activePlans.length > 0;

  return (
    <section>
      {!hasAny ? (
        <div className="empty-state">
          <p>{t("noPlans")}</p>
        </div>
      ) : (
        (["today", "upcoming", "later"] as PlanGroup[]).map((groupKey) => {
          const plans = buckets[groupKey];
          if (plans.length === 0) return null;
          return (
            <div key={groupKey} className="planning-group">
              <div className="planning-group-header">{groupLabels[groupKey]}</div>
              <div className="item-list">
                {plans.map((plan) => (
                  <PlanCard
                    key={plan.calendarEventId}
                    plan={plan}
                    today={today}
                    onEdit={onEdit}
                    onCancel={onCancelPlan}
                  />
                ))}
              </div>
            </div>
          );
        })
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
                  today={today}
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

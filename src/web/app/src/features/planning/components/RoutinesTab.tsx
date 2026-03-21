import { useTranslation } from "react-i18next";
import type { RoutineListItem } from "../../../api/domusmindApi";

const DAY_KEYS = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"] as const;

interface Props {
  routineItems: RoutineListItem[];
  routinesStatus: string;
  memberMap: Record<string, string>;
  onEdit: (routineId: string) => void;
  onPause: (routineId: string) => void;
  onResume: (routineId: string) => void;
}

export function RoutinesTab({ routineItems, routinesStatus, memberMap, onEdit, onPause, onResume }: Props) {
  const { t } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");

  function formatDays(routine: RoutineListItem): string {
    if (routine.frequency === "Weekly" && routine.daysOfWeek.length > 0) {
      return routine.daysOfWeek
        .slice()
        .sort((a, b) => a - b)
        .map((d) => t(DAY_KEYS[d]))
        .join(", ");
    }
    if (
      (routine.frequency === "Monthly" || routine.frequency === "Yearly") &&
      routine.daysOfMonth.length > 0
    ) {
      return routine.daysOfMonth.join(", ");
    }
    return "";
  }

  function assignedLabel(routine: RoutineListItem): string {
    if (routine.scope === "Members" && routine.targetMemberIds.length > 0) {
      return routine.targetMemberIds.map((id) => memberMap[id] ?? id).join(", ");
    }
    return t("scopeHousehold");
  }

  if (routinesStatus === "loading") {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  if (routineItems.length === 0) {
    return (
      <div className="empty-state">
        <p>{t("empty")}</p>
        <p>{t("emptyHint")}</p>
      </div>
    );
  }

  return (
    <div className="item-list">
      {routineItems.map((routine) => {
        const days = formatDays(routine);
        const assigned = assignedLabel(routine);
        return (
          <div
            key={routine.routineId}
            className="item-card"
            style={{ borderLeft: `3px solid ${routine.color}` }}
            onClick={() => onEdit(routine.routineId)}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                onEdit(routine.routineId);
              }
            }}
          >
            <div className="item-card-body">
              <div className="item-card-title">{routine.name}</div>
              <div className="item-card-subtitle">
                {t(`frequency${routine.frequency}` as Parameters<typeof t>[0])}
                {days ? ` · ${days}` : ""}
                {routine.time ? ` · ${routine.time.slice(0, 5)}` : ""}
                {` · ${assigned}`}
              </div>
              <div className="item-card-subtitle" style={{ marginTop: "0.2rem" }}>
                <span style={{ color: routine.status === "Paused" ? "var(--muted)" : "var(--success)", fontWeight: 600 }}>
                  {routine.status === "Paused" ? t("paused") : t("active")}
                </span>
              </div>
            </div>
            <div className="item-card-actions">
              {routine.status === "Active" ? (
                <button
                  className="btn btn-ghost btn-sm"
                  onClick={(e) => { e.stopPropagation(); onPause(routine.routineId); }}
                >
                  {t("pause")}
                </button>
              ) : (
                <button
                  className="btn btn-sm"
                  onClick={(e) => { e.stopPropagation(); onResume(routine.routineId); }}
                >
                  {t("resume")}
                </button>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}

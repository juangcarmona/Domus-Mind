import { useTranslation } from "react-i18next";
import { EntityCard } from "../../../components/EntityCard";
import { formatRoutineAssigned, formatRoutineRecurrence } from "../utils/routineFormatters";
import type { RoutineListItem } from "../../../api/domusmindApi";

type FrequencyGroup = "Daily" | "Weekly" | "Monthly" | "Yearly";
const FREQUENCY_ORDER: FrequencyGroup[] = ["Daily", "Weekly", "Monthly", "Yearly"];

interface Props {
  routineItems: RoutineListItem[];
  routinesStatus: string;
  memberMap: Record<string, string>;
  onEdit: (routineId: string) => void;
  onPause: (routineId: string) => void;
  onResume: (routineId: string) => void;
}

function RoutineRow({
  routine,
  memberMap,
  onEdit,
  onPause,
  onResume,
}: {
  routine: RoutineListItem;
  memberMap: Record<string, string>;
  onEdit: (id: string) => void;
  onPause: (id: string) => void;
  onResume: (id: string) => void;
}) {
  const { t } = useTranslation("routines");
  const recurrence = formatRoutineRecurrence(routine, t);
  const assigned = formatRoutineAssigned(routine, memberMap, t);
  const subtitle = [recurrence, assigned].filter(Boolean).join(" · ");

  return (
    <EntityCard
      title={routine.name}
      subtitle={subtitle}
      accentColor={routine.color}
      dimmed={routine.status === "Paused"}
      onClick={() => onEdit(routine.routineId)}
      actions={
        routine.status === "Active" ? (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={(e) => { e.stopPropagation(); onPause(routine.routineId); }}
          >
            {t("pause")}
          </button>
        ) : (
          <button
            type="button"
            className="btn btn-sm"
            onClick={(e) => { e.stopPropagation(); onResume(routine.routineId); }}
          >
            {t("resume")}
          </button>
        )
      }
    />
  );
}

export function RoutinesTab({ routineItems, routinesStatus, memberMap, onEdit, onPause, onResume }: Props) {
  const { t } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");

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

  const activeByFreq = Object.fromEntries(
    FREQUENCY_ORDER.map((f) => [f, [] as RoutineListItem[]])
  ) as Record<FrequencyGroup, RoutineListItem[]>;
  const paused: RoutineListItem[] = [];

  for (const r of routineItems) {
    if (r.status === "Paused") {
      paused.push(r);
    } else {
      const bucket: FrequencyGroup = FREQUENCY_ORDER.includes(r.frequency as FrequencyGroup)
        ? (r.frequency as FrequencyGroup)
        : "Daily";
      activeByFreq[bucket].push(r);
    }
  }

  const freqLabel: Record<FrequencyGroup, string> = {
    Daily: t("frequencyDaily"),
    Weekly: t("frequencyWeekly"),
    Monthly: t("frequencyMonthly"),
    Yearly: t("frequencyYearly"),
  };

  const hasActive = FREQUENCY_ORDER.some((f) => activeByFreq[f].length > 0);

  return (
    <div>
      {hasActive &&
        FREQUENCY_ORDER.map((freq) => {
          const items = activeByFreq[freq];
          if (items.length === 0) return null;
          return (
            <div key={freq} className="planning-group">
              <div className="planning-group-header">{freqLabel[freq]}</div>
              <div className="item-list">
                {items.map((r) => (
                  <RoutineRow
                    key={r.routineId}
                    routine={r}
                    memberMap={memberMap}
                    onEdit={onEdit}
                    onPause={onPause}
                    onResume={onResume}
                  />
                ))}
              </div>
            </div>
          );
        })}

      {paused.length > 0 && (
        <div className="planning-group planning-group--dimmed">
          <div className="planning-group-header">{t("paused")}</div>
          <div className="item-list">
            {paused.map((r) => (
              <RoutineRow
                key={r.routineId}
                routine={r}
                memberMap={memberMap}
                onEdit={onEdit}
                onPause={onPause}
                onResume={onResume}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

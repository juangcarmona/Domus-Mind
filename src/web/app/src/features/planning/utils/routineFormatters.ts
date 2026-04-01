import type { RoutineListItem } from "../../../api/domusmindApi";
import type { TFunction } from "i18next";

const DAY_KEYS = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"] as const;

/**
 * Returns a compact recurrence sentence for a routine row.
 * Examples: "Daily · 08:00", "Mon, Wed, Fri · 09:00", "Monthly · 1, 15"
 * Requires a `t` function from the `routines` i18n namespace.
 */
export function formatRoutineRecurrence(
  routine: RoutineListItem,
  t: TFunction<"routines">,
): string {
  const timePart = routine.time ? ` · ${routine.time.slice(0, 5)}` : "";
  switch (routine.frequency) {
    case "Daily":
      return `${t("frequencyDaily")}${timePart}`;
    case "Weekly": {
      const days = formatRoutineDays(routine, t);
      return days ? `${days}${timePart}` : `${t("frequencyWeekly")}${timePart}`;
    }
    case "Monthly": {
      const days = formatRoutineDays(routine, t);
      return days
        ? `${t("frequencyMonthly")} · ${days}${timePart}`
        : `${t("frequencyMonthly")}${timePart}`;
    }
    case "Yearly": {
      const days = formatRoutineDays(routine, t);
      return days
        ? `${t("frequencyYearly")} · ${days}${timePart}`
        : `${t("frequencyYearly")}${timePart}`;
    }
    default:
      return routine.frequency + timePart;
  }
}

/**
 * Returns a human-readable day-of-week / day-of-month string for a routine.
 * Requires a `t` function from the `routines` i18n namespace.
 */
export function formatRoutineDays(
  routine: RoutineListItem,
  t: TFunction<"routines">,
): string {
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

/**
 * Returns a display label for who the routine targets.
 * Requires a `t` function from the `routines` i18n namespace.
 */
export function formatRoutineAssigned(
  routine: RoutineListItem,
  memberMap: Record<string, string>,
  t: TFunction<"routines">,
): string {
  if (routine.scope === "Members" && routine.targetMemberIds.length > 0) {
    return routine.targetMemberIds.map((id) => memberMap[id] ?? id).join(", ");
  }
  return t("scopeHousehold");
}

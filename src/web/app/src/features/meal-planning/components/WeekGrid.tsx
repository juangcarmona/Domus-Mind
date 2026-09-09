import { useTranslation } from "react-i18next";
import type { MealPlanDetail, MealSlotDetail } from "../../../api/types/mealPlanningTypes";
import { MealSlotCell, MEAL_TYPE_ORDER } from "./MealSlotCell";

const ALL_DAYS = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
] as const;

type DayName = typeof ALL_DAYS[number];

function buildDaysOrder(firstDayOfWeek?: string | null): DayName[] {
  const idx = ALL_DAYS.findIndex(
    (d) => d.toLowerCase() === (firstDayOfWeek ?? "monday").toLowerCase(),
  );
  const start = idx < 0 ? 0 : idx;
  return [...ALL_DAYS.slice(start), ...ALL_DAYS.slice(0, start)] as DayName[];
}

interface WeekGridProps {
  plan: MealPlanDetail;
  selectedSlotKey: string | null; // "DayOfWeek:MealType" composite
  onSlotClick: (slot: MealSlotDetail) => void;
  firstDayOfWeek?: string | null;
}

/**
 * WeekGrid — 7-column grid of meal slots, one column per day.
 * Pure/presentational: receives plan data, emits slot click.
 */
export function WeekGrid({ plan, selectedSlotKey, onSlotClick, firstDayOfWeek }: WeekGridProps) {
  const { t } = useTranslation("mealPlanning");
  const DAYS_ORDER = buildDaysOrder(firstDayOfWeek);

  return (
    <div className="mp-week-grid">
      {/* Day header row */}
      <div className="mp-week-grid-header">
        {DAYS_ORDER.map((day) => (
          <div key={day} className="mp-week-grid-day-header">
            <span className="mp-week-grid-day-label-full">
              {t(`days.${day}` as Parameters<typeof t>[0])}
            </span>
            <span className="mp-week-grid-day-label-short">
              {t(`daysShort.${day}` as Parameters<typeof t>[0])}
            </span>
          </div>
        ))}
      </div>

      {/* Slot columns */}
      <div className="mp-week-grid-body">
        {DAYS_ORDER.map((day) => {
          const daySlots = (plan.slots ?? []).filter((s) => s.dayOfWeek === day);
          const sorted = [...daySlots].sort(
            (a, b) =>
              MEAL_TYPE_ORDER.indexOf(a.mealType as typeof MEAL_TYPE_ORDER[number]) -
              MEAL_TYPE_ORDER.indexOf(b.mealType as typeof MEAL_TYPE_ORDER[number]),
          );

          return (
            <div key={day} className="mp-week-grid-day-col">
              {sorted.map((slot) => {
                const key = `${slot.dayOfWeek}:${slot.mealType}`;
                return (
                  <MealSlotCell
                    key={key}
                    slot={slot}
                    selected={key === selectedSlotKey}
                    onClick={onSlotClick}
                  />
                );
              })}
            </div>
          );
        })}
      </div>
    </div>
  );
}

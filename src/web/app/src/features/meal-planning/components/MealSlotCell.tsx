import { useTranslation } from "react-i18next";
import type { MealSlotDetail } from "../../../api/types/mealPlanningTypes";

const MEAL_TYPE_ORDER = [
  "Breakfast",
  "MidMorningSnack",
  "Lunch",
  "AfternoonSnack",
  "Dinner",
] as const;

interface MealSlotCellProps {
  slot: MealSlotDetail;
  selected: boolean;
  onClick: (slot: MealSlotDetail) => void;
}

/**
 * MealSlotCell — a single meal-type cell within a day column in the week grid.
 * Pure/presentational: emits onClick, no dispatch.
 */
export function MealSlotCell({ slot, selected, onClick }: MealSlotCellProps) {
  const { t } = useTranslation("mealPlanning");
  const { mealSourceType } = slot;

  const hasContent = mealSourceType !== "Unplanned";

  const label = (() => {
    switch (mealSourceType) {
      case "Recipe": return slot.recipe?.name ?? null;
      case "FreeText": return slot.freeText;
      case "Leftovers": return t("sourceTypes.Leftovers");
      case "External": return t("sourceTypes.External");
      default: return null;
    }
  })();

  return (
    <button
      type="button"
      className={[
        "mp-slot-cell",
        hasContent ? `mp-slot-cell--${mealSourceType.toLowerCase()}` : "mp-slot-cell--empty",
        selected ? "mp-slot-cell--selected" : "",
        slot.isLocked ? "mp-slot-cell--locked" : "",
      ]
        .filter(Boolean)
        .join(" ")}
      onClick={() => onClick(slot)}
      aria-pressed={selected}
      title={
        label ?? t("slotEmpty")
      }
    >
      <span className="mp-slot-cell-type">
        {t(`mealTypes.${slot.mealType}` as Parameters<typeof t>[0])}
        {slot.isLocked && <span className="mp-slot-lock" aria-label={t("slotLocked")}> 🔒</span>}
        {slot.isOptional && <span className="mp-slot-optional-dot" aria-label={t("slotOptional")} />}
      </span>
      {label ? (
        <span className="mp-slot-cell-recipe">{label}</span>
      ) : null}
      {slot.notes && <span className="mp-slot-cell-notes">{slot.notes}</span>}
    </button>
  );
}

export { MEAL_TYPE_ORDER };

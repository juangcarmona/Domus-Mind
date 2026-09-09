import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { MealSlotDetail, RecipeSummary } from "../../../api/types/mealPlanningTypes";
import { RecipePickerPanel } from "./RecipePickerPanel";

type SourceType = "Unplanned" | "Recipe" | "FreeText" | "Leftovers" | "External";

const SOURCE_TYPES: SourceType[] = ["Unplanned", "Recipe", "FreeText", "Leftovers", "External"];

interface SlotInspectorContentProps {
  slot: MealSlotDetail;
  recipes: RecipeSummary[];
  recipesStatus: "idle" | "loading" | "success" | "error";
  updateSlotStatus: "idle" | "loading" | "error";
  onSave: (
    mealSourceType: string,
    recipeId: string | null,
    freeText: string | null,
    notes: string | null,
  ) => void;
  onCreateRecipe: () => void;
}

/**
 * SlotInspectorContent — inspector body for a selected meal slot.
 * Shows source-type selector, recipe picker or free text, notes, and flags.
 */
export function SlotInspectorContent({
  slot,
  recipes,
  recipesStatus,
  updateSlotStatus,
  onSave,
  onCreateRecipe,
}: SlotInspectorContentProps) {
  const { t } = useTranslation("mealPlanning");

  const [sourceType, setSourceType] = useState<SourceType>(
    (SOURCE_TYPES.includes(slot.mealSourceType as SourceType)
      ? slot.mealSourceType
      : "Unplanned") as SourceType,
  );
  const [pendingRecipeId, setPendingRecipeId] = useState<string | null>(
    slot.recipe?.recipeId ?? null,
  );
  const [freeText, setFreeText] = useState(slot.freeText ?? "");
  const [notes, setNotes] = useState(slot.notes ?? "");

  const isDirty =
    sourceType !== slot.mealSourceType ||
    notes.trim() !== (slot.notes ?? "") ||
    (sourceType === "Recipe" &&
      pendingRecipeId !== (slot.recipe?.recipeId ?? null)) ||
    (sourceType === "FreeText" &&
      freeText.trim() !== (slot.freeText ?? ""));

  function handleSave() {
    onSave(
      sourceType,
      sourceType === "Recipe" ? pendingRecipeId : null,
      sourceType === "FreeText" ? freeText.trim() || null : null,
      notes.trim() || null,
    );
  }

  return (
    <div className="mp-slot-inspector">
      {/* Slot identity */}
      <div className="mp-slot-inspector-meta">
        <span className="mp-slot-inspector-day">
          {t(`days.${slot.dayOfWeek}` as Parameters<typeof t>[0])}
        </span>
        <span className="mp-slot-inspector-type">
          {t(`mealTypes.${slot.mealType}` as Parameters<typeof t>[0])}
        </span>
        {slot.isLocked && (
          <span className="mp-slot-badge mp-slot-badge--locked">
            {t("slotLocked")}
          </span>
        )}
        {slot.isOptional && (
          <span className="mp-slot-badge">{t("slotOptional")}</span>
        )}
      </div>

      {/* Source type selector */}
      <div className="mp-source-picker" role="group" aria-label={t("slotSourceType")}>
        {SOURCE_TYPES.map((type) => (
          <button
            key={type}
            type="button"
            className={[
              "mp-source-picker-btn",
              sourceType === type ? "is-active" : "",
            ]
              .filter(Boolean)
              .join(" ")}
            onClick={() => setSourceType(type)}
            disabled={slot.isLocked}
          >
            {t(`sourceTypes.${type}` as Parameters<typeof t>[0])}
          </button>
        ))}
      </div>

      {/* Source-type content */}
      {sourceType === "Recipe" && (
        <RecipePickerPanel
          recipes={recipes}
          recipesStatus={recipesStatus}
          currentRecipeId={pendingRecipeId}
          slotMealType={slot.mealType}
          onSelect={(id) => setPendingRecipeId(id)}
          onCreateNew={onCreateRecipe}
        />
      )}

      {sourceType === "FreeText" && (
        <div className="mp-form-field">
          <label
            className="mp-form-label"
            htmlFor={`slot-freetext-${slot.dayOfWeek}-${slot.mealType}`}
          >
            {t("freeTextLabel")}
          </label>
          <input
            id={`slot-freetext-${slot.dayOfWeek}-${slot.mealType}`}
            type="text"
            className="mp-form-input"
            placeholder={t("freeTextPlaceholder")}
            value={freeText}
            onChange={(e) => setFreeText(e.target.value)}
            disabled={slot.isLocked}
          />
        </div>
      )}

      {sourceType === "Leftovers" && (
        <p className="mp-form-hint">{t("leftoversHint")}</p>
      )}

      {sourceType === "External" && (
        <p className="mp-form-hint">{t("externalHint")}</p>
      )}

      {/* Notes */}
      <div className="mp-form-field">
        <label
          className="mp-form-label"
          htmlFor={`slot-notes-${slot.dayOfWeek}-${slot.mealType}`}
        >
          {t("slotNotes")}
        </label>
        <textarea
          id={`slot-notes-${slot.dayOfWeek}-${slot.mealType}`}
          className="mp-form-input mp-form-textarea"
          placeholder={t("slotNotesPlaceholder")}
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          rows={2}
          disabled={slot.isLocked}
        />
      </div>

      {/* Save action */}
      {isDirty && !slot.isLocked && (
        <div className="mp-slot-inspector-actions">
          <button
            type="button"
            className="btn btn-sm btn-primary"
            onClick={handleSave}
            disabled={updateSlotStatus === "loading"}
          >
            {t("saveSlot")}
          </button>
        </div>
      )}

      {updateSlotStatus === "error" && (
        <p className="mp-form-error">{t("assignError")}</p>
      )}
    </div>
  );
}

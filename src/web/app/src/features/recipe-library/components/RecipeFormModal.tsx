import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { RecipeDetail, RecipeSummary } from "../../../api/types/mealPlanningTypes";

const MEAL_TYPES = ["Breakfast", "Lunch", "Dinner", "MidMorningSnack", "AfternoonSnack"];

interface RecipeFormModalProps {
  /** When editing an existing recipe, provide it here. Omit for creation. */
  initial?: RecipeSummary | RecipeDetail;
  isSubmitting: boolean;
  error: string | null;
  onSubmit: (data: RecipeFormData) => void;
  onCancel: () => void;
}

export interface RecipeFormData {
  name: string;
  description: string | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  servings: number | null;
  isFavorite: boolean;
  allowedMealTypes: string[];
  tags: string[];
}

export function RecipeFormModal({
  initial,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: RecipeFormModalProps) {
  const { t } = useTranslation("recipeLibrary");

  const [name, setName] = useState(initial?.name ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
  const [prepTime, setPrepTime] = useState(
    initial?.prepTimeMinutes != null ? String(initial.prepTimeMinutes) : "",
  );
  const [cookTime, setCookTime] = useState(
    initial?.cookTimeMinutes != null ? String(initial.cookTimeMinutes) : "",
  );
  const [servings, setServings] = useState(
    initial?.servings != null ? String(initial.servings) : "",
  );
  const [isFavorite, setIsFavorite] = useState(initial?.isFavorite ?? false);
  const [allowedMealTypes, setAllowedMealTypes] = useState<string[]>(
    initial?.allowedMealTypes ?? [],
  );
  const [tagsRaw, setTagsRaw] = useState((initial?.tags ?? []).join(", "));

  // Reset form if `initial` changes (e.g., modal reused)
  useEffect(() => {
    setName(initial?.name ?? "");
    setDescription(initial?.description ?? "");
    setPrepTime(initial?.prepTimeMinutes != null ? String(initial.prepTimeMinutes) : "");
    setCookTime(initial?.cookTimeMinutes != null ? String(initial.cookTimeMinutes) : "");
    setServings(initial?.servings != null ? String(initial.servings) : "");
    setIsFavorite(initial?.isFavorite ?? false);
    setAllowedMealTypes(initial?.allowedMealTypes ?? []);
    setTagsRaw((initial?.tags ?? []).join(", "));
  }, [initial]);

  function toggleMealType(mt: string) {
    setAllowedMealTypes((prev) =>
      prev.includes(mt) ? prev.filter((x) => x !== mt) : [...prev, mt],
    );
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const tags = tagsRaw
      .split(",")
      .map((t) => t.trim())
      .filter(Boolean);
    onSubmit({
      name: name.trim(),
      description: description.trim() || null,
      prepTimeMinutes: prepTime ? parseInt(prepTime, 10) : null,
      cookTimeMinutes: cookTime ? parseInt(cookTime, 10) : null,
      servings: servings ? parseInt(servings, 10) : null,
      isFavorite,
      allowedMealTypes,
      tags,
    });
  }

  const isEditing = !!initial;

  return (
    <div className="mp-modal-overlay" role="dialog" aria-modal="true">
      <div className="mp-modal">
        <h2 className="mp-modal-heading">
          {isEditing ? t("editHeading") : t("createHeading")}
        </h2>
        <form onSubmit={handleSubmit} noValidate>
          {/* Name */}
          <div className="mp-form-field">
            <label className="mp-form-label" htmlFor="rl-name">
              {t("nameLabel")}
            </label>
            <input
              id="rl-name"
              type="text"
              className="mp-form-input"
              placeholder={t("namePlaceholder")}
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              autoFocus
            />
          </div>

          {/* Description */}
          <div className="mp-form-field">
            <label className="mp-form-label" htmlFor="rl-description">
              {t("descriptionLabel")}
            </label>
            <input
              id="rl-description"
              type="text"
              className="mp-form-input"
              placeholder={t("descriptionPlaceholder")}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          {/* Times + Servings */}
          <div className="mp-form-row">
            <div className="mp-form-field">
              <label className="mp-form-label" htmlFor="rl-prep">
                {t("prepTimeLabel")}
              </label>
              <input
                id="rl-prep"
                type="number"
                className="mp-form-input mp-form-input--compact"
                min={0}
                value={prepTime}
                onChange={(e) => setPrepTime(e.target.value)}
              />
            </div>
            <div className="mp-form-field">
              <label className="mp-form-label" htmlFor="rl-cook">
                {t("cookTimeLabel")}
              </label>
              <input
                id="rl-cook"
                type="number"
                className="mp-form-input mp-form-input--compact"
                min={0}
                value={cookTime}
                onChange={(e) => setCookTime(e.target.value)}
              />
            </div>
            <div className="mp-form-field">
              <label className="mp-form-label" htmlFor="rl-servings">
                {t("servingsLabel")}
              </label>
              <input
                id="rl-servings"
                type="number"
                className="mp-form-input mp-form-input--compact"
                min={1}
                value={servings}
                onChange={(e) => setServings(e.target.value)}
              />
            </div>
          </div>

          {/* Allowed meal types */}
          <div className="mp-form-field">
            <span className="mp-form-label">{t("allowedMealTypesLabel")}</span>
            <div className="mp-source-picker" style={{ marginTop: "var(--spacing-1)" }}>
              {MEAL_TYPES.map((mt) => (
                <button
                  key={mt}
                  type="button"
                  className={[
                    "mp-source-picker-btn",
                    allowedMealTypes.includes(mt) ? "is-active" : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                  onClick={() => toggleMealType(mt)}
                >
                  {mt}
                </button>
              ))}
            </div>
          </div>

          {/* Tags */}
          <div className="mp-form-field">
            <label className="mp-form-label" htmlFor="rl-tags">
              {t("tagsLabel")}
            </label>
            <input
              id="rl-tags"
              type="text"
              className="mp-form-input"
              placeholder="e.g. quick, vegetarian, summer"
              value={tagsRaw}
              onChange={(e) => setTagsRaw(e.target.value)}
            />
          </div>

          {/* Favourite */}
          <div className="mp-form-field mp-form-field--inline">
            <label className="mp-form-label" htmlFor="rl-favorite">
              {t("isFavoriteLabel")}
            </label>
            <input
              id="rl-favorite"
              type="checkbox"
              checked={isFavorite}
              onChange={(e) => setIsFavorite(e.target.checked)}
            />
          </div>

          {error && <p className="mp-form-error">{error}</p>}

          <div className="mp-modal-actions">
            <button
              type="button"
              className="btn btn-sm btn-ghost"
              onClick={onCancel}
              disabled={isSubmitting}
            >
              {t("cancelForm")}
            </button>
            <button
              type="submit"
              className="btn btn-sm btn-primary"
              disabled={isSubmitting || !name.trim()}
            >
              {t("saveRecipe")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

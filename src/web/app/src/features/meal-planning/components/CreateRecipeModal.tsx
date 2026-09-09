import { useState } from "react";
import { useTranslation } from "react-i18next";

interface CreateRecipeModalProps {
  onConfirm: (data: {
    name: string;
    description?: string;
    prepTimeMinutes?: number;
    cookTimeMinutes?: number;
    servings?: number;
  }) => void;
  onCancel: () => void;
  isSubmitting: boolean;
  error?: string | null;
}

/**
 * CreateRecipeModal — inline modal for creating a new recipe.
 * Purely handles form state and calls onConfirm with raw data.
 */
export function CreateRecipeModal({
  onConfirm,
  onCancel,
  isSubmitting,
  error,
}: CreateRecipeModalProps) {
  const { t } = useTranslation("mealPlanning");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [prepTime, setPrepTime] = useState("");
  const [cookTime, setCookTime] = useState("");
  const [servings, setServings] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const trimmedName = name.trim();
    if (!trimmedName) return;
    onConfirm({
      name: trimmedName,
      description: description.trim() || undefined,
      prepTimeMinutes: prepTime ? parseInt(prepTime, 10) : undefined,
      cookTimeMinutes: cookTime ? parseInt(cookTime, 10) : undefined,
      servings: servings ? parseInt(servings, 10) : undefined,
    });
  }

  return (
    <div className="mp-modal-backdrop" role="dialog" aria-modal="true">
      <div className="mp-modal">
        <div className="mp-modal-header">
          <h2 className="mp-modal-title">{t("createRecipeHeading")}</h2>
          <button
            type="button"
            className="mp-modal-close"
            onClick={onCancel}
            aria-label={t("cancelCreate")}
          >
            ✕
          </button>
        </div>

        <form className="mp-modal-form" onSubmit={handleSubmit}>
          <div className="mp-form-field">
            <label className="mp-form-label" htmlFor="recipe-name">
              {t("recipeName")}
            </label>
            <input
              id="recipe-name"
              type="text"
              className="mp-form-input"
              placeholder={t("recipeNamePlaceholder")}
              value={name}
              onChange={(e) => setName(e.target.value)}
              autoFocus
              required
            />
          </div>

          <div className="mp-form-field">
            <label className="mp-form-label" htmlFor="recipe-description">
              {t("recipeDescription")}
            </label>
            <textarea
              id="recipe-description"
              className="mp-form-input mp-form-textarea"
              placeholder={t("recipeDescriptionPlaceholder")}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={2}
            />
          </div>

          <div className="mp-form-row">
            <div className="mp-form-field">
              <label className="mp-form-label" htmlFor="recipe-prep">
                {t("recipePrepTime")}
              </label>
              <input
                id="recipe-prep"
                type="number"
                className="mp-form-input mp-form-input--compact"
                min={0}
                value={prepTime}
                onChange={(e) => setPrepTime(e.target.value)}
              />
            </div>
            <div className="mp-form-field">
              <label className="mp-form-label" htmlFor="recipe-cook">
                {t("recipeCookTime")}
              </label>
              <input
                id="recipe-cook"
                type="number"
                className="mp-form-input mp-form-input--compact"
                min={0}
                value={cookTime}
                onChange={(e) => setCookTime(e.target.value)}
              />
            </div>
            <div className="mp-form-field">
              <label className="mp-form-label" htmlFor="recipe-servings">
                {t("recipeServings")}
              </label>
              <input
                id="recipe-servings"
                type="number"
                className="mp-form-input mp-form-input--compact"
                min={1}
                value={servings}
                onChange={(e) => setServings(e.target.value)}
              />
            </div>
          </div>

          {error && <p className="mp-form-error">{error}</p>}

          <p className="mp-form-hint">{t("createRecipeIngredientsHint")}</p>

          <div className="mp-modal-actions">
            <button
              type="button"
              className="btn btn-sm btn-ghost"
              onClick={onCancel}
              disabled={isSubmitting}
            >
              {t("cancelCreate")}
            </button>
            <button
              type="submit"
              className="btn btn-sm btn-primary"
              disabled={isSubmitting || !name.trim()}
            >
              {t("createRecipe")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

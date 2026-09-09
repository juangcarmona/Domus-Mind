import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchRecipeDetail,
  addIngredient,
  updateIngredient,
  removeIngredient,
  clearIngredientError,
} from "../../../store/recipeLibrarySlice";
import type { RecipeDetail, IngredientDetail } from "../../../api/types/mealPlanningTypes";

interface RecipeDetailPanelProps {
  recipe: RecipeDetail;
  onEdit: () => void;
  onDelete: () => void;
}

// ── Ingredient row ────────────────────────────────────────────────────────────

function IngredientRow({
  ingredient,
  recipeId,
}: {
  ingredient: IngredientDetail;
  recipeId: string;
}) {
  const { t } = useTranslation("recipeLibrary");
  const dispatch = useAppDispatch();
  const [editing, setEditing] = useState(false);
  const [qty, setQty] = useState(ingredient.quantity?.toString() ?? "");
  const [unit, setUnit] = useState(ingredient.unit ?? "");

  function handleSave() {
    dispatch(
      updateIngredient({
        recipeId,
        ingredientName: ingredient.name,
        body: {
          quantity: qty ? parseFloat(qty) : null,
          unit: unit.trim() || null,
        },
      }),
    ).then((result) => {
      if (result.meta.requestStatus === "fulfilled") {
        setEditing(false);
        dispatch(fetchRecipeDetail(recipeId));
      }
    });
  }

  function handleRemove() {
    dispatch(removeIngredient({ recipeId, ingredientName: ingredient.name })).then(
      (result) => {
        if (result.meta.requestStatus === "fulfilled") {
          dispatch(fetchRecipeDetail(recipeId));
        }
      },
    );
  }

  if (editing) {
    return (
      <li className="rl-ingredient-row">
        <div className="rl-ingredient-form">
          <div className="rl-ingredient-form-row">
            <input
              type="number"
              className="rl-ingredient-form-input"
              placeholder={t("ingredientQuantity")}
              value={qty}
              onChange={(e) => setQty(e.target.value)}
              min={0}
              step="any"
            />
            <input
              type="text"
              className="rl-ingredient-form-input"
              placeholder={t("ingredientUnit")}
              value={unit}
              onChange={(e) => setUnit(e.target.value)}
            />
          </div>
          <div className="rl-ingredient-form-actions">
            <button
              type="button"
              className="btn btn-xs btn-ghost"
              onClick={() => setEditing(false)}
            >
              {t("cancelIngredient")}
            </button>
            <button type="button" className="btn btn-xs btn-primary" onClick={handleSave}>
              {t("saveIngredient")}
            </button>
          </div>
        </div>
      </li>
    );
  }

  return (
    <li className="rl-ingredient-row">
      <span className="rl-ingredient-name">{ingredient.name}</span>
      {(ingredient.quantity != null || ingredient.unit) && (
        <span className="rl-ingredient-qty">
          {ingredient.quantity != null ? ingredient.quantity : ""}
          {ingredient.unit ? ` ${ingredient.unit}` : ""}
        </span>
      )}
      <span className="rl-ingredient-actions">
        <button
          type="button"
          className="btn btn-xs btn-ghost"
          aria-label={t("editIngredient")}
          onClick={() => setEditing(true)}
        >
          {t("editIngredient")}
        </button>
        <button
          type="button"
          className="btn btn-xs btn-ghost"
          aria-label={t("removeIngredient")}
          onClick={handleRemove}
        >
          {t("removeIngredient")}
        </button>
      </span>
    </li>
  );
}

// ── Add ingredient form ───────────────────────────────────────────────────────

function AddIngredientForm({ recipeId }: { recipeId: string }) {
  const { t } = useTranslation("recipeLibrary");
  const dispatch = useAppDispatch();
  const [name, setName] = useState("");
  const [qty, setQty] = useState("");
  const [unit, setUnit] = useState("");
  const [open, setOpen] = useState(false);

  function handleAdd() {
    if (!name.trim()) return;
    dispatch(
      addIngredient({
        recipeId,
        body: {
          name: name.trim(),
          quantity: qty ? parseFloat(qty) : null,
          unit: unit.trim() || null,
        },
      }),
    ).then((result) => {
      if (result.meta.requestStatus === "fulfilled") {
        setName("");
        setQty("");
        setUnit("");
        setOpen(false);
        dispatch(fetchRecipeDetail(recipeId));
      }
    });
  }

  if (!open) {
    return (
      <button
        type="button"
        className="btn btn-xs btn-ghost"
        onClick={() => setOpen(true)}
      >
        + {t("addIngredient")}
      </button>
    );
  }

  return (
    <div className="rl-ingredient-form">
      <input
        type="text"
        className="rl-ingredient-form-input"
        placeholder={t("ingredientName")}
        value={name}
        onChange={(e) => setName(e.target.value)}
        autoFocus
      />
      <div className="rl-ingredient-form-row">
        <input
          type="number"
          className="rl-ingredient-form-input"
          placeholder={t("ingredientQuantity")}
          value={qty}
          onChange={(e) => setQty(e.target.value)}
          min={0}
          step="any"
        />
        <input
          type="text"
          className="rl-ingredient-form-input"
          placeholder={t("ingredientUnit")}
          value={unit}
          onChange={(e) => setUnit(e.target.value)}
        />
      </div>
      <div className="rl-ingredient-form-actions">
        <button
          type="button"
          className="btn btn-xs btn-ghost"
          onClick={() => setOpen(false)}
        >
          {t("cancelIngredient")}
        </button>
        <button
          type="button"
          className="btn btn-xs btn-primary"
          onClick={handleAdd}
          disabled={!name.trim()}
        >
          {t("saveIngredient")}
        </button>
      </div>
    </div>
  );
}

// ── Main component ────────────────────────────────────────────────────────────

export function RecipeDetailPanel({ recipe, onEdit, onDelete }: RecipeDetailPanelProps) {
  const { t } = useTranslation("recipeLibrary");
  const dispatch = useAppDispatch();
  const ingredientError = useAppSelector(
    (s) => s.recipeLibrary.ingredientMutationError,
  );

  return (
    <div className="rl-detail-pane">
      {/* Header */}
      <div className="rl-detail-header">
        <h2 className="rl-detail-title">
          {recipe.name}
          {recipe.isFavorite && (
            <span className="rl-recipe-favorite-icon" aria-label={t("favorite")}>
              {" ★"}
            </span>
          )}
        </h2>
        {recipe.description && (
          <p className="rl-detail-description">{recipe.description}</p>
        )}
        <div className="rl-detail-actions">
          <button type="button" className="btn btn-sm btn-ghost" onClick={onEdit}>
            {t("editRecipe")}
          </button>
          <button type="button" className="btn btn-sm btn-danger-ghost" onClick={onDelete}>
            {t("deleteRecipe")}
          </button>
        </div>
      </div>

      {/* Stats */}
      {(recipe.prepTimeMinutes != null ||
        recipe.cookTimeMinutes != null ||
        recipe.servings != null) && (
        <div className="rl-detail-stats">
          {recipe.prepTimeMinutes != null && (
            <div className="rl-stat">
              <span className="rl-stat-label">{t("prep")}</span>
              <span className="rl-stat-value">
                {t("minutes", { count: recipe.prepTimeMinutes })}
              </span>
            </div>
          )}
          {recipe.cookTimeMinutes != null && (
            <div className="rl-stat">
              <span className="rl-stat-label">{t("cook")}</span>
              <span className="rl-stat-value">
                {t("minutes", { count: recipe.cookTimeMinutes })}
              </span>
            </div>
          )}
          {recipe.servings != null && (
            <div className="rl-stat">
              <span className="rl-stat-label">{t("servings")}</span>
              <span className="rl-stat-value">{recipe.servings}</span>
            </div>
          )}
        </div>
      )}

      {/* Allowed meal types */}
      {recipe.allowedMealTypes.length > 0 && (
        <div className="rl-detail-section">
          <h3 className="rl-detail-section-heading">{t("mealTypes")}</h3>
          <div className="rl-meal-type-list">
            {recipe.allowedMealTypes.map((mt) => (
              <span key={mt} className="rl-meal-type-badge">
                {mt}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Tags */}
      {recipe.tags.length > 0 && (
        <div className="rl-detail-section">
          <h3 className="rl-detail-section-heading">{t("tags")}</h3>
          <div className="rl-tag-list">
            {recipe.tags.map((tag) => (
              <span key={tag} className="rl-tag">
                {tag}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Ingredients */}
      <div className="rl-detail-section">
        <h3 className="rl-detail-section-heading">{t("ingredients")}</h3>

        {ingredientError && (
          <p
            className="mp-form-error"
            style={{ marginBottom: "var(--spacing-2)" }}
            onClick={() => dispatch(clearIngredientError())}
          >
            {ingredientError}
          </p>
        )}

        {recipe.ingredients.length === 0 ? (
          <p
            className="rl-list-empty"
            style={{ padding: "var(--spacing-2) 0", textAlign: "left" }}
          >
            {t("noIngredients")}
          </p>
        ) : (
          <ul className="rl-ingredient-list">
            {recipe.ingredients.map((ing) => (
              <IngredientRow key={ing.name} ingredient={ing} recipeId={recipe.id} />
            ))}
          </ul>
        )}

        <AddIngredientForm recipeId={recipe.id} />
      </div>
    </div>
  );
}

import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { RecipeSummary } from "../../../api/types/mealPlanningTypes";

interface RecipePickerPanelProps {
  recipes: RecipeSummary[];
  recipesStatus: "idle" | "loading" | "success" | "error";
  currentRecipeId: string | null;
  /** When provided, only compatible recipes are shown (allowedMealTypes empty or includes this type). */
  slotMealType?: string | null;
  onSelect: (recipeId: string | null) => void;
  onCreateNew: () => void;
}

/**
 * RecipePickerPanel — searchable recipe list for slot assignment.
 * Rendered inside the slot inspector. Pure/presentational.
 */
export function RecipePickerPanel({
  recipes,
  recipesStatus,
  currentRecipeId,
  slotMealType,
  onSelect,
  onCreateNew,
}: RecipePickerPanelProps) {
  const { t } = useTranslation("mealPlanning");
  const [search, setSearch] = useState("");

  // Reset search when panel opens (currentRecipeId changes)
  useEffect(() => {
    setSearch("");
  }, [currentRecipeId]);

  // Filter by allowedMealTypes when a slotMealType is known
  const mealTypeFiltered = slotMealType
    ? recipes.filter(
        (r) =>
          r.allowedMealTypes.length === 0 ||
          r.allowedMealTypes.includes(slotMealType),
      )
    : recipes;

  const filtered = mealTypeFiltered.filter((r) =>
    r.name.toLowerCase().includes(search.toLowerCase()),
  );

  return (
    <div className="mp-recipe-picker">
      <div className="mp-recipe-picker-search-row">
        <input
          type="text"
          className="mp-recipe-picker-search"
          placeholder={t("searchRecipes")}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          autoComplete="off"
        />
        <button
          type="button"
          className="btn btn-sm btn-ghost mp-recipe-picker-new"
          onClick={onCreateNew}
        >
          + {t("newRecipe")}
        </button>
      </div>

      {recipesStatus === "loading" && (
        <p className="mp-recipe-picker-loading">{t("loadingRecipes")}</p>
      )}

      {recipesStatus !== "loading" && filtered.length === 0 && (
        <p className="mp-recipe-picker-empty">
          {search ? t("noRecipesMatch") : t("recipesEmpty")}
        </p>
      )}

      {filtered.length > 0 && (
        <ul className="mp-recipe-picker-list">
          {/* Clear option */}
          {currentRecipeId && (
            <li>
              <button
                type="button"
                className="mp-recipe-picker-item mp-recipe-picker-item--clear"
                onClick={() => onSelect(null)}
              >
                {t("clearSlot")}
              </button>
            </li>
          )}
          {filtered.map((recipe) => (
            <li key={recipe.id}>
              <button
                type="button"
                className={[
                  "mp-recipe-picker-item",
                  recipe.id === currentRecipeId
                    ? "mp-recipe-picker-item--selected"
                    : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
                onClick={() => onSelect(recipe.id)}
              >
                <span className="mp-recipe-picker-item-name">{recipe.name}</span>
                {recipe.cookTimeMinutes && (
                  <span className="mp-recipe-picker-item-meta">
                    {recipe.cookTimeMinutes} min
                  </span>
                )}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

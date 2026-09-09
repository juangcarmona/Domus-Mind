// Recipe Library surface — full household recipe collection.
// /recipes — view and manage the household recipe library.

import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchRecipes,
  fetchRecipeDetail,
  updateRecipe,
  deleteRecipe,
  clearSelectedRecipe,
  clearUpdateError,
  clearDeleteError,
} from "../../../store/recipeLibrarySlice";
import { createRecipe } from "../../../store/mealPlanningSlice";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { RecipeDetailPanel } from "../components/RecipeDetailPanel";
import { RecipeFormModal, type RecipeFormData } from "../components/RecipeFormModal";
import type { RecipeSummary } from "../../../api/types/mealPlanningTypes";
import "../recipe-library.css";

export function RecipesPage() {
  const { t } = useTranslation("recipeLibrary");
  const dispatch = useAppDispatch();
  const isMobile = useIsMobile();

  const family = useAppSelector((s) => s.household.family);
  const {
    recipes,
    listStatus,
    selectedRecipe,
    detailStatus,
    updateStatus,
    updateError,
    deleteStatus,
    deleteError,
  } = useAppSelector((s) => s.recipeLibrary);

  const [search, setSearch] = useState("");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  // Create recipe state from mealPlanningSlice
  const createStatus = useAppSelector((s) => s.mealPlanning.createRecipeStatus);
  const createError = useAppSelector((s) => s.mealPlanning.createError);

  // Fetch recipes once per family
  useEffect(() => {
    if (!family?.familyId || listStatus !== "idle") return;
    dispatch(fetchRecipes(family.familyId));
  }, [family?.familyId, listStatus, dispatch]);

  // Fetch detail when a recipe is selected
  useEffect(() => {
    if (!selectedId || detailStatus !== "idle") return;
    if (selectedRecipe?.id !== selectedId) {
      dispatch(fetchRecipeDetail(selectedId));
    }
  }, [selectedId, detailStatus, selectedRecipe, dispatch]);

  // Close delete confirm and de-select after successful delete
  useEffect(() => {
    if (deleteStatus === "idle" && showDeleteConfirm) {
      setShowDeleteConfirm(false);
      setSelectedId(null);
      dispatch(clearSelectedRecipe());
    }
  }, [deleteStatus]); // eslint-disable-line react-hooks/exhaustive-deps

  // ── Handlers ───────────────────────────────────────────────────────────────

  function handleSelectRecipe(id: string) {
    if (id === selectedId) return;
    setSelectedId(id);
    dispatch(clearSelectedRecipe());
  }

  function handleNewRecipe() {
    setIsEditing(false);
    setShowForm(true);
  }

  function handleEditRecipe() {
    setIsEditing(true);
    setShowForm(true);
    dispatch(clearUpdateError());
  }

  function handleDeleteRequest() {
    setShowDeleteConfirm(true);
    dispatch(clearDeleteError());
  }

  function handleDeleteConfirm() {
    if (!selectedId) return;
    dispatch(deleteRecipe(selectedId));
  }

  function handleFormSubmit(data: RecipeFormData) {
    if (!family?.familyId) return;
    if (isEditing && selectedId) {
      dispatch(
        updateRecipe({
          recipeId: selectedId,
          body: {
            name: data.name,
            description: data.description,
            prepTimeMinutes: data.prepTimeMinutes,
            cookTimeMinutes: data.cookTimeMinutes,
            servings: data.servings,
            isFavorite: data.isFavorite,
            allowedMealTypes: data.allowedMealTypes,
            tags: data.tags,
          },
        }),
      ).then((result) => {
        if (result.meta.requestStatus === "fulfilled") {
          setShowForm(false);
          dispatch(fetchRecipeDetail(selectedId));
          dispatch(fetchRecipes(family.familyId!));
        }
      });
    } else {
      dispatch(
        createRecipe({
          familyId: family.familyId,
          name: data.name,
          description: data.description ?? undefined,
          prepTimeMinutes: data.prepTimeMinutes ?? undefined,
          cookTimeMinutes: data.cookTimeMinutes ?? undefined,
          servings: data.servings ?? undefined,
          isFavorite: data.isFavorite,
          allowedMealTypes: data.allowedMealTypes,
          tags: data.tags,
        }),
      ).then((result) => {
        if (result.meta.requestStatus === "fulfilled") {
          setShowForm(false);
          dispatch(fetchRecipes(family.familyId!));
        }
      });
    }
  }

  // ── Filtered list ──────────────────────────────────────────────────────────

  const filtered: RecipeSummary[] = recipes.filter((r) =>
    r.name.toLowerCase().includes(search.toLowerCase()),
  );

  // ── Recipe detail content ──────────────────────────────────────────────────

  const detailContent =
    selectedRecipe && selectedRecipe.id === selectedId ? (
      showDeleteConfirm ? (
        <div className="rl-delete-confirm">
          <p className="rl-delete-confirm-body">{t("deleteConfirmBody")}</p>
          {deleteError && (
            <p className="mp-form-error">
              {deleteError.includes("active") ? t("deleteBlockedBySlot") : deleteError}
            </p>
          )}
          <div className="rl-delete-confirm-actions">
            <button
              type="button"
              className="btn btn-sm btn-ghost"
              onClick={() => setShowDeleteConfirm(false)}
              disabled={deleteStatus === "loading"}
            >
              {t("deleteCancel")}
            </button>
            <button
              type="button"
              className="btn btn-sm btn-danger"
              onClick={handleDeleteConfirm}
              disabled={deleteStatus === "loading"}
            >
              {t("deleteConfirmAction")}
            </button>
          </div>
        </div>
      ) : (
        <RecipeDetailPanel
          recipe={selectedRecipe}
          onEdit={handleEditRecipe}
          onDelete={handleDeleteRequest}
        />
      )
    ) : detailStatus === "loading" ? (
      <div style={{ padding: "var(--spacing-4)", color: "var(--color-text-secondary)" }}>
        {t("detailLoading")}
      </div>
    ) : null;

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className="rl-page">
      {/* Header */}
      <div className="rl-header">
        <h1 className="rl-header-title">{t("title")}</h1>
        <div className="rl-header-actions">
          <button
            type="button"
            className="btn btn-sm btn-primary"
            onClick={handleNewRecipe}
          >
            {t("newRecipe")}
          </button>
        </div>
      </div>

      {/* Search */}
      <div className="rl-search">
        <input
          type="search"
          className="rl-search-input"
          placeholder={t("searchPlaceholder")}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label={t("searchPlaceholder")}
        />
      </div>

      {/* Body: list + inspector */}
      <div className="rl-body">
        <div className="rl-list-pane">
          {listStatus === "loading" && (
            <p className="rl-list-empty">{t("loading")}</p>
          )}

          {listStatus !== "loading" && filtered.length === 0 && (
            <div className="rl-list-empty">
              {recipes.length === 0 ? (
                <>
                  <p>{t("empty")}</p>
                  <p className="rl-list-empty-hint">{t("emptyHint")}</p>
                </>
              ) : (
                <p>{t("noMatch")}</p>
              )}
            </div>
          )}

          {filtered.length > 0 && (
            <ul className="rl-list" role="listbox" aria-label={t("title")}>
              {filtered.map((recipe) => (
                <li key={recipe.id} role="option" aria-selected={recipe.id === selectedId}>
                  <button
                    type="button"
                    className={[
                      "rl-recipe-row",
                      recipe.id === selectedId ? "is-selected" : "",
                    ]
                      .filter(Boolean)
                      .join(" ")}
                    onClick={() => handleSelectRecipe(recipe.id)}
                  >
                    <div className="rl-recipe-row-body">
                      <span className="rl-recipe-row-name">
                        {recipe.isFavorite && (
                          <span className="rl-recipe-favorite-icon" aria-hidden="true">
                            ★{" "}
                          </span>
                        )}
                        {recipe.name}
                      </span>
                      <div className="rl-recipe-row-meta">
                        {recipe.totalTimeMinutes != null && (
                          <span className="rl-recipe-row-meta-item">
                            {t("minutes", { count: recipe.totalTimeMinutes })}
                          </span>
                        )}
                        {recipe.servings != null && (
                          <span className="rl-recipe-row-meta-item">
                            {recipe.servings} srv
                          </span>
                        )}
                        {recipe.ingredientCount > 0 && (
                          <span className="rl-recipe-row-meta-item">
                            {recipe.ingredientCount} ing.
                          </span>
                        )}
                      </div>
                    </div>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>

        {/* Desktop inspector panel */}
        {!isMobile && selectedId && (
          <InspectorPanel onClose={() => setSelectedId(null)}>
            {detailContent}
          </InspectorPanel>
        )}
      </div>

      {/* Mobile bottom sheet */}
      {isMobile && selectedId && (
        <BottomSheetDetail open={!!selectedId} onClose={() => setSelectedId(null)}>
          {detailContent}
        </BottomSheetDetail>
      )}

      {/* Create/Edit form modal */}
      {showForm && (
        <RecipeFormModal
          initial={isEditing ? (selectedRecipe ?? undefined) : undefined}
          isSubmitting={
            isEditing ? updateStatus === "loading" : createStatus === "loading"
          }
          error={isEditing ? updateError : createError}
          onSubmit={handleFormSubmit}
          onCancel={() => setShowForm(false)}
        />
      )}
    </div>
  );
}

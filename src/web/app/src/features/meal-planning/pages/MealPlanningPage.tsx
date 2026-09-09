// Meal Planning surface — weekly view.
// /meal-planning              — current week (derived from household first-day-of-week setting)
// /meal-planning/:weekStart   — specific week (ISO date aligned to household week start)

import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchMealPlan,
  createMealPlan,
  updateMealSlot,
  copyFromPreviousWeek,
  requestShoppingList,
  fetchFamilyRecipes,
  createRecipe,
  setCurrentWeek,
  clearShoppingListStatus,
  clearCopyError,
  clearCreateError,
} from "../../../store/mealPlanningSlice";
import { startOfWeek, toIsoDate } from "../../agenda-today/utils/dateUtils";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { PageHeader } from "../../../components/PageHeader";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { WeekNavigator, shiftWeek } from "../components/WeekNavigator";
import { WeekGrid } from "../components/WeekGrid";
import { SlotInspectorContent } from "../components/SlotInspectorContent";
import { CreateRecipeModal } from "../components/CreateRecipeModal";
import type { MealSlotDetail } from "../../../api/types/mealPlanningTypes";
import "../meal-planning.css";

function slotKey(slot: MealSlotDetail): string {
  return `${slot.dayOfWeek}:${slot.mealType}`;
}

export function MealPlanningPage() {
  const { weekStart: routeWeekStart } = useParams<{ weekStart?: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation("mealPlanning");
  const dispatch = useAppDispatch();
  const isMobile = useIsMobile();

  const family = useAppSelector((s) => s.household.family);
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;

  const {
    currentPlan,
    planStatus,
    planError,
    currentWeekStart,
    recipes,
    recipesStatus,
    updateSlotStatus,
    createRecipeStatus,
    createError,
    copyStatus,
    copyError,
    shoppingListStatus,
  } = useAppSelector((s) => s.mealPlanning);

  const [selectedSlot, setSelectedSlot] = useState<MealSlotDetail | null>(null);
  const [showCreateRecipe, setShowCreateRecipe] = useState(false);
  const [createRecipeError, setCreateRecipeError] = useState<string | null>(null);

  // Resolve current week from household setting (Agenda uses the same utility)
  const currentHouseholdWeekStart = toIsoDate(startOfWeek(new Date(), firstDayOfWeek));

  // Sync route param → store week
  const weekStart = routeWeekStart ?? currentHouseholdWeekStart;
  useEffect(() => {
    if (weekStart !== currentWeekStart) {
      dispatch(setCurrentWeek(weekStart));
      setSelectedSlot(null);
    }
  }, [weekStart, currentWeekStart, dispatch]);

  // Fetch plan whenever week or family changes
  useEffect(() => {
    if (!family?.familyId) return;
    dispatch(fetchMealPlan({ familyId: family.familyId, weekStart }));
  }, [family?.familyId, weekStart, dispatch]);

  // Fetch recipes once per family
  useEffect(() => {
    if (!family?.familyId || recipesStatus !== "idle") return;
    dispatch(fetchFamilyRecipes(family.familyId));
  }, [family?.familyId, recipesStatus, dispatch]);

  // Keep selectedSlot in sync after plan update (re-find by composite key)
  useEffect(() => {
    if (!selectedSlot || !currentPlan) return;
    const key = slotKey(selectedSlot);
    const updated = currentPlan.slots.find((s) => slotKey(s) === key);
    if (updated) setSelectedSlot(updated);
  }, [currentPlan]); // eslint-disable-line react-hooks/exhaustive-deps

  // ── Navigation ─────────────────────────────────────────────────────────────

  function navigateWeek(direction: "prev" | "next") {
    const next = shiftWeek(weekStart, direction);
    navigate(`/meal-planning/${next}`);
    dispatch(clearCopyError());
    dispatch(clearCreateError());
  }

  // ── Actions ────────────────────────────────────────────────────────────────

  function handleCreatePlan() {
    if (!family?.familyId) return;
    dispatch(clearCreateError());
    dispatch(createMealPlan({ familyId: family.familyId, weekStart }));
  }

  function handleCopyFromPreviousWeek() {
    if (!family?.familyId) return;
    dispatch(clearCopyError());
    dispatch(copyFromPreviousWeek({ familyId: family.familyId, weekStart }));
  }

  function handleRequestShoppingList() {
    if (!currentPlan || !family?.familyId) return;
    dispatch(clearShoppingListStatus());
    dispatch(
      requestShoppingList({
        planId: currentPlan.planId,
        familyId: family.familyId,
        shoppingListName: t("shoppingListAutoName", { date: weekStart }),
      }),
    );
  }

  function handleRetryLoad() {
    if (!family?.familyId) return;
    dispatch(fetchMealPlan({ familyId: family.familyId, weekStart }));
  }

  const handleUpdateSlot = useCallback(
    (
      slot: MealSlotDetail,
      mealSourceType: string,
      recipeId: string | null,
      freeText: string | null,
      notes: string | null,
    ) => {
      if (!currentPlan || !family?.familyId) return;
      dispatch(
        updateMealSlot({
          planId: currentPlan.planId,
          familyId: family.familyId,
          weekStart: currentPlan.weekStart,
          dayOfWeek: slot.dayOfWeek,
          mealType: slot.mealType,
          mealSourceType,
          recipeId,
          freeText,
          notes,
        }),
      );
    },
    [dispatch, currentPlan, family?.familyId],
  );

  function handleCreateRecipe(data: {
    name: string;
    description?: string;
    prepTimeMinutes?: number;
    cookTimeMinutes?: number;
    servings?: number;
  }) {
    if (!family?.familyId) return;
    setCreateRecipeError(null);
    dispatch(createRecipe({ familyId: family.familyId, ...data }))
      .unwrap()
      .then(() => setShowCreateRecipe(false))
      .catch((err: unknown) => {
        setCreateRecipeError(
          typeof err === "string" ? err : t("createRecipeError"),
        );
      });
  }

  // ── Derived ────────────────────────────────────────────────────────────────

  const hasRecipeSlots =
    currentPlan?.slots.some((s) => s.mealSourceType === "Recipe") ?? false;

  const canCopyFromPrev = !currentPlan && planStatus === "success" && copyStatus !== "loading";
  const canRequestShoppingList =
    !!currentPlan && !currentPlan.shoppingListId && hasRecipeSlots && shoppingListStatus !== "loading";

  const copyErrorMessage =
    copyError === "noPreviousPlan"
      ? t("copyErrorNoPreviousPlan")
      : copyError === "alreadyExisted"
        ? t("copyErrorAlreadyExisted")
        : copyError
          ? t("copyErrorGeneric")
          : null;

  const selectedSlotKey = selectedSlot ? slotKey(selectedSlot) : null;

  const inspectorContent = selectedSlot ? (
    <SlotInspectorContent
      key={selectedSlotKey}
      slot={selectedSlot}
      recipes={recipes}
      recipesStatus={recipesStatus}
      updateSlotStatus={updateSlotStatus}
      onSave={(mealSourceType, recipeId, freeText, notes) =>
        handleUpdateSlot(selectedSlot, mealSourceType, recipeId, freeText, notes)
      }
      onCreateRecipe={() => setShowCreateRecipe(true)}
    />
  ) : null;

  return (
    <div className="l-surface mp-surface">
      <PageHeader
        title={t("title")}
        nav={
          <WeekNavigator
            weekStart={weekStart}
            onPrev={() => navigateWeek("prev")}
            onNext={() => navigateWeek("next")}
          />
        }
      />

      <div className="l-surface-body">
        <div className="l-surface-content">

          {/* ── Loading ── */}
          {planStatus === "loading" && (
            <p className="mp-loading">{t("loading")}</p>
          )}

          {/* ── Error (genuine load failure: network/auth only) ── */}
          {planStatus === "error" && (
            <div className="mp-load-error">
              <p className="mp-error">{planError ?? t("loadError")}</p>
              <button
                type="button"
                className="btn btn-sm btn-ghost"
                onClick={handleRetryLoad}
              >
                {t("retryLoad")}
              </button>
            </div>
          )}

          {/* ── No plan this week ── */}
          {planStatus === "success" && !currentPlan && (
            <div className="mp-empty-week">
              <div className="mp-empty-week-header">
                <p className="mp-empty-week-title">{t("emptyWeek")}</p>
              </div>

              <div className="mp-empty-week-actions">
                <button
                  type="button"
                  className="btn btn-sm btn-primary"
                  onClick={handleCreatePlan}
                  disabled={planStatus !== "success"}
                >
                  {t("startFromScratch")}
                </button>
                <button
                  type="button"
                  className="btn btn-sm btn-ghost"
                  onClick={handleCopyFromPreviousWeek}
                  disabled={!canCopyFromPrev}
                >
                  {copyStatus === "loading" ? t("loading") : t("copyFromPreviousWeek")}
                </button>
                <button
                  type="button"
                  className="btn btn-sm btn-ghost"
                  disabled
                  title={t("applyTemplateSoon")}
                >
                  {t("applyTemplate")}
                </button>
              </div>

              {/* Inline notices for action failures */}
              {copyErrorMessage && (
                <p className="mp-notice mp-notice--warn">{copyErrorMessage}</p>
              )}
              {createError && (
                <p className="mp-notice mp-notice--warn">{createError}</p>
              )}
            </div>
          )}

          {/* ── Plan exists ── */}
          {currentPlan && (
            <>
              {/* Compact notice for copy conflict (alreadyExisted) */}
              {copyErrorMessage && (
                <p className="mp-notice mp-notice--warn">{copyErrorMessage}</p>
              )}

              {/* Toolbar */}
              <div className="mp-plan-actions">
                <div className="mp-plan-actions-left">
                  <span className="mp-plan-status">
                    {t(`planStatus.${currentPlan.status}` as Parameters<typeof t>[0])}
                  </span>
                  <button
                    type="button"
                    className="btn btn-sm btn-ghost"
                    disabled
                    title={t("applyTemplateSoon")}
                  >
                    {t("applyTemplate")}
                  </button>
                </div>
                <div className="mp-plan-actions-right">
                  {currentPlan.shoppingListId ? (
                    <span className="mp-plan-action-done">{t("shoppingListCreated")}</span>
                  ) : (
                    <button
                      type="button"
                      className="btn btn-sm btn-ghost"
                      onClick={handleRequestShoppingList}
                      disabled={!canRequestShoppingList}
                      title={!hasRecipeSlots ? t("shoppingListNeedsRecipes") : undefined}
                    >
                      {shoppingListStatus === "loading"
                        ? t("loading")
                        : t("requestShoppingList")}
                    </button>
                  )}
                </div>
              </div>

              {shoppingListStatus === "error" && (
                <p className="mp-notice mp-notice--warn">{t("shoppingListError")}</p>
              )}

              <WeekGrid
                plan={currentPlan}
                selectedSlotKey={selectedSlotKey}
                firstDayOfWeek={firstDayOfWeek}
                onSlotClick={(slot) => {
                  setSelectedSlot((prev) =>
                    prev && slotKey(prev) === slotKey(slot) ? null : slot,
                  );
                }}
              />
            </>
          )}
        </div>

        {/* Desktop inspector */}
        {!isMobile && selectedSlot && (
          <InspectorPanel
            title={t(`mealTypes.${selectedSlot.mealType}` as Parameters<typeof t>[0])}
            onClose={() => setSelectedSlot(null)}
          >
            {inspectorContent}
          </InspectorPanel>
        )}
      </div>

      {/* Mobile bottom sheet */}
      {isMobile && (
        <BottomSheetDetail
          open={!!selectedSlot}
          title={
            selectedSlot
              ? t(`mealTypes.${selectedSlot.mealType}` as Parameters<typeof t>[0])
              : undefined
          }
          onClose={() => setSelectedSlot(null)}
        >
          {inspectorContent}
        </BottomSheetDetail>
      )}

      {/* Create recipe modal */}
      {showCreateRecipe && (
        <CreateRecipeModal
          onConfirm={handleCreateRecipe}
          onCancel={() => {
            setShowCreateRecipe(false);
            setCreateRecipeError(null);
          }}
          isSubmitting={createRecipeStatus === "loading"}
          error={createRecipeError}
        />
      )}
    </div>
  );
}

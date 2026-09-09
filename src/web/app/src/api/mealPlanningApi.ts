import { request } from "./request";
import type {
  CreateMealPlanRequest,
  CreateMealPlanResponse,
  GetMealPlanResponse,
  UpdateMealSlotRequest,
  UpdateMealSlotResponse,
  CopyMealPlanFromPreviousWeekRequest,
  CopyMealPlanFromPreviousWeekResponse,
  RequestShoppingListRequest,
  RequestShoppingListResponse,
  CreateRecipeRequest,
  CreateRecipeResponse,
  GetFamilyRecipesResponse,
  RecipeDetail,
  UpdateRecipeRequest,
  UpdateRecipeResponse,
  DeleteRecipeResponse,
  AddRecipeIngredientRequest,
  AddRecipeIngredientResponse,
  UpdateRecipeIngredientRequest,
  UpdateRecipeIngredientResponse,
  RemoveRecipeIngredientResponse,
} from "./types/mealPlanningTypes";

export const mealPlanningApi = {
  /** Fetch a meal plan by family + week start date. Returns null mealPlan when no plan exists. */
  getMealPlan: (familyId: string, weekStart: string) =>
    request<GetMealPlanResponse>(
      `/api/meal-plans/family/${familyId}/week/${weekStart}`,
    ),

  /** Create a new meal plan for a given week. */
  createMealPlan: (body: CreateMealPlanRequest) =>
    request<CreateMealPlanResponse>("/api/meal-plans", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  /** Update a single meal slot (source type, recipe, free text, notes). */
  updateMealSlot: (
    planId: string,
    dayOfWeek: string,
    mealType: string,
    body: UpdateMealSlotRequest,
  ) =>
    request<UpdateMealSlotResponse>(
      `/api/meal-plans/${planId}/slots/${dayOfWeek}/${mealType}`,
      { method: "PUT", body: JSON.stringify(body) },
    ),

  /** Create a new meal plan by copying the previous week's plan. */
  copyFromPreviousWeek: (body: CopyMealPlanFromPreviousWeekRequest) =>
    request<CopyMealPlanFromPreviousWeekResponse>(
      "/api/meal-plans/copy-from-previous-week",
      { method: "POST", body: JSON.stringify(body) },
    ),

  /** Generate a shopping list from the meal plan's recipe slots. */
  requestShoppingList: (planId: string, body: RequestShoppingListRequest) =>
    request<RequestShoppingListResponse>(
      `/api/meal-plans/${planId}/shopping-list`,
      { method: "POST", body: JSON.stringify(body) },
    ),

  /** Get all recipes for a family, optionally filtered by meal type compatibility. */
  getFamilyRecipes: (familyId: string, mealType?: string | null) => {
    const url = mealType
      ? `/api/recipes/family/${familyId}?mealType=${encodeURIComponent(mealType)}`
      : `/api/recipes/family/${familyId}`;
    return request<GetFamilyRecipesResponse>(url);
  },

  /** Create a new recipe for a family. */
  createRecipe: (body: CreateRecipeRequest) =>
    request<CreateRecipeResponse>("/api/recipes", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  /** Get the full detail of a recipe including ingredients. */
  getRecipeDetail: (recipeId: string) =>
    request<RecipeDetail>(`/api/recipes/${recipeId}`),

  /** Update a recipe's metadata. */
  updateRecipe: (recipeId: string, body: UpdateRecipeRequest) =>
    request<UpdateRecipeResponse>(`/api/recipes/${recipeId}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  /** Delete a recipe. Rejected if referenced by an active meal plan slot. */
  deleteRecipe: (recipeId: string) =>
    request<DeleteRecipeResponse>(`/api/recipes/${recipeId}`, {
      method: "DELETE",
    }),

  /** Add an ingredient to a recipe. */
  addIngredient: (recipeId: string, body: AddRecipeIngredientRequest) =>
    request<AddRecipeIngredientResponse>(`/api/recipes/${recipeId}/ingredients`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  /** Update an ingredient's quantity and unit. */
  updateIngredient: (recipeId: string, ingredientName: string, body: UpdateRecipeIngredientRequest) =>
    request<UpdateRecipeIngredientResponse>(
      `/api/recipes/${recipeId}/ingredients/${encodeURIComponent(ingredientName)}`,
      { method: "PUT", body: JSON.stringify(body) },
    ),

  /** Remove an ingredient from a recipe. */
  removeIngredient: (recipeId: string, ingredientName: string) =>
    request<RemoveRecipeIngredientResponse>(
      `/api/recipes/${recipeId}/ingredients/${encodeURIComponent(ingredientName)}`,
      { method: "DELETE" },
    ),
};

// ── Meal Planning API contract types ────────────────────────────────────────
// All fields are primitives (string, number, boolean). No domain types.
// Date strings are ISO "YYYY-MM-DD"; timestamps are ISO UTC strings.

// ── Source types ─────────────────────────────────────────────────────────────

export type MealSourceType =
  | "Unplanned"
  | "Recipe"
  | "FreeText"
  | "External"
  | "Leftovers";

// ── Slot ─────────────────────────────────────────────────────────────────────

export interface MealSlotRecipeDetail {
  recipeId: string;
  name: string;
  servings: number | null;
  prepTimeMinutes: number | null;
  totalTimeMinutes: number | null;
  allowedMealTypes: string[];
}

export interface MealSlotDetail {
  dayOfWeek: string;   // "Monday" | … | "Sunday"
  mealType: string;    // "Breakfast" | "MidMorningSnack" | "Lunch" | "AfternoonSnack" | "Dinner"
  mealSourceType: MealSourceType;
  recipe: MealSlotRecipeDetail | null;
  freeText: string | null;
  notes: string | null;
  isOptional: boolean;
  isLocked: boolean;
}

// ── MealPlan ─────────────────────────────────────────────────────────────────

export interface MealPlanDetail {
  planId: string;
  familyId: string;
  weekStart: string; // "YYYY-MM-DD" (Monday)
  weekEnd: string;   // "YYYY-MM-DD" (Sunday)
  status: string;    // "Draft" | "Active" | "Completed"
  appliedTemplateId: string | null;
  shoppingListId: string | null;
  shoppingListVersion: number;
  lastDerivedAt: string | null;
  slots: MealSlotDetail[];
}

export interface GetMealPlanResponse {
  mealPlan: MealPlanDetail | null;
}

export interface CreateMealPlanRequest {
  mealPlanId: string;
  familyId: string;
  weekStart: string; // "YYYY-MM-DD" (Monday)
  responsibilityDomainId?: string | null;
}

export interface CreateMealPlanResponse {
  id: string;
  familyId: string;
  weekStart: string;
  weekEnd: string;
  status: string;
  createdAtUtc: string;
  alreadyExisted: boolean;
}

// ── Slot update ───────────────────────────────────────────────────────────────

export interface UpdateMealSlotRequest {
  mealSourceType: string;
  recipeId?: string | null;
  freeText?: string | null;
  notes?: string | null;
  isOptional?: boolean;
  isLocked?: boolean;
}

export interface UpdateMealSlotResponse {
  mealPlanId: string;
  dayOfWeek: string;
  mealType: string;
  mealSourceType: string;
  recipeId: string | null;
  freeText: string | null;
  notes: string | null;
  isOptional: boolean;
  isLocked: boolean;
}

// ── Copy plan ─────────────────────────────────────────────────────────────────

export interface CopyMealPlanFromPreviousWeekRequest {
  mealPlanId: string;
  familyId: string;
  weekStart: string;
  sourceMealPlanId?: string | null;
}

export interface CopyMealPlanFromPreviousWeekResponse {
  mealPlanId: string | null;
  familyId: string;
  weekStart: string;
  weekEnd: string;
  sourceMealPlanId: string | null;
  status: string | null;
  slotCount: number;
  success: boolean;
  errorCode: string | null;
  alreadyExisted: boolean;
}

// ── Shopping list ─────────────────────────────────────────────────────────────

export interface RequestShoppingListRequest {
  familyId: string;
  shoppingListName?: string | null;
}

export interface RequestShoppingListResponse {
  mealPlanId: string;
  shoppingListId: string;
  shoppingListName: string;
  itemCount: number;
}

// ── Recipe ────────────────────────────────────────────────────────────────────

export interface RecipeSummary {
  id: string;
  familyId: string;
  name: string;
  description: string | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  totalTimeMinutes: number | null;
  servings: number | null;
  isFavorite: boolean;
  tags: string[];
  allowedMealTypes: string[];
  ingredientCount: number;
  createdAtUtc: string;
}

export interface GetFamilyRecipesResponse {
  recipes: RecipeSummary[];
}

export interface CreateRecipeRequest {
  recipeId: string;
  familyId: string;
  name: string;
  description?: string | null;
  prepTimeMinutes?: number | null;
  cookTimeMinutes?: number | null;
  servings?: number | null;
  isFavorite?: boolean;
  allowedMealTypes?: string[] | null;
  tags?: string[] | null;
}

export interface CreateRecipeResponse {
  id: string;
  familyId: string;
  name: string;
  ingredientCount: number;
  totalTimeMinutes: number | null;
  isFavorite: boolean;
}

// ── Recipe detail (full, includes ingredients) ────────────────────────────────

export interface IngredientDetail {
  name: string;
  quantity: number | null;
  unit: string | null;
}

export interface RecipeDetail {
  id: string;
  familyId: string;
  name: string;
  description: string | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  totalTimeMinutes: number | null;
  servings: number | null;
  isFavorite: boolean;
  tags: string[];
  allowedMealTypes: string[];
  ingredients: IngredientDetail[];
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface UpdateRecipeRequest {
  name: string;
  description?: string | null;
  prepTimeMinutes?: number | null;
  cookTimeMinutes?: number | null;
  servings?: number | null;
  isFavorite: boolean;
  allowedMealTypes?: string[] | null;
  tags?: string[] | null;
}

export interface UpdateRecipeResponse {
  id: string;
  familyId: string;
  name: string;
  totalTimeMinutes: number | null;
  isFavorite: boolean;
}

export interface DeleteRecipeResponse {
  id: string;
  deleted: boolean;
}

export interface AddRecipeIngredientRequest {
  name: string;
  quantity?: number | null;
  unit?: string | null;
}

export interface AddRecipeIngredientResponse {
  recipeId: string;
  ingredientName: string;
  ingredientCount: number;
}

export interface UpdateRecipeIngredientRequest {
  quantity?: number | null;
  unit?: string | null;
}

export interface UpdateRecipeIngredientResponse {
  recipeId: string;
  ingredientName: string;
  quantity: number | null;
  unit: string | null;
}

export interface RemoveRecipeIngredientResponse {
  recipeId: string;
  ingredientName: string;
  ingredientCount: number;
}

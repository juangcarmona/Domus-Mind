import { createSlice, createAsyncThunk, type PayloadAction } from "@reduxjs/toolkit";
import { mealPlanningApi } from "../api/mealPlanningApi";
import type {
  MealPlanDetail,
  RecipeSummary,
} from "../api/types/mealPlanningTypes";

// ── State ─────────────────────────────────────────────────────────────────────

interface MealPlanningState {
  currentPlan: MealPlanDetail | null;
  planStatus: "idle" | "loading" | "success" | "error";
  planError: string | null;
  currentWeekStart: string;
  recipes: RecipeSummary[];
  recipesStatus: "idle" | "loading" | "success" | "error";
  updateSlotStatus: "idle" | "loading" | "error";
  createRecipeStatus: "idle" | "loading" | "error";
  createError: string | null;
  copyStatus: "idle" | "loading" | "error";
  copyError: string | null;
  shoppingListStatus: "idle" | "loading" | "success" | "error";
  shoppingListId: string | null;
}

const initialState: MealPlanningState = {
  currentPlan: null,
  planStatus: "idle",
  planError: null,
  currentWeekStart: "",
  recipes: [],
  recipesStatus: "idle",
  updateSlotStatus: "idle",
  createRecipeStatus: "idle",
  createError: null,
  copyStatus: "idle",
  copyError: null,
  shoppingListStatus: "idle",
  shoppingListId: null,
};

// ── Thunks ────────────────────────────────────────────────────────────────────

export const fetchMealPlan = createAsyncThunk(
  "mealPlanning/fetchPlan",
  async (
    { familyId, weekStart }: { familyId: string; weekStart: string },
    { rejectWithValue },
  ) => {
    try {
      const res = await mealPlanningApi.getMealPlan(familyId, weekStart);
      return res.mealPlan;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load meal plan",
      );
    }
  },
);

export const createMealPlan = createAsyncThunk(
  "mealPlanning/createPlan",
  async (
    { familyId, weekStart }: { familyId: string; weekStart: string },
    { rejectWithValue },
  ) => {
    try {
      const mealPlanId = crypto.randomUUID();
      await mealPlanningApi.createMealPlan({ mealPlanId, familyId, weekStart });
      const res = await mealPlanningApi.getMealPlan(familyId, weekStart);
      return res.mealPlan;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to create meal plan",
      );
    }
  },
);

export const updateMealSlot = createAsyncThunk(
  "mealPlanning/updateSlot",
  async (
    {
      planId,
      familyId,
      weekStart,
      dayOfWeek,
      mealType,
      mealSourceType,
      recipeId,
      freeText,
      notes,
    }: {
      planId: string;
      familyId: string;
      weekStart: string;
      dayOfWeek: string;
      mealType: string;
      mealSourceType: string;
      recipeId?: string | null;
      freeText?: string | null;
      notes?: string | null;
    },
    { rejectWithValue },
  ) => {
    try {
      await mealPlanningApi.updateMealSlot(planId, dayOfWeek, mealType, {
        mealSourceType,
        recipeId,
        freeText,
        notes,
      });
      const res = await mealPlanningApi.getMealPlan(familyId, weekStart);
      return res.mealPlan;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update slot",
      );
    }
  },
);

export const copyFromPreviousWeek = createAsyncThunk(
  "mealPlanning/copyFromPreviousWeek",
  async (
    { familyId, weekStart }: { familyId: string; weekStart: string },
    { rejectWithValue },
  ) => {
    try {
      const mealPlanId = crypto.randomUUID();
      const copyRes = await mealPlanningApi.copyFromPreviousWeek({
        mealPlanId,
        familyId,
        weekStart,
      });
      if (!copyRes.success && copyRes.errorCode === "NoPreviousPlan") {
        return { kind: "noPreviousPlan" as const };
      }
      // AlreadyExisted or success — fetch the full plan detail either way
      const res = await mealPlanningApi.getMealPlan(familyId, weekStart);
      if (copyRes.alreadyExisted) {
        return { kind: "alreadyExisted" as const, plan: res.mealPlan };
      }
      return { kind: "ok" as const, plan: res.mealPlan };
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to copy plan",
      );
    }
  },
);

export const requestShoppingList = createAsyncThunk(
  "mealPlanning/requestShoppingList",
  async (
    {
      planId,
      familyId,
      shoppingListName,
    }: { planId: string; familyId: string; shoppingListName?: string },
    { rejectWithValue },
  ) => {
    try {
      return await mealPlanningApi.requestShoppingList(planId, {
        familyId,
        shoppingListName,
      });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to generate shopping list",
      );
    }
  },
);

export const fetchFamilyRecipes = createAsyncThunk(
  "mealPlanning/fetchRecipes",
  async (familyId: string, { rejectWithValue }) => {
    try {
      const res = await mealPlanningApi.getFamilyRecipes(familyId);
      return res.recipes;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load recipes",
      );
    }
  },
);

export const createRecipe = createAsyncThunk(
  "mealPlanning/createRecipe",
  async (
    {
      familyId,
      name,
      description,
      prepTimeMinutes,
      cookTimeMinutes,
      servings,
      isFavorite,
      allowedMealTypes,
      tags,
    }: {
      familyId: string;
      name: string;
      description?: string | null;
      prepTimeMinutes?: number | null;
      cookTimeMinutes?: number | null;
      servings?: number | null;
      isFavorite?: boolean;
      allowedMealTypes?: string[];
      tags?: string[];
    },
    { rejectWithValue },
  ) => {
    try {
      const recipeId = crypto.randomUUID();
      return await mealPlanningApi.createRecipe({
        recipeId,
        familyId,
        name,
        description,
        prepTimeMinutes,
        cookTimeMinutes,
        servings,
        isFavorite,
        allowedMealTypes,
        tags,
      });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to create recipe",
      );
    }
  },
);

// ── Slice ─────────────────────────────────────────────────────────────────────

const mealPlanningSlice = createSlice({
  name: "mealPlanning",
  initialState,
  reducers: {
    setCurrentWeek(state, action: PayloadAction<string>) {
      state.currentWeekStart = action.payload;
      state.currentPlan = null;
      state.planStatus = "idle";
      state.planError = null;
      state.createError = null;
      state.copyError = null;
      state.copyStatus = "idle";
      state.shoppingListStatus = "idle";
      state.shoppingListId = null;
    },
    clearPlan(state) {
      state.currentPlan = null;
      state.planStatus = "idle";
      state.planError = null;
    },
    clearShoppingListStatus(state) {
      state.shoppingListStatus = "idle";
      state.shoppingListId = null;
    },
    clearCopyError(state) {
      state.copyError = null;
      state.copyStatus = "idle";
    },
    clearCreateError(state) {
      state.createError = null;
    },
  },
  extraReducers: (builder) => {
    // ── fetchMealPlan ─────────────────────────────────────────────────────────
    builder
      .addCase(fetchMealPlan.pending, (state) => {
        state.planStatus = "loading";
        state.planError = null;
      })
      .addCase(fetchMealPlan.fulfilled, (state, action) => {
        state.planStatus = "success";
        state.currentPlan = action.payload;
      })
      .addCase(fetchMealPlan.rejected, (state, action) => {
        state.planStatus = "error";
        state.planError = action.payload as string;
      });

    // ── createMealPlan ────────────────────────────────────────────────────────
    builder
      .addCase(createMealPlan.pending, (state) => {
        state.planStatus = "loading";
        state.createError = null;
      })
      .addCase(createMealPlan.fulfilled, (state, action) => {
        state.planStatus = "success";
        state.currentPlan = action.payload;
        state.createError = null;
      })
      .addCase(createMealPlan.rejected, (state, action) => {
        // Do NOT set planStatus=error — page stays in its current state
        state.planStatus = "success";
        state.createError = action.payload as string;
      });

    // ── updateMealSlot ────────────────────────────────────────────────────────
    builder
      .addCase(updateMealSlot.pending, (state) => {
        state.updateSlotStatus = "loading";
      })
      .addCase(updateMealSlot.fulfilled, (state, action) => {
        state.updateSlotStatus = "idle";
        state.currentPlan = action.payload;
      })
      .addCase(updateMealSlot.rejected, (state) => {
        state.updateSlotStatus = "error";
      });

    // ── copyFromPreviousWeek ──────────────────────────────────────────────────
    builder
      .addCase(copyFromPreviousWeek.pending, (state) => {
        state.copyStatus = "loading";
        state.copyError = null;
      })
      .addCase(copyFromPreviousWeek.fulfilled, (state, action) => {
        const result = action.payload;
        if (result.kind === "noPreviousPlan") {
          state.copyStatus = "error";
          state.copyError = "noPreviousPlan";
        } else if (result.kind === "alreadyExisted") {
          // Load the existing plan; surface a compact notice
          state.copyStatus = "error";
          state.copyError = "alreadyExisted";
          state.planStatus = "success";
          state.currentPlan = result.plan;
        } else {
          state.copyStatus = "idle";
          state.copyError = null;
          state.planStatus = "success";
          state.currentPlan = result.plan;
        }
      })
      .addCase(copyFromPreviousWeek.rejected, (state, action) => {
        // Network/auth failure — only touch copy state, not plan state
        state.copyStatus = "error";
        state.copyError = action.payload as string;
      });

    // ── requestShoppingList ───────────────────────────────────────────────────
    builder
      .addCase(requestShoppingList.pending, (state) => {
        state.shoppingListStatus = "loading";
      })
      .addCase(requestShoppingList.fulfilled, (state, action) => {
        state.shoppingListStatus = "success";
        state.shoppingListId = action.payload.shoppingListId;
        if (state.currentPlan) {
          state.currentPlan.shoppingListId = action.payload.shoppingListId;
        }
      })
      .addCase(requestShoppingList.rejected, (state) => {
        state.shoppingListStatus = "error";
      });

    // ── fetchFamilyRecipes ────────────────────────────────────────────────────
    builder
      .addCase(fetchFamilyRecipes.pending, (state) => {
        state.recipesStatus = "loading";
      })
      .addCase(fetchFamilyRecipes.fulfilled, (state, action) => {
        state.recipesStatus = "success";
        state.recipes = action.payload;
      })
      .addCase(fetchFamilyRecipes.rejected, (state) => {
        state.recipesStatus = "error";
      });

    // ── createRecipe ──────────────────────────────────────────────────────────
    builder
      .addCase(createRecipe.pending, (state) => {
        state.createRecipeStatus = "loading";
      })
      .addCase(createRecipe.fulfilled, (state) => {
        state.createRecipeStatus = "idle";
        // Invalidate so next open re-fetches
        state.recipesStatus = "idle";
      })
      .addCase(createRecipe.rejected, (state) => {
        state.createRecipeStatus = "error";
      });
  },
});

export const { setCurrentWeek, clearPlan, clearShoppingListStatus, clearCopyError, clearCreateError } =
  mealPlanningSlice.actions;
export default mealPlanningSlice.reducer;

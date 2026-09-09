import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { mealPlanningApi } from "../api/mealPlanningApi";
import type {
  RecipeSummary,
  RecipeDetail,
  UpdateRecipeRequest,
  AddRecipeIngredientRequest,
  UpdateRecipeIngredientRequest,
} from "../api/types/mealPlanningTypes";

// ── State ─────────────────────────────────────────────────────────────────────

interface RecipeLibraryState {
  recipes: RecipeSummary[];
  listStatus: "idle" | "loading" | "success" | "error";
  listError: string | null;
  selectedRecipe: RecipeDetail | null;
  detailStatus: "idle" | "loading" | "success" | "error";
  updateStatus: "idle" | "loading" | "error";
  updateError: string | null;
  deleteStatus: "idle" | "loading" | "error";
  deleteError: string | null;
  ingredientMutationStatus: "idle" | "loading" | "error";
  ingredientMutationError: string | null;
}

const initialState: RecipeLibraryState = {
  recipes: [],
  listStatus: "idle",
  listError: null,
  selectedRecipe: null,
  detailStatus: "idle",
  updateStatus: "idle",
  updateError: null,
  deleteStatus: "idle",
  deleteError: null,
  ingredientMutationStatus: "idle",
  ingredientMutationError: null,
};

// ── Thunks ────────────────────────────────────────────────────────────────────

export const fetchRecipes = createAsyncThunk(
  "recipeLibrary/fetchRecipes",
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

export const fetchRecipeDetail = createAsyncThunk(
  "recipeLibrary/fetchDetail",
  async (recipeId: string, { rejectWithValue }) => {
    try {
      return await mealPlanningApi.getRecipeDetail(recipeId);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load recipe",
      );
    }
  },
);

export const updateRecipe = createAsyncThunk(
  "recipeLibrary/updateRecipe",
  async (
    { recipeId, body }: { recipeId: string; body: UpdateRecipeRequest },
    { rejectWithValue },
  ) => {
    try {
      return await mealPlanningApi.updateRecipe(recipeId, body);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update recipe",
      );
    }
  },
);

export const deleteRecipe = createAsyncThunk(
  "recipeLibrary/deleteRecipe",
  async (recipeId: string, { rejectWithValue }) => {
    try {
      await mealPlanningApi.deleteRecipe(recipeId);
      return recipeId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to delete recipe",
      );
    }
  },
);

export const addIngredient = createAsyncThunk(
  "recipeLibrary/addIngredient",
  async (
    { recipeId, body }: { recipeId: string; body: AddRecipeIngredientRequest },
    { rejectWithValue },
  ) => {
    try {
      return await mealPlanningApi.addIngredient(recipeId, body);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to add ingredient",
      );
    }
  },
);

export const updateIngredient = createAsyncThunk(
  "recipeLibrary/updateIngredient",
  async (
    {
      recipeId,
      ingredientName,
      body,
    }: {
      recipeId: string;
      ingredientName: string;
      body: UpdateRecipeIngredientRequest;
    },
    { rejectWithValue },
  ) => {
    try {
      return await mealPlanningApi.updateIngredient(recipeId, ingredientName, body);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update ingredient",
      );
    }
  },
);

export const removeIngredient = createAsyncThunk(
  "recipeLibrary/removeIngredient",
  async (
    { recipeId, ingredientName }: { recipeId: string; ingredientName: string },
    { rejectWithValue },
  ) => {
    try {
      return await mealPlanningApi.removeIngredient(recipeId, ingredientName);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to remove ingredient",
      );
    }
  },
);

// ── Slice ─────────────────────────────────────────────────────────────────────

const recipeLibrarySlice = createSlice({
  name: "recipeLibrary",
  initialState,
  reducers: {
    clearSelectedRecipe(state) {
      state.selectedRecipe = null;
      state.detailStatus = "idle";
    },
    clearUpdateError(state) {
      state.updateError = null;
      state.updateStatus = "idle";
    },
    clearDeleteError(state) {
      state.deleteError = null;
      state.deleteStatus = "idle";
    },
    clearIngredientError(state) {
      state.ingredientMutationError = null;
      state.ingredientMutationStatus = "idle";
    },
  },
  extraReducers: (builder) => {
    // ── fetchRecipes ──────────────────────────────────────────────────────────
    builder
      .addCase(fetchRecipes.pending, (state) => {
        state.listStatus = "loading";
        state.listError = null;
      })
      .addCase(fetchRecipes.fulfilled, (state, action) => {
        state.listStatus = "success";
        state.recipes = action.payload;
      })
      .addCase(fetchRecipes.rejected, (state, action) => {
        state.listStatus = "error";
        state.listError = action.payload as string;
      });

    // ── fetchRecipeDetail ─────────────────────────────────────────────────────
    builder
      .addCase(fetchRecipeDetail.pending, (state) => {
        state.detailStatus = "loading";
      })
      .addCase(fetchRecipeDetail.fulfilled, (state, action) => {
        state.detailStatus = "success";
        state.selectedRecipe = action.payload;
      })
      .addCase(fetchRecipeDetail.rejected, (state) => {
        state.detailStatus = "error";
      });

    // ── updateRecipe ──────────────────────────────────────────────────────────
    builder
      .addCase(updateRecipe.pending, (state) => {
        state.updateStatus = "loading";
        state.updateError = null;
      })
      .addCase(updateRecipe.fulfilled, (state) => {
        state.updateStatus = "idle";
        // Invalidate list and detail so they re-fetch
        state.listStatus = "idle";
        state.detailStatus = "idle";
      })
      .addCase(updateRecipe.rejected, (state, action) => {
        state.updateStatus = "error";
        state.updateError = action.payload as string;
      });

    // ── deleteRecipe ──────────────────────────────────────────────────────────
    builder
      .addCase(deleteRecipe.pending, (state) => {
        state.deleteStatus = "loading";
        state.deleteError = null;
      })
      .addCase(deleteRecipe.fulfilled, (state, action) => {
        state.deleteStatus = "idle";
        state.recipes = state.recipes.filter((r) => r.id !== action.payload);
        if (state.selectedRecipe?.id === action.payload) {
          state.selectedRecipe = null;
          state.detailStatus = "idle";
        }
      })
      .addCase(deleteRecipe.rejected, (state, action) => {
        state.deleteStatus = "error";
        state.deleteError = action.payload as string;
      });

    // ── ingredient mutations — invalidate detail on success ───────────────────
    builder
      .addCase(addIngredient.pending, (state) => {
        state.ingredientMutationStatus = "loading";
        state.ingredientMutationError = null;
      })
      .addCase(addIngredient.fulfilled, (state) => {
        state.ingredientMutationStatus = "idle";
        state.detailStatus = "idle"; // trigger re-fetch
      })
      .addCase(addIngredient.rejected, (state, action) => {
        state.ingredientMutationStatus = "error";
        state.ingredientMutationError = action.payload as string;
      });

    builder
      .addCase(updateIngredient.pending, (state) => {
        state.ingredientMutationStatus = "loading";
        state.ingredientMutationError = null;
      })
      .addCase(updateIngredient.fulfilled, (state) => {
        state.ingredientMutationStatus = "idle";
        state.detailStatus = "idle";
      })
      .addCase(updateIngredient.rejected, (state, action) => {
        state.ingredientMutationStatus = "error";
        state.ingredientMutationError = action.payload as string;
      });

    builder
      .addCase(removeIngredient.pending, (state) => {
        state.ingredientMutationStatus = "loading";
        state.ingredientMutationError = null;
      })
      .addCase(removeIngredient.fulfilled, (state) => {
        state.ingredientMutationStatus = "idle";
        state.detailStatus = "idle";
      })
      .addCase(removeIngredient.rejected, (state, action) => {
        state.ingredientMutationStatus = "error";
        state.ingredientMutationError = action.payload as string;
      });
  },
});

export const {
  clearSelectedRecipe,
  clearUpdateError,
  clearDeleteError,
  clearIngredientError,
} = recipeLibrarySlice.actions;
export default recipeLibrarySlice.reducer;

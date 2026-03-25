import { createSlice, createAsyncThunk, type PayloadAction } from "@reduxjs/toolkit";
import { sharedListsApi } from "../api/sharedListsApi";
import type {
  SharedListSummary,
  SharedListItemDetail,
  GetSharedListDetailResponse,
} from "../api/types/sharedListTypes";

// ── State ────────────────────────────────────────────────────────────────────

interface SharedListsState {
  // Index: all lists for the current family
  lists: SharedListSummary[];
  listsStatus: "idle" | "loading" | "success" | "error";
  listsError: string | null;

  // Detail: currently open list
  detail: GetSharedListDetailResponse | null;
  detailStatus: "idle" | "loading" | "success" | "error";
  detailError: string | null;

  // Mutation ops
  createStatus: "idle" | "loading" | "error";
  addItemStatus: "idle" | "loading" | "error";
}

const initialState: SharedListsState = {
  lists: [],
  listsStatus: "idle",
  listsError: null,

  detail: null,
  detailStatus: "idle",
  detailError: null,

  createStatus: "idle",
  addItemStatus: "idle",
};

// ── Thunks ───────────────────────────────────────────────────────────────────

export const fetchFamilySharedLists = createAsyncThunk(
  "sharedLists/fetchAll",
  async (familyId: string, { rejectWithValue }) => {
    try {
      const res = await sharedListsApi.getFamilySharedLists(familyId);
      return res.lists;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load shared lists",
      );
    }
  },
);

export const fetchSharedListDetail = createAsyncThunk(
  "sharedLists/fetchDetail",
  async (listId: string, { rejectWithValue }) => {
    try {
      return await sharedListsApi.getSharedListDetail(listId);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load list detail",
      );
    }
  },
);

export const createSharedList = createAsyncThunk(
  "sharedLists/create",
  async (
    { familyId, name, kind }: { familyId: string; name: string; kind: string },
    { rejectWithValue },
  ) => {
    try {
      return await sharedListsApi.createSharedList({ familyId, name, kind });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to create list",
      );
    }
  },
);

export const addItemToSharedList = createAsyncThunk(
  "sharedLists/addItem",
  async (
    { listId, name }: { listId: string; name: string },
    { rejectWithValue },
  ) => {
    try {
      return await sharedListsApi.addItemToSharedList(listId, { name });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to add item",
      );
    }
  },
);

export const toggleSharedListItem = createAsyncThunk(
  "sharedLists/toggleItem",
  async (
    { listId, itemId }: { listId: string; itemId: string },
    { rejectWithValue },
  ) => {
    try {
      return await sharedListsApi.toggleSharedListItem(listId, itemId, {});
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to toggle item",
      );
    }
  },
);

// ── Slice ────────────────────────────────────────────────────────────────────

const sharedListsSlice = createSlice({
  name: "sharedLists",
  initialState,
  reducers: {
    // Optimistic toggle: flip item checked state immediately in detail
    optimisticToggleItem(state, action: PayloadAction<{ itemId: string }>) {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) item.checked = !item.checked;
    },
    clearDetail(state) {
      state.detail = null;
      state.detailStatus = "idle";
      state.detailError = null;
    },
  },
  extraReducers: (builder) => {
    // fetchFamilySharedLists
    builder
      .addCase(fetchFamilySharedLists.pending, (state) => {
        state.listsStatus = "loading";
        state.listsError = null;
      })
      .addCase(fetchFamilySharedLists.fulfilled, (state, action) => {
        state.listsStatus = "success";
        state.lists = action.payload;
      })
      .addCase(fetchFamilySharedLists.rejected, (state, action) => {
        state.listsStatus = "error";
        state.listsError = action.payload as string;
      });

    // fetchSharedListDetail
    builder
      .addCase(fetchSharedListDetail.pending, (state) => {
        state.detailStatus = "loading";
        state.detailError = null;
      })
      .addCase(fetchSharedListDetail.fulfilled, (state, action) => {
        state.detailStatus = "success";
        state.detail = action.payload;
      })
      .addCase(fetchSharedListDetail.rejected, (state, action) => {
        state.detailStatus = "error";
        state.detailError = action.payload as string;
      });

    // createSharedList — add to index on success
    builder
      .addCase(createSharedList.pending, (state) => {
        state.createStatus = "loading";
      })
      .addCase(createSharedList.fulfilled, (state, action) => {
        state.createStatus = "idle";
        state.lists.push({
          id: action.payload.listId,
          name: action.payload.name,
          kind: action.payload.kind,
          areaId: action.payload.areaId,
          linkedEntityType: action.payload.linkedEntityType,
          linkedEntityId: action.payload.linkedEntityId,
          itemCount: 0,
          uncheckedCount: 0,
        });
      })
      .addCase(createSharedList.rejected, (state) => {
        state.createStatus = "error";
      });

    // addItemToSharedList — append to detail on success
    builder
      .addCase(addItemToSharedList.pending, (state) => {
        state.addItemStatus = "loading";
      })
      .addCase(addItemToSharedList.fulfilled, (state, action) => {
        state.addItemStatus = "idle";
        if (state.detail && state.detail.listId === action.payload.listId) {
          const newItem: SharedListItemDetail = {
            itemId: action.payload.itemId,
            name: action.payload.name,
            checked: action.payload.checked,
            quantity: action.payload.quantity,
            note: action.payload.note,
            order: action.payload.order,
            updatedAtUtc: new Date().toISOString(),
            updatedByMemberId: null,
          };
          state.detail.items.push(newItem);
        }
        // Update index summary counts
        const summary = state.lists.find((l) => l.id === action.payload.listId);
        if (summary) {
          summary.itemCount += 1;
          if (!action.payload.checked) summary.uncheckedCount += 1;
        }
      })
      .addCase(addItemToSharedList.rejected, (state) => {
        state.addItemStatus = "error";
      });

    // toggleSharedListItem — sync detail item state from server response
    builder.addCase(toggleSharedListItem.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find(
        (i) => i.itemId === action.payload.itemId,
      );
      if (item) {
        item.checked = action.payload.checked;
        item.updatedAtUtc = action.payload.updatedAtUtc;
        item.updatedByMemberId = action.payload.updatedByMemberId;
      }
      // Update unchecked count in index summary
      const listId = state.detail.listId;
      const summary = state.lists.find((l) => l.id === listId);
      if (summary) {
        summary.uncheckedCount = action.payload.uncheckedCount;
      }
    });
  },
});

export const { optimisticToggleItem, clearDetail } = sharedListsSlice.actions;
export default sharedListsSlice.reducer;

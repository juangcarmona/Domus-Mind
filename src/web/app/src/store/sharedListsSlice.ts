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

export const updateSharedListItem = createAsyncThunk(
  "sharedLists/updateItem",
  async (
    { listId, itemId, name, quantity, note }: { listId: string; itemId: string; name: string; quantity?: string | null; note?: string | null },
    { rejectWithValue },
  ) => {
    try {
      return await sharedListsApi.updateSharedListItem(listId, itemId, { name, quantity, note });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update item",
      );
    }
  },
);

export const removeSharedListItem = createAsyncThunk(
  "sharedLists/removeItem",
  async (
    { listId, itemId }: { listId: string; itemId: string },
    { rejectWithValue },
  ) => {
    try {
      await sharedListsApi.removeSharedListItem(listId, itemId);
      return { listId, itemId };
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to remove item",
      );
    }
  },
);

export const renameSharedList = createAsyncThunk(
  "sharedLists/rename",
  async (
    { listId, name }: { listId: string; name: string },
    { rejectWithValue },
  ) => {
    try {
      return await sharedListsApi.renameSharedList(listId, { name });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to rename list",
      );
    }
  },
);

export const deleteSharedList = createAsyncThunk(
  "sharedLists/delete",
  async (listId: string, { rejectWithValue }) => {
    try {
      await sharedListsApi.deleteSharedList(listId);
      return listId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to delete list",
      );
    }
  },
);

export const linkSharedListToEvent = createAsyncThunk(
  "sharedLists/linkToEvent",
  async (
    { listId, eventId }: { listId: string; eventId: string },
    { rejectWithValue },
  ) => {
    try {
      return await sharedListsApi.linkSharedList(listId, {
        linkedEntityType: "CalendarEvent",
        linkedEntityId: eventId,
      });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to link list",
      );
    }
  },
);

export const unlinkSharedList = createAsyncThunk(
  "sharedLists/unlink",
  async (listId: string, { rejectWithValue }) => {
    try {
      await sharedListsApi.unlinkSharedList(listId);
      return listId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to unlink list",
      );
    }
  },
);

export const reorderSharedListItems = createAsyncThunk(
  "sharedLists/reorderItems",
  async (
    { listId, itemIds }: { listId: string; itemIds: string[] },
    { rejectWithValue },
  ) => {
    try {
      await sharedListsApi.reorderSharedListItems(listId, { itemIds });
      return { itemIds };
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to reorder items",
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
    // Optimistic rename: update item name in detail immediately
    optimisticRenameItem(state, action: PayloadAction<{ itemId: string; name: string }>) {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) item.name = action.payload.name;
    },
    // Optimistic remove: remove item from detail immediately
    optimisticRemoveItem(state, action: PayloadAction<{ itemId: string }>) {
      if (!state.detail) return;
      state.detail.items = state.detail.items.filter(
        (i) => i.itemId !== action.payload.itemId,
      );
    },
    // Optimistic reorder: update item orders immediately in detail
    optimisticReorderItems(state, action: PayloadAction<{ itemIds: string[] }>) {
      if (!state.detail) return;
      action.payload.itemIds.forEach((id, index) => {
        const item = state.detail!.items.find((i) => i.itemId === id);
        if (item) item.order = index + 1;
      });
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

    // updateSharedListItem — sync confirmed name from server
    builder.addCase(updateSharedListItem.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) {
        item.name = action.payload.name;
        item.quantity = action.payload.quantity;
        item.note = action.payload.note;
        item.updatedAtUtc = action.payload.updatedAtUtc;
      }
    });

    // removeSharedListItem — confirm removal (optimistic already applied)
    // On rejection, we'd need to restore the item — for now we refetch detail on error
    builder.addCase(removeSharedListItem.fulfilled, (state, action) => {
      // Optimistic remove already applied, update index summary
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary && summary.itemCount > 0) summary.itemCount -= 1;
    });

    // renameSharedList — update name in detail and index
    builder.addCase(renameSharedList.fulfilled, (state, action) => {
      if (state.detail?.listId === action.payload.listId) {
        state.detail.name = action.payload.name;
      }
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary) summary.name = action.payload.name;
    });

    // deleteSharedList — remove from index, clear detail if open
    builder.addCase(deleteSharedList.fulfilled, (state, action) => {
      state.lists = state.lists.filter((l) => l.id !== action.payload);
      if (state.detail?.listId === action.payload) {
        state.detail = null;
        state.detailStatus = "idle";
      }
    });

    // linkSharedListToEvent — refresh detail linkage fields in index
    builder.addCase(linkSharedListToEvent.fulfilled, (state, action) => {
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary) {
        summary.linkedEntityType = action.payload.linkedEntityType;
        summary.linkedEntityId = action.payload.linkedEntityId;
      }
    });

    // unlinkSharedList — clear linkage in index
    builder.addCase(unlinkSharedList.fulfilled, (state, action) => {
      const summary = state.lists.find((l) => l.id === action.payload);
      if (summary) {
        summary.linkedEntityType = null;
        summary.linkedEntityId = null;
      }
    });
  },
});

export const { optimisticToggleItem, optimisticRenameItem, optimisticRemoveItem, optimisticReorderItems, clearDetail } = sharedListsSlice.actions;
export default sharedListsSlice.reducer;

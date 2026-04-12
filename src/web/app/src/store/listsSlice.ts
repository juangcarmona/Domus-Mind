import { createSlice, createAsyncThunk, type PayloadAction } from "@reduxjs/toolkit";
import { listsApi } from "../api/listsApi";
import type {
  SharedListSummary,
  SharedListItemDetail,
  GetSharedListDetailResponse,
} from "../api/types/listTypes";
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
      const res = await listsApi.getFamilyLists(familyId);
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
      return await listsApi.getSharedListDetail(listId);
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
      return await listsApi.createSharedList({ familyId, name, kind });
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
      return await listsApi.addItemToSharedList(listId, { name });
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
      return await listsApi.toggleSharedListItem(listId, itemId, {});
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
      return await listsApi.updateSharedListItem(listId, itemId, { name, quantity, note });
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
      await listsApi.removeSharedListItem(listId, itemId);
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
      return await listsApi.renameSharedList(listId, { name });
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
      await listsApi.deleteSharedList(listId);
      return listId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to delete list",
      );
    }
  },
);

export const updateSharedListMetadata = createAsyncThunk(
  "sharedLists/updateMetadata",
  async (
    {
      listId,
      name,
      areaId,
      clearArea,
      linkedPlanId,
      clearLinkedPlan,
      kind,
      color,
      clearColor,
    }: {
      listId: string;
      name?: string | null;
      areaId?: string | null;
      clearArea?: boolean;
      linkedPlanId?: string | null;
      clearLinkedPlan?: boolean;
      kind?: string | null;
      color?: string | null;
      clearColor?: boolean;
    },
    { rejectWithValue },
  ) => {
    try {
      return await listsApi.updateSharedList(listId, {
        name,
        areaId,
        clearArea,
        linkedPlanId,
        clearLinkedPlan,
        kind,
        color,
        clearColor,
      });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update list",
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
      return await listsApi.linkSharedList(listId, {
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
      await listsApi.unlinkSharedList(listId);
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
      await listsApi.reorderSharedListItems(listId, { itemIds });
      return { itemIds };
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to reorder items",
      );
    }
  },
);

export const setItemImportance = createAsyncThunk(
  "sharedLists/setItemImportance",
  async (
    { listId, itemId, importance }: { listId: string; itemId: string; importance: boolean },
    { rejectWithValue },
  ) => {
    try {
      return await listsApi.setItemImportance(listId, itemId, { importance });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to set importance",
      );
    }
  },
);

export const setItemTemporal = createAsyncThunk(
  "sharedLists/setItemTemporal",
  async (
    { listId, itemId, dueDate, reminder, repeat }: {
      listId: string; itemId: string;
      dueDate?: string | null; reminder?: string | null; repeat?: string | null;
    },
    { rejectWithValue },
  ) => {
    try {
      return await listsApi.setItemTemporal(listId, itemId, { dueDate, reminder, repeat });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to set temporal fields",
      );
    }
  },
);

export const clearItemTemporal = createAsyncThunk(
  "sharedLists/clearItemTemporal",
  async (
    { listId, itemId }: { listId: string; itemId: string },
    { rejectWithValue },
  ) => {
    try {
      const res = await listsApi.clearItemTemporal(listId, itemId);
      return { ...res, itemId };
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to clear temporal fields",
      );
    }
  },
);

export const setItemContext = createAsyncThunk(
  "sharedLists/setItemContext",
  async (
    { listId, itemId, itemAreaId, targetMemberId }: {
      listId: string;
      itemId: string;
      itemAreaId?: string | null;
      targetMemberId?: string | null;
    },
    { rejectWithValue },
  ) => {
    try {
      return await listsApi.setItemContext(listId, itemId, { itemAreaId, targetMemberId });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to set item context",
      );
    }
  },
);

// ── Slice ────────────────────────────────────────────────────────────────────

const listsSlice = createSlice({
  name: "lists",
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
    // Optimistic importance: flip importance immediately
    optimisticSetImportance(state, action: PayloadAction<{ itemId: string; importance: boolean }>) {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) item.importance = action.payload.importance;
    },
    // Optimistic temporal: update temporal fields immediately
    optimisticSetTemporal(state, action: PayloadAction<{
      itemId: string;
      dueDate?: string | null;
      reminder?: string | null;
      repeat?: string | null;
    }>) {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (!item) return;
      if (action.payload.dueDate !== undefined) item.dueDate = action.payload.dueDate;
      if (action.payload.reminder !== undefined) item.reminder = action.payload.reminder;
      if (action.payload.repeat !== undefined) item.repeat = action.payload.repeat;
    },
    // Optimistic clear temporal
    optimisticClearTemporal(state, action: PayloadAction<{ itemId: string }>) {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) { item.dueDate = null; item.reminder = null; item.repeat = null; }
    },
    // Optimistic set item context (area + member)
    optimisticSetItemContext(state, action: PayloadAction<{ itemId: string; itemAreaId?: string | null; targetMemberId?: string | null }>) {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (!item) return;
      if (action.payload.itemAreaId !== undefined) item.itemAreaId = action.payload.itemAreaId;
      if (action.payload.targetMemberId !== undefined) item.targetMemberId = action.payload.targetMemberId;
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

    // createSharedList - add to index on success
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

    // addItemToSharedList - append to detail on success
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
            importance: action.payload.importance,
            dueDate: action.payload.dueDate,
            reminder: action.payload.reminder,
            repeat: action.payload.repeat,
            updatedAtUtc: new Date().toISOString(),
            updatedByMemberId: null,
            itemAreaId: null,
            targetMemberId: null,
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

    // toggleSharedListItem - sync detail item state from server response
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

    // updateSharedListItem - sync confirmed name from server
    builder.addCase(updateSharedListItem.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) {
        item.name = action.payload.name;
        item.quantity = action.payload.quantity;
        item.note = action.payload.note;
        item.importance = action.payload.importance;
        item.dueDate = action.payload.dueDate;
        item.reminder = action.payload.reminder;
        item.repeat = action.payload.repeat;
        item.updatedAtUtc = action.payload.updatedAtUtc;
      }
    });

    // removeSharedListItem - confirm removal (optimistic already applied)
    // On rejection, we'd need to restore the item - for now we refetch detail on error
    builder.addCase(removeSharedListItem.fulfilled, (state, action) => {
      // Optimistic remove already applied, update index summary
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary && summary.itemCount > 0) summary.itemCount -= 1;
    });

    // renameSharedList - update name in detail and index
    builder.addCase(renameSharedList.fulfilled, (state, action) => {
      if (state.detail?.listId === action.payload.listId) {
        state.detail.name = action.payload.name;
      }
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary) summary.name = action.payload.name;
    });

    // updateSharedListMetadata - sync list-level metadata
    builder.addCase(updateSharedListMetadata.fulfilled, (state, action) => {
      if (state.detail?.listId === action.payload.listId) {
        state.detail.name = action.payload.name;
        state.detail.areaId = action.payload.areaId;
        state.detail.color = action.payload.color;
      }
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary) {
        summary.name = action.payload.name;
        summary.areaId = action.payload.areaId;
        summary.kind = action.payload.kind;
      }
    });

    // deleteSharedList - remove from index, clear detail if open
    builder.addCase(deleteSharedList.fulfilled, (state, action) => {
      state.lists = state.lists.filter((l) => l.id !== action.payload);
      if (state.detail?.listId === action.payload) {
        state.detail = null;
        state.detailStatus = "idle";
      }
    });

    // linkSharedListToEvent - refresh detail linkage fields in index
    builder.addCase(linkSharedListToEvent.fulfilled, (state, action) => {
      const summary = state.lists.find((l) => l.id === action.payload.listId);
      if (summary) {
        summary.linkedEntityType = action.payload.linkedEntityType;
        summary.linkedEntityId = action.payload.linkedEntityId;
      }
    });

    // unlinkSharedList - clear linkage in index
    builder.addCase(unlinkSharedList.fulfilled, (state, action) => {
      const summary = state.lists.find((l) => l.id === action.payload);
      if (summary) {
        summary.linkedEntityType = null;
        summary.linkedEntityId = null;
      }
    });

    // setItemImportance - sync confirmed importance from server
    builder.addCase(setItemImportance.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) {
        item.importance = action.payload.importance;
        item.updatedAtUtc = action.payload.updatedAtUtc;
      }
    });

    // setItemTemporal - sync confirmed temporal fields from server
    builder.addCase(setItemTemporal.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) {
        item.dueDate = action.payload.dueDate;
        item.reminder = action.payload.reminder;
        item.repeat = action.payload.repeat;
        item.updatedAtUtc = action.payload.updatedAtUtc;
      }
    });

    // clearItemTemporal - clear all temporal fields
    builder.addCase(clearItemTemporal.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) {
        item.dueDate = null;
        item.reminder = null;
        item.repeat = null;
        item.updatedAtUtc = action.payload.updatedAtUtc;
      }
    });

    // setItemContext - sync confirmed item context from server
    builder.addCase(setItemContext.fulfilled, (state, action) => {
      if (!state.detail) return;
      const item = state.detail.items.find((i) => i.itemId === action.payload.itemId);
      if (item) {
        item.itemAreaId = action.payload.itemAreaId;
        item.targetMemberId = action.payload.targetMemberId;
        item.updatedAtUtc = action.payload.updatedAtUtc;
      }
    });
  },
});

export const { optimisticToggleItem, optimisticRenameItem, optimisticRemoveItem, optimisticReorderItems, optimisticSetImportance, optimisticSetTemporal, optimisticClearTemporal, optimisticSetItemContext, clearDetail } = listsSlice.actions;
export default listsSlice.reducer;

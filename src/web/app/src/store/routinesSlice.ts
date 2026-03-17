import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { domusmindApi, type RoutineListItem, type CreateRoutineRequest, type UpdateRoutineRequest } from "../api/domusmindApi";

interface RoutinesState {
  items: RoutineListItem[];
  status: "idle" | "loading" | "success" | "error";
  error: string | null;
}

const initialState: RoutinesState = {
  items: [],
  status: "idle",
  error: null,
};

export const fetchRoutines = createAsyncThunk(
  "routines/fetch",
  async (familyId: string, { rejectWithValue }) => {
    try {
      const res = await domusmindApi.getRoutines(familyId);
      return res.routines;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load routines",
      );
    }
  },
);

export const createRoutine = createAsyncThunk(
  "routines/create",
  async (body: CreateRoutineRequest, { rejectWithValue }) => {
    try {
      return await domusmindApi.createRoutine(body);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to create routine",
      );
    }
  },
);

export const updateRoutine = createAsyncThunk(
  "routines/update",
  async (
    { routineId, ...rest }: { routineId: string } & UpdateRoutineRequest,
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.updateRoutine(routineId, rest);
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update routine",
      );
    }
  },
);

export const pauseRoutine = createAsyncThunk(
  "routines/pause",
  async ({ routineId, familyId }: { routineId: string; familyId: string }, { dispatch, rejectWithValue }) => {
    try {
      await domusmindApi.pauseRoutine(routineId);
      dispatch(fetchRoutines(familyId));
      return routineId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to pause routine",
      );
    }
  },
);

export const resumeRoutine = createAsyncThunk(
  "routines/resume",
  async ({ routineId, familyId }: { routineId: string; familyId: string }, { dispatch, rejectWithValue }) => {
    try {
      await domusmindApi.resumeRoutine(routineId);
      dispatch(fetchRoutines(familyId));
      return routineId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to resume routine",
      );
    }
  },
);

const routinesSlice = createSlice({
  name: "routines",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchRoutines.pending, (state) => {
        state.status = "loading";
        state.error = null;
      })
      .addCase(fetchRoutines.fulfilled, (state, action) => {
        state.status = "success";
        state.items = action.payload;
      })
      .addCase(fetchRoutines.rejected, (state, action) => {
        state.status = "error";
        state.error = action.payload as string;
      })
      .addCase(createRoutine.fulfilled, (state, action) => {
        state.items.push(action.payload);
      })
      .addCase(updateRoutine.fulfilled, (state, action) => {
        const idx = state.items.findIndex((r) => r.routineId === action.payload.routineId);
        if (idx !== -1) state.items[idx] = action.payload;
      });
  },
});

export default routinesSlice.reducer;

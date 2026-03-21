import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { domusmindApi } from "../api/domusmindApi";

interface TaskOp {
  status: "idle" | "loading" | "success" | "error";
  error: string | null;
}

interface TasksState {
  createOp: TaskOp;
}

const initialState: TasksState = {
  createOp: { status: "idle", error: null },
};

export const createTask = createAsyncThunk(
  "tasks/create",
  async (
      {
        familyId,
        title,
        description,
        dueDate,
        dueTime,
        color,
      }: {
        familyId: string;
        title: string;
        description?: string;
        dueDate?: string | null;
        dueTime?: string | null;
        color?: string;
      },
      { rejectWithValue },
    ) => {
      try {
        return await domusmindApi.createTask({
          title,
          familyId,
          description,
          dueDate,
          dueTime,
          color,
        });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to create chore",
      );
    }
  },
);

export const completeTask = createAsyncThunk(
  "tasks/complete",
  async (taskId: string, { rejectWithValue }) => {
    try {
      await domusmindApi.completeTask(taskId);
      return taskId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to complete chore",
      );
    }
  },
);

export const cancelTask = createAsyncThunk(
  "tasks/cancel",
  async (taskId: string, { rejectWithValue }) => {
    try {
      await domusmindApi.cancelTask(taskId);
      return taskId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to cancel chore",
      );
    }
  },
);

export const assignTask = createAsyncThunk(
  "tasks/assign",
  async (
    { taskId, assigneeId }: { taskId: string; assigneeId: string },
    { rejectWithValue },
  ) => {
    try {
      await domusmindApi.assignTask(taskId, { assigneeId });
      return { taskId, assigneeId };
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to assign chore",
      );
    }
  },
);

const tasksSlice = createSlice({
  name: "tasks",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(createTask.pending, (state) => {
        state.createOp = { status: "loading", error: null };
      })
      .addCase(createTask.fulfilled, (state) => {
        state.createOp = { status: "success", error: null };
      })
      .addCase(createTask.rejected, (state, action) => {
        state.createOp = {
          status: "error",
          error: action.payload as string,
        };
      });
  },
});

export default tasksSlice.reducer;

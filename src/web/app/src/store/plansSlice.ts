import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import {
  domusmindApi,
  type FamilyTimelineEventItem,
} from "../api/domusmindApi";

interface PlansState {
  items: FamilyTimelineEventItem[];
  status: "idle" | "loading" | "success" | "error";
  error: string | null;
}

const initialState: PlansState = {
  items: [],
  status: "idle",
  error: null,
};

export const fetchPlans = createAsyncThunk(
  "plans/fetch",
  async (familyId: string, { rejectWithValue }) => {
    try {
      const res = await domusmindApi.getEvents(familyId);
      return res.events;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load plans",
      );
    }
  },
);

export const scheduleEvent = createAsyncThunk(
  "plans/schedule",
  async (
    {
      familyId,
      title,
      date,
      time,
      endDate,
      endTime,
      description,
    }: {
      familyId: string;
      title: string;
      date: string;
      time?: string;
      endDate?: string;
      endTime?: string;
      description?: string;
    },
    { rejectWithValue },
  ) => {
    try {
      const res = await domusmindApi.scheduleEvent({
        title,
        familyId,
        date,
        time,
        endDate,
        endTime,
        description,
      });
      return {
        calendarEventId: res.calendarEventId,
        title: res.title,
        startTime: res.startTime,
        endTime: res.endTime,
        status: res.status,
        participantMemberIds: [],
        participants: [],
      } as FamilyTimelineEventItem;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to schedule plan",
      );
    }
  },
);

export const cancelEvent = createAsyncThunk(
  "plans/cancel",
  async (
    { eventId, familyId }: { eventId: string; familyId: string },
    { rejectWithValue, dispatch },
  ) => {
    try {
      await domusmindApi.cancelEvent(eventId);
      dispatch(fetchPlans(familyId));
      return eventId;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to cancel plan",
      );
    }
  },
);

const plansSlice = createSlice({
  name: "plans",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchPlans.pending, (state) => {
        state.status = "loading";
        state.error = null;
      })
      .addCase(fetchPlans.fulfilled, (state, action) => {
        state.items = action.payload;
        state.status = "success";
      })
      .addCase(fetchPlans.rejected, (state, action) => {
        state.status = "error";
        state.error = action.payload as string;
      })
      .addCase(scheduleEvent.fulfilled, (state, action) => {
        state.items.push(action.payload);
      });
  },
});

export default plansSlice.reducer;

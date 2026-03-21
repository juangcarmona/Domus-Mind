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

function eventStartIso(date: string, time?: string | null): string {
  return time ? `${date}T${time}:00` : `${date}T00:00:00`;
}

function eventEndIso(
  date: string,
  time?: string | null,
  fallbackDate?: string,
): string | null {
  if (!date) return null;
  return time ? `${date}T${time}:00` : `${(fallbackDate ?? date)}T00:00:00`;
}

const initialState: PlansState = {
  items: [],
  status: "idle",
  error: null,
};

export const fetchPlans = createAsyncThunk(
  "plans/fetch",
  async ({ familyId, from }: { familyId: string; from?: string }, { rejectWithValue }) => {
    try {
      const res = await domusmindApi.getEvents(familyId, from);
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
      color,
      participantMemberIds,
    }: {
      familyId: string;
      title: string;
      date: string;
      time?: string;
      endDate?: string;
      endTime?: string;
      description?: string;
      color?: string;
      participantMemberIds?: string[];
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
        color,
        participantMemberIds,
      });
      return {
        calendarEventId: res.calendarEventId,
        title: res.title,
        startTime: eventStartIso(res.date, res.time),
        endTime: res.endDate ? eventEndIso(res.endDate, res.endTime, res.date) : null,
        date: res.date,
        time: res.time,
        endDate: res.endDate,
        endTimeValue: res.endTime,
        status: res.status,
        color: res.color,
        participantMemberIds: participantMemberIds ?? [],
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
    { eventId, familyId, from }: { eventId: string; familyId: string; from?: string },
    { rejectWithValue, dispatch },
  ) => {
    try {
      await domusmindApi.cancelEvent(eventId);
      dispatch(fetchPlans({ familyId, from }));
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
        state.items = action.payload.map((event) => {
          if (event.date) {
            return {
              ...event,
              startTime: eventStartIso(event.date, event.time),
              // Preserve the raw end time string before endTime is overwritten
              // with the computed ISO datetime. EditEntityModal reads endTimeValue
              // to populate the end-time field of PlanCrudForm.
              endTimeValue: event.endDate ? (event.endTime ?? null) : null,
              endTime: event.endDate ? eventEndIso(event.endDate, event.endTime) : null,
            };
          }
          return event;
        });
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

import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export type ViewMode = "timeline" | "day" | "week" | "month";

interface CoordinationState {
  selectedDate: string; // ISO YYYY-MM-DD
  viewMode: ViewMode;
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

const initialState: CoordinationState = {
  selectedDate: todayIso(),
  viewMode: "timeline",
};

const coordinationSlice = createSlice({
  name: "coordination",
  initialState,
  reducers: {
    setSelectedDate(state, action: PayloadAction<string>) {
      state.selectedDate = action.payload;
    },
    setViewMode(state, action: PayloadAction<ViewMode>) {
      state.viewMode = action.payload;
    },
    selectDayAndSwitchToDay(state, action: PayloadAction<string>) {
      state.selectedDate = action.payload;
      state.viewMode = "day";
    },
  },
});

export const { setSelectedDate, setViewMode, selectDayAndSwitchToDay } =
  coordinationSlice.actions;
export default coordinationSlice.reducer;

import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

/** The two mid-term time lenses available on the Today surface. */
export type TodayLens = "week" | "month";

interface TodayState {
  selectedDate: string; // ISO YYYY-MM-DD
  lens: TodayLens;
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

const initialState: TodayState = {
  selectedDate: todayIso(),
  lens: "week",
};

const todaySlice = createSlice({
  name: "today",
  initialState,
  reducers: {
    setSelectedDate(state, action: PayloadAction<string>) {
      state.selectedDate = action.payload;
    },
    setLens(state, action: PayloadAction<TodayLens>) {
      state.lens = action.payload;
    },
  },
});

export const { setSelectedDate, setLens } = todaySlice.actions;
export default todaySlice.reducer;

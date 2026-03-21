import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface TodayState {
  selectedDate: string; // ISO YYYY-MM-DD
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

const initialState: TodayState = {
  selectedDate: todayIso(),
};

const todaySlice = createSlice({
  name: "today",
  initialState,
  reducers: {
    setSelectedDate(state, action: PayloadAction<string>) {
      state.selectedDate = action.payload;
    },
  },
});

export const { setSelectedDate } = todaySlice.actions;
export default todaySlice.reducer;

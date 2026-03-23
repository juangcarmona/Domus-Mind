import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import {
  domusmindApi,
  type HouseholdAreaItem,
} from "../api/domusmindApi";

interface AreasState {
  items: HouseholdAreaItem[];
  status: "idle" | "loading" | "success" | "error";
  error: string | null;
}

const initialState: AreasState = {
  items: [],
  status: "idle",
  error: null,
};

export const fetchAreas = createAsyncThunk(
  "areas/fetch",
  async (familyId: string, { rejectWithValue }) => {
    try {
      const res = await domusmindApi.getAreas(familyId);
      return res.areas;
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to load areas",
      );
    }
  },
);

const areasSlice = createSlice({
  name: "areas",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchAreas.pending, (state) => {
        state.status = "loading";
        state.error = null;
      })
      .addCase(fetchAreas.fulfilled, (state, action) => {
        state.items = action.payload;
        state.status = "success";
      })
      .addCase(fetchAreas.rejected, (state, action) => {
        state.status = "error";
        state.error = action.payload as string;
      });
  },
});

export default areasSlice.reducer;

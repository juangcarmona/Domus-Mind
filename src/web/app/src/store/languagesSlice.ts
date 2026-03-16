import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { domusmindApi, type SupportedLanguageItem } from "../api/domusmindApi";

interface LanguagesState {
  items: SupportedLanguageItem[];
  status: "idle" | "loading" | "ready" | "error";
}

const initialState: LanguagesState = {
  items: [],
  status: "idle",
};

export const fetchSupportedLanguages = createAsyncThunk(
  "languages/fetchSupported",
  async () => {
    const res = await domusmindApi.getSupportedLanguages();
    return res.languages;
  },
);

const languagesSlice = createSlice({
  name: "languages",
  initialState,
  reducers: {},
  extraReducers(builder) {
    builder
      .addCase(fetchSupportedLanguages.pending, (state) => {
        state.status = "loading";
      })
      .addCase(fetchSupportedLanguages.fulfilled, (state, action) => {
        state.items = action.payload;
        state.status = "ready";
      })
      .addCase(fetchSupportedLanguages.rejected, (state) => {
        state.status = "error";
      });
  },
});

export default languagesSlice.reducer;

import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import i18n from "../i18n/index";
import { bootstrapHousehold } from "./householdSlice";
import { updateHouseholdSettings } from "./householdSlice";

export type Theme = "light" | "dark" | "system";

const THEME_KEY = "dm_theme";

interface UiState {
  /**
   * Active UI language code (e.g. "es", "en").
   * Derived from the household's primaryLanguageCode after bootstrap.
   * Falls back to whatever i18next LanguageDetector resolved at startup.
   */
  language: string;
  /**
   * Date display format token from the household settings (e.g. "dd/MM/yyyy").
   * Null until bootstrap completes or if the household has no preference.
   */
  dateFormat: string | null;
  /** User-scoped theme preference, persisted in localStorage. */
  theme: Theme;
}

function resolveInitialLanguage(): string {
  return i18n.language?.split("-")[0] || "en";
}

const initialState: UiState = {
  language: resolveInitialLanguage(),
  dateFormat: null,
  theme: (localStorage.getItem(THEME_KEY) as Theme) || "system",
};

const uiSlice = createSlice({
  name: "ui",
  initialState,
  reducers: {
    setTheme(state, action: PayloadAction<Theme>) {
      state.theme = action.payload;
      localStorage.setItem(THEME_KEY, action.payload);
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(bootstrapHousehold.fulfilled, (state, action) => {
        if (action.payload?.family) {
          const { primaryLanguageCode, dateFormatPreference } = action.payload.family;
          if (primaryLanguageCode) state.language = primaryLanguageCode;
          state.dateFormat = dateFormatPreference ?? null;
        }
      })
      .addCase(updateHouseholdSettings.fulfilled, (state, action) => {
        if (action.payload) {
          const { primaryLanguageCode, dateFormatPreference } = action.payload;
          if (primaryLanguageCode) state.language = primaryLanguageCode;
          state.dateFormat = dateFormatPreference ?? null;
        }
      });
  },
});

export const { setTheme } = uiSlice.actions;
export default uiSlice.reducer;

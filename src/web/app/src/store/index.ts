import { configureStore } from "@reduxjs/toolkit";
import householdReducer from "./householdSlice";
import timelineReducer from "./timelineSlice";
import areasReducer from "./areasSlice";
import plansReducer from "./plansSlice";
import tasksReducer from "./tasksSlice";
import routinesReducer from "./routinesSlice";
import languagesReducer from "./languagesSlice";
import uiReducer from "./uiSlice";
import listsReducer from "./listsSlice";
import externalCalendarReducer from "./externalCalendarSlice";
import mealPlanningReducer from "./mealPlanningSlice";
import recipeLibraryReducer from "./recipeLibrarySlice";

export const store = configureStore({
  reducer: {
    household: householdReducer,
    timeline: timelineReducer,
    areas: areasReducer,
    plans: plansReducer,
    tasks: tasksReducer,
    routines: routinesReducer,
    languages: languagesReducer,
    ui: uiReducer,
    lists: listsReducer,
    externalCalendar: externalCalendarReducer,
    mealPlanning: mealPlanningReducer,
    recipeLibrary: recipeLibraryReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

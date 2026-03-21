import { configureStore } from "@reduxjs/toolkit";
import householdReducer from "./householdSlice";
import timelineReducer from "./timelineSlice";
import areasReducer from "./areasSlice";
import plansReducer from "./plansSlice";
import tasksReducer from "./tasksSlice";
import routinesReducer from "./routinesSlice";
import languagesReducer from "./languagesSlice";
import todayReducer from "./todaySlice";
import uiReducer from "./uiSlice";

export const store = configureStore({
  reducer: {
    household: householdReducer,
    timeline: timelineReducer,
    areas: areasReducer,
    plans: plansReducer,
    tasks: tasksReducer,
    routines: routinesReducer,
    languages: languagesReducer,
    today: todayReducer,
    ui: uiReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

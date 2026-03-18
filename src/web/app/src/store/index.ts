import { configureStore } from "@reduxjs/toolkit";
import householdReducer from "./householdSlice";
import timelineReducer from "./timelineSlice";
import areasReducer from "./areasSlice";
import plansReducer from "./plansSlice";
import tasksReducer from "./tasksSlice";
import routinesReducer from "./routinesSlice";
import languagesReducer from "./languagesSlice";
import coordinationReducer from "./coordinationSlice";

export const store = configureStore({
  reducer: {
    household: householdReducer,
    timeline: timelineReducer,
    areas: areasReducer,
    plans: plansReducer,
    tasks: tasksReducer,
    routines: routinesReducer,
    languages: languagesReducer,
    coordination: coordinationReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

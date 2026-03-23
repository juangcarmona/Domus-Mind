import { familyApi } from "./familyApi";
import { calendarApi } from "./calendarApi";
import { domainApi } from "./domainApi";

export { getStoredToken } from "./request";

// Composite client — all existing consumers import from this file unchanged
export const domusmindApi = {
  ...familyApi,
  ...calendarApi,
  ...domainApi,
};

// Re-export all types so existing consumers keep working
export type { ApiError } from "./request";
export type * from "./types/memberTypes";
export type * from "./types/calendarTypes";
export type * from "./types/domainTypes";

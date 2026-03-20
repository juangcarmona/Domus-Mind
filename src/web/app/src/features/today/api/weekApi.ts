import { domusmindApi } from "../../../api/domusmindApi";
import type { WeeklyGridResponse } from "../types";

export const weekApi = {
  getWeeklyGrid: (familyId: string, weekStart?: string): Promise<WeeklyGridResponse> =>
    domusmindApi.getWeeklyGrid(familyId, weekStart),
};

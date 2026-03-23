import { request } from "./request";
import type {
  ScheduleEventRequest,
  ScheduleEventResponse,
  FamilyTimelineResponse,
  RescheduleEventRequest,
} from "./types/calendarTypes";
import type { WeeklyGridResponse } from "../features/today/types";

export const calendarApi = {
  scheduleEvent: (body: ScheduleEventRequest) =>
    request<ScheduleEventResponse>("/api/events", { method: "POST", body: JSON.stringify(body) }),

  getEvents: (familyId: string, from?: string, to?: string) => {
    const params = new URLSearchParams({ familyId });
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    return request<FamilyTimelineResponse>(`/api/events?${params}`);
  },

  rescheduleEvent: (id: string, body: RescheduleEventRequest) =>
    request<unknown>(`/api/events/${id}/reschedule`, { method: "POST", body: JSON.stringify(body) }),

  cancelEvent: (id: string) =>
    request<unknown>(`/api/events/${id}/cancel`, { method: "POST" }),

  addEventParticipant: (id: string, memberId: string) =>
    request<unknown>(`/api/events/${id}/participants`, { method: "POST", body: JSON.stringify({ memberId }) }),

  removeEventParticipant: (id: string, memberId: string) =>
    request<unknown>(`/api/events/${id}/participants/${memberId}`, { method: "DELETE" }),

  getWeeklyGrid: (familyId: string, weekStart?: string) => {
    const params = weekStart ? `?weekStart=${encodeURIComponent(weekStart)}` : "";
    return request<WeeklyGridResponse>(`/api/families/${familyId}/weekly-grid${params}`);
  },
};

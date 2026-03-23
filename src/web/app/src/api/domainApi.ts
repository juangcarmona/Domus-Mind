import { request } from "./request";
import type {
  HouseholdAreasResponse,
  CreateTaskRequest,
  CreateTaskResponse,
  AssignTaskRequest,
} from "./types/domainTypes";
import type {
  RoutineListItem,
  RoutineListResponse,
  CreateRoutineRequest,
  UpdateRoutineRequest,
} from "./types/calendarTypes";

export const domainApi = {
  /* Areas */
  getAreas: (familyId: string) =>
    request<HouseholdAreasResponse>(`/api/families/${familyId}/areas`),

  /* Tasks */
  createTask: (body: CreateTaskRequest) =>
    request<CreateTaskResponse>("/api/tasks", { method: "POST", body: JSON.stringify(body) }),

  assignTask: (taskId: string, body: AssignTaskRequest) =>
    request<unknown>(`/api/tasks/${taskId}/assign`, { method: "POST", body: JSON.stringify(body) }),

  completeTask: (taskId: string) =>
    request<unknown>(`/api/tasks/${taskId}/complete`, { method: "POST" }),

  cancelTask: (taskId: string) =>
    request<unknown>(`/api/tasks/${taskId}/cancel`, { method: "POST" }),

  rescheduleTask: (taskId: string, dueDate: string | null, dueTime?: string | null, title?: string | null, color?: string | null) =>
    request<unknown>(`/api/tasks/${taskId}/reschedule`, {
      method: "POST",
      body: JSON.stringify({ dueDate, dueTime: dueTime ?? null, title: title ?? null, color: color ?? null }),
    }),

  /* Routines */
  getRoutines: (familyId: string) =>
    request<RoutineListResponse>(`/api/routines?familyId=${familyId}`),

  createRoutine: (body: CreateRoutineRequest) =>
    request<RoutineListItem>("/api/routines", { method: "POST", body: JSON.stringify(body) }),

  updateRoutine: (routineId: string, body: UpdateRoutineRequest) =>
    request<RoutineListItem>(`/api/routines/${routineId}`, { method: "PUT", body: JSON.stringify(body) }),

  pauseRoutine: (routineId: string) =>
    request<unknown>(`/api/routines/${routineId}/pause`, { method: "POST" }),

  resumeRoutine: (routineId: string) =>
    request<unknown>(`/api/routines/${routineId}/resume`, { method: "POST" }),
};

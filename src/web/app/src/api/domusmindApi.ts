import { getStoredToken } from "../lib/tokenStorage";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "";

export { getStoredToken };

export interface ApiError {
  status: number;
  message: string;
}

async function request<T>(
  path: string,
  options?: RequestInit,
  token?: string | null,
): Promise<T> {
  const resolvedToken = token ?? getStoredToken();
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...(resolvedToken ? { Authorization: `Bearer ${resolvedToken}` } : {}),
    ...options?.headers,
  };
  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });
  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      if (body?.error) message = body.error;
    } catch {
      // ignore
    }
    throw { status: res.status, message } as ApiError;
  }
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

/* ---- Types ---- */

export interface FamilyResponse {
  familyId: string;
  name: string;
  primaryLanguageCode: string | null;
  createdAtUtc: string;
  memberCount: number;
  firstDayOfWeek: string | null;
  dateFormatPreference: string | null;
}

export interface CreateFamilyRequest {
  name: string;
  primaryLanguageCode?: string | null;
}

export interface SupportedLanguageItem {
  code: string;
  culture: string;
  displayName: string;
  nativeDisplayName: string;
  isDefault: boolean;
  sortOrder: number;
}

export interface FamilyMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  joinedAtUtc: string;
  authUserId: string | null;
}

export interface AddMemberRequest {
  name: string;
  role: string;
}

export interface AddMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  joinedAtUtc: string;
}

export interface LinkMemberAccountRequest {
  username: string;
  temporaryPassword: string;
}

export interface LinkMemberAccountResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  username: string;
  authUserId: string;
  linkedAtUtc: string;
}

export interface InviteMemberRequest {
  name: string;
  role: string;
  birthDate?: string | null;
  isManager: boolean;
  username: string;
  temporaryPassword: string;
}

export interface InviteMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  username: string;
  joinedAtUtc: string;
}

export interface UpdateMemberRequest {
  name: string;
  role: string;
  birthDate?: string | null;
  isManager: boolean;
}

export interface UpdateMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  joinedAtUtc: string;
}

export interface ParticipantProjection {
  memberId: string;
  displayName: string;
}

export interface EnrichedTimelineEntry {
  entryId: string;
  entryType: "CalendarEvent" | "Task" | "Routine";
  title: string;
  effectiveDate: string | null;
  status: string;
  priority: "High" | "Medium" | "Low";
  group: "Overdue" | "Today" | "Tomorrow" | "ThisWeek" | "Later" | "Undated";
  isOverdue: boolean;
  isUnassigned: boolean;
  assigneeId: string | null;
  participants: ParticipantProjection[] | null;
  color: string;
}

export interface TimelineGroup {
  groupKey: string;
  entries: EnrichedTimelineEntry[];
}

export interface EnrichedTimelineResponse {
  groups: TimelineGroup[];
  totalEntries: number;
}

export interface ScheduleEventRequest {
  title: string;
  familyId: string;
  date: string;
  time?: string;
  endDate?: string;
  endTime?: string;
  description?: string;
  color?: string;
  participantMemberIds?: string[];
}

export interface ScheduleEventResponse {
  calendarEventId: string;
  familyId: string;
  title: string;
  date: string;
  time: string | null;
  endDate: string | null;
  endTime: string | null;
  status: string;
  color: string;
  createdAtUtc: string;
}

export interface FamilyTimelineEventItem {
  calendarEventId: string;
  title: string;
  startTime: string;
  endTime: string | null;
  status: string;
  color: string;
  participantMemberIds: string[];
  participants: ParticipantProjection[];
  date?: string;
  time?: string | null;
  endDate?: string | null;
  endTimeValue?: string | null;
}

export interface RoutineListItem {
  routineId: string;
  familyId: string;
  name: string;
  scope: string;
  kind: string;
  color: string;
  frequency: string;
  daysOfWeek: number[];
  daysOfMonth: number[];
  monthOfYear: number | null;
  time: string | null;
  targetMemberIds: string[];
  status: string;
  createdAtUtc: string;
}

export interface RoutineListResponse {
  routines: RoutineListItem[];
}

export interface CreateRoutineRequest {
  name: string;
  familyId: string;
  scope: string;
  kind: string;
  color: string;
  frequency: string;
  daysOfWeek: number[];
  daysOfMonth: number[];
  monthOfYear?: number | null;
  time?: string | null;
  targetMemberIds: string[];
}

export interface UpdateRoutineRequest {
  name: string;
  scope: string;
  kind: string;
  color: string;
  frequency: string;
  daysOfWeek: number[];
  daysOfMonth: number[];
  monthOfYear?: number | null;
  time?: string | null;
  targetMemberIds: string[];
}

export interface FamilyTimelineResponse {
  events: FamilyTimelineEventItem[];
}

export interface RescheduleEventRequest {
  date: string;
  time?: string;
  endDate?: string;
  endTime?: string;
  title?: string;
  description?: string | null;
  color?: string;
}

export interface HouseholdAreaItem {
  areaId: string;
  name: string;
  primaryOwnerId: string | null;
  primaryOwnerName: string | null;
  secondaryOwnerIds: string[];
}

export interface HouseholdAreasResponse {
  areas: HouseholdAreaItem[];
}

export interface CreateResponsibilityDomainRequest {
  name: string;
  familyId: string;
}

export interface CreateResponsibilityDomainResponse {
  responsibilityDomainId: string;
  familyId: string;
  name: string;
  createdAtUtc: string;
}

export interface AssignPrimaryOwnerRequest {
  memberId: string;
}

export interface AssignSecondaryOwnerRequest {
  memberId: string;
}

export interface TransferResponsibilityRequest {
  newPrimaryOwnerId: string;
}

export interface CreateTaskRequest {
  title: string;
  familyId: string;
  description?: string;
  dueDate?: string | null;
  dueTime?: string | null;
  color?: string;
}

export interface CreateTaskResponse {
  taskId: string;
  familyId: string;
  title: string;
  description: string | null;
  dueDate: string | null;
  status: string;
  color: string;
  createdAtUtc: string;
}

export interface AssignTaskRequest {
  assigneeId: string;
}

export interface AdditionalMemberRequest {
  name: string;
  birthDate?: string | null;
  type?: string | null;
  manager?: boolean;
}

export interface CompleteOnboardingRequest {
  selfName: string;
  selfBirthDate?: string | null;
  additionalMembers?: AdditionalMemberRequest[];
}

export interface OnboardingMemberItem {
  memberId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  joinedAtUtc: string;
}

export interface CompleteOnboardingResponse {
  familyId: string;
  familyName: string;
  members: OnboardingMemberItem[];
}

export interface UpdateFamilySettingsRequest {
  name: string;
  primaryLanguageCode?: string | null;
  firstDayOfWeek?: string | null;
  dateFormatPreference?: string | null;
}

export interface UpdateFamilySettingsResponse {
  familyId: string;
  name: string;
  primaryLanguageCode: string | null;
  firstDayOfWeek: string | null;
  dateFormatPreference: string | null;
}

/* ---- API client ---- */

export const domusmindApi = {
  /* Family */
  createFamily: (body: CreateFamilyRequest) =>
    request<FamilyResponse>("/api/families", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  getMyFamily: () =>
    request<FamilyResponse>("/api/families/mine"),

  getFamily: (familyId: string) =>
    request<FamilyResponse>(`/api/families/${familyId}`),

  getMembers: (familyId: string) =>
    request<FamilyMemberResponse[]>(`/api/families/${familyId}/members`),

  addMember: (familyId: string, body: AddMemberRequest) =>
    request<AddMemberResponse>(`/api/families/${familyId}/members`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  inviteMember: (familyId: string, body: InviteMemberRequest) =>
    request<InviteMemberResponse>(`/api/families/${familyId}/members/invite`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  linkMemberAccount: (familyId: string, memberId: string, body: LinkMemberAccountRequest) =>
    request<LinkMemberAccountResponse>(
      `/api/families/${familyId}/members/${memberId}/link-account`,
      {
        method: "POST",
        body: JSON.stringify(body),
      },
    ),

  updateMember: (familyId: string, memberId: string, body: UpdateMemberRequest) =>
    request<UpdateMemberResponse>(`/api/families/${familyId}/members/${memberId}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  completeOnboarding: (familyId: string, body: CompleteOnboardingRequest) =>
    request<CompleteOnboardingResponse>(`/api/families/${familyId}/onboarding`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  updateFamilySettings: (familyId: string, body: UpdateFamilySettingsRequest) =>
    request<UpdateFamilySettingsResponse>(`/api/families/${familyId}/settings`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  getEnrichedTimeline: (
    familyId: string,
    opts?: {
      types?: string;
      memberFilter?: string;
      from?: string;
      to?: string;
      statuses?: string;
    },
  ) => {
    const params = new URLSearchParams();
    if (opts?.types) params.set("types", opts.types);
    if (opts?.memberFilter) params.set("memberFilter", opts.memberFilter);
    if (opts?.from) params.set("from", opts.from);
    if (opts?.to) params.set("to", opts.to);
    if (opts?.statuses) params.set("statuses", opts.statuses);
    const qs = params.toString();
    return request<EnrichedTimelineResponse>(
      `/api/families/${familyId}/timeline/enriched${qs ? `?${qs}` : ""}`,
    );
  },

  /* Events / Plans */
  scheduleEvent: (body: ScheduleEventRequest) =>
    request<ScheduleEventResponse>("/api/events", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  getEvents: (familyId: string, from?: string, to?: string) => {
    const params = new URLSearchParams({ familyId });
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    return request<FamilyTimelineResponse>(`/api/events?${params}`);
  },

  rescheduleEvent: (id: string, body: RescheduleEventRequest) =>
    request<unknown>(`/api/events/${id}/reschedule`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  cancelEvent: (id: string) =>
    request<unknown>(`/api/events/${id}/cancel`, { method: "POST" }),

  addEventParticipant: (id: string, memberId: string) =>
    request<unknown>(`/api/events/${id}/participants`, {
      method: "POST",
      body: JSON.stringify({ memberId }),
    }),

  removeEventParticipant: (id: string, memberId: string) =>
    request<unknown>(`/api/events/${id}/participants/${memberId}`, {
      method: "DELETE",
    }),

  /* Responsibility Domains / Areas */
  getAreas: (familyId: string) =>
    request<HouseholdAreasResponse>(
      `/api/responsibility-domains?familyId=${familyId}`,
    ),

  createArea: (body: CreateResponsibilityDomainRequest) =>
    request<CreateResponsibilityDomainResponse>(
      "/api/responsibility-domains",
      { method: "POST", body: JSON.stringify(body) },
    ),

  assignPrimaryOwner: (areaId: string, body: AssignPrimaryOwnerRequest) =>
    request<unknown>(`/api/responsibility-domains/${areaId}/primary-owner`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  assignSecondaryOwner: (areaId: string, body: AssignSecondaryOwnerRequest) =>
    request<unknown>(
      `/api/responsibility-domains/${areaId}/secondary-owners`,
      { method: "POST", body: JSON.stringify(body) },
    ),

  transferArea: (areaId: string, body: TransferResponsibilityRequest) =>
    request<unknown>(`/api/responsibility-domains/${areaId}/transfer`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  /* Tasks */
  createTask: (body: CreateTaskRequest) =>
    request<CreateTaskResponse>("/api/tasks", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  assignTask: (taskId: string, body: AssignTaskRequest) =>
    request<unknown>(`/api/tasks/${taskId}/assign`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  completeTask: (taskId: string) =>
    request<unknown>(`/api/tasks/${taskId}/complete`, { method: "POST" }),

  cancelTask: (taskId: string) =>
    request<unknown>(`/api/tasks/${taskId}/cancel`, { method: "POST" }),

  rescheduleTask: (
    taskId: string,
    dueDate: string | null,
    dueTime?: string | null,
    title?: string | null,
    color?: string | null,
  ) =>
    request<unknown>(`/api/tasks/${taskId}/reschedule`, {
      method: "POST",
      body: JSON.stringify({ dueDate, dueTime: dueTime ?? null, title: title ?? null, color: color ?? null }),
    }),

  /* Languages */
  getSupportedLanguages: () =>
    // Public endpoint — no auth token needed
    request<{ languages: SupportedLanguageItem[] }>("/api/languages", {}, null),

  /* Weekly grid */
  getWeeklyGrid: (familyId: string, weekStart?: string) => {
    const params = weekStart ? `?weekStart=${encodeURIComponent(weekStart)}` : "";
    return request<import("../features/today/types").WeeklyGridResponse>(
      `/api/families/${familyId}/weekly-grid${params}`,
    );
  },

  /* Routines */
  getRoutines: (familyId: string) =>
    request<RoutineListResponse>(`/api/routines?familyId=${familyId}`),

  createRoutine: (body: CreateRoutineRequest) =>
    request<RoutineListItem>("/api/routines", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  updateRoutine: (routineId: string, body: UpdateRoutineRequest) =>
    request<RoutineListItem>(`/api/routines/${routineId}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  pauseRoutine: (routineId: string) =>
    request<unknown>(`/api/routines/${routineId}/pause`, { method: "POST" }),

  resumeRoutine: (routineId: string) =>
    request<unknown>(`/api/routines/${routineId}/resume`, { method: "POST" }),
};

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

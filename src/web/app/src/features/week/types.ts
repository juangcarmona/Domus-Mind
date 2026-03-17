import type { ParticipantProjection } from "../../api/domusmindApi";

export interface WeeklyGridEventItem {
  eventId: string;
  title: string;
  startTime: string;
  endTime: string | null;
  status: string;
  participants: ParticipantProjection[];
}

export interface WeeklyGridTaskItem {
  taskId: string;
  title: string;
  dueDate: string | null;
  status: string;
}

export interface WeeklyGridRoutineItem {
  routineId: string;
  name: string;
  kind: string;
  color: string | null;
  frequency: string;
  time: string | null;
  scope: string;
}

export interface WeeklyGridCell {
  date: string;
  events: WeeklyGridEventItem[];
  tasks: WeeklyGridTaskItem[];
  routines: WeeklyGridRoutineItem[];
}

export interface WeeklyGridMember {
  memberId: string;
  name: string;
  role: string;
  cells: WeeklyGridCell[];
}

export interface WeeklyGridResponse {
  weekStart: string;
  weekEnd: string;
  members: WeeklyGridMember[];
  sharedCells: WeeklyGridCell[];
}

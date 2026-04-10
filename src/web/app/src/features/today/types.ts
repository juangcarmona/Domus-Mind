import type { ParticipantProjection } from "../../api/domusmindApi";

export interface WeeklyGridEventItem {
  eventId: string;
  title: string;
  date: string;        // YYYY-MM-DD
  time: string | null; // HH:mm
  endDate: string | null;
  endTime: string | null;
  status: string;
  color: string;
  participants: ParticipantProjection[];
  isReadOnly?: boolean;
  source?: string | null;
  providerLabel?: string | null;
  openInProviderUrl?: string | null;
}

export interface WeeklyGridTaskItem {
  taskId: string;
  title: string;
  dueDate: string | null;
  status: string;
  color: string;
}

export interface WeeklyGridRoutineItem {
  routineId: string;
  name: string;
  kind: string;
  color: string | null;
  frequency: string;
  time: string | null;
  endTime: string | null;
  scope: string;
}

export interface WeeklyGridCell {
  date: string;
  events: WeeklyGridEventItem[];
  tasks: WeeklyGridTaskItem[];
  routines: WeeklyGridRoutineItem[];
}

/** Per-day item-type summary used by the month view density pips. */
export interface DayTypeSummary {
  events: number;
  tasks: number;
  routines: number;
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

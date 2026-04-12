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
  calendarName?: string | null;
  location?: string | null;
}

export interface WeeklyGridTaskItem {
  taskId: string;
  title: string;
  dueDate: string | null;
  status: string;
  color: string;
}

export interface WeeklyGridListItem {
  listId: string;
  listName: string;
  color?: string | null;
  itemId: string;
  title: string;
  note: string | null;
  checked: boolean;
  importance: boolean;
  dueDate: string | null;
  reminder: string | null;
  repeat: string | null;
  itemAreaId?: string | null;
  itemAreaName?: string | null;
  targetMemberId?: string | null;
  targetMemberName?: string | null;
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
  listItems: WeeklyGridListItem[];
}

/** Per-day item-type summary used by the month view density pips. */
export interface DayTypeSummary {
  events: number;
  tasks: number;
  routines: number;
  listItems: number;
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

import type {
  WeeklyGridCell,
  WeeklyGridEventItem,
  WeeklyGridListItem,
  WeeklyGridTaskItem,
  WeeklyGridRoutineItem,
} from "../types";

// ----------------------------------------------------------------
// Shared calendar entry model
// Used by both the Today Panel and the Weekly Grid.
// ----------------------------------------------------------------

export type CalendarEntryDisplayType =
  | "overdue"
  | "task"
  | "list-important"
  | "event"
  | "routine"
  | "list"
  | "completed";

export type CalendarEntrySourceType = "event" | "task" | "routine" | "list-item";

/**
 * Normalized presentation model for a single calendar entry.
 *
 * This is the contract shared between:
 *   - Today Panel  (TodayMemberCard, TodayBoard)
 *   - Weekly Grid  (WeeklyGridCell)
 *
 * Color is always the user-defined value from the source entry.
 * It must never be overridden or normalized here.
 */
export interface CalendarEntry {
  id: string;
  sourceType: CalendarEntrySourceType;
  /** Drives glyph selection and CSS emphasis class. */
  displayType: CalendarEntryDisplayType;
  title: string;
  /** HH:mm or null. Events may carry a time; tasks and routines may not. */
  time: string | null;
  /**
   * HH:mm end time for events with a known duration.
   * Used by the agenda Day timeline to render duration blocks.
   * Null for tasks, routines, and untimed events.
   */
  endTime?: string | null;
  /** ISO date (YYYY-MM-DD) for event/routine timeline context when available. */
  date?: string | null;
  /** Optional end date for multi-day events. */
  endDate?: string | null;
  /** True for all-day events from external providers. */
  isAllDay?: boolean;
  /** Secondary label, e.g. event participant names. Null when not applicable. */
  subtitle: string | null;
  /** Raw status string from the API: "Pending", "Completed", "Cancelled", etc. */
  status: string;
  /** User-defined entry color. Preserve exactly as received; do not normalize. */
  color: string | null;
  isCompleted: boolean;
  isOverdue: boolean;
  isReadOnly?: boolean;
  sourceLabel?: string | null;
  openInProviderUrl?: string | null;
  calendarName?: string | null;
  location?: string | null;
  /** For routines: human-readable frequency/recurrence summary. */
  recurrenceSummary?: string | null;
  /** For routines: household or member scope. */
  scope?: string | null;
  /** For tasks: ISO due date or null when not set. */
  dueDate?: string | null;
  /** For projected list items: source list id. */
  listId?: string | null;
  /** For projected list items: source list name. */
  listName?: string | null;
  /** For projected list items: local note. */
  note?: string | null;
  /** For projected list items: recurrence summary token. */
  repeat?: string | null;
  /** For projected list items: reminder instant in ISO format. */
  reminder?: string | null;
  /** For projected list items: importance marker. */
  importance?: boolean;
  /** For projected list items: optional item-level area context. */
  itemAreaId?: string | null;
  itemAreaName?: string | null;
  /** For projected list items: optional target member context. */
  targetMemberId?: string | null;
  targetMemberName?: string | null;
}

// ----------------------------------------------------------------
// Display priority — strict spec ordering
// ----------------------------------------------------------------

export const ENTRY_DISPLAY_PRIORITY: Record<CalendarEntryDisplayType, number> =
  {
    overdue: 0,
    task: 1,
    "list-important": 2,
    event: 3,
    routine: 4,
    list: 5,
    completed: 6,
  };

// ----------------------------------------------------------------
// Glyph map — single source of truth for visual grammar
//
// Spec reference (today-panel.md):
//   !      overdue task
//   □      pending task
//   ● HH:mm  event / plan (time appended inline)
//   ⟳      routine (recurring)
//   ✓      completed
// ----------------------------------------------------------------

export const ENTRY_GLYPH: Record<CalendarEntryDisplayType, string> = {
  overdue: "! □", // compound: overdue indicator + task symbol
  task: "□",
  "list-important": "☆",
  event: "●", // time is shown separately in the glyph span: ● 19:30
  routine: "⟳",
  list: "◇",
  completed: "✓",
};

// ----------------------------------------------------------------
// Normalization helpers — raw API item → CalendarEntry
//
// These helpers do NOT perform overdue detection.
// Overdue logic is Today-specific; see todayPanelHelpers.ts.
// ----------------------------------------------------------------

function isTerminalStatus(status: string): boolean {
  const s = status.toLowerCase();
  return s === "completed" || s === "cancelled";
}

/**
 * Normalize a raw event item.
 * Cancelled events map to displayType "completed" (they will not occur).
 *
 * @param participants  Optional pre-joined participant display names.
 */
export function normalizeEventItem(
  e: WeeklyGridEventItem,
  participants?: string | null,
): CalendarEntry {
  const isCancelled = e.status.toLowerCase() === "cancelled";
  return {
    id: e.eventId,
    sourceType: "event",
    displayType: isCancelled ? "completed" : "event",
    title: e.title,
    time: e.time ?? null,
    endTime: e.endTime ?? null,
    date: e.date,
    endDate: e.endDate ?? null,
    isAllDay: e.time == null,
    subtitle: participants ?? null,
    status: e.status,
    color: e.color ?? null,
    isCompleted: isCancelled,
    isOverdue: false,
    isReadOnly: e.isReadOnly ?? false,
    sourceLabel: e.providerLabel ?? null,
    openInProviderUrl: e.openInProviderUrl ?? null,
    calendarName: e.calendarName ?? null,
    location: e.location ?? null,
  };
}

/**
 * Normalize a raw task item.
 * Terminal statuses (Completed, Cancelled) map to displayType "completed".
 * No overdue logic — callers that need overdue detection should use
 * buildMemberEntries() in todayPanelHelpers.ts.
 */
export function normalizeTaskItem(t: WeeklyGridTaskItem): CalendarEntry {
  const terminal = isTerminalStatus(t.status);
  return {
    id: t.taskId,
    sourceType: "task",
    displayType: terminal ? "completed" : "task",
    title: t.title,
    time: null,
    subtitle: null,
    status: t.status,
    color: t.color ?? null,
    isCompleted: terminal,
    isOverdue: false,
    dueDate: t.dueDate ?? null,
  };
}

/**
 * Normalize a raw routine item.
 * Routines have no terminal state in the current data model.
 */
export function normalizeRoutineItem(
  r: WeeklyGridRoutineItem,
): CalendarEntry {
  return {
    id: r.routineId,
    sourceType: "routine",
    displayType: "routine",
    title: r.name,
    time: r.time ?? null,
    endTime: r.endTime ?? null,
    subtitle: null,
    status: r.kind,
    color: r.color ?? null,
    isCompleted: false,
    isOverdue: false,
    recurrenceSummary: r.frequency ?? null,
    scope: r.scope ?? null,
  };
}

/** Normalize a projected shared-list item. */
export function normalizeListItem(item: WeeklyGridListItem): CalendarEntry {
  const isCompleted = item.checked;
  const displayType: CalendarEntryDisplayType = isCompleted
    ? "completed"
    : item.importance
    ? "list-important"
    : "list";

  return {
    id: item.itemId,
    sourceType: "list-item",
    displayType,
    title: item.title,
    time: null,
    subtitle: item.listName,
    status: item.checked ? "done" : "pending",
    color: item.color ?? null,
    isCompleted,
    isOverdue: false,
    isReadOnly: true,
    sourceLabel: "List",
    listId: item.listId,
    listName: item.listName,
    note: item.note,
    dueDate: item.dueDate,
    reminder: item.reminder,
    repeat: item.repeat,
    importance: item.importance,
    itemAreaId: item.itemAreaId ?? null,
    itemAreaName: item.itemAreaName ?? null,
    targetMemberId: item.targetMemberId ?? null,
    targetMemberName: item.targetMemberName ?? null,
  };
}

/**
 * Normalize all items in a cell to CalendarEntry records.
 * Order: events → tasks → routines (source order kept within each type).
 * No priority sorting or overdue detection.
 *
 * Use this for Weekly grid cells where source order is sufficient.
 * For Today Panel ordering and overdue detection, use buildMemberEntries()
 * in todayPanelHelpers.ts.
 */
export function normalizeCellItems(cell: WeeklyGridCell): CalendarEntry[] {
  const entries: CalendarEntry[] = [];

  for (const e of cell.events ?? []) {
    const participantNames =
      e.participants?.map((p) => p.displayName).join(", ") || null;
    entries.push(normalizeEventItem(e, participantNames));
  }

  for (const t of cell.tasks ?? []) {
    entries.push(normalizeTaskItem(t));
  }

  for (const r of cell.routines ?? []) {
    entries.push(normalizeRoutineItem(r));
  }

  for (const li of cell.listItems ?? []) {
    entries.push(normalizeListItem(li));
  }

  return entries;
}

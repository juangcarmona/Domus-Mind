import type { WeeklyGridCell, WeeklyGridMember } from "../types";

// ----------------------------------------------------------------
// Today Panel - normalized view-model
// ----------------------------------------------------------------

export type TodayEntryDisplayType =
  | "overdue"
  | "task"
  | "event"
  | "routine"
  | "completed";

/**
 * A single unified entry in the Today Panel.
 *
 * All item types (events, tasks, routines) are normalised into this model
 * before rendering so that ordering and display logic lives in pure helpers,
 * not scattered through JSX.
 */
export interface TodayEntry {
  id: string;
  sourceType: "event" | "task" | "routine";
  displayType: TodayEntryDisplayType;
  title: string;
  /** HH:mm or null */
  time: string | null;
  /** Raw status string from the API */
  status: string;
  color: string | null;
  isCompleted: boolean;
  isOverdue: boolean;
}

/** Used by the rendered panel to decide what to show in each state. */
export interface TodayPanelDisplayState {
  /** Items shown in collapsed state (max 2, non-completed first; completed only when nothing else exists). */
  visibleCollapsed: TodayEntry[];
  /** How many non-completed items are hidden when collapsed (shown as +N). */
  overflowCount: number;
  /** All active (non-completed) items, for use in expanded state. */
  activeItems: TodayEntry[];
  /** Completed items, shown at the bottom of expanded state only. */
  completedItems: TodayEntry[];
  /** True when there are no relevant items at all (no actives, no completed). */
  isEmpty: boolean;
}

// ----------------------------------------------------------------
// Priority for strict ordering
// ----------------------------------------------------------------

const DISPLAY_PRIORITY: Record<TodayEntryDisplayType, number> = {
  overdue: 0,
  task: 1,
  event: 2,
  routine: 3,
  completed: 4,
};

// ----------------------------------------------------------------
// Helpers
// ----------------------------------------------------------------

function isTaskTerminal(status: string): boolean {
  const s = status.toLowerCase();
  return s === "completed" || s === "cancelled";
}

/**
 * Normalises the items in a single cell into TodayEntry records.
 *
 * NOTE: tasks in a grid cell always have dueDate === cell.date (the backend
 * query filters tasks by exact schedule date). This means tasks from the
 * selected day's own cell will never be flagged overdue here – overdue flag
 * is set when a past-day cell is explicitly passed (see buildMemberEntries).
 */
function normalizeCell(
  cell: WeeklyGridCell,
  selectedDate: string,
): TodayEntry[] {
  const entries: TodayEntry[] = [];

  for (const e of cell.events ?? []) {
    const isCancelled = e.status.toLowerCase() === "cancelled";
    entries.push({
      id: e.eventId,
      sourceType: "event",
      displayType: isCancelled ? "completed" : "event",
      title: e.title,
      time: e.time ?? null,
      status: e.status,
      color: e.color ?? null,
      isCompleted: isCancelled,
      isOverdue: false,
    });
  }

  for (const t of cell.tasks ?? []) {
    const terminal = isTaskTerminal(t.status);
    // Overdue when the task's due date is strictly before selectedDate and
    // the task has not reached a terminal state. This only fires when a past-
    // day cell is processed (within-week overdue, see buildMemberEntries).
    const isOverdue =
      !terminal && t.dueDate !== null && t.dueDate < selectedDate;
    const displayType: TodayEntryDisplayType = terminal
      ? "completed"
      : isOverdue
        ? "overdue"
        : "task";
    entries.push({
      id: t.taskId,
      sourceType: "task",
      displayType,
      title: t.title,
      time: null,
      status: t.status,
      color: t.color ?? null,
      isCompleted: terminal,
      isOverdue,
    });
  }

  for (const r of cell.routines ?? []) {
    entries.push({
      id: r.routineId,
      sourceType: "routine",
      displayType: "routine",
      title: r.name,
      time: r.time ?? null,
      status: r.kind,
      color: r.color ?? null,
      isCompleted: false,
      isOverdue: false,
    });
  }

  return entries;
}

/**
 * Sorts a mixed entry list using the strict Today Panel ordering:
 * 1. overdue  2. task  3. event  4. routine  5. completed
 *
 * Within the same priority level, events are additionally sorted by time
 * (events without a time sort after timed events). All other types preserve
 * their original relative order (stable sort).
 */
export function sortEntries(entries: TodayEntry[]): TodayEntry[] {
  return [...entries].sort((a, b) => {
    const pa = DISPLAY_PRIORITY[a.displayType];
    const pb = DISPLAY_PRIORITY[b.displayType];
    if (pa !== pb) return pa - pb;

    // Within events, sort by time (null times last)
    if (a.sourceType === "event" && b.sourceType === "event") {
      if (a.time === null && b.time === null) return 0;
      if (a.time === null) return 1;
      if (b.time === null) return -1;
      return a.time.localeCompare(b.time);
    }

    return 0; // preserve relative order for equal-priority non-events
  });
}

/**
 * Splits a sorted entry list into the pieces needed by the panel:
 * - visibleCollapsed: up to 2 non-completed items (falls back to completed
 *   if there are no active items, per spec: "may show those as the visible items")
 * - overflowCount: how many active items are hidden in collapsed state (+N)
 * - activeItems: all non-completed items (for expanded state)
 * - completedItems: completed/cancelled items (expanded state only, low emphasis)
 */
export function splitForDisplay(entries: TodayEntry[]): TodayPanelDisplayState {
  const activeItems = entries.filter((e) => e.displayType !== "completed");
  const completedItems = entries.filter((e) => e.displayType === "completed");
  const isEmpty = entries.length === 0;

  // Collapsed source: prefer active items; fall back to completed only if
  // there are no active items (spec: "if they are the only items").
  const collapsedSource = activeItems.length > 0 ? activeItems : completedItems;
  const visibleCollapsed = collapsedSource.slice(0, 2);
  const overflowCount = activeItems.length > 2 ? activeItems.length - 2 : 0;

  return { visibleCollapsed, overflowCount, activeItems, completedItems, isEmpty };
}

// ----------------------------------------------------------------
// Public entry-point functions
// ----------------------------------------------------------------

/**
 * Builds the full sorted entry list for a single member on the selected date.
 *
 * Overdue detection covers within-week past days by scanning all cells with
 * dates < selectedDate in the same grid response.
 *
 * DATA GAPS:
 * - Tasks overdue from previous weeks are NOT available in the weekly grid
 *   API (the grid covers only the requested 7-day window). A dedicated
 *   overdue-tasks query would be needed for full spec compliance.
 * - Unscheduled tasks (no due date) are never present in grid cells and
 *   therefore require a separate API endpoint. They are not shown here.
 *   TODO: add a "No date (N)" compact entry once the backend exposes them.
 */
export function buildMemberEntries(
  member: WeeklyGridMember,
  selectedDate: string,
): TodayEntry[] {
  const selectedCell = member.cells.find(
    (c) => c.date.slice(0, 10) === selectedDate,
  ) ?? { date: selectedDate, events: [], tasks: [], routines: [] };

  const todayEntries = normalizeCell(selectedCell, selectedDate);

  // Within-week overdue: collect pending tasks from earlier cells this week.
  const overdueEntries: TodayEntry[] = [];
  for (const cell of member.cells) {
    const cellDate = cell.date.slice(0, 10);
    if (cellDate >= selectedDate) continue; // only past cells
    for (const t of cell.tasks ?? []) {
      if (!isTaskTerminal(t.status)) {
        overdueEntries.push({
          id: t.taskId,
          sourceType: "task",
          displayType: "overdue",
          title: t.title,
          time: null,
          status: t.status,
          color: t.color ?? null,
          isCompleted: false,
          isOverdue: true,
        });
      }
    }
  }

  return sortEntries([...overdueEntries, ...todayEntries]);
}

/**
 * Builds the sorted entry list for the Household (shared/unassigned) row.
 *
 * Uses only the selected date's shared cell. Personal member items must not
 * appear here.
 *
 * DATA GAPS: same as buildMemberEntries (no prior-week overdue, no unscheduled).
 */
export function buildSharedEntries(
  sharedCells: WeeklyGridCell[],
  selectedDate: string,
): TodayEntry[] {
  const cell = sharedCells.find((c) => c.date.slice(0, 10) === selectedDate);
  if (!cell) return [];
  return sortEntries(normalizeCell(cell, selectedDate));
}

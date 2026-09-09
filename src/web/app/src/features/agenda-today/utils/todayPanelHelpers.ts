import type { WeeklyGridCell, WeeklyGridMember } from "../types";
import {
  type CalendarEntry,
  type CalendarEntryDisplayType,
  ENTRY_DISPLAY_PRIORITY,
  normalizeEventItem,
  normalizeListItem,
  normalizeTaskItem,
  normalizeRoutineItem,
} from "./calendarEntry";

// ----------------------------------------------------------------
// Re-exports for consumers that were using the old TodayEntry name.
// CalendarEntry is the canonical type; TodayEntry is an alias.
// ----------------------------------------------------------------

export type { CalendarEntry };
export type TodayEntry = CalendarEntry;
export type TodayEntryDisplayType = CalendarEntryDisplayType;

// ----------------------------------------------------------------
// Today Panel display state
// ----------------------------------------------------------------

/** Driving model for a member card in collapsed and expanded states. */
export interface TodayPanelDisplayState {
  /** Items shown in collapsed state (max 2, active first; completed only when they are the only items). */
  visibleCollapsed: CalendarEntry[];
  /** How many active items are hidden when collapsed (shown as +N). */
  overflowCount: number;
  /** All active (non-completed) items — full list for expanded state. */
  activeItems: CalendarEntry[];
  /** Completed/cancelled items — expanded state only, low emphasis. */
  completedItems: CalendarEntry[];
  /** True when there are no items at all. */
  isEmpty: boolean;
}

// ----------------------------------------------------------------
// Today-specific normalization (adds overdue detection)
// ----------------------------------------------------------------

function isTerminalStatus(status: string): boolean {
  const s = status.toLowerCase();
  return s === "completed" || s === "cancelled";
}

/**
 * Normalize all items in a cell into CalendarEntry records, adding within-cell
 * overdue detection for tasks whose dueDate < selectedDate.
 *
 * NOTE: tasks in a grid cell always have dueDate === cell.date (the backend
 * filters tasks by exact schedule date per day). Within-cell overdue therefore
 * fires only when a past-day cell is explicitly passed (see buildMemberEntries).
 */
function normalizeCellWithOverdue(
  cell: WeeklyGridCell,
  selectedDate: string,
): CalendarEntry[] {
  const entries: CalendarEntry[] = [];

  for (const e of cell.events ?? []) {
    const participantNames =
      e.participants?.map((p) => p.displayName).join(", ") || null;
    entries.push(normalizeEventItem(e, participantNames));
  }

  for (const t of cell.tasks ?? []) {
    const terminal = isTerminalStatus(t.status);
    const isOverdue = !terminal && t.dueDate !== null && t.dueDate < selectedDate;
    if (terminal || isOverdue) {
      // Override displayType to overdue or completed as appropriate.
      entries.push({
        ...normalizeTaskItem(t),
        displayType: isOverdue ? "overdue" : "completed",
        isOverdue,
      });
    } else {
      entries.push(normalizeTaskItem(t));
    }
  }

  for (const r of cell.routines ?? []) {
    entries.push(normalizeRoutineItem(r));
  }

  for (const li of cell.listItems ?? []) {
    entries.push(normalizeListItem(li));
  }

  return entries;
}

// ----------------------------------------------------------------
// Sorting
// ----------------------------------------------------------------

/**
 * Sort a mixed entry list using the strict Today Panel ordering:
 *   1. overdue  2. task  3. event  4. routine  5. completed
 *
 * Within events, timed events sort before untimed. All other types keep
 * their original relative order (stable sort on equal priority).
 */
export function sortEntries(entries: CalendarEntry[]): CalendarEntry[] {
  return [...entries].sort((a, b) => {
    const pa = ENTRY_DISPLAY_PRIORITY[a.displayType];
    const pb = ENTRY_DISPLAY_PRIORITY[b.displayType];
    if (pa !== pb) return pa - pb;

    if (a.sourceType === "event" && b.sourceType === "event") {
      if (a.time === null && b.time === null) return 0;
      if (a.time === null) return 1;
      if (b.time === null) return -1;
      return a.time.localeCompare(b.time);
    }

    return 0;
  });
}

// ----------------------------------------------------------------
// Collapse/expand slicing
// ----------------------------------------------------------------

/**
 * Split a sorted entry list into the display pieces needed by a member card.
 *
 * Collapsed source prefers active items; falls back to completed only when
 * there are no active items (spec: "may show those as the visible items").
 */
export function splitForDisplay(
  entries: CalendarEntry[],
): TodayPanelDisplayState {
  const activeItems = entries.filter((e) => e.displayType !== "completed");
  const completedItems = entries.filter((e) => e.displayType === "completed");
  const isEmpty = entries.length === 0;

  const collapsedSource = activeItems.length > 0 ? activeItems : completedItems;
  const visibleCollapsed = collapsedSource.slice(0, 2);
  const overflowCount = activeItems.length > 2 ? activeItems.length - 2 : 0;

  return { visibleCollapsed, overflowCount, activeItems, completedItems, isEmpty };
}

// ----------------------------------------------------------------
// Public entry-point functions
// ----------------------------------------------------------------

/**
 * Build the full sorted entry list for a single member on the selected date.
 *
 * Overdue detection includes within-week past days: any pending tasks from
 * earlier cells in the same grid response are surfaced as overdue entries.
 *
 * DATA GAPS:
 * - Tasks overdue from previous weeks are NOT present in the grid (the API
 *   covers only the requested 7-day window). A dedicated overdue-tasks query
 *   would be needed for full spec compliance.
 *   TODO: add prior-week overdue once backend exposes an overdue query.
 * - Unscheduled tasks (no due date) are not in any grid cell and require a
 *   separate API endpoint. They are not shown here.
 *   TODO: add "No date (N)" compact entry once backend exposes them.
 */
export function buildMemberEntries(
  member: WeeklyGridMember,
  selectedDate: string,
  sharedCellForDate?: WeeklyGridCell | null,
): CalendarEntry[] {
  const selectedCell = member.cells.find(
    (c) => c.date.slice(0, 10) === selectedDate,
  ) ?? { date: selectedDate, events: [], tasks: [], routines: [], listItems: [] };

  const todayEntries = normalizeCellWithOverdue(selectedCell, selectedDate);

  if (sharedCellForDate) {
    for (const li of sharedCellForDate.listItems ?? []) {
      todayEntries.push(normalizeListItem(li));
    }
  }

  // Within-week overdue: pending tasks from earlier cells in the same grid.
  const overdueEntries: CalendarEntry[] = [];
  for (const cell of member.cells) {
    const cellDate = cell.date.slice(0, 10);
    if (cellDate >= selectedDate) continue;
    for (const t of cell.tasks ?? []) {
      if (!isTerminalStatus(t.status)) {
        overdueEntries.push({
          ...normalizeTaskItem(t),
          displayType: "overdue",
          isOverdue: true,
        });
      }
    }
  }

  return sortEntries([...overdueEntries, ...todayEntries]);
}

/**
 * Build the sorted entry list for the Household (shared/unassigned) row.
 *
 * Uses only the selected date's shared cell. Personal member items must not
 * appear here — callers must only pass the grid's sharedCells array.
 *
 * DATA GAPS: same as buildMemberEntries (no prior-week overdue, no unscheduled).
 */
export function buildSharedEntries(
  sharedCells: WeeklyGridCell[],
  selectedDate: string,
): CalendarEntry[] {
  const cell = sharedCells.find((c) => c.date.slice(0, 10) === selectedDate);
  if (!cell) return [];
  return sortEntries(normalizeCellWithOverdue(cell, selectedDate));
}


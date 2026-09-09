import { describe, it, expect } from "vitest";
import type { WeeklyGridMember, WeeklyGridCell } from "../types";
import type { CalendarEntry } from "./calendarEntry";
import {
  sortEntries,
  splitForDisplay,
  buildMemberEntries,
  buildSharedEntries,
} from "./todayPanelHelpers";

// ----------------------------------------------------------------
// Factory helpers
// ----------------------------------------------------------------

function makeTask(
  overrides: Partial<{
    taskId: string;
    title: string;
    dueDate: string | null;
    status: string;
    color: string;
  }> = {},
) {
  return {
    taskId: overrides.taskId ?? "t1",
    title: overrides.title ?? "Task",
    dueDate: overrides.dueDate ?? "2026-03-27",
    status: overrides.status ?? "Pending",
    color: overrides.color ?? "#ccc",
  };
}

function makeEvent(
  overrides: Partial<{
    eventId: string;
    title: string;
    date: string;
    time: string | null;
    endDate: string | null;
    endTime: string | null;
    status: string;
    color: string;
    participants: [];
  }> = {},
) {
  return {
    eventId: overrides.eventId ?? "e1",
    title: overrides.title ?? "Event",
    date: overrides.date ?? "2026-03-27",
    time: overrides.time ?? null,
    endDate: overrides.endDate ?? null,
    endTime: overrides.endTime ?? null,
    status: overrides.status ?? "Scheduled",
    color: overrides.color ?? "#aaa",
    participants: overrides.participants ?? [],
  };
}

function makeRoutine(
  overrides: Partial<{
    routineId: string;
    name: string;
    kind: string;
    color: string | null;
    frequency: string;
    time: string | null;
    endTime: string | null;
    scope: string;
  }> = {},
) {
  return {
    routineId: overrides.routineId ?? "r1",
    name: overrides.name ?? "Routine",
    kind: overrides.kind ?? "Weekly",
    color: overrides.color ?? null,
    frequency: overrides.frequency ?? "Weekly",
    time: overrides.time ?? null,
    endTime: overrides.endTime ?? null,
    scope: overrides.scope ?? "Household",
  };
}

function makeCell(
  date: string,
  overrides: Partial<WeeklyGridCell> = {},
): WeeklyGridCell {
  return {
    date,
    events: [],
    tasks: [],
    routines: [],
    listItems: [],
    ...overrides,
  };
}

function makeMember(
  cells: WeeklyGridCell[],
  overrides: Partial<Omit<WeeklyGridMember, "cells">> = {},
): WeeklyGridMember {
  return {
    memberId: overrides.memberId ?? "m1",
    name: overrides.name ?? "Alice",
    role: overrides.role ?? "Adult",
    cells,
  };
}

const TODAY = "2026-03-27";

/**
 * Convenience factory for CalendarEntry fixtures.
 * Provide only the fields that matter for the test; defaults fill the rest.
 */
function makeEntry(
  overrides: Partial<CalendarEntry> &
    Pick<CalendarEntry, "id" | "displayType">,
): CalendarEntry {
  const terminal =
    overrides.displayType === "completed" ||
    overrides.status?.toLowerCase() === "completed" ||
    overrides.status?.toLowerCase() === "cancelled";
  return {
    sourceType: "task",
    title: "Item",
    time: null,
    subtitle: null,
    status: "Pending",
    color: null,
    isCompleted: terminal,
    isOverdue: overrides.displayType === "overdue",
    ...overrides,
  };
}

// ----------------------------------------------------------------
// sortEntries
// ----------------------------------------------------------------

describe("sortEntries", () => {
  it("orders overdue before task before event before routine before completed", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "5", displayType: "completed", status: "Completed" }),
      makeEntry({ id: "4", sourceType: "routine", displayType: "routine", status: "Active" }),
      makeEntry({ id: "3", sourceType: "event", displayType: "event", status: "Scheduled" }),
      makeEntry({ id: "2", displayType: "task" }),
      makeEntry({ id: "1", displayType: "overdue" }),
    ];

    const sorted = sortEntries(entries);
    expect(sorted.map((e) => e.displayType)).toEqual([
      "overdue",
      "task",
      "event",
      "routine",
      "completed",
    ]);
  });

  it("sorts events by time within the same priority", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "3", sourceType: "event", displayType: "event", time: "20:00", status: "Scheduled" }),
      makeEntry({ id: "1", sourceType: "event", displayType: "event", time: null, status: "Scheduled" }),
      makeEntry({ id: "2", sourceType: "event", displayType: "event", time: "09:00", status: "Scheduled" }),
    ];

    const sorted = sortEntries(entries);
    expect(sorted.map((e) => e.time)).toEqual(["09:00", "20:00", null]);
  });

  it("preserves relative order for same-priority non-event items", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "b", displayType: "task", title: "Second" }),
      makeEntry({ id: "a", displayType: "task", title: "First" }),
    ];
    const sorted = sortEntries(entries);
    expect(sorted.map((e) => e.id)).toEqual(["b", "a"]);
  });
});

// ----------------------------------------------------------------
// splitForDisplay
// ----------------------------------------------------------------

describe("splitForDisplay", () => {
  it("isEmpty is true when no entries", () => {
    const state = splitForDisplay([]);
    expect(state.isEmpty).toBe(true);
    expect(state.visibleCollapsed).toHaveLength(0);
    expect(state.overflowCount).toBe(0);
  });

  it("shows max 2 items in collapsed state", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "1", displayType: "task" }),
      makeEntry({ id: "2", displayType: "task" }),
      makeEntry({ id: "3", displayType: "task" }),
    ];
    const state = splitForDisplay(entries);
    expect(state.visibleCollapsed).toHaveLength(2);
    expect(state.overflowCount).toBe(1);
  });

  it("+N counts hidden active items only (completed items are not counted in overflow)", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "1", displayType: "task" }),
      makeEntry({ id: "2", displayType: "task" }),
      makeEntry({ id: "3", displayType: "completed", status: "Completed" }),
    ];
    const state = splitForDisplay(entries);
    // 2 active items visible, 0 overflow (2 fit exactly), 1 completed hidden in collapsed
    expect(state.visibleCollapsed).toHaveLength(2);
    expect(state.overflowCount).toBe(0);
    expect(state.completedItems).toHaveLength(1);
  });

  it("completed items are separated into completedItems array", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "1", displayType: "task" }),
      makeEntry({ id: "2", displayType: "completed", status: "Completed" }),
    ];
    const state = splitForDisplay(entries);
    expect(state.activeItems).toHaveLength(1);
    expect(state.completedItems).toHaveLength(1);
    expect(state.completedItems[0].id).toBe("2");
  });

  it("completed items appear in visibleCollapsed when they are the only items", () => {
    const entries: CalendarEntry[] = [
      makeEntry({ id: "c1", displayType: "completed", status: "Completed" }),
      makeEntry({ id: "c2", displayType: "completed", status: "Completed" }),
    ];
    const state = splitForDisplay(entries);
    expect(state.activeItems).toHaveLength(0);
    expect(state.completedItems).toHaveLength(2);
    // Falls back to completed as collapsed source
    expect(state.visibleCollapsed).toHaveLength(2);
    expect(state.visibleCollapsed[0].displayType).toBe("completed");
  });

  it("overflow is 0 when there are exactly 2 or fewer active items", () => {
    const two: CalendarEntry[] = [
      makeEntry({ id: "1", displayType: "task" }),
      makeEntry({ id: "2", displayType: "task" }),
    ];
    expect(splitForDisplay(two).overflowCount).toBe(0);

    const one: CalendarEntry[] = [
      makeEntry({ id: "1", displayType: "task" }),
    ];
    expect(splitForDisplay(one).overflowCount).toBe(0);
  });
});

// ----------------------------------------------------------------
// buildMemberEntries
// ----------------------------------------------------------------

describe("buildMemberEntries", () => {
  it("returns empty array when member has no cells", () => {
    const member = makeMember([]);
    expect(buildMemberEntries(member, TODAY)).toHaveLength(0);
  });

  it("normalises tasks and events into a mixed sorted list", () => {
    const cell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "t1", title: "Task A" })],
      events: [makeEvent({ eventId: "e1", title: "Event A", time: "15:00" })],
    });
    const member = makeMember([cell]);
    const entries = buildMemberEntries(member, TODAY);
    // task before event in strict ordering
    expect(entries[0].displayType).toBe("task");
    expect(entries[1].displayType).toBe("event");
  });

  it("marks Completed tasks as displayType:completed", () => {
    const cell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "t1", status: "Completed" })],
    });
    const member = makeMember([cell]);
    const entries = buildMemberEntries(member, TODAY);
    expect(entries[0].displayType).toBe("completed");
    expect(entries[0].isCompleted).toBe(true);
  });

  it("marks Cancelled tasks as displayType:completed", () => {
    const cell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "t1", status: "Cancelled" })],
    });
    const member = makeMember([cell]);
    const entries = buildMemberEntries(member, TODAY);
    expect(entries[0].displayType).toBe("completed");
  });

  it("marks cancelled events as displayType:completed", () => {
    const cell = makeCell(TODAY, {
      events: [makeEvent({ eventId: "e1", status: "Cancelled" })],
    });
    const member = makeMember([cell]);
    const entries = buildMemberEntries(member, TODAY);
    expect(entries[0].displayType).toBe("completed");
    expect(entries[0].isCompleted).toBe(true);
  });

  it("surfaces within-week overdue tasks from past day cells", () => {
    const pastDate = "2026-03-25"; // 2 days before TODAY
    const pastCell = makeCell(pastDate, {
      tasks: [makeTask({ taskId: "t-past", dueDate: pastDate, status: "Pending", title: "Past task" })],
    });
    const todayCell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "t-today", title: "Today task" })],
    });
    const member = makeMember([pastCell, todayCell]);
    const entries = buildMemberEntries(member, TODAY);

    const overdueEntry = entries.find((e) => e.id === "t-past");
    expect(overdueEntry).toBeDefined();
    expect(overdueEntry?.displayType).toBe("overdue");
    expect(overdueEntry?.isOverdue).toBe(true);
  });

  it("does NOT surface past-cell completed tasks (they are historical, not today's work)", () => {
    const pastDate = "2026-03-25";
    const pastCell = makeCell(pastDate, {
      tasks: [makeTask({ taskId: "t-done", dueDate: pastDate, status: "Completed" })],
    });
    const member = makeMember([pastCell]);
    const entries = buildMemberEntries(member, TODAY);

    // Completed tasks from previous days are not relevant to today's panel.
    // Only overdue *pending* tasks from past cells are shown.
    const entry = entries.find((e) => e.id === "t-done");
    expect(entry).toBeUndefined();
  });

  it("places overdue entries before regular tasks in the sorted result", () => {
    const pastDate = "2026-03-25";
    const pastCell = makeCell(pastDate, {
      tasks: [makeTask({ taskId: "overdue", dueDate: pastDate, status: "Pending" })],
    });
    const todayCell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "today-task", title: "Today" })],
    });
    const member = makeMember([pastCell, todayCell]);
    const entries = buildMemberEntries(member, TODAY);

    expect(entries[0].id).toBe("overdue");
    expect(entries[0].displayType).toBe("overdue");
    expect(entries[1].id).toBe("today-task");
  });

  it("returns empty array when cell is missing for selectedDate", () => {
    const cell = makeCell("2026-03-26"); // yesterday, no items
    const member = makeMember([cell]);
    // No cell for TODAY - should still return empty without error
    const entries = buildMemberEntries(member, TODAY);
    expect(entries).toHaveLength(0);
  });
});

// ----------------------------------------------------------------
// buildSharedEntries
// ----------------------------------------------------------------

describe("buildSharedEntries", () => {
  it("returns empty when no shared cells exist", () => {
    expect(buildSharedEntries([], TODAY)).toHaveLength(0);
  });

  it("returns empty when shared cell for selectedDate contains no items", () => {
    const cell = makeCell(TODAY);
    expect(buildSharedEntries([cell], TODAY)).toHaveLength(0);
  });

  it("returns normalised entries from the shared cell for selectedDate", () => {
    const cell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "shared-t" })],
      events: [makeEvent({ eventId: "shared-e" })],
    });
    const entries = buildSharedEntries([cell], TODAY);
    // task before event
    expect(entries[0].sourceType).toBe("task");
    expect(entries[1].sourceType).toBe("event");
  });

  it("does NOT include items from a past or future shared cell", () => {
    const yesterday = makeCell("2026-03-26", {
      tasks: [makeTask({ taskId: "past" })],
    });
    const future = makeCell("2026-03-28", {
      tasks: [makeTask({ taskId: "future" })],
    });
    const today = makeCell(TODAY);
    const entries = buildSharedEntries([yesterday, today, future], TODAY);
    // Only today's cell is used - which is empty
    expect(entries).toHaveLength(0);
  });

  it("personal items must not come from shared entries (only the provided shared cells are used)", () => {
    // buildSharedEntries only uses sharedCells - callers must not pass member cells.
    // This test documents the contract: the function itself is safe because it only scans
    // the cells array given to it.
    const sharedCell = makeCell(TODAY, {
      tasks: [makeTask({ taskId: "s1", title: "Shared" })],
    });
    const entries = buildSharedEntries([sharedCell], TODAY);
    expect(entries).toHaveLength(1);
    expect(entries[0].id).toBe("s1");
  });
});

// ----------------------------------------------------------------
// Graceful handling of missing/null data
// ----------------------------------------------------------------

describe("graceful handling of missing data", () => {
  it("handles cells with undefined arrays without throwing", () => {
    const cell = { date: TODAY } as unknown as WeeklyGridCell;
    const member = makeMember([cell]);
    expect(() => buildMemberEntries(member, TODAY)).not.toThrow();
  });

  it("handles tasks with null dueDate without throwing", () => {
    const cell = makeCell(TODAY, {
      tasks: [makeTask({ dueDate: null })],
    });
    const member = makeMember([cell]);
    const entries = buildMemberEntries(member, TODAY);
    expect(entries[0].displayType).toBe("task");
  });

  it("handles routines with null color without throwing", () => {
    const cell = makeCell(TODAY, {
      routines: [makeRoutine({ color: null })],
    });
    const member = makeMember([cell]);
    const entries = buildMemberEntries(member, TODAY);
    expect(entries[0].color).toBeNull();
  });

  it("splitForDisplay handles empty entries without overflow or errors", () => {
    const result = splitForDisplay([]);
    expect(result.overflowCount).toBe(0);
    expect(result.isEmpty).toBe(true);
    expect(result.visibleCollapsed).toHaveLength(0);
    expect(result.activeItems).toHaveLength(0);
    expect(result.completedItems).toHaveLength(0);
  });
});

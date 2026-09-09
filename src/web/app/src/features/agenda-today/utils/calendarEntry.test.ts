import { describe, it, expect } from "vitest";
import {
  normalizeEventItem,
  normalizeTaskItem,
  normalizeRoutineItem,
  normalizeCellItems,
  ENTRY_GLYPH,
  ENTRY_DISPLAY_PRIORITY,
} from "./calendarEntry";
import type { WeeklyGridCell } from "../types";

// ----------------------------------------------------------------
// ENTRY_GLYPH — visual grammar contract
// ----------------------------------------------------------------

describe("ENTRY_GLYPH", () => {
  it("defines the expected spec-mandated glyphs", () => {
    expect(ENTRY_GLYPH.overdue).toBe("! □");
    expect(ENTRY_GLYPH.task).toBe("□");
    expect(ENTRY_GLYPH.event).toBe("●");
    expect(ENTRY_GLYPH.routine).toBe("⟳");
    expect(ENTRY_GLYPH.completed).toBe("✓");
  });
});

// ----------------------------------------------------------------
// ENTRY_DISPLAY_PRIORITY
// ----------------------------------------------------------------

describe("ENTRY_DISPLAY_PRIORITY", () => {
  it("has overdue < task < event < routine < completed", () => {
    expect(ENTRY_DISPLAY_PRIORITY.overdue).toBeLessThan(ENTRY_DISPLAY_PRIORITY.task);
    expect(ENTRY_DISPLAY_PRIORITY.task).toBeLessThan(ENTRY_DISPLAY_PRIORITY.event);
    expect(ENTRY_DISPLAY_PRIORITY.event).toBeLessThan(ENTRY_DISPLAY_PRIORITY.routine);
    expect(ENTRY_DISPLAY_PRIORITY.routine).toBeLessThan(ENTRY_DISPLAY_PRIORITY.completed);
  });
});

// ----------------------------------------------------------------
// normalizeEventItem
// ----------------------------------------------------------------

const baseEvent = {
  eventId: "e1",
  title: "Dentist",
  date: "2026-03-27",
  time: "10:00",
  endDate: null,
  endTime: null,
  status: "Scheduled",
  color: "#4f46e5",
  participants: [{ memberId: "m1", displayName: "Alice" }],
};

describe("normalizeEventItem", () => {
  it("maps a scheduled event to displayType:event", () => {
    const entry = normalizeEventItem(baseEvent);
    expect(entry.displayType).toBe("event");
    expect(entry.isCompleted).toBe(false);
    expect(entry.isOverdue).toBe(false);
  });

  it("preserves id, title, time, and color exactly", () => {
    const entry = normalizeEventItem(baseEvent);
    expect(entry.id).toBe("e1");
    expect(entry.title).toBe("Dentist");
    expect(entry.time).toBe("10:00");
    expect(entry.color).toBe("#4f46e5");
  });

  it("joins participant names into subtitle", () => {
    const event = {
      ...baseEvent,
      participants: [
        { memberId: "m1", displayName: "Alice" },
        { memberId: "m2", displayName: "Bob" },
      ],
    };
    const entry = normalizeEventItem(event, "Alice, Bob");
    expect(entry.subtitle).toBe("Alice, Bob");
  });

  it("maps a Cancelled event to displayType:completed", () => {
    const entry = normalizeEventItem({ ...baseEvent, status: "Cancelled" });
    expect(entry.displayType).toBe("completed");
    expect(entry.isCompleted).toBe(true);
  });

  it("uses null subtitle when not provided", () => {
    const entry = normalizeEventItem(baseEvent);
    expect(entry.subtitle).toBeNull();
  });

  it("uses null color when event has no color (edge case)", () => {
    const entry = normalizeEventItem({ ...baseEvent, color: undefined as unknown as string });
    expect(entry.color).toBeNull();
  });
});

// ----------------------------------------------------------------
// normalizeTaskItem
// ----------------------------------------------------------------

const baseTask = {
  taskId: "t1",
  title: "Buy milk",
  dueDate: "2026-03-27",
  status: "Pending",
  color: "#22c55e",
};

describe("normalizeTaskItem", () => {
  it("maps a Pending task to displayType:task", () => {
    const entry = normalizeTaskItem(baseTask);
    expect(entry.displayType).toBe("task");
    expect(entry.isCompleted).toBe(false);
    expect(entry.isOverdue).toBe(false);
  });

  it("preserves id, title, and color exactly", () => {
    const entry = normalizeTaskItem(baseTask);
    expect(entry.id).toBe("t1");
    expect(entry.title).toBe("Buy milk");
    expect(entry.color).toBe("#22c55e");
  });

  it("time is always null (tasks have no time in current model)", () => {
    const entry = normalizeTaskItem(baseTask);
    expect(entry.time).toBeNull();
  });

  it("subtitle is always null for tasks", () => {
    const entry = normalizeTaskItem(baseTask);
    expect(entry.subtitle).toBeNull();
  });

  it("maps a Completed task to displayType:completed", () => {
    const entry = normalizeTaskItem({ ...baseTask, status: "Completed" });
    expect(entry.displayType).toBe("completed");
    expect(entry.isCompleted).toBe(true);
  });

  it("maps a Cancelled task to displayType:completed", () => {
    const entry = normalizeTaskItem({ ...baseTask, status: "Cancelled" });
    expect(entry.displayType).toBe("completed");
    expect(entry.isCompleted).toBe(true);
  });

  it("does NOT set isOverdue — overdue detection is Today-specific", () => {
    // A task with a past dueDate should not be flagged overdue by the shared normalizer.
    const entry = normalizeTaskItem({ ...baseTask, dueDate: "2020-01-01" });
    expect(entry.isOverdue).toBe(false);
    expect(entry.displayType).toBe("task");
  });
});

// ----------------------------------------------------------------
// normalizeRoutineItem
// ----------------------------------------------------------------

const baseRoutine = {
  routineId: "r1",
  name: "Daily walk",
  kind: "Daily",
  color: "#f59e0b",
  frequency: "Daily",
  time: "07:00",
  endTime: null,
  scope: "Member",
};

describe("normalizeRoutineItem", () => {
  it("maps a routine to displayType:routine", () => {
    const entry = normalizeRoutineItem(baseRoutine);
    expect(entry.displayType).toBe("routine");
    expect(entry.isCompleted).toBe(false);
    expect(entry.isOverdue).toBe(false);
  });

  it("preserves id, title, time, and color", () => {
    const entry = normalizeRoutineItem(baseRoutine);
    expect(entry.id).toBe("r1");
    expect(entry.title).toBe("Daily walk");
    expect(entry.time).toBe("07:00");
    expect(entry.color).toBe("#f59e0b");
  });

  it("handles null color without throwing", () => {
    const entry = normalizeRoutineItem({ ...baseRoutine, color: null });
    expect(entry.color).toBeNull();
  });

  it("maps kind to status field", () => {
    const entry = normalizeRoutineItem(baseRoutine);
    expect(entry.status).toBe("Daily");
  });

  it("propagates endTime when present", () => {
    const entry = normalizeRoutineItem({ ...baseRoutine, endTime: "08:00" });
    expect(entry.endTime).toBe("08:00");
    expect(entry.time).toBe("07:00");
  });

  it("endTime is null when not provided", () => {
    const entry = normalizeRoutineItem({ ...baseRoutine, endTime: null });
    expect(entry.endTime).toBeNull();
  });
});

// ----------------------------------------------------------------
// normalizeCellItems
// ----------------------------------------------------------------

const makeCell = (overrides: Partial<WeeklyGridCell> = {}): WeeklyGridCell => ({
  date: "2026-03-27",
  events: [],
  tasks: [],
  routines: [],
  listItems: [],
  ...overrides,
});

describe("normalizeCellItems", () => {
  it("returns empty array for an empty cell", () => {
    expect(normalizeCellItems(makeCell())).toHaveLength(0);
  });

  it("normalizes events before tasks before routines (source order within types)", () => {
    const cell = makeCell({
      events: [baseEvent],
      tasks: [baseTask],
      routines: [baseRoutine],
    });
    const entries = normalizeCellItems(cell);
    expect(entries[0].sourceType).toBe("event");
    expect(entries[1].sourceType).toBe("task");
    expect(entries[2].sourceType).toBe("routine");
  });

  it("includes completed items (no status filtering)", () => {
    const cell = makeCell({
      tasks: [
        { ...baseTask, taskId: "done", status: "Completed" },
        { ...baseTask, taskId: "pending" },
      ],
    });
    const entries = normalizeCellItems(cell);
    expect(entries).toHaveLength(2);
    const done = entries.find((e) => e.id === "done");
    expect(done?.displayType).toBe("completed");
  });

  it("does NOT perform overdue detection (no cross-cell scanning)", () => {
    // A cell with a task dated yesterday should not be flagged overdue by normalizeCellItems.
    const cell = makeCell({
      date: "2026-03-26",
      tasks: [{ ...baseTask, dueDate: "2026-03-26", status: "Pending" }],
    });
    const entries = normalizeCellItems(cell);
    expect(entries[0].isOverdue).toBe(false);
    expect(entries[0].displayType).toBe("task");
  });

  it("joins participant names from events into subtitle", () => {
    const eventWithParticipants = {
      ...baseEvent,
      participants: [
        { memberId: "m1", displayName: "Alice" },
        { memberId: "m2", displayName: "Bob" },
      ],
    };
    const cell = makeCell({ events: [eventWithParticipants] });
    const entries = normalizeCellItems(cell);
    expect(entries[0].subtitle).toBe("Alice, Bob");
  });

  it("sets subtitle to null for events with no participants", () => {
    const eventNoParticipants = { ...baseEvent, participants: [] };
    const cell = makeCell({ events: [eventNoParticipants] });
    const entries = normalizeCellItems(cell);
    expect(entries[0].subtitle).toBeNull();
  });

  it("handles cells with undefined arrays without throwing", () => {
    const cell = { date: "2026-03-27" } as unknown as WeeklyGridCell;
    expect(() => normalizeCellItems(cell)).not.toThrow();
    expect(normalizeCellItems(cell)).toHaveLength(0);
  });
});

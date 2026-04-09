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

// ----------------------------------------------------------------
// Imported / read-only event contract
//
// These tests guard the specific contract that makes imported Outlook
// events work correctly everywhere in the UI:
//   - isReadOnly must survive normalization so AgendaPage.handleItemClick
//     can gate the edit path.
//   - sourceLabel/openInProviderUrl must be threaded through so the
//     ImportedEventDetail panel can display them.
//   - The id used for click lookup must round-trip: normalizeCellItems
//     must store the exact eventId sent by the backend so the corpus
//     search in handleItemClick can find the entry by id.
// ----------------------------------------------------------------

const importedEvent = {
  eventId: "d4f3eed1-0000-0000-0000-000000000001",
  title: "Team Stand-up",
  date: "2026-04-10",
  time: "09:30",
  endDate: "2026-04-10",
  endTime: "09:45",
  status: "confirmed",
  color: "#64748B",
  participants: [],
  isReadOnly: true,
  source: "external_calendar",
  providerLabel: "Outlook",
  openInProviderUrl: "https://outlook.office.com/calendar/item/1",
  location: "Teams call",
};

describe("normalizeEventItem — imported / read-only events", () => {
  it("preserves isReadOnly:true from the source event", () => {
    const entry = normalizeEventItem(importedEvent);
    expect(entry.isReadOnly).toBe(true);
  });

  it("preserves isReadOnly:false for native editable events", () => {
    const entry = normalizeEventItem({ ...baseEvent, isReadOnly: false });
    expect(entry.isReadOnly).toBe(false);
  });

  it("defaults isReadOnly to false when the field is absent", () => {
    // WeeklyGridEventItem.isReadOnly is optional; native events omit it.
    const entry = normalizeEventItem({ ...baseEvent });
    expect(entry.isReadOnly).toBe(false);
  });

  it("preserves sourceLabel from providerLabel", () => {
    const entry = normalizeEventItem(importedEvent);
    expect(entry.sourceLabel).toBe("Outlook");
  });

  it("preserves openInProviderUrl", () => {
    const entry = normalizeEventItem(importedEvent);
    expect(entry.openInProviderUrl).toBe("https://outlook.office.com/calendar/item/1");
  });

  it("preserves location", () => {
    const entry = normalizeEventItem(importedEvent);
    expect(entry.location).toBe("Teams call");
  });

  it("round-trips the eventId as entry.id for click lookup", () => {
    // handleItemClick in AgendaPage searches by (id, sourceType).
    // The id stored in CalendarEntry must equal the eventId from the raw item.
    const entry = normalizeEventItem(importedEvent);
    expect(entry.id).toBe(importedEvent.eventId);
    expect(entry.sourceType).toBe("event");
  });
});

describe("normalizeCellItems — imported event in cell", () => {
  it("imported event in cell preserves isReadOnly through cell normalization", () => {
    const cell = makeCell({ events: [importedEvent] });
    const entries = normalizeCellItems(cell);
    expect(entries).toHaveLength(1);
    expect(entries[0].isReadOnly).toBe(true);
  });

  it("imported event id is findable in cell corpus by eventId", () => {
    const cell = makeCell({ events: [importedEvent] });
    const entries = normalizeCellItems(cell);
    const found = entries.find(
      (e) => e.id === importedEvent.eventId && e.sourceType === "event",
    );
    expect(found).toBeDefined();
    expect(found?.isReadOnly).toBe(true);
  });

  it("mixed cell: imported event does not affect native event isReadOnly", () => {
    const cell = makeCell({ events: [baseEvent, importedEvent] });
    const entries = normalizeCellItems(cell);
    const native = entries.find((e) => e.id === baseEvent.eventId);
    const imported = entries.find((e) => e.id === importedEvent.eventId);
    expect(native?.isReadOnly).toBe(false);
    expect(imported?.isReadOnly).toBe(true);
  });
});

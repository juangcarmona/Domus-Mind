# Shared List Item — Capability Model and Projection Rules

Status: Transitional
Canonical upstream: 00_product/surfaces/lists.md
Do not use this document to override canonical product docs/specs

## Purpose

The List Item model defines the structure and capabilities of items within lists.

---

## Purpose

This document locks the item capability model before implementation.

It defines:

- the canonical item state shape
- allowed field combinations
- invalid states
- state transitions
- deterministic projection rules into Agenda
- ownership of each behavior

No ambiguity is permitted here. If something is not defined, it is not implemented.

---

## Canonical Item State Shape

```
SharedListItem {
  id              : SharedListItemId       (required, stable)
  listId          : SharedListId           (required, immutable)
  name            : SharedListItemName     (required, non-empty)
  checked         : bool                   (required, default: false)
  quantity        : string?                (optional)
  note            : string?                (optional)
  order           : int                    (required, unique within list)
  importance      : bool                   (optional, default: false)
  dueDate         : DateOnly?              (optional)
  reminder        : DateTimeOffset?        (optional)
  repeat          : RepeatRule?            (optional)
  updatedAtUtc    : DateTimeOffset         (required, updated on any change)
}
```

---

## Capability Groups

### Group 1 — Base (always present)

| Field    | Type   | Rules                        |
| -------- | ------ | ---------------------------- |
| name     | string | non-empty                    |
| checked  | bool   | default false                |

### Group 2 — Extended Base (optional, always allowed)

| Field    | Type    | Rules                                              |
| -------- | ------- | -------------------------------------------------- |
| quantity | string? | may be set or cleared at any time                 |
| note     | string? | may be set or cleared at any time                 |

### Group 3 — Importance (optional)

| Field      | Type | Rules                                |
| ---------- | ---- | ------------------------------------ |
| importance | bool | independently optional; default false |

Setting `importance = true` marks the item as starred.
Setting `importance = false` removes the star.
Importance does not interact with and is not required by any temporal field.

### Group 4 — Temporal (optional, conditional)

| Field    | Type             | Rules                                   |
| -------- | ---------------- | --------------------------------------- |
| dueDate  | DateOnly?        | optional; if set, enables Agenda projection by date |
| reminder | DateTimeOffset?  | optional; if set, enables Agenda projection by reminder time |
| repeat   | RepeatRule?      | optional; repeat may be set independently of dueDate; defines a recurrence schedule that is itself an Agenda projection anchor |

---

## Invariants

1. `name` must be non-empty at all times.
2. `repeat` may be set independently of `dueDate`. Repeat is itself a temporal anchor. When both are set, `dueDate` acts as the anchor date for the first (or current) recurrence.
3. If `dueDate` is cleared while `repeat` is set, the item remains Agenda-eligible via the `repeat` rule alone.
4. `reminder` may be set independently of `dueDate`, using an absolute datetime.
5. `importance` is a binary flag. It is never a score or a ranking system.
6. `checked` state does not affect validity of any capability field.
7. A checked item with temporal fields remains valid. Temporal fields are not cleared by toggle.
8. Assignment is not a field on SharedListItem. It must never be introduced.
9. Status systems (beyond checked/unchecked) are not a field on SharedListItem.

---

## Valid State Combinations

```
✓  name only
✓  name + quantity
✓  name + note
✓  name + importance
✓  name + dueDate
✓  name + reminder (standalone, absolute)
✓  name + repeat (standalone; repeat defines its own recurrence schedule)
✓  name + dueDate + reminder
✓  name + dueDate + repeat
✓  name + reminder + repeat
✓  name + dueDate + reminder + repeat
✓  name + importance + dueDate + reminder + repeat
```

All temporal fields are independently optional.
No temporal field requires another as a prerequisite.

---

## State Transitions

### Setting temporal fields

```
SetSharedListItemTemporal(dueDate?, reminder?, repeat?)
  → dueDate set OR reminder set OR repeat set → item becomes Agenda-eligible
  → emits: SharedListItemScheduled (if item transitions from non-temporal to temporal)
  → emits: SharedListItemUpdated (if item was already temporal and fields change)
```

### Clearing temporal fields

```
ClearSharedListItemTemporal
  → clears dueDate, reminder, repeat (all three, atomically)
  → item is removed from Agenda projection
  → emits: SharedListItemScheduled (to signal projection invalidation)
```

Rationale for reusing `SharedListItemScheduled` on clear: consumers watching this event know to recompute the projection. A separate `SharedListItemUnscheduled` event is an option but adds complexity without benefit in V1.

### Toggling checked state

```
ToggleSharedListItem
  → checked: false → true
  → checked: true → false
  → does NOT modify importance, temporal fields, or any other capability
  → checked item with temporal fields remains Agenda-eligible (appears de-emphasized)
```

### Setting importance

```
SetSharedListItemImportance(importance: bool)
  → sets or clears the importance flag
  → does NOT affect temporal fields or checked state
  → emits: SharedListItemImportanceSet
```

---

## Projection Rules (Deterministic)

A SharedListItem projects into the Agenda surface when:

```
CONDITION A: dueDate is set AND dueDate falls within the requested date window
OR
CONDITION B: reminder is set AND reminder datetime falls within the requested date window
OR
CONDITION C: repeat is set AND the repeat rule produces an occurrence within the requested date window
             (repeat is independently sufficient; dueDate is not required for C to fire)
```

If none of A, B, or C is satisfied → item does not appear in Agenda.

### Projection invalidation

Item leaves Agenda projection when:

- all temporal fields are cleared (ClearSharedListItemTemporal), OR
- dueDate/reminder/repeat move outside the Agenda query window

### Checked item projection

- A checked item that meets A, B, or C still appears in Agenda
- It is rendered de-emphasized (same visual treatment as completed tasks)
- It is not excluded from the projection until its temporal fields are cleared or it is removed

### Ordering within Agenda day

Within a single day, projected list items are ordered as follows relative to other entry types:

| Position | Entry type |
| -------- | ---------- |
| 1 | Overdue items (any type, past dueDate, unchecked) |
| 2 | Tasks due today |
| 3 | Projected list items due today — importance = true, unchecked |
| 4 | Plans (by startTime ascending, all-day after timed) |
| 5 | Routines |
| 6 | Projected list items due today — importance = false, unchecked |
| 7 | Completed tasks and checked list items |

### Conflict with tasks

A list item appearing in Agenda does not conflict with a task covering the same subject.
They are separate domain objects. No deduplication or merging occurs.
The user sees both; context makes the distinction clear.

---

## Ownership of Behavior

| Capability                        | Owner                     | Notes                                           |
| --------------------------------- | ------------------------- | ----------------------------------------------- |
| Item storage and state            | Shared Lists              | All fields persisted by Lists' aggregate        |
| Checked/unchecked toggle          | Shared Lists              | `ToggleSharedListItem` command                  |
| Importance flag                   | Shared Lists              | `SetSharedListItemImportance` command           |
| Temporal field management         | Shared Lists              | `SetSharedListItemTemporal`, `ClearSharedListItemTemporal` |
| Time semantics (what a date means)| Calendar (interpretation) | Calendar defines the concept of time; Lists references it |
| Agenda projection computation     | Application layer         | Read query assembled by application, not domain event driven |
| Agenda rendering                  | Agenda surface            | Visual rendering rules owned by Agenda spec     |
| Edit of temporal item in Agenda   | Not possible from Agenda  | Must navigate to Lists surface                  |

### Key ownership boundary

Lists does not become Calendar.
Lists carries `dueDate` as a date value. Calendar gives that date meaning in the household.
Lists does not schedule, reschedule, or own recurring time definitions in the Calendar sense.
`RepeatRule` on an item is a lightweight recurrence hint for projection. It is not a Calendar `RecurrenceRule`.

---

## Migration Rules

All existing SharedListItem records default to:

```
importance = false
dueDate    = null
reminder   = null
repeat     = null
```

No data migration is required beyond adding nullable columns with null defaults.

No existing items gain temporal status without an explicit user action.
No existing items project into Agenda until their temporal fields are set.
This is a strictly additive change to the item model.

---

## Agenda Scope Placement Rules

Temporal list items project into Agenda read models according to these rules.

There is no member-level scoping for list items in V1.
List items belong to the household, not to a specific member.

### Household-scoped temporal list item

A temporal list item from a list with no member association projects into:

- **Household + Day**: shared row (household-level section)
- **Household + Week**: appears in the household lane on the relevant day column(s)
- **Household + Month**: contributes to the day cell count on relevant days

It appears in **Member scope only** if a member navigates through the Household board entry, which shows all household items.

### Member-scoped temporal list item

V1 does not support member-scoped list items.
`SharedListItem` does not carry a `memberId` or `assigneeId`.
All temporal list items are household-scoped in V1.

If a user wants to anchor a temporal item to a person, the correct model is a Task (Tasks context) with assignment.

### Plan-linked temporal list item

A temporal list item in a list that is linked to a Calendar Event (plan) projects independently from the plan link.

- The item projects based on its own temporal fields, not the event's schedule.
- The item projects when conditions A, B, or C are satisfied, regardless of whether the linked event is also visible.
- The plan's presence in Agenda does not cause the item to appear.
- The item's appearance in Agenda does not cause the plan's list to expand inline.

These are two separate appearance mechanisms:
1. The plan appears with a compact list reference cue (unchecked count of its linked list).
2. The item appears independently via the projection mechanism.

---

## What This Model Does NOT Include

The following are explicitly excluded from V1 item capabilities:

- assignment to a member (belongs to Tasks)
- status lifecycle beyond checked/unchecked (belongs to Tasks)
- steps or subtasks (deferred)
- attachments (deferred)
- comments (deferred)
- labels or tags (deferred)
- estimated effort (deferred)
- links to tasks (deferred)

These exclusions are intentional.
Introducing them without a clear product requirement would collapse the boundary between Lists and Tasks.

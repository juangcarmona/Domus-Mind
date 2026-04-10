# Spec — View Family Timeline

## Purpose

Project the unified household temporal read model for a requested date window.

This feature assembles the household-scope Agenda read model — the view of everything affecting the household as a whole, across all members.

## Context

- Module: Calendar (read model projection)
- Read model: Family Timeline
- Query: `GetFamilyTimeline`

This is a query-only feature. It does not modify aggregates.

## Inputs

Required:

- `familyId`
- `date` (reference date)

Optional:

- `mode` — `day` | `week` | `month` (default: `day`)

## Preconditions

- family must exist

## Decision: List Items in the Family Timeline

**List items with temporal fields are included in the family timeline.**

Rationale:

- household execution containers (lists) are household-scoped
- items with due dates and reminders represent household commitments
- omitting them from the household timeline would contradict the progressive capability model
- the timeline answers "what matters today for this household" — temporally-enriched list items are part of that answer

Constraint:

- projected list items must remain distinguishable from tasks and plans
- projected list items must carry a `type: list-item` discriminator
- projected list items without temporal fields do not appear in the timeline

## Projection Sources

The family timeline read model assembles entries from the following sources:

| Entry Type          | Source Context | Condition                                               |
| ------------------- | -------------- | ------------------------------------------------------- |
| Plan                | Calendar       | belongs to the family, within the window                |
| Task                | Tasks          | belongs to the family, due within the window            |
| Routine             | Tasks          | projected occurrence for the family or a member         |
| Projected list item | Shared Lists   | item has temporal fields (due date or reminder) falling within the window |

Note: **External calendar entries are excluded from the family timeline.**

External entries belong to a specific member's personal calendar connection.
They appear in Member scope only (see `view-member-agenda`).
Including external entries in the household timeline would conflate private imported data with shared household coordination.

## Entry Types

### Plan

Fields to surface:

- `type: plan`
- title
- startTime, endTime
- participants (member refs)
- isAllDay

### Task

Fields to surface:

- `type: task`
- title
- dueDate
- status
- assigneeId (optional)

### Routine

Fields to surface:

- `type: routine`
- title
- recurrence summary
- scope (household or member-scoped)

### Projected List Item

Fields to surface:

- `type: list-item`
- title (item name)
- dueDate
- reminder
- checked state
- importance flag
- listId
- listName

Rules:

- projected list items appear only when they have a due date or reminder within the window
- checked items appear de-emphasized but remain present if the window date matches
- projected list items are not editable from Timeline read output

## Read Model Shape

```json
{
  "familyId": "family_456",
  "date": "2026-04-10",
  "mode": "day",
  "members": [
    {
      "memberId": "member_123",
      "displayName": "Ana",
      "entries": [...]
    },
    {
      "memberId": "member_456",
      "displayName": "Lucas",
      "entries": [...]
    }
  ],
  "household": {
    "entries": [
      {
        "type": "plan",
        "id": "event_abc",
        "title": "School trip",
        "startTime": "2026-04-10T08:00:00Z",
        "isAllDay": false
      },
      {
        "type": "list-item",
        "id": "item_789",
        "title": "Sign permission form",
        "dueDate": "2026-04-10",
        "checked": false,
        "importance": true,
        "listId": "list_001",
        "listName": "School preparation"
      }
    ]
  }
}
```

## Scoping Rules

Household entries:

- plans assigned to no specific member
- tasks assigned to no specific member
- routines scoped to household
- projected list items (lists are household-scoped by default)

Member entries:

- plans in which the member participates
- tasks assigned to the member
- routines scoped to the member
- projected list items appear under household unless the list carries explicit member association

## Priority Order

Within a single day, per section:

1. overdue items (past dueDate, unchecked)
2. tasks due on this date
3. projected list items due on this date — importance first
4. plans (by startTime ascending, all-day after timed)
5. routines
6. projected list items due on this date — no importance
7. completed / checked items

## Failure Cases

- family not found

## Notes

This query is a read-only projection.
It does not create, modify, or infer any aggregate.
List items must never be converted to tasks by this query or any downstream handler.
Calendar remains the source of truth for time. Lists carry temporal references, not temporal ownership.

# Spec — View Member Agenda

## Purpose

Project the temporal view for a single household member across a requested date window.

This feature assembles the read model that powers the Member scope of the Agenda surface.

## Context

- Module: Calendar (read model projection)
- Read model: Member Agenda
- Query: `GetMemberAgenda`

This is a query-only feature. It does not modify aggregates.

## Inputs

Required:

- `memberId`
- `familyId`
- `date` (reference date)

Optional:

- `mode` — `day` | `week` | `month` (default: `day`)

## Preconditions

- family must exist
- member must belong to the family

## Projection Sources

The member agenda read model assembles entries from the following sources:

| Entry Type         | Source Context | Condition                                              |
| ------------------ | -------------- | ------------------------------------------------------ |
| Plan               | Calendar       | participant includes the member, or is a household plan |
| Task               | Tasks          | assigned to the member, or due within the window       |
| Routine            | Tasks          | projected occurrence for the member or household scope |
| External entry     | Calendar       | owned by the member's active calendar connections, within sync horizon |
| Projected list item | Shared Lists  | item has temporal fields (due date or reminder) falling within the window |

## Entry Types

### Plan

Represents a Calendar `Event` aggregate rendered as a plan.

Fields to surface:

- `type: plan`
- title
- startTime, endTime
- participants
- isAllDay

### Task

Represents a Task aggregate due within the requested window.

Fields to surface:

- `type: task`
- title
- dueDate
- status

### Routine

Projected occurrence for the requested date window.

Fields to surface:

- `type: routine`
- title
- recurrence summary

### External Calendar Entry

Read-only imported entry from an external calendar connection.

Fields to surface:

- `type: external-entry`
- title
- startTime, endTime
- sourceLabel (e.g. `Outlook`)
- externalLink (for `Open in Outlook` action)

Rules:

- external entries appear in Member scope only
- external entries are read-only — no edit action available

### Projected List Item

A Shared List item carrying temporal fields (due date, reminder, or repeat) that falls within the requested window.

Fields to surface:

- `type: list-item`
- title (item name)
- dueDate
- reminder
- checked state
- importance flag
- listId (for navigation to Lists surface)
- listName

Rules:

- projected list items appear only when they have a due date or reminder within the window
- projected list items are distinguishable from tasks in the read model (`type: list-item`)
- projected list items are not editable from Agenda — editing navigates to the list
- checked items appear de-emphasized but remain present if date falls within window

## Read Model Shape

```json
{
  "memberId": "member_123",
  "familyId": "family_456",
  "date": "2026-04-10",
  "mode": "day",
  "entries": [
    {
      "type": "plan",
      "id": "event_abc",
      "title": "Dentist",
      "startTime": "2026-04-10T10:00:00Z",
      "endTime": "2026-04-10T11:00:00Z",
      "isAllDay": false
    },
    {
      "type": "task",
      "id": "task_xyz",
      "title": "Pay electricity bill",
      "dueDate": "2026-04-10",
      "status": "pending"
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
    },
    {
      "type": "external-entry",
      "id": "ext_001",
      "title": "Budget review",
      "startTime": "2026-04-10T14:00:00Z",
      "endTime": "2026-04-10T15:00:00Z",
      "sourceLabel": "Outlook",
      "externalLink": "https://..."
    }
  ]
}
```

## Priority Order

Within a single day:

1. overdue items (any type with past dueDate, unchecked)
2. tasks due on this date
3. projected list items due on this date — importance first
4. plans (by startTime ascending, all-day after timed)
5. routines
6. projected list items due on this date — no importance
7. completed / checked items

## Failure Cases

- family not found
- member not found
- member does not belong to family

## Notes

This query is a read-only projection.
It does not create, modify, or infer any aggregate.
List items must never be converted to tasks by this query or any downstream handler.

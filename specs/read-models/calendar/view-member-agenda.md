# Spec - View Agenda in Member Scope

## Purpose

Provide the person-scoped Agenda read model used to render one member's day, week, or month with native household items and relevant imported external calendar entries.

## Context

- Module: Calendar (read model composed with Tasks)
- Read Model: `MemberAgenda`
- Query: `GetMemberAgenda`

## Inputs

Required:

- `familyId`
- `memberId`
- `windowStartUtc`
- `windowEndUtc`
- `mode` where allowed values are `day | week | month`

Optional:

- `includeCompleted`

## Preconditions

- target family must exist
- target member must exist and belong to the family
- requested window must be valid

## Data Sources

The member-scoped Agenda read model may include:

- native calendar events involving the member
- tasks assigned to the member or relevant to the selected member scope
- projected routine occurrences relevant to the member
- imported external calendar entries from the member's selected external calendar feeds
- projected list items: SharedList items with temporal fields (dueDate, reminder, or repeat) where at least one of those fields produces a date falling within the requested window

This query assembles a unified view from multiple write-model sources.
No entity is merged. Each entry carries its own `type` discriminator.

## Query Behavior

The system retrieves agenda items relevant to the selected member and requested window.

Imported external calendar entries appear only when all of the following are true:

- the connection belongs to the selected member
- the provider feed is selected
- the connection is active
- the entry overlaps the requested window
- the entry falls inside the feed's active sync horizon
- the entry is not tombstoned as deleted

Imported external entries are returned as read-only items.

They must not be merged into native event identity.
They remain distinguishable in the read model.

## Result Structure

Each agenda item includes:

- `type` where allowed values include `event | task | routine | external-calendar-entry | list-item`
- `title`
- `startsAtUtc`
- `endsAtUtc`
- `allDay`
- `status`
- `isReadOnly`

Native item fields may include:

- `eventId`
- `taskId`
- `routineId`

Projected list item fields include:

- `itemId`
- `listId`
- `listName`
- `checked`
- `importance`
- `dueDate`
- `reminder`
- `isReadOnly = true` (items are not editable from Agenda)

Projected list item rules:

- appear when `dueDate`, `reminder`, or `repeat` produces a date falling within the requested window
- `repeat` is independently sufficient to trigger projection (dueDate is not required)
- checked items appear de-emphasized but are not excluded from the result
- the `type` field is always `list-item` — never `task` or `event` — regardless of what temporal fields are set
- list items are household-scoped: V1 does not support per-member scoping of list items
- all household temporal list items appear in member scope (the member sees household items)
- plan-linked temporal list items appear via this projection independently of the linked event; the event and item are separate entries

Imported external entry fields include:

- `connectionId`
- `calendarId`
- `externalEventId`
- `provider = microsoft`
- `providerLabel = Outlook`
- `openInProviderUrl` when available
- `location`
- `participantSummary`
- `sourceLastModifiedUtc`

## Success Result

Return:

- ordered agenda items for the requested member and window
- native and imported items clearly distinguished by type and read-only state

## Failure Cases

- family not found
- member not found
- invalid time range

## Notes

Phase 1 imported external calendar entries appear only in Agenda member scope.

They do not appear in Household Agenda or in the `FamilyTimeline` read model.
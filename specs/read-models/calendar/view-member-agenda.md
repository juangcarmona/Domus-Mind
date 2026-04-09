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

- `type` where allowed values include `event | task | routine | external-calendar-entry`
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
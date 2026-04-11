# Spec - View Family Timeline

## Purpose

Provide a unified chronological view of household activity.

The timeline aggregates events, tasks, and projected list items for a family.

## Context

* Module: Calendar (read model)
* Read Model: `FamilyTimeline`
* Slice: `view-family-timeline`
* Query: `GetFamilyTimeline`

## Inputs

Required:

* `familyId`

Optional:

* `startTime`
* `endTime`
* `participantId`

## Preconditions

* target family must exist
* time range must be valid if provided

## Data Sources

The timeline read model may include:

* scheduled events
* upcoming tasks
* recently completed tasks
* projected routine occurrences
* projected list items: SharedList items with temporal fields (dueDate, reminder, or repeat) where at least one of those fields produces a date falling within the requested window

**Deliberately not included in the phase 1 household timeline:**

* Imported external calendar entries: these are personal/member-scoped integration state. Mixing them into the household-native timeline would blur external personal commitments with native household planning state. They project into the member-scoped Agenda read model only.

The query assembles a unified chronological view from Calendar, Tasks, and Shared Lists.
No entity is merged. Each entry carries its own `type` discriminator.

## Query Behavior

The system retrieves timeline items ordered by time.

Items are merged from multiple contexts into a single chronological view.

Participant filtering may match any participant associated with an event or task.

## Result Structure

Each timeline item includes:

* `type` where allowed values are `event | task | routine | list-item`
* `title`
* `time`
* `participants`
* `status`
* `importance` (for list-item type only)
* `listId` (for list-item type only)
* `listName` (for list-item type only)

The `type` field is the source discriminator. It must never be collapsed:

- a list item is always `list-item` — not `task` or `event`
- a routine occurrence is always `routine` — not `event`
- a task is always `task`
- an event is always `event`

Participant display data should be richer than raw IDs.

A participant projection may include:

* `participantId`
* `displayName`
* `kind` (`member | dependent | pet`)
* optional display metadata needed by the UI

## Success Result

Return:

* ordered list of timeline items
* filtered by family and optional time range

## Failure Cases

* family not found
* invalid time range

## Notes

This query represents the primary operational overview of household activity in DomusMind.

Coordination **cues** are reserved for read models such as the weekly grid.

They are projection-only artifacts and are **not part of the timeline contract yet**.

Imported Outlook calendar entries are intentionally excluded so the household-native timeline does not blur external personal commitments with native household planning state.

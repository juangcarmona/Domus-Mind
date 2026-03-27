# Spec - Remove Reminder

## Purpose

Remove a reminder from an existing event.

This operation updates the reminder schedule without affecting the event itself.

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `remove-reminder`
- Command: `RemoveReminder`

## Inputs

Required:

- `eventId`
- `offset`

Optional:

- `reason`

## Preconditions

- event must exist
- reminder with the specified offset must exist
- command modifies only the `Event` aggregate

## State Changes

On success, the reminder definition is removed from the event.

Remaining reminders are unchanged.

## Invariants

- reminder offsets must remain unique
- reminders must reference a valid event schedule
- event identity must remain stable

## Events

Emit:

- `ReminderRemoved`

## Success Result

Return:

- `eventId`
- `offset`
- `status = removed`

## Failure Cases

- event not found
- reminder offset not found

## Notes

Reminder delivery is handled by infrastructure; this operation affects only the event definition.
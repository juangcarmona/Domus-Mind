# Spec - Add Reminder

## Purpose

Add a reminder to an event.

Reminders notify participants before an event occurs.

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `add-reminder`
- Command: `AddReminder`

## Inputs

Required:

- `eventId`
- `offset`

Optional:

- `channel`

## Preconditions

- event must exist
- reminder offset must be valid
- reminder must not duplicate an existing offset
- command modifies only the `Event` aggregate

## State Changes

On success, the reminder definition is added to the event.

The reminder will trigger a notification at the specified offset.

## Invariants

- reminder offsets must be unique per event
- reminder must reference a valid event schedule
- event identity must remain stable

## Events

Emit:

- `ReminderAdded`

## Success Result

Return:

- `eventId`
- `offset`
- `status = created`

## Failure Cases

- event not found
- duplicate reminder offset
- invalid offset

## Notes

Reminder delivery is handled by infrastructure services.
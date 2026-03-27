# Spec - Reschedule Event

## Purpose

Update the schedule of an existing event.

Rescheduling adjusts when an event occurs while preserving its identity and participants.

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `reschedule-event`
- Command: `RescheduleEvent`

## Inputs

Required:

- `eventId`
- `newStartTime`

Optional:

- `newEndTime`
- `reason`

## Preconditions

- event must exist
- event must not be cancelled
- if provided, `newEndTime` must be after `newStartTime`
- command modifies only the `Event` aggregate

## State Changes

On success, the system updates the event schedule.

Participants and other event properties remain unchanged.

## Invariants

- event identity must remain stable
- event must belong to the same family
- schedule must remain valid

## Events

Emit:

- `EventRescheduled`

## Success Result

Return:

- `eventId`
- `newStartTime`
- `status = rescheduled`

## Failure Cases

- event not found
- event cancelled
- invalid schedule

## Notes

Rescheduling may trigger downstream reactions such as task updates or reminder recalculation.
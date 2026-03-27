# Spec - Cancel Event

## Purpose

Cancel a scheduled event that will no longer occur.

Cancellation preserves event history but prevents further execution.

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `cancel-event`
- Command: `CancelEvent`

## Inputs

Required:

- `eventId`

Optional:

- `reason`
- `cancelledByMemberId`

## Preconditions

- event must exist
- event must not already be cancelled
- command modifies only the `Event` aggregate

## State Changes

On success, the system marks the event as cancelled.

The event remains in history but is no longer considered active.

## Invariants

- cancelled events cannot be rescheduled
- cancelled events cannot add participants
- event identity must remain stable

## Events

Emit:

- `EventCancelled`

## Success Result

Return:

- `eventId`
- `status = cancelled`

## Failure Cases

- event not found
- event already cancelled

## Notes

Cancellation may trigger downstream reactions such as task cancellation or reminder removal.
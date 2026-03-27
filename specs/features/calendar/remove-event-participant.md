# Spec - Remove Event Participant

## Purpose

Remove a participant from an existing event.

This operation updates the attendance set while preserving the event identity.

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `remove-event-participant`
- Command: `RemoveEventParticipant`

## Inputs

Required:

- `eventId`
- `participantId`

Optional:

- `reason`

## Preconditions

- event must exist
- participant must currently belong to the event
- command modifies only the `Event` aggregate

## State Changes

On success, the participant is removed from the event participant set.

The event remains scheduled and other participants remain unchanged.

## Invariants

- event identity must remain stable
- participants must be unique within an event
- removed participants must no longer appear in the participant set

## Events

Emit:

- `EventParticipantRemoved`

## Success Result

Return:

- `eventId`
- `participantId`
- `status = removed`

## Failure Cases

- event not found
- participant not found in event

## Notes

Removing a participant may trigger downstream adjustments such as task reassignment.
# Spec - Add Event Participant

## Purpose

Add a participant to an existing event.

Participants represent family members, dependents, or pets involved in the event. :contentReference[oaicite:0]{index=0}

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `add-event-participant`
- Command: `AddEventParticipant`

## Inputs

Required:

- `eventId`
- `participantId`
- `participantType`

Optional:

- `role`

## Preconditions

- event must exist
- participant must belong to the same family
- participant must not already be part of the event
- command modifies only the `Event` aggregate

## State Changes

On success, the participant is added to the event.

The participant becomes part of the event attendance set.

## Invariants

- participants must be unique within an event
- participants must reference valid family entities
- event identity must remain stable

## Events

Emit:

- `EventParticipantAdded`

## Success Result

Return:

- `eventId`
- `participantId`
- `status = added`

## Failure Cases

- event not found
- participant not found
- duplicate participant

## Notes

Participants may influence task generation or responsibility routing.
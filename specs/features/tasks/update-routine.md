# Spec - Update Routine

## Purpose

Update the recurrence or definition of an existing routine.

This allows recurring household work to evolve over time. 

## Context

- Module: Tasks
- Aggregate: `Routine`
- Slice: `update-routine`
- Command: `UpdateRoutine`

## Inputs

Required:

- `routineId`

Optional:

- `title`
- `description`
- `recurrenceRule`
- `defaultAssigneeId`

## Preconditions

- routine must exist
- if provided, recurrence rule must be valid
- if provided, assignee must belong to the same family
- command modifies only the `Routine` aggregate

## State Changes

On success, the routine definition is updated.

Future generated tasks use the new routine configuration.

## Invariants

- routine identity must remain stable
- routines must define a valid recurrence rule
- generated tasks must continue referencing the originating routine

## Events

Emit:

- `RoutineUpdated`

## Success Result

Return:

- `routineId`
- `status = updated`

## Failure Cases

- routine not found
- invalid recurrence rule
- invalid assignee

## Notes

Updating a routine affects future executions, not past generated tasks.
# Spec - Resume Routine

## Purpose

Resume a paused routine so it starts generating tasks again.

This restores recurring operational behavior after a temporary stop. 

## Context

- Module: Tasks
- Aggregate: `Routine`
- Slice: `resume-routine`
- Command: `ResumeRoutine`

## Inputs

Required:

- `routineId`

Optional:

- `effectiveFrom`

## Preconditions

- routine must exist
- routine must currently be paused
- command modifies only the `Routine` aggregate

## State Changes

On success, the routine status becomes active again.

Future scheduled executions may generate tasks.

## Invariants

- routine identity must remain stable
- resumed routines must have a valid recurrence rule
- resumed routines must transition from paused to active only

## Events

Emit:

- `RoutineResumed`

## Success Result

Return:

- `routineId`
- `status = resumed`

## Failure Cases

- routine not found
- routine not paused
- invalid routine state

## Notes

Resume reactivates future generation only; missed executions are not automatically recreated unless defined separately.
# Spec - Pause Routine

## Purpose

Pause an active routine so it temporarily stops generating tasks.

This is used when recurring work should be suspended without deleting the routine. 

## Context

- Module: Tasks
- Aggregate: `Routine`
- Slice: `pause-routine`
- Command: `PauseRoutine`

## Inputs

Required:

- `routineId`

Optional:

- `reason`
- `pausedUntil`

## Preconditions

- routine must exist
- routine must be active
- command modifies only the `Routine` aggregate

## State Changes

On success, the routine status becomes paused.

No new tasks should be generated while paused.

## Invariants

- paused routines must retain identity
- paused routines cannot generate new tasks
- existing generated tasks remain unchanged

## Events

Emit:

- `RoutinePaused`

## Success Result

Return:

- `routineId`
- `status = paused`

## Failure Cases

- routine not found
- routine already paused
- routine inactive or deleted

## Notes

Pause is reversible and does not remove routine history.
# Spec - Cancel Task

## Purpose

Cancel a task that should no longer be executed.

Cancellation preserves task history while removing it from active work. 

## Context

- Module: Tasks
- Aggregate: `Task`
- Slice: `cancel-task`
- Command: `CancelTask`

## Inputs

Required:

- `taskId`

Optional:

- `reason`
- `cancelledByMemberId`

## Preconditions

- task must exist
- task must not already be cancelled
- task must not be completed
- command modifies only the `Task` aggregate

## State Changes

On success, the task status becomes `cancelled`.

The task remains in history but is no longer actionable.

## Invariants

- cancelled tasks cannot be completed
- cancelled tasks cannot return to pending
- task identity must remain stable

## Events

Emit:

- `TaskCancelled`

## Success Result

Return:

- `taskId`
- `status = cancelled`

## Failure Cases

- task not found
- task already cancelled
- task already completed

## Notes

Cancellation may occur when an event is cancelled or work becomes irrelevant.
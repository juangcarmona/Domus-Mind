# Spec - Reschedule Task

## Purpose

Update the due date or execution time of an existing task.

Rescheduling adjusts when work should be performed without changing the task identity. 

## Context

- Module: Tasks
- Aggregate: `Task`
- Slice: `reschedule-task`
- Command: `RescheduleTask`

## Inputs

Required:

- `taskId`
- `newDueDate`

Optional:

- `reason`

## Preconditions

- task must exist
- task must not be completed
- task must not be cancelled
- command modifies only the `Task` aggregate

## State Changes

On success, the task due date is updated.

The task remains active and assigned.

## Invariants

- task identity must remain stable
- due date must be valid
- completed tasks cannot be rescheduled

## Events

Emit:

- `TaskRescheduled`

## Success Result

Return:

- `taskId`
- `newDueDate`
- `status = rescheduled`

## Failure Cases

- task not found
- task already completed
- invalid due date

## Notes

Rescheduling is common when events shift or responsibilities change.
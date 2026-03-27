# Spec - Complete Task

## Purpose

Mark a task as completed after the assigned member finishes the work.

Completion represents the final state of the task lifecycle. 

## Context

- Module: Tasks
- Aggregate: `Task`
- Slice: `complete-task`
- Command: `CompleteTask`

## Inputs

Required:

- `taskId`

Optional:

- `completedByMemberId`
- `completedAt`

## Preconditions

- task must exist
- task must not already be completed
- task must not be cancelled
- command modifies only the `Task` aggregate :contentReference[oaicite:1]{index=1}

## State Changes

On success, the system transitions the task status to `completed`.

The completion timestamp may be recorded for history and metrics. 

## Invariants

- completed tasks cannot return to pending
- cancelled tasks cannot be completed
- task identity must remain stable 

## Events

Emit:

- `TaskCompleted` :contentReference[oaicite:4]{index=4}

## Success Result

Return:

- `taskId`
- `status = completed`

## Failure Cases

- task not found
- task already completed
- task cancelled

## Notes

Completion may trigger downstream reactions such as routine generation or analytics.
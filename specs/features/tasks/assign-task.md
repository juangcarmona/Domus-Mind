# Spec - Assign Task

## Purpose

Assign a task to a family member responsible for executing it.

Assignment establishes who should perform the operational work. 

## Context

- Module: Tasks
- Aggregate: `Task`
- Slice: `assign-task`
- Command: `AssignTask`

## Inputs

Required:

- `taskId`
- `memberId`

Optional:

- `assignedAt`

## Preconditions

- task must exist
- member must exist in the family
- command must modify only the `Task` aggregate :contentReference[oaicite:6]{index=6}

## State Changes

On success, the system assigns the task to the specified member.

The assignment becomes the active executor of the task. 

## Invariants

- a task may have at most one primary assignee
- the assignee must belong to the same family
- assignment must reference a valid member 

## Events

Emit:

- `TaskAssigned` :contentReference[oaicite:9]{index=9}

## Success Result

Return:

- `taskId`
- `memberId`
- `status = assigned`

## Failure Cases

- task not found
- member not found
- invalid assignment

## Notes

Tasks may be assigned manually or derived from responsibilities or events.
# Context Spec — Tasks

## Purpose

Defines the functional scope of the Tasks context.

This context owns execution of household work.

---

## Responsibilities

- create tasks
- assign tasks
- complete tasks
- cancel tasks
- reschedule tasks
- define routines
- update and control routines

---

## Aggregates

- `Task`
- `Routine`

---

## Owned Concepts

- Task
- Routine

---

## Invariants

- every task belongs to one family
- a task has at most one primary assignee
- completed tasks cannot return to pending
- cancelled tasks cannot be completed
- routines must define valid recurrence rules (Daily, Weekly, Monthly, Yearly)
- routines do not generate Task aggregates; they are projected on-the-fly

---

## Events Emitted

- `TaskCreated`
- `TaskAssigned`
- `TaskReassigned`
- `TaskCompleted`
- `TaskCancelled`
- `TaskRescheduled`
- `RoutineCreated`
- `RoutineUpdated`
- `RoutinePaused`
- `RoutineResumed`

---

## Events Consumed

- `MemberAdded`
- `MemberRemoved`
- `PrimaryOwnerAssigned`
- `ResponsibilityTransferred`

---

## Related Feature Specs

- create-task
- assign-task
- complete-task
- cancel-task
- reschedule-task
- create-routine
- update-routine
- pause-routine
- resume-routine
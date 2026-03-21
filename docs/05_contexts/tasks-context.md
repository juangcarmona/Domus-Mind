# DomusMind — Tasks Context

## Purpose

The Tasks context defines the **execution layer of household work**.

It represents the concrete actions required to operate family life.

It is responsible for:

* tasks
* routines
* task assignment
* task completion
* task lifecycle

This context answers:

* what needs to be done
* who should do it
* when it should be done
* whether it has been completed 

---

# Responsibilities

The Tasks context is responsible for:

* creating tasks
* assigning tasks to members
* defining recurring routines
* tracking task completion
* managing task status

It represents **operational work**, not planning or attendance scheduling.

---

# Aggregate Roots

## Task

Represents a single actionable unit of work.

Examples:

* buy groceries
* prepare school bag
* take dog to vet
* pay electricity bill
* bring documents

The Task aggregate owns:

* task identity
* assignment
* due date
* status
* origin reference (optional)

A **Task** represents a concrete executable instance of work.

**"Task" is both the household-facing term and the internal domain term.** No translation is needed between layers.

---

## Routine

Represents a **recurring operational pattern definition**.

Examples:

```
weekly grocery shopping
daily pet feeding
weekly house cleaning
monthly bill review
```

Important clarification:

A **Routine is a definition**, not a task.

It describes how operational behavior repeats over time.

Routine does **not represent attendance commitments**.

Fixed-time commitments belong to the **Calendar context**.

Example distinction:

```
Football practice every Tuesday → Calendar Event (Plan in household language)
Pack sports bag before practice → Task (created explicitly by the user)
```

---

# Routine Projection Model

Routines are **projected on-the-fly** by the application layer.

When building timeline views, weekly grids, or coordination dashboards, the application evaluates each active routine's recurrence schedule against the requested date range using `OccursOn(date)`. Matching routines surface as entries in the read model.

Routines do **not generate Task aggregates**.

This keeps the domain clean:

* no background scheduling jobs required
* no persisted occurrence records
* read model is always consistent with current routine state

The separation between Routine (definition) and Task (one-off executable work item) is intentional and must be preserved.

Example:

```
Routine: Weekly Trash (every Tuesday, scope: Household)
→ Appears as a read-model entry on each Tuesday in the timeline
→ No Task aggregate is created from a Routine
```

**Manually created tasks are independent.** A task created by a user happens to look similar to a routine entry in the timeline, but originates from a direct user action, not from routine evaluation.

---

# Internal Entities

## TaskAssignment

Represents who is responsible for executing a task.

Assignments reference members defined in the Family context.

## TaskOrigin

Represents the source that created a task.

Possible origins:

* manual
* external integration

> **Note:** Tasks are not generated automatically from routines or events in the current model. All tasks originate from explicit user or integration actions.

---

# Value Objects

Suggested value objects:

* `TaskId`
* `RoutineId`
* `FamilyId`
* `TaskTitle`
* `TaskDescription`
* `TaskStatus`
* `DueDate`
* `RecurrenceRule`
* `TaskOriginType`

Optional future value objects:

* `Priority`
* `EstimatedEffort`
* `TaskTag`

Identifiers must remain strongly typed.

---

# Invariants

The Tasks aggregates must enforce the following invariants.

## Identity

* every task must have a stable `TaskId`
* every routine must have a stable `RoutineId`
* every task belongs to exactly one family

## Assignment

* a task may have zero or one primary assignee
* an assignee must be a valid family member
* duplicate active assignments are not allowed

## Lifecycle

Valid task states:

* pending
* in progress
* completed
* cancelled

Rules:

* completed tasks cannot return to pending
* cancelled tasks cannot be completed
* tasks may only transition through valid state paths

## Routine Integrity

* routines must define a valid recurrence rule
* routines define operational patterns, not fixed-time attendance
* supported frequencies: Daily, Weekly, Monthly, Yearly

## Ownership Boundary

* only the Tasks context may modify task state
* assignment must reference Family members
* responsibility domains may categorize tasks but cannot modify them directly

---

# Commands

Core commands owned by this context.

Task commands:

* `CreateTask`
* `AssignTask`
* `UnassignTask`
* `StartTask`
* `CompleteTask`
* `CancelTask`
* `RenameTask`
* `RescheduleTask`

Routine commands:

* `CreateRoutine`
* `UpdateRoutine`
* `PauseRoutine`
* `ResumeRoutine`
* `DeleteRoutine`

---

# Queries

Core queries supported by this context.

Task queries:

* `GetTask`
* `GetTasksByFamily`
* `GetTasksByAssignee`
* `GetTasksDueToday`
* `GetPendingTasks`

Routine queries:

* `GetRoutine`
* `GetRoutinesByFamily`
* `GetActiveRoutines`

Suggested future queries:

* `GetTaskBoard`
* `GetOverdueTasks`
* `GetTaskCompletionStats`

---

# Domain Events Emitted

The Tasks context emits.

Task events:

* `TaskCreated`
* `TaskAssigned`
* `TaskCompleted`
* `TaskCancelled`
* `TaskRescheduled`
* `TaskReassigned`

Routine events:

* `RoutineCreated`
* `RoutineUpdated`
* `RoutinePaused`
* `RoutineResumed`
* `RoutineDeleted`
* `RoutineTaskGenerated`

These events must be emitted only after successful state change.

---

# Domain Events Consumed

The Tasks context depends on other upstream contexts.

From Family:

* `MemberAdded`
* `MemberRemoved`

Possible reactions:

* validate assignments
* remove invalid assignments

From Calendar:

* `EventScheduled`
* `EventRescheduled`

Possible reactions:

* generate preparation tasks

From Responsibility:

* `PrimaryOwnerAssigned`
* `ResponsibilityTransferred`

Possible reactions:

* suggest or auto-assign tasks

Default rule:

**Tasks reacts to system activity but owns task execution state.**

---

# Read Models

Useful read models for this context.

## Task Board

Contains:

* pending tasks
* tasks in progress
* completed tasks

Grouped by:

* assignee
* due date
* responsibility domain

---

## Personal Task List

Contains:

* tasks assigned to a specific member

Fields:

* task title
* due date
* status
* origin

---

## Household Task Overview

Contains:

* all open tasks
* grouped by responsibility domain

Useful for operational coordination.

---

## Routine Overview

Contains:

* active routines
* next execution time
* assigned members

Useful for long-term household maintenance.

---

# Boundaries With Other Contexts

## Family Context

Family owns member identity.

Tasks references `MemberId` for assignment.

Tasks must not modify membership.

---

## Responsibility Context

Responsibility domains may categorize tasks.

Example:

```
food
school
maintenance
```

Tasks may reference `ResponsibilityDomainId`.

Responsibility owns ownership rules.

---

## Calendar Context

Calendar owns **time-bound attendance commitments**.

Examples:

```
school
football practice
doctor appointment
```

Tasks owns **operational work generated from those commitments**.

Example:

```
Event: School trip
Tasks:
prepare backpack
sign permission form
```

Calendar owns the event.
Tasks owns the resulting work.

---

# Ubiquitous Language Notes

Within this context:

* `Task` means a concrete unit of work
* `Routine` means a recurring operational definition
* `Assignee` means the member responsible for execution
* `TaskStatus` means the lifecycle state of a task

Avoid ambiguous terms such as:

* todo
* reminder
* checklist item
* activity

unless explicitly modeled.

---

# Slice Mapping

Initial slices mapped to this context.

Task slices:

* `create-task`
* `assign-task`
* `complete-task`
* `cancel-task`
* `reschedule-task`

Routine slices:

* `create-routine`
* `pause-routine`
* `resume-routine`

These slices operate on `Task` and `Routine` aggregates.

---

# Transaction Rules

Rules:

* one command modifies one aggregate
* task updates occur inside the `Task` transaction boundary
* routine updates occur inside the `Routine` boundary
* downstream reactions occur via events

Example:

```
CompleteTask
→ updates Task
→ emits TaskCompleted
```

---

# Design Notes

Tasks represent **execution**, not planning or attendance scheduling.

The context must remain focused on:

* actionable work
* execution tracking
* completion state

It must not absorb logic that belongs to:

* scheduling events
* defining responsibility ownership
* modeling family identity

Those belong to other contexts.

---

# Summary

The Tasks context defines the **execution engine of DomusMind**.

It owns:

* tasks
* routines
* task lifecycle
* assignment
* completion

Routines define **recurring operational behavior**.

They may produce:

* executable task instances
* read-model coordination cues

Fixed-time attendance commitments belong to **Calendar**.

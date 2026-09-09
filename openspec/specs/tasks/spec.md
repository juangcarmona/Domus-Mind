# Tasks Specification

## Purpose

Tasks is the household's structured execution layer. It answers: what explicit work needs to be done, who is responsible for doing it, when it should happen, and whether it has been completed.

A **Task** is a concrete, actionable unit of household work. It has an assignee, a due date, and a defined lifecycle. Tasks are created explicitly — they are never automatically generated from Routines or Calendar events.

A **Routine** is a recurring operational definition. It describes household work that repeats on a schedule. A Routine does not produce Task aggregates; it is projected on-the-fly into read surfaces on dates that match its recurrence rule.

Tasks and Routines belong to the Tasks context. A list item that carries a due date is not a Task. A recurring calendar plan is not a Routine. These distinctions are firm.

---

## Requirements

### Requirement: Task Creation

A household SHALL be able to create a task with a title as the only required input.

Optional inputs: assignee (a family member), due date, and a responsibility domain reference for contextual grouping. A newly created task starts in **pending** state. Its origin is manual.

#### Scenario: Household creates a task with title only

- GIVEN a household exists
- WHEN a member creates a task with a title
- THEN a task is created in pending state
- AND no assignee or due date is required

#### Scenario: Household creates a task with an assignee and due date

- GIVEN a valid family member exists
- WHEN a task is created with a title, that member as assignee, and a due date
- THEN a task is created in pending state
- AND it is assigned to the specified member and scheduled for the given due date

#### Scenario: Creating a task with a non-member assignee is rejected

- GIVEN a member ID that does not belong to the household
- WHEN a task is created with that ID as assignee
- THEN the task is not created
- AND a validation error is returned

---

### Requirement: Task Assignment

A household SHALL be able to assign a task to a family member.

A task may have at most one primary assignee at a time. The assignee must be a valid member of the same family. Assignment may be changed after creation. Assignment is always an explicit action — it is never automatic, though it may be contextually motivated by responsibility ownership or calendar events.

#### Scenario: Task is assigned to a member

- GIVEN a task exists
- AND a valid family member exists
- WHEN the task is assigned to that member
- THEN the task records the member as its assignee

#### Scenario: Assigning a task to a non-member is rejected

- GIVEN a task exists
- WHEN an assignment is made to a member ID that does not belong to the household
- THEN the assignment is rejected

---

### Requirement: Task Rescheduling

A household SHALL be able to update the due date of a task that is not yet completed or cancelled.

Rescheduling does not change the task's identity or assignment.

#### Scenario: Due date of a pending task is updated

- GIVEN a task exists in pending state
- WHEN the household provides a new due date
- THEN the task due date is updated
- AND the task remains in its current state and assignment

#### Scenario: Completed task cannot be rescheduled

- GIVEN a task in completed state
- WHEN the household attempts to reschedule it
- THEN the operation is rejected

#### Scenario: Cancelled task cannot be rescheduled

- GIVEN a task in cancelled state
- WHEN the household attempts to reschedule it
- THEN the operation is rejected

---

### Requirement: Task Completion

A household SHALL be able to mark a task as completed.

Completed tasks cannot return to pending. Cancelled tasks cannot be completed. Completion may optionally record which member completed the task and at what time.

#### Scenario: Task is marked as completed

- GIVEN a task exists in pending or in-progress state
- WHEN the household marks it as completed
- THEN the task status becomes completed

#### Scenario: Already completed task cannot be completed again

- GIVEN a task in completed state
- WHEN the household attempts to complete it again
- THEN the operation is rejected

#### Scenario: Cancelled task cannot be completed

- GIVEN a task in cancelled state
- WHEN the household attempts to complete it
- THEN the operation is rejected

---

### Requirement: Task Cancellation

A household SHALL be able to cancel a task that should no longer be executed.

Cancellation preserves the task in history but removes it from active work. Cancelled tasks cannot be completed or return to pending.

#### Scenario: Task is cancelled

- GIVEN a task exists in pending or in-progress state
- WHEN the household cancels the task
- THEN the task status becomes cancelled
- AND it is no longer treated as active work

#### Scenario: Completed task cannot be cancelled

- GIVEN a task in completed state
- WHEN the household attempts to cancel it
- THEN the operation is rejected

#### Scenario: Already cancelled task cannot be cancelled again

- GIVEN a task in cancelled state
- WHEN the household attempts to cancel it again
- THEN the operation is rejected

---

### Requirement: Routine Creation

A household SHALL be able to define a recurring operational routine.

Required inputs: name, scope (Household or Members), kind (Scheduled or Cue), color, and a recurrence rule defined by frequency and day selectors.

Supported frequencies: Daily, Weekly, Monthly, Yearly.
- Weekly requires at least one day of week.
- Monthly requires at least one day of month.
- Yearly requires a month of year and at least one day of month.
- Member-scoped routines require at least one target member.

A newly created routine starts in **active** status. A Routine does not produce Task aggregates — it is projected on-the-fly into read surfaces on dates that match its recurrence rule. A routine may optionally specify an execution time; when provided, it may affect ordering in time-aware views.

#### Scenario: Household creates a weekly routine

- GIVEN a household exists
- WHEN a member creates a routine with a name, household scope, and a weekly frequency on specific days
- THEN a routine is created in active status
- AND it appears in agenda projections on matching days

#### Scenario: Routine with invalid recurrence is rejected

- GIVEN a weekly frequency is specified
- WHEN no days of week are provided
- THEN the routine is not created
- AND a validation error is returned

#### Scenario: Member-scoped routine requires target members

- GIVEN a member scope is specified
- WHEN no target members are provided
- THEN the routine is not created
- AND a validation error is returned

---

### Requirement: Routine Update

A household SHALL be able to update the definition of an existing routine.

Updatable fields include name, recurrence rule, color, scope, and target members. The routine's identity remains stable. Only future projections are affected; the routine's history is unchanged.

#### Scenario: Routine definition is updated

- GIVEN an active routine exists
- WHEN the household provides updated fields
- THEN the routine definition is updated
- AND future agenda projections reflect the new definition

---

### Requirement: Routine Pause

A household SHALL be able to pause an active routine.

A paused routine stops appearing in agenda projections. The routine retains its identity and definition. A routine that is already paused cannot be paused again. A pause may optionally specify a date until which the routine is paused.

#### Scenario: Active routine is paused

- GIVEN a routine in active status
- WHEN the household pauses the routine
- THEN the routine status becomes paused
- AND it no longer appears in agenda projections

#### Scenario: Paused routine cannot be paused again

- GIVEN a routine in paused status
- WHEN the household attempts to pause it
- THEN the operation is rejected

---

### Requirement: Routine Resume

A household SHALL be able to resume a paused routine.

A resumed routine becomes active and begins appearing in agenda projections again. Occurrences missed during the paused period are not retroactively recreated. A routine that is not paused cannot be resumed.

#### Scenario: Paused routine is resumed

- GIVEN a routine in paused status
- WHEN the household resumes the routine
- THEN the routine status becomes active
- AND it begins appearing in agenda projections again
- AND no occurrences from the paused period are added retroactively

#### Scenario: Non-paused routine cannot be resumed

- GIVEN a routine in active status
- WHEN the household attempts to resume it
- THEN the operation is rejected

---

### Requirement: Agenda Projection

Tasks with a due date SHALL appear in the Agenda surface on their due date. Active routines SHALL appear in the Agenda surface on-the-fly on dates that match their recurrence rule.

Neither projection creates or modifies aggregates. Tasks and Routines remain owned by the Tasks context. Agenda is a read surface only.

#### Scenario: Task with due date appears in Agenda

- GIVEN a task exists with a due date
- WHEN the household views Agenda for that date
- THEN the task appears in the day view for that date

#### Scenario: Active routine appears on its scheduled dates

- GIVEN an active routine with a weekly recurrence on Tuesdays
- WHEN the household views Agenda for a Tuesday
- THEN the routine appears as a projected occurrence on that day

#### Scenario: Paused routine does not appear in Agenda

- GIVEN a routine in paused status
- WHEN the household views Agenda for a date matching its recurrence rule
- THEN the routine does not appear

---

## Notes

1. **Routine generation contradiction** — The feature specs for `update-routine`, `pause-routine`, and `resume-routine` reference "generated tasks" (e.g., "future generated tasks use the new routine configuration," "existing generated tasks remain unchanged"). This directly contradicts `docs/04_contexts/tasks.md`, which explicitly states routines do **not** generate Task aggregates and are projected on-the-fly only. This spec follows the context document as canonical. References to "generated tasks" in the feature specs should be treated as stale.

2. **Task in-progress state** — `docs/04_contexts/tasks.md` defines four task lifecycle states: pending, in progress, completed, cancelled. The `StartTask` command (pending → in progress) is listed in the domain command inventory but has no feature spec. The behavioral rule for initiating the in-progress transition is not currently documented.

3. **Unassignment** — `UnassignTask` is listed as a domain command but has no feature spec. The behavior of removing an assignee from a task is not specified.

4. **Task Renaming** — `RenameTask` is listed as a domain command but has no feature spec.

5. **Routine Deletion** — `DeleteRoutine` is listed as a domain command but has no feature spec.

6. **Routine kind (Scheduled | Cue)** — Routine creation requires a `Kind` field with values Scheduled or Cue. The behavioral distinction between these values is not described in any source document.

7. **Responsibility domain reference** — A task may optionally reference a responsibility domain for contextual grouping in read models. The behavior when the referenced domain is archived or deleted is not specified.

8. **Timed pause auto-resume** — `pause-routine.md` introduces a `pausedUntil` optional date on Routine Pause. Whether reaching that date triggers automatic resumption or requires an explicit resume command is not specified in any source document.

---

## Source References

- `docs/04_contexts/tasks.md` — primary context document: aggregate definitions, lifecycle invariants, projection model, domain events, boundary rules
- `specs/features/tasks/create-task.md`
- `specs/features/tasks/assign-task.md`
- `specs/features/tasks/reschedule-task.md`
- `specs/features/tasks/complete-task.md`
- `specs/features/tasks/cancel-task.md`
- `specs/features/tasks/create-routine.md`
- `specs/features/tasks/update-routine.md`
- `specs/features/tasks/pause-routine.md`
- `specs/features/tasks/resume-routine.md`
- `00_product/surfaces/agenda.md` — Agenda projection grammar, temporal entry model, item display grammar
- `docs/03_domain/ubiquitous-language.md` — canonical term definitions for Task, Routine, Agenda, Projection

# DomusMind — System Spec

## Purpose

This document defines the functional scope of DomusMind V1 at system level.

It links:

- bounded contexts
- feature specifications
- core capabilities

It is the entry point for executable product scope.

---

## V1 Scope

DomusMind V1 includes three core bounded contexts:

- Family
- Calendar
- Tasks

These contexts provide the minimum viable household operating model.

Areas are a lightweight supporting concept used for optional classification inside timeline-oriented read models.

---

## Core Capabilities

V1 supports the following capability groups:

- family structure management
- event scheduling
- task execution
- routine management
- unified family timeline
- optional classification of entries using areas

---

## Context Map

### Family
Owns household identity and structure.

### Calendar
Owns events, schedules, participants, and reminders.

### Tasks
Owns tasks, routines, assignment, and completion.

### Areas
Lightweight classification used by read models to improve recognition and filtering.

---

## Context Dependencies

- Calendar depends on Family
- Tasks depends on Family
- Tasks may react to Calendar events
- Areas are a supporting concept inside Calendar, Timeline, and Tasks read models

---

## V1 Feature Set

### Family
- create-family
- add-member

### Calendar
- schedule-event
- reschedule-event
- cancel-event
- add-event-participant
- remove-event-participant
- add-reminder
- remove-reminder
- view-family-timeline

### Tasks
- create-task
- assign-task
- reassign-task
- complete-task
- cancel-task
- reschedule-task
- create-routine
- update-routine
- pause-routine
- resume-routine

### Supporting Read Models
- get-household-areas

---

## Deferred to V1.1

The following were considered for V1 but are deferred to V1.1.
They have no blocking dependency on V1 completion.

### Family
- assign-relationship — relationship semantics between members are modeled in the domain but the capability is not exposed via API or UI in V1
- remove-member — member removal requires validating open task assignments and participant references; deferred to avoid cascading complexity in V1

---

## Out of Scope for V1

The following are explicitly outside V1:

- properties
- documents
- food and meal planning
- inventory
- pets as separate operational context
- finance
- AI automation
- external integrations

---

## Implementation Rule

Every feature spec must map to:

- one bounded context when it mutates domain state
- one aggregate when it mutates domain state
- one vertical slice

Supporting read models may compose data without introducing a new bounded context.

---

## Success Criteria

DomusMind V1 is complete when:

- household identity can be created
- members can be managed
- events can be scheduled
- tasks can be executed
- routines can be maintained
- timeline can be queried
- entries can optionally be classified with areas where available

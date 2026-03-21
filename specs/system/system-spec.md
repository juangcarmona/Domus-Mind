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

DomusMind V1 includes four core bounded contexts:

- Family
- Responsibilities
- Calendar
- Tasks

These contexts provide the minimum viable household operating model.

---

## Core Capabilities

V1 supports the following capability groups:

- family structure management
- responsibility ownership
- event scheduling
- task execution
- routine management
- unified family timeline

---

## Context Map

### Family
Owns household identity and structure.

### Responsibilities
Owns accountability and ownership of household domains.

### Calendar
Owns events, schedules, participants, and reminders.

### Tasks
Owns tasks, routines, assignment, and completion.

---

## Context Dependencies

- Responsibilities depends on Family
- Calendar depends on Family
- Tasks depends on Family
- Tasks may react to Calendar events
- Tasks may reference Responsibility domains

---

## V1 Feature Set

### Family
- create-family
- add-member

### Responsibilities
- create-responsibility-domain
- assign-primary-owner
- assign-secondary-owner
- transfer-responsibility

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

- one bounded context
- one aggregate
- one vertical slice

No feature may bypass aggregate boundaries.

---

## Success Criteria

DomusMind V1 is complete when:

- household identity can be created
- members can be managed
- responsibilities can be assigned
- events can be scheduled
- tasks can be executed
- routines can be maintained
- timeline can be queried
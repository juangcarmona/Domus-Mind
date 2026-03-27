# DomusMind - Domain Events

Status: Canonical
Audience: Product / Engineering / Architecture
Scope: V1
Owns: Cross-context event principles and stable event contracts
Depends on: context-map.md, system-spec.md, context documents
Replaces: previous broad/future-facing event catalog

## Purpose

Domain events represent facts that have already occurred inside DomusMind.

They exist to support:

- decoupled bounded contexts
- cross-context reactions
- auditability
- stable system behavior after state change

This document defines only the current event model for the active V1 domain shape.

It does not define future domains, speculative automation, or out-of-scope integrations.

---

## Core Principles

### Events represent facts

A domain event describes something that has already happened.

Examples:

```text
FamilyCreated
MemberAdded
EventScheduled
TaskCompleted
````

Events are immutable.

### Events use past-tense names

Correct:

```text
EventScheduled
TaskAssigned
SharedListCreated
```

Incorrect:

```text
ScheduleEvent
AssignTask
CreateSharedList
```

Commands express intent.
Events express outcomes.

### Events are emitted after successful state change

An event is published only after the owning aggregate has changed state successfully.

Events are never speculative.

### Events belong to one bounded context

Each event has exactly one owning bounded context.

Rules:

* only the owning context may emit the event
* other contexts may subscribe
* other contexts must not emit the same event as if they owned it

### Events are long-lived contracts

Domain events are system contracts.

Avoid casual renaming or semantic drift.

When semantics change materially:

* introduce a new event, or
* version the contract explicitly

---

## Event Structure

Every domain event must carry enough metadata to support traceability and processing.

Minimum shape:

```text
EventId
EventType
OccurredAt
AggregateId
Payload
```

Example:

```json
{
  "eventId": "evt_98fa3b",
  "eventType": "MemberAdded",
  "occurredAt": "2026-01-01T10:00:00Z",
  "aggregateId": "family_123",
  "payload": {
    "memberId": "member_456",
    "displayName": "Lucas"
  }
}
```

The exact transport envelope may vary by implementation, but the semantic contract must remain clear.

---

## Current Event Categories

DomusMind V1 currently recognizes five core bounded contexts:

* Family
* Responsibilities
* Calendar
* Tasks
* Shared Lists

Only events belonging to these active contexts belong in this document.

---

## Family Events

Family owns household identity.

Stable events:

```text
FamilyCreated
MemberAdded
MemberRemoved
```

Typical downstream uses:

* validate assignments
* validate participants
* validate task assignees
* validate list collaborators if needed by the model

---

## Responsibility Events

Responsibilities owns accountability structure.

Stable events:

```text
ResponsibilityDomainCreated
ResponsibilityDomainRenamed
PrimaryOwnerAssigned
SecondaryOwnerAssigned
SecondaryOwnerRemoved
ResponsibilityParticipantAdded
ResponsibilityParticipantRemoved
ResponsibilityTransferred
ResponsibilityDomainArchived
```

Typical downstream uses:

* update read models
* inform task suggestion or assignment rules
* support responsibility visibility in household views

---

## Calendar Events

Calendar owns time-bound commitments.

Stable events:

```text
EventScheduled
EventRescheduled
EventCancelled
EventCompleted
EventParticipantAdded
EventParticipantRemoved
ReminderAdded
ReminderRemoved
```

Typical downstream uses:

* update household timeline
* update coordination views
* enable downstream preparation work in Tasks

---

## Task Events

Tasks owns execution state and recurring operational definitions.

Stable task events:

```text
TaskCreated
TaskAssigned
TaskReassigned
TaskCompleted
TaskCancelled
TaskRescheduled
```

Stable routine events:

```text
RoutineCreated
RoutineUpdated
RoutinePaused
RoutineResumed
RoutineDeleted
```

Typical downstream uses:

* update household timeline
* update week coordination views
* update personal and household task projections

---

## Shared List Events

Shared Lists owns collaborative list-based capture and shared list state.

Current stable events should map only to implemented list behavior.

Recommended V1 event set:

```text
SharedListCreated
SharedListRenamed
SharedListArchived
SharedListItemAdded
SharedListItemToggled
SharedListItemRemoved
```

Typical downstream uses:

* update list read models
* surface household shopping / supply / checklist state
* support timeline or coordination projections only where explicitly designed

If the implementation does not yet support one of these transitions, do not emit the event until the behavior exists in the domain.

---

## Cross-Context Consumption

Contexts may consume events from other contexts, but must not reach across boundaries to mutate foreign aggregates directly.

Examples:

### Family → Responsibilities

```text
MemberAdded
MemberRemoved
```

Possible reactions:

* validate assignments
* reconcile invalid owners or participants

### Family → Calendar

```text
MemberAdded
MemberRemoved
```

Possible reactions:

* validate participants
* reconcile invalid participant references

### Family → Tasks

```text
MemberAdded
MemberRemoved
```

Possible reactions:

* validate assignees
* remove or flag invalid assignments

### Calendar → Tasks

```text
EventScheduled
EventRescheduled
```

Possible reactions:

* support preparation-work flows where explicitly modeled

### Responsibilities → Tasks

```text
PrimaryOwnerAssigned
ResponsibilityTransferred
```

Possible reactions:

* update suggestion logic
* update responsibility-aware projections

Cross-context reactions happen after commit.

---

## Processing Model

Typical flow:

```text
Command
→ Aggregate
→ State Change
→ Domain Event Emitted
→ Persisted / Logged
→ Subscribers React
```

Event handling should preserve these rules:

* emit only after successful state change
* avoid synchronous cross-context mutation
* keep handlers focused and deterministic
* prefer additive downstream reactions over hidden coupling

---

## Storage and Logging

Domain events may be stored for:

* audit trails
* debugging
* projections
* integration with internal processing

Full event sourcing is not required.

Event logging does not change event ownership rules.

---

## Versioning

When event contracts evolve:

* prefer backward-compatible payload changes where feasible
* introduce a new version only when semantics materially change
* keep consumers tolerant to older payloads when possible

Avoid unnecessary version churn.

---

## Scope Guardrail

This document does not define events for domains that are not part of the active V1 model.

Out of scope here:

* property
* administration
* inventory
* food
* finance
* pets as a separate operational context
* AI automation
* external integrations

Those concepts must not appear here unless they become part of the active system scope.

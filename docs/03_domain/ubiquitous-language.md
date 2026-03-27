# DomusMind - Ubiquitous Language

Status: Canonical
Audience: Product / Engineering / Architecture / API Design
Scope: V1
Owns: Shared domain vocabulary for active system concepts
Depends on: system-spec.md, context-map.md, context documents
Replaces: previous broad/future-facing vocabulary document

## Purpose

This document defines the shared vocabulary of the active DomusMind domain.

All code, documentation, APIs, and design discussions must use these terms consistently.

The goal is simple:

- one concept
- one meaning
- one preferred term

This document covers only current V1 concepts.

It does not define vocabulary for speculative or out-of-scope domains.

---

## Language Layers

DomusMind uses three language layers.

### 1. Internal domain language

The precise terms used by bounded contexts and code.

Examples:

- Event
- Task
- Routine
- Responsibility Domain
- Shared List

### 2. Household-facing language

The terms users should see in product surfaces.

Examples:

- Plan
- Task
- Routine
- List

### 3. Read-model language

Projection-only terms used to explain derived views.

Examples:

- Timeline
- Today view
- Week view
- Marker

Read-model terms are not aggregates unless explicitly modeled as such.

---

## Core System Concepts

### Family

A **Family** is the primary household boundary of the system.

It is the root organizational unit for identity and most operations.

A family defines:

- who belongs to the household
- the boundary for permissions and visibility
- the boundary for most context-owned data

### Member

A **Member** is a person belonging to a family.

Members may participate in:

- responsibilities
- plans
- tasks
- routines
- shared lists

A member is part of the Family context.

### Responsibility Domain

A **Responsibility Domain** is an area of household accountability.

Examples:

```text
school
food
maintenance
administration
````

A responsibility domain answers:

> Who owns this area of household life?

### Responsibility Assignment

A **Responsibility Assignment** defines how a member participates in a responsibility domain.

Roles may include:

* primary owner
* secondary owner
* participant

Responsibility structure belongs to the Responsibilities context.

---

## Time and Coordination

### Plan

A **Plan** is the household-facing term for something scheduled in time that affects the household.

Examples:

```text
football practice
dentist appointment
school trip
family dinner
```

Users should see **Plan** in household-facing experiences where natural language matters.

A plan answers:

* what is happening
* when it happens
* who is involved

### Event

An **Event** is the internal Calendar-context model for a time-bound commitment.

Events:

* belong to the Calendar context
* have schedule semantics
* may have participants
* may have reminders
* may recur

Users do not need to think in terms of Events.
In household language, they experience them as Plans.

### Timeline

The **Household Timeline** is the chronological read model of things affecting the household.

It may include:

* plans
* tasks
* routines
* reminders
* other explicitly supported household entries

The timeline answers:

> What matters today for this household?

The Timeline is a read model, not a bounded context.

### Reminder

A **Reminder** is a scheduled prompt associated with a time-bound commitment.

In V1, reminders belong to Calendar semantics around Events.

A reminder is not a task.

---

## Household Work

### Task

A **Task** is a concrete action that must be completed.

Examples:

```text
buy groceries
prepare school bag
pay electricity bill
bring documents
```

Task is both:

* the internal domain term
* the household-facing term

There is no translation layer here.

A task:

* belongs to the Tasks context
* may be assigned
* may have a due date
* has a lifecycle state

### Routine

A **Routine** is a recurring definition of operational household work.

Examples:

```text
weekly trash
daily pet feeding
monthly bill review
weekly grocery shopping
```

A routine defines repeating work.
It is not itself a task instance.

A routine:

* belongs to the Tasks context
* expresses recurrence for operational work
* appears in read models through projection
* must not be confused with a recurring plan

### Shared List

A **Shared List** is a collaborative household list owned by the Shared Lists context.

Examples:

```text
groceries
home supplies
packing checklist
school materials
```

A shared list is used for lightweight shared capture and shared state.

A shared list is not:

* a task board
* a calendar
* a responsibility domain

It is a list.

### Shared List Item

A **Shared List Item** is an entry inside a shared list.

Examples:

```text
milk
trash bags
passport
notebook
```

A shared list item supports simple shared coordination such as:

* capture
* visibility
* checked / unchecked state

Do not call a shared list item a task unless it truly belongs to the Tasks context.

---

## Read-Model Concepts

### Marker

A **Marker** is a projection-only coordination cue.

Examples:

```text
busy day
school morning
trash day
```

A marker:

* is not a domain entity
* is not persisted as an aggregate
* exists only to improve visibility in read models

### Today View

The **Today View** is a product surface showing what matters today.

It is not a bounded context.
It is a projection over data owned by multiple contexts.

### Week View

The **Week View** is a coordination surface showing the upcoming household week.

It may combine information from:

* Calendar
* Tasks
* Shared Lists
* derived coordination cues where explicitly supported

It is a read-model surface, not a domain model.

---

## Architectural Concepts

### Domain Event

A **Domain Event** is a fact that has already occurred in the domain.

Examples:

```text
FamilyCreated
EventScheduled
TaskCompleted
SharedListItemAdded
```

Domain events are emitted by the owning bounded context after successful state change.

### Aggregate

An **Aggregate** is a consistency boundary in the domain model.

It enforces invariants and owns state transitions.

Examples in the active domain include:

```text
Family
Event
Task
Routine
ResponsibilityDomain
SharedList
```

### Aggregate Root

An **Aggregate Root** is the entry point through which an aggregate is modified.

All state-changing behavior must pass through the root.

### Bounded Context

A **Bounded Context** is a model boundary with its own language and rules.

DomusMind V1 has five core bounded contexts:

```text
Family
Responsibilities
Calendar
Tasks
Shared Lists
```

Nothing else should be treated as a current bounded context unless system scope changes.

---

## Consistency Rules

To maintain conceptual clarity:

* use **Plan** as the household-facing term for calendar events
* use **Event** as the Calendar-context internal term
* use **Task** as both household-facing and internal term
* do not reintroduce **Chore** as a separate core term
* use **Routine** only for recurring operational work
* use **Shared List** and **Shared List Item** for list-based coordination
* do not call list items tasks unless they truly belong to the Tasks context
* do not use read-model terms as if they were aggregates
* avoid synonyms when a canonical term already exists

---

## Terms to Avoid

Avoid these unless a future scope change explicitly introduces them:

* Property
* Asset
* Inventory
* Meal Plan
* Recipe
* Contract
* Document as a standalone bounded-context concept
* Pet as a separate bounded context
* Chore as a distinct modeled entity

These may exist in future discussions, but they are not part of the current active ubiquitous language.

---

## Summary

DomusMind’s active ubiquitous language is intentionally narrow.

Current core vocabulary centers on:

* Family
* Member
* Responsibility Domain
* Plan / Event
* Task
* Routine
* Shared List
* Shared List Item
* Timeline
* Domain Event

This document must stay aligned with the active system scope.
When scope changes, update this file deliberately.

```

Use these as direct replacements.


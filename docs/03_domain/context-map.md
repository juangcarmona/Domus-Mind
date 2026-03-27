# DomusMind - Context Map

This document describes how bounded contexts collaborate inside DomusMind.

Core contexts in V1:

* Family
* Responsibilities
* Calendar
* Tasks
* Shared Lists

---

# Context Relationships

Family is the upstream identity provider.

Responsibilities defines accountability using Family members.

Calendar defines time structure using Family participants.

Tasks defines execution using Family members and references to responsibilities and events.

Shared Lists defines household list-based coordination, shared capture, and lightweight shared state.

---

# Dependency Graph

The dependency structure is **not a linear chain**.

Responsibilities, Calendar, Tasks, and Shared Lists all depend on Family. Tasks may react to Calendar and Responsibilities. Shared Lists may reference Responsibilities and may optionally link to Calendar entities, but remains behaviorally independent.

```
                 Family
          /         |         |         \
         ↓          ↓         ↓          ↓
Responsibilities Calendar    Tasks   Shared Lists
         \          /
          ↓        ↓
             event reactions
```

Dependency interpretation:

* **Family** provides identity and relationship structure
* **Responsibilities** depends on Family for ownership assignments
* **Calendar** depends on Family for participant identity
* **Tasks** depends on Family for assignees and may react to Calendar and Responsibilities events
* **Shared Lists** depends on Family for ownership and identity
* Shared Lists may reference Responsibilities domains for grouping and soft ownership
* Shared Lists may optionally link to Calendar entities for contextual use
* Shared Lists does not depend on Tasks
* Tasks does not depend on Shared Lists

Interpretation:

* Family → identity
* Responsibilities → accountability
* Calendar → time
* Tasks → execution
* Shared Lists → shared capture and lightweight shared state
---

## Collaboration Model

Contexts collaborate using **domain events**.

No context may directly modify another context's aggregates.

Communication rules:

* identity flows from Family
* accountability flows from Responsibilities
* time flows from Calendar
* execution happens in Tasks
* shared capture happens in Shared Lists

Contexts react to events rather than forming direct structural dependencies.


## Shared Lists Interaction

Shared Lists introduces a new coordination pattern:

* persistent shared state
* shared capture
* reusable checklists
* toggle-based semantics

Examples:

### List Used During Event

Calendar emits:

EventScheduled

Shared Lists does not react automatically, but a list may be linked manually:

Example:

Event: Trip

Linked list:

* documents
* clothes
* equipment

---

### Household List Usage

User interaction (not event-driven):

* member updates list at home
* another member uses list in store
* real-time shared coordination

No domain events required across contexts.

---

### Responsibilities Context Interaction

Responsibilities may provide:

* area categorization
* soft ownership

Shared Lists must not modify responsibility assignments.
---

# Context Interaction Examples

## Member Added

Family emits:

```
MemberAdded
```

Other contexts may react:

* Responsibilities may update assignment validity
* Calendar may validate participants
* Tasks may validate task assignments
* Shared Lists may validate family-scoped sharing rules

---

## Event Scheduled

Calendar emits:

```
EventScheduled
```

Tasks may react:

```
Generate preparation tasks
```

Example:

Event: School Trip

Generated tasks:

* prepare backpack
* sign permission form

---

## Responsibility Assigned

Responsibilities emits:

```
PrimaryOwnerAssigned
```

Other contexts may react by updating references that depend on explicit household ownership.

Shared Lists may continue to reference the same area without taking over responsibility logic.

---

# Context Boundaries

Each context owns specific responsibilities.

Family owns:

* household identity
* members
* dependents
* pets

Responsibilities owns:

* responsibility domains
* ownership assignments

Calendar owns:

* events
* schedules
* reminders

Tasks owns:

* tasks
* routines
* completion state

Contexts must not leak responsibilities across boundaries.

---

# Design Principle

DomusMind follows **strict bounded contexts**.

Contexts communicate through events and identifiers.

No context should require direct access to another context's internal model.

Dependencies represent **identity and information flow**, not permission to modify state.

---

# Summary

The DomusMind core model is built around five cooperating contexts:

* **Family** → identity
* **Responsibilities** → accountability
* **Calendar** → time
* **Tasks** → execution
* **Shared Lists** → shared capture and lightweight shared state

Responsibilities, Calendar, Tasks, and Shared Lists all depend on Family.

Tasks integrates signals from Family, Calendar, and Responsibilities to coordinate execution.

Shared Lists provides a parallel coordination layer focused on reusable, persistent checklist state, independent from execution and time semantics.
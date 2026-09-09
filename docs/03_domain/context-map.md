# DomusMind - Context Map

This document describes how bounded contexts collaborate inside DomusMind.

Core contexts in V1:

* Family
* Responsibilities
* Calendar
* Tasks
* Shared Lists

V2 contexts:

* Meal Planning

---

# Context Relationships

Family is the upstream identity provider.

Responsibilities defines accountability using Family members.

Calendar defines time structure using Family participants.

Tasks defines execution using Family members and references to responsibilities and events.

Shared Lists defines the household execution container for captured items — supporting a spectrum from lightweight memory to actionable, temporally-enriched entries that project into the Agenda surface.

Meal Planning (V2) defines weekly household meal coordination and recipe management. It depends on Family for identity, Shared Lists for shopping list creation, and Responsibilities for soft area ownership. It does not own shopping lists — those are created in the Shared Lists context via event-driven integration.

---

# Dependency Graph

The dependency structure is **not a linear chain**.

Responsibilities, Calendar, Tasks, and Shared Lists all depend on Family. Tasks may react to Calendar and Responsibilities. Shared Lists may reference Responsibilities and may optionally link to Calendar entities. Shared Lists may project temporally-enriched items into the Agenda surface without owning Calendar semantics.

Meal Planning (V2) depends on Family for identity. It depends on Shared Lists for shopping list creation via event-driven integration. It references Responsibilities for soft area ownership. Meal slot entries may project into the Agenda surface as a read concern.

```
                       Family
          /       |         |         \         \
         ↓        ↓         ↓          ↓         ↓
Responsibilities Calendar  Tasks  Shared Lists  Meal Planning
         \        /                    ↑     \       |
          ↓      ↓                     |      ↓      ↓ (event)
        event reactions            (event)  Agenda  Shopping
                                  ShoppingListRequested → SharedList
```

Dependency interpretation:

* **Family** provides identity and relationship structure
* **Responsibilities** depends on Family for ownership assignments
* **Calendar** depends on Family for participant identity
* **Tasks** depends on Family for assignees and may react to Calendar and Responsibilities events
* **Shared Lists** depends on Family for ownership and identity
* Shared Lists may reference Responsibilities domains for grouping and soft ownership
* Shared Lists may optionally link to Calendar entities for contextual use
* Shared Lists may carry item-level temporal fields (due date, reminder, repeat)
* Items with temporal fields project into the Agenda surface — Calendar remains the source of truth for time
* Shared Lists does not depend on Tasks
* Tasks does not depend on Shared Lists
* **Meal Planning** depends on Family for identity
* Meal Planning depends on Shared Lists indirectly via `ShoppingListRequested` event — Shared Lists creates the shopping list, Meal Planning does not own it
* Meal Planning carries a soft reference to Responsibilities (food area) — Responsibilities does not depend on Meal Planning
* Meal slot entries project into Agenda as a read concern — Calendar remains the source of truth for time

Interpretation:

* Family → identity
* Responsibilities → accountability
* Calendar → time (source of truth)
* Tasks → structured, explicit execution
* Shared Lists → household execution container (lightweight to actionable)
* Meal Planning → weekly meal coordination and recipe management

### Context Execution Boundary

Lists own **capture and flexible execution**.
Tasks own **structured execution lifecycle**.
Calendar owns **time**.
Meal Planning owns **weekly meal structure and recipe library**.

These roles are complementary and intentionally distinct.
A list item is not a task.
A temporal field on a list item is a reference into time, not ownership of time.
---

## Collaboration Model

Contexts collaborate using **domain events**.

No context may directly modify another context's aggregates.

Communication rules:

* identity flows from Family
* accountability flows from Responsibilities
* time flows from Calendar (Calendar is the source of truth for time)
* structured execution happens in Tasks
* household capture and flexible execution happen in Shared Lists
* temporal item data in Shared Lists projects into Agenda — it does not flow back into Calendar
* meal planning coordination belongs to Meal Planning; shopping list execution belongs to Shared Lists

Contexts react to events rather than forming direct structural dependencies.


## Shared Lists Interaction

Shared Lists introduces a household execution container pattern:

* persistent shared state
* shared capture
* reusable execution containers
* toggle-based semantics
* item-level importance, temporal fields, and repeat rules

### Temporal Item Projection

When an item carries temporal fields (due date, reminder, repeat), it becomes eligible for projection into the Agenda surface.

Projection rules:

* projection is read-only from Calendar's perspective
* projected items remain editable only through the Lists surface
* projected items must be distinguishable from Calendar events and Tasks in Agenda
* Calendar retains full ownership of time semantics
* no cross-context event is required for read-only projection

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

## Meal Planning Interaction (V2)

Meal Planning coordinates weekly household meals and maintains the recipe library.

### Shopping List Generation

Meal Planning emits:

```
ShoppingListRequested
```

Shared Lists reacts by:

* creating a `SharedList` of kind `shopping`
* pre-populating it with consolidated ingredient items from the meal plan
* emitting `SharedListCreated`

Meal Planning does not own the resulting shopping list.
The shopping list is a first-class `SharedList` from the moment it is created.
Further purchase coordination (checking items, adding extras) happens entirely within Shared Lists.

### Tasks from Meal Planning

No automatic task creation from meal plans.

When a household member wants to create a preparation task (e.g., "defrost chicken"), they explicitly create a Task in the Tasks context.
The task may optionally carry a reference to the meal plan for context.
No event-driven automation takes place.

### Responsibilities Area Reference

A `MealPlan` may carry a soft reference to a `ResponsibilityDomainId` (e.g., `food`).

This is informational and optional.
Meal Planning does not assign or modify responsibility ownership.

### Agenda Projection

Meal slot entries for a given week project into the Agenda surface for visibility alongside tasks and plans.

Projection rules:

* projection is read-only
* projected meal slots are visually distinguishable from Calendar events and Tasks
* Calendar remains the source of truth for time

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

EventScheduled

Tasks may react:

* update read models
* support coordination views
* enable future task suggestions without creating tasks automatically

Example:

Event: School Trip

Related work (if explicitly created by the household):

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
* time (source of truth)

Tasks owns:

* tasks
* routines
* structured execution lifecycle
* completion state and assignment

Shared Lists owns:

* list containers
* item identity and base state
* item capability state (importance, temporal fields)
* household capture and flexible execution

Meal Planning owns:

* meal plan identity, structure, and lifecycle
* meal slot assignment within a plan
* household recipe library
* weekly meal templates
* ingredient consolidation and shopping list derivation request

Contexts must not leak responsibilities across boundaries.

A list item with a due date does not become a Calendar event.
A list item with a due date does not become a Task.
Temporal enrichment on an item enables **projection** into the Agenda surface.
Projection is a read concern, not an ownership transfer.

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
* **Calendar** → time (source of truth)
* **Tasks** → structured execution lifecycle
* **Shared Lists** → household execution container (capture to action, with optional time reference)

Responsibilities, Calendar, Tasks, and Shared Lists all depend on Family.

Tasks integrates signals from Family, Calendar, and Responsibilities to coordinate structured execution.

Shared Lists provides a flexible execution layer from simple memory to temporally-enriched actionable items that project into the Agenda surface. It is not a duplicate of Tasks. It is not a clone of Calendar. It is the household's primary capture container.
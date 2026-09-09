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

Note: While the product-facing term is "List", internal domain language may still contain residual "SharedList" names during migration. The target end-state should be "List" consistently unless there is a deliberate reason not to rename aggregate/class names.

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
- Agenda day view
- Agenda week view
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

It includes:

* plans (Calendar Events)
* tasks (Tasks context, due-date bearing)
* routines (Tasks context, projected occurrences)
* temporal list items (Shared Lists context, items with due date, reminder, or repeat)

The timeline is a read model, not a bounded context.
It is assembled from multiple write-model sources.
It does not collapse or merge the entities it projects.

Phase 1 external calendar entries are **not** included in the household timeline.
They project into the member-scoped Agenda read model only.

### Reminder

A **Reminder** is a scheduled prompt associated with a time-bound commitment or a temporal list item.

Reminders may belong to Calendar semantics (on an Event) or to a list item's temporal fields.
In the Event model, reminders are relative (e.g. 30 minutes before).
On a list item, reminder is an absolute `DateTimeOffset` used for Agenda projection.

A reminder is not a task.

---

## Agenda: Unified Temporal Read Surface

The **Agenda** is the unified temporal read surface for the household.

It gathers temporal entries from multiple write-model sources and projects them into a single surface:

| Source | Entry type |
| ------ | ---------- |
| Calendar: Event | Plan |
| Tasks: Task | Task |
| Tasks: Routine | Routine (projected occurrence) |
| Shared Lists: SharedListItem (with temporal fields) | List Item projection |
| External integration: ExternalCalendarEntry | Imported external entry (member scope only) |

**Architectural invariant: write model is divided; read model is unified.**

- Calendar owns Event. Agenda does not.
- Tasks owns Task and Routine. Agenda does not.
- Shared Lists owns SharedListItem. Agenda does not.
- External calendar entries are read-only. Agenda projects them, not owns them.

Agenda is the only place where these entry types appear together.
No entity crosses a context boundary in the write model.
Projection is a read concern only.

**Shared temporal vocabulary** — due date, reminder, repeat — is reused across contexts.
This is intentional. Shared vocabulary does not imply shared entities.

### External Calendar Connection

An **External Calendar Connection** is a member-scoped delegated connection to a third-party calendar provider account.

Examples:

- Microsoft Outlook account

An external calendar connection answers:

- which member owns the connection
- which provider account is connected
- which provider calendars are selected for ingestion
- how synchronization is configured

An external calendar connection is not a native household plan source.
It is an integration boundary for importing read-only time data.

### External Calendar Feed

An **External Calendar Feed** is one selected provider calendar under an external calendar connection.

Examples:

- Outlook default calendar
- Outlook work calendar
- Outlook school calendar

An external calendar feed answers:

- which provider calendar is selected
- which sync horizon is active
- which incremental sync cursor applies to that view

### External Calendar Entry

An **External Calendar Entry** is a read-only imported calendar occurrence from an external provider.

Examples:

- work meeting from Outlook
- school pickup reminder from Outlook
- dentist appointment from Outlook

An external calendar entry:

- belongs to an external calendar feed
- is not a Calendar `Event` aggregate
- must not be converted automatically into a native household Plan
- may appear in Agenda projections when relevant

### Sync Horizon

The **Sync Horizon** is the bounded time window used for external calendar ingestion.

Examples:

- now - 1 day to now + 30 days
- now - 1 day to now + 90 days
- now - 1 day to now + 365 days

The sync horizon answers:

- which external occurrences are stored locally
- which delta cursor remains valid for incremental synchronization

The sync horizon is part of the identity of external calendar synchronization state.

---

## Household Work

### Task

A **Task** is a concrete, explicitly managed structured execution entity.

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
* may be assigned to a member
* may have a due date
* has a defined lifecycle state (pending → in progress → completed / cancelled)

A task is not a list item.
A task carries structured execution semantics that a list item does not require.
Do not confuse a list item that has a due date with a Task — they are different things in different contexts.

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

A **Shared List** is the household execution container owned by the Shared Lists context.

Examples:

```text
groceries
home supplies
packing checklist
school materials
```

A shared list is used for household capture and flexible execution.

It supports a spectrum from simple memory to actionable, temporally-enriched items.

A shared list is not:

* a task board
* a calendar
* a responsibility domain

It is a household execution container.

### Shared List Item

A **Shared List Item** is a polymorphic execution unit inside a shared list.

Examples:

```text
milk
trash bags
passport
notebook
```

A shared list item supports a range of execution semantics:

* base: name, checked/unchecked state, note, quantity
* optional: importance flag
* optional: temporal fields — due date, reminder, repeat

Items with temporal fields may **project** into the Agenda surface.

A shared list item is not a task.
A shared list item does not become a task automatically, regardless of what capabilities it carries.

### Projection

**Projection** is the read-model operation by which temporally-enriched list items appear in the Agenda surface.

A list item with a due date, reminder, or repeat rule is eligible for projection.

Projection rules:

* the item remains owned by Shared Lists
* the item appears in Agenda with a list-origin distinguishing cue
* the item cannot be edited from Agenda — edits go through Lists
* Calendar is not involved in the projection — it is a read surface concern
* no cross-context command is issued to produce the projection

Projection is a read concern, not an ownership transfer.

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

### Agenda View

The **Agenda View** is the household temporal read surface.

It can show household or individual scope across day, week, and month time windows.

It is not a bounded context.
It is a set of projections over data owned by Calendar, Tasks, Family, and Shared Lists.

Projected sources include:

* Calendar events (Plans)
* Tasks (due date-bearing)
* Routines (on-the-fly projection)
* Imported external calendar entries (in Member scope)
* Shared List Items carrying temporal fields (due date or reminder)

### Day View

The **Day View** is a single-day projection within the Agenda surface.

In household scope it presents a compact per-member board.
In member scope it presents an hour-positioned timeline for one person.

### Week View

The **Week View** is a 7-day coordination projection within the Agenda surface.

It may combine information from:

* Calendar
* Tasks
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
* Contract
* Document as a standalone bounded-context concept
* Pet as a separate bounded context
* Chore as a distinct modeled entity

These may exist in future discussions, but they are not part of the current active ubiquitous language.

---

## Meal Planning Vocabulary (V2)

The following terms enter the active vocabulary with the Meal Planning (V2) context.

### Meal Plan

A **Meal Plan** is the weekly household coordination unit for meals.

It organizes meal slot assignments across a Monday–Sunday week.

- belongs to Meal Planning context
- household-facing term: **Meal Plan**
- internal domain term: `MealPlan`

### Meal Slot

A **Meal Slot** is a single meal position within a meal plan defined by day of week and meal type.

A meal slot may or may not have an assigned recipe.

- belongs to `MealPlan` as an internal entity
- identified by: day of week × meal type

### Recipe

A **Recipe** is a household-defined set of ingredients with optional preparation notes.

Recipes belong to the household recipe library.

- belongs to Meal Planning context
- may be assigned to one or more meal slots

### Ingredient

An **Ingredient** is a named component of a recipe with an optional quantity and unit.

- belongs to `Recipe` as an internal entity

### Weekly Template

A **Weekly Template** is a reusable named weekly meal pattern.

It enables households to apply a familiar routine to a new week without planning from scratch.

- belongs to Meal Planning context
- internal term: `WeeklyTemplate`

### Shopping List (in Meal Planning context)

When a household requests a shopping list from a meal plan, Meal Planning emits `ShoppingListRequested`.  
The resulting list is a `SharedList` of kind `shopping` in the **Shared Lists context**.

The shopping list is always a Shared Lists artifact.
Meal Planning does not own shopping lists.

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


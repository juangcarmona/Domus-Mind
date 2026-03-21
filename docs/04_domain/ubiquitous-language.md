# DomusMind — Ubiquitous Language

## Purpose

This document defines the **shared vocabulary of the DomusMind domain**.

All code, documentation, APIs, and discussions about the system must use these terms consistently.

The goal is to maintain **conceptual clarity across the system**.

The vocabulary distinguishes between:

* **internal domain model terms**
* **household-facing language**
* **read-model concepts**

This separation ensures the system can evolve internally while remaining intuitive for household users. 

---

# Core Concepts

## Family

A **Family** is the primary organizational unit of the system.

A family represents a group of people managing a shared household and its associated responsibilities, resources, and logistics.

A family acts as the root boundary for most system operations.

---

## Member

A **Member** is a person belonging to a family.

Members may represent:

* adults
* children
* dependents
* caregivers

Members participate in responsibilities, plans, tasks, and routines.

---

## Dependent

A **Dependent** is an entity requiring care or responsibility from family members.

Examples:

```
children
elderly relatives
pets
```

Dependents generate plans, tasks, and responsibilities.

---

## Responsibility Domain

A **Responsibility Domain** represents an area of household management.

Examples:

```
food
school
finances
maintenance
travel
pets
administration
```

Responsibility domains distribute cognitive ownership across the family.

---

## Responsibility Assignment

A **Responsibility Assignment** defines which members manage a responsibility domain.

Assignments may include:

* primary owner
* secondary owners
* participants

---

# Time and Coordination

## Plan (Household Term)

A **Plan** is a household-facing concept representing something scheduled in time that affects the family.

Examples:

```
Mateo football practice
Lucía dentist appointment
School excursion
Family trip
```

Plans appear in the **Household Timeline** and communicate what the household is doing.

Plans are the **household language representation** of calendar activities.

Plans are **primarily one-time scheduled commitments** with a specific date, time, and optional participants. Recurring time-bound commitments (such as football practice every Tuesday) may be modeled as Calendar Events with a recurrence rule — they still appear in the household experience as **Plans**.

A **Routine** is fundamentally different from a recurring Plan. A Routine represents repeating *operational household work* (trash, cleaning, bill review). A Plan represents a *scheduled commitment* that places participants at a specific time and place. The distinction is intentional and must be preserved.

---

## Event (Internal Model)

An **Event** is the internal calendar-domain model representing a scheduled activity at a specific point in time.

Examples:

```
school excursion
medical appointment
meeting
exam
trip
maintenance visit
```

Events:

* belong to the **Calendar bounded context**
* are stored and processed by the system
* may generate reminders or tasks

Events are **not a user-facing concept**.

In the household experience, users interact with **Plans**, not Events.

---

## Timeline

The **Household Timeline** represents the chronological sequence of things affecting the household.

The timeline aggregates entries originating from multiple contexts, including:

* plans
* tasks
* routines
* reminders

The timeline answers the question:

> What matters today for this household?

---

# Household Work

## Task

A **Task** is the household concept representing a concrete action that must be completed.

Examples:

```
Buy groceries
Prepare school bag
Pay electricity bill
Take dog to the vet
Bring documents
```

**Task** is both the household-facing term and the internal domain model term. There is no translation between them — a task is a task.

Tasks:

* belong to the **Tasks bounded context**
* are created explicitly through user action
* may be assigned to a member
* carry a due date and a status lifecycle
* originate from manual creation

Tasks appear in the **Household Timeline** and in coordination views (Today board, Week grid).

> **Historical note:** An earlier version of this document used the term "Chore" as the household-facing equivalent of Task. That distinction has been removed. "Task" is the single term used at all layers.

---

## Routine

A **Routine** represents **recurring operational behavior in the household**.

Examples:

```
weekly grocery shopping
school preparation
house cleaning
pet feeding
weekly trash
monthly bill review
```

A routine defines **how operational work repeats over time**. Routines have a frequency (daily, weekly, monthly, yearly) and an optional time and scope (household or specific members).

Important clarifications:

* routines define **recurring operational patterns** — they are not tasks
* routines belong to the **Tasks context**
* routines appear in read models (timeline, weekly grid) by being projected on-the-fly against their recurrence schedule
* routines do **not generate Task aggregates** — tasks arise only from explicit user action

**Routines are not recurring Plans.** The boundary is:

| Concept | Example | Where it lives |
|---------|---------|----------------|
| Routine | Trash every Tuesday | Tasks context |
| Recurring Plan | Football practice every Tuesday | Calendar context (Event with recurrence rule) |

The key question: *Is this operational household work, or a scheduled attendance commitment?*

* Operational household work → **Routine**
* Scheduled commitment with participants and a time slot → **Plan** (recurring Calendar Event)

---

# Read Model Concepts

## Marker / Weekly Cue

A **Marker** (or **Weekly Cue**) is a **read-model-only concept** used by coordination views.

Markers are **not domain entities** and do not exist as aggregates.

Markers exist purely in projections such as:

```
WeeklyHouseholdGrid
Timeline coordination views
```

Markers help visualize patterns such as:

```
Trash day
School day
Busy week indicator
```

Markers:

* do not store domain state
* do not emit events
* are generated dynamically from domain data

They exist purely to support **household coordination views**.

---

# Assets and Records

## Property

A **Property** represents a real estate asset managed by the family.

Examples:

```
primary residence
secondary home
rental property
```

Properties may generate expenses, maintenance events, and administrative obligations.

---

## Asset

An **Asset** represents a durable resource owned or managed by the household.

Examples:

```
vehicles
appliances
tools
equipment
```

Assets may require maintenance and generate plans or tasks.

---

## Document

A **Document** represents an important record that must be stored or tracked.

Examples:

```
identity documents
insurance policies
contracts
certificates
```

Documents may include expiration or renewal dates.

---

## Contract

A **Contract** represents a formal agreement associated with the household.

Examples:

```
insurance policies
service subscriptions
property agreements
```

Contracts may generate renewals and financial obligations.

---

## Inventory

**Inventory** represents consumable resources used by the household.

Examples:

```
food
cleaning products
supplies
```

Inventory levels may trigger replenishment or shopping lists.

---

## Recipe

A **Recipe** represents a structured description of how to prepare a meal.

Recipes define ingredients and preparation steps.

---

## Meal Plan

A **Meal Plan** represents a scheduled set of meals across a time period.

Meal plans may generate shopping lists and cooking tasks.

---

## Pet

A **Pet** is a dependent animal belonging to the household.

Pets generate responsibilities such as:

* feeding
* veterinary care
* medication
* exercise

Pets may generate plans, tasks, and reminders.

---

## Reminder

A **Reminder** is a time-triggered notification.

Reminders typically originate from:

* events
* documents
* contracts
* maintenance schedules

---

# Domain Events

## Domain Event

A **Domain Event** represents a meaningful state change within the system.

Examples:

```
FamilyCreated
MemberAdded
EventScheduled
ResponsibilityAssigned
TaskCompleted
```

Domain events allow different parts of the system to react without tight coupling.

---

# Architectural Concepts

These concepts appear in system design and code.

---

## Aggregate

An **Aggregate** is a cluster of domain objects treated as a single consistency boundary.

Aggregates enforce domain invariants and emit domain events.

Examples:

```
Family
Event
Task
Routine
Property
MealPlan
```

---

## Aggregate Root

The **Aggregate Root** is the entry point to an aggregate.

All modifications must occur through the root.

Examples:

```
Family
Event
Task
Routine
Property
```

---

## Bounded Context

A **Bounded Context** defines a boundary where a specific domain model applies.

Examples of DomusMind contexts:

```
Family
Responsibilities
Calendar
Tasks
Food
Properties
Administration
Pets
Finance
```

Each context owns its internal model and rules.

---

# Language Consistency Rules

To maintain clarity:

* Household-facing terminology must prioritize **Plan** and **Chore** over Event and Task.
* **Event** and **Task** remain internal domain concepts.
* **Routine** refers strictly to recurring operational behavior in the Tasks context.
* **Markers / Weekly Cues** exist only in read models and must not appear in the domain layer.
* Domain terms must be used consistently in code and documentation.
* API endpoints should reflect domain language.
* Database models must map clearly to domain concepts.
* Synonyms for domain entities should be avoided.

The ubiquitous language ensures that the **domain model remains coherent as the system evolves**.

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

Members participate in responsibilities, plans, chores, and routines.

---

## Dependent

A **Dependent** is an entity requiring care or responsibility from family members.

Examples:

```
children
elderly relatives
pets
```

Dependents generate plans, chores, and responsibilities.

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
* chores
* tasks
* reminders

The timeline answers the question:

> What matters today for this household?

---

# Household Work

## Chore (Household Term)

A **Chore** is a household-facing term representing an operational responsibility assigned to someone.

Examples:

```
Trash → Juan
Dishwasher → Lucía
Laundry → Marta
```

Chores are visible in the **Household Timeline** and represent ongoing operational work required to keep the household functioning.

Chores may originate from:

* routines
* responsibilities
* manual creation

Chores are the **household-language representation of Tasks**.

---

## Task (Internal Model)

A **Task** is the internal domain concept representing an action that must be completed.

Tasks belong to the **Tasks bounded context** and may originate from:

* events
* routines
* responsibilities
* manual input

Tasks may be assigned to members.

In the household experience, tasks typically appear as **Chores**.

---

## Routine

A **Routine** represents **recurring operational behavior in the household**.

Examples:

```
weekly grocery shopping
school preparation
house cleaning
pet feeding
```

A routine defines **how operational work repeats over time**.

Important clarification:

A routine is **not any recurring activity in the system**.

Specifically:

* routines generate **operational tasks**
* routines belong to the **Tasks context**
* routines produce tasks according to recurrence rules

Examples of things that are **not routines**:

```
weekly football practice
monthly dentist appointment
annual holiday trip
```

These are **calendar plans**, not operational routines.

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

# DomusMind — Aggregate Design

## Purpose

This document defines the **aggregate structure of the DomusMind domain**.

Aggregates establish:

- consistency boundaries
- invariant enforcement
- transaction scope
- domain event emission

They are the **core units of domain integrity**.

All state modifications must occur through aggregate roots.

---

# Aggregate Principles

## Consistency Boundaries

An aggregate defines a boundary within which domain invariants must always hold.

State inside an aggregate must be **transactionally consistent**.

State outside the aggregate must be modified through **events or separate commands**.

---

## Aggregate Root Control

Each aggregate has a single **Aggregate Root**.

Rules:

- all modifications must go through the root
- internal entities cannot be modified directly
- the root enforces invariants
- the root emits domain events

---

## Small Aggregates

Aggregates should remain **small and focused**.

Large aggregates reduce concurrency and create unnecessary coupling.

Cross-aggregate coordination must occur through:

- domain events
- application services
- eventual consistency

---

## Identity

Each aggregate instance has a **stable identifier**.

Examples:

```

FamilyId
EventId
PropertyId
TaskId

```

External references between aggregates should use **identifiers only**, not object references.

---

# Core Aggregates

The following aggregates form the core of the DomusMind domain.

---

# Family Aggregate

## Aggregate Root

```

Family

```

## Responsibility

Represents the **household unit** and its members.

The family aggregate owns:

- members
- dependents
- pets
- relationships

## Internal Entities

```

Member
Dependent
Pet
Relationship

```

## Invariants

- a member must belong to exactly one family
- member identifiers must be unique within the family
- dependents must belong to the same family
- relationships must reference existing members

## Domain Events

```

FamilyCreated
MemberAdded
MemberRemoved
PetAdded
PetRemoved
RelationshipAssigned

```

---

# Responsibility Domain Aggregate

## Aggregate Root

```

ResponsibilityDomain

```

## Responsibility

Represents an **area of household responsibility**.

Examples:

```

food
school
finances
administration
maintenance
travel
pets

```

## Internal Entities

```

ResponsibilityAssignment

```

## Invariants

- each responsibility domain has at most one primary owner
- secondary owners must be members of the same family
- assignments must reference existing members

## Domain Events

```

ResponsibilityDomainCreated
ResponsibilityAssigned
ResponsibilityTransferred
SecondaryOwnerAssigned

```

---

# Event Aggregate

## Aggregate Root

```

Event

```

## Responsibility

Represents a **scheduled activity affecting the household timeline**.

Examples:

```

school excursion
medical appointment
trip
meeting
maintenance visit

```

## Internal Entities

```

Reminder
Participant

```

## Invariants

- an event must have a scheduled time
- an event belongs to one family timeline
- cancelled events cannot be modified
- reminders must reference an existing event
- participants must be family members or dependents

## Domain Events

```

EventScheduled
EventRescheduled
EventCancelled
ReminderAdded
ReminderRemoved

```

---

# Task Aggregate

## Aggregate Root

```

Task

```

## Responsibility

Represents an **action that must be completed**.

Tasks may originate from:

- events
- routines
- responsibilities
- manual creation

## Internal Entities

```

TaskAssignment

```

## Invariants

- a task must belong to a family
- a task may have zero or one primary assignee
- completed tasks cannot return to pending
- cancelled tasks cannot be completed

## Domain Events

```

TaskCreated
TaskAssigned
TaskReassigned
TaskCompleted
TaskCancelled
TaskRescheduled

```

---

# Routine Aggregate

## Aggregate Root

```

Routine

```

## Responsibility

Represents a **recurring operational pattern**.

Examples:

```

weekly grocery shopping
school preparation
house cleaning
pet feeding

```

## Invariants

- routines must define recurrence rules
- routines must belong to a family
- supported frequencies: Daily, Weekly, Monthly, Yearly
- routines are projected on-the-fly; they do not generate Task aggregates

## Domain Events

```

RoutineCreated
RoutineUpdated
RoutinePaused
RoutineResumed

```

---

# Property Aggregate

## Aggregate Root

```

Property

```

## Responsibility

Represents a **real estate asset managed by the family**.

Examples:

```

primary residence
second home
rental property

```

## Internal Entities

```

PropertyExpense
PropertyIncome
MaintenanceRecord

```

## Invariants

- property must belong to a family
- expenses must reference a property
- maintenance records must reference a property

## Domain Events

```

PropertyRegistered
PropertyExpenseRecorded
PropertyIncomeRecorded
MaintenanceScheduled
MaintenanceCompleted

```

---

# Inventory Aggregate

## Aggregate Root

```

InventoryItem

```

## Responsibility

Represents a **consumable resource within the household**.

Examples:

```

food
cleaning products
supplies

```

## Invariants

- inventory items belong to a family
- inventory quantity cannot be negative

## Domain Events

```

InventoryItemAdded
InventoryItemUpdated
InventoryItemDepleted
ShoppingListGenerated

```

---

# Meal Planning Aggregate

## Aggregate Root

```

MealPlan

```

## Responsibility

Represents a **planned set of meals over time**.

## Internal Entities

```

Meal

```

## Invariants

- meals must reference valid recipes
- meal plans must belong to a family
- meals must occur within the meal plan time range

## Domain Events

```

MealPlanCreated
MealPlanned
ShoppingListGeneratedFromMealPlan

```

---

# Document Aggregate

## Aggregate Root

```

Document

```

## Responsibility

Represents an **administrative record** tracked by the household.

Examples:

```

identity documents
insurance policies
contracts
warranties

```

## Invariants

- documents must belong to a family
- expiration dates must be valid
- contracts must reference a valid document type

## Domain Events

```

DocumentStored
DocumentExpirationApproaching
ContractRegistered
ContractRenewalApproaching

```

---

# Aggregate Interaction

Aggregates do **not directly modify each other**.

Cross-aggregate collaboration occurs through:

- application services
- domain events

Example:

```

EventScheduled
↓
TaskGeneratedFromEvent
↓
ReminderCreated

```

Each step occurs in its own aggregate boundary.

---

# Transaction Boundaries

Transactions must remain **within a single aggregate**.

Example:

```

Create Event
→ emit EventScheduled

```

Separate processes may react to the event and generate tasks or reminders.

This ensures scalability and loose coupling.

---

# Aggregate Evolution

The aggregate model may evolve as new domains emerge.

Future aggregates may include:

```

Vehicle
FinanceAccount
Subscription
TravelPlan

```

New aggregates must follow the same principles:

- clear responsibility
- strong invariants
- well-defined events

---

# Summary

Aggregates define the **structural backbone of the DomusMind domain**.

They provide:

- consistency boundaries
- invariant enforcement
- domain event emission
- transactional integrity

The aggregate design ensures that DomusMind can evolve while preserving domain coherence.


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
RoutineId
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

## Purpose

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

# Event Aggregate

## Aggregate Root

```
Event
```

## Purpose

Represents a **scheduled activity affecting the household timeline**.

---

# Task Aggregate

## Aggregate Root

```
Task
```

## Purpose

Represents a **concrete action that must be completed**.

---

# Routine Aggregate

## Aggregate Root

```
Routine
```

## Purpose

Represents **recurring operational household work**.

---

# Supporting Concepts

Areas are **not aggregates**.

They are lightweight labels used by read models to classify timeline entries without creating new consistency boundaries or business rules.

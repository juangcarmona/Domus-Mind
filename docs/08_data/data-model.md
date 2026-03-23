# DomusMind — Data Model

## Purpose

This document defines the persistence model principles of DomusMind.

It describes how aggregates, IDs, read models, and events are stored without coupling the domain to infrastructure.

---

## Persistence Principles

- persistence supports the domain model
- aggregates are the primary write boundary
- read models may be optimized for queries
- the domain must not depend on EF Core
- event storage is append-only
- supporting labels such as areas remain lightweight

---

## Aggregate Persistence

Initial V1 write model aggregates:

- `Family`
- `Event`
- `Task`
- `Routine`

Each command modifies exactly one aggregate boundary.

Persistence should preserve aggregate integrity and optimistic concurrency.

---

## Write Model

The write model is aggregate-oriented.

Typical characteristics:

- normalized enough to preserve integrity
- explicit ownership by module
- transaction scoped to one aggregate

Examples:

- Family tables own members and relationships
- Event tables own participants and reminders
- Task tables own assignments
- Routine tables own recurrence definitions
- Areas may be stored as lightweight classification records without becoming aggregates

---

## Read Model

Read models are query-oriented and may differ from aggregate storage.

Examples:

- `FamilyTimeline`
- `TaskBoard`
- `TimelineGroupedByArea`
- `FamilyRoster`

Read queries should prefer direct EF Core projection with `AsNoTracking()` into read models or API models.

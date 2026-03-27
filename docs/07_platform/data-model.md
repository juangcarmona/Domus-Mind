# DomusMind - Data Model

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
- IDs are strongly typed in code and serialized as strings externally 

---

## Aggregate Persistence

Initial V1 write model aggregates:

- `Family`
- `ResponsibilityDomain`
- `Event`
- `Task`
- `Routine` 

Each command modifies exactly one aggregate boundary.

Persistence should preserve aggregate integrity and optimistic concurrency.

---

## Identifier Strategy

DomusMind uses ULID-based strongly typed identifiers.

Examples:

- `FamilyId`
- `MemberId`
- `EventId`
- `TaskId`
- `RoutineId`

Storage recommendation:

- fixed-length string
- `CHAR(26)` or equivalent where appropriate

---

## Common Persistence Fields

Entities and rows may include common technical fields where useful:

- `Id`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `Version`

Rules:

- timestamps are infrastructure concerns unless explicitly part of domain behavior
- version is used for optimistic concurrency
- do not force a single inheritance-based base entity into the domain model

A shared persistence base type is acceptable in infrastructure if it does not leak into the domain.

---

## Write Model

The write model is aggregate-oriented.

Typical characteristics:

- normalized enough to preserve integrity
- explicit ownership by module
- transaction scoped to one aggregate

Examples:

- Family tables own members and relationships
- Responsibility tables own assignments
- Event tables own participants and reminders
- Task tables own assignments
- Routine tables own recurrence definitions

---

## Read Model

Read models are query-oriented and may differ from aggregate storage.

Examples:

- `FamilyTimeline`
- `TaskBoard`
- `ResponsibilityMatrix`
- `FamilyRoster`

Read queries should prefer direct EF Core projection with `AsNoTracking()` into read models or API models.

---

## Module Boundaries in Storage

Preferred direction:

- keep module ownership explicit
- avoid cross-module shared tables when possible
- reference external aggregates by ID only

Example:

- Calendar stores `MemberId` as participant reference
- Calendar does not own Member persistence

---

## Event Log

Committed domain events are persisted in an append-only event log. 

Recommended fields:

- `EventId`
- `EventType`
- `AggregateType`
- `AggregateId`
- `Module`
- `OccurredAtUtc`
- `Version`
- `PayloadJson`
- `CorrelationId`
- `CausationId`

The event log supports:

- auditability
- retries
- projections
- future integrations

Event sourcing is not required in V1.

---

## Concurrency

DomusMind should use optimistic concurrency for aggregate updates.

Typical mechanism:

- version column / concurrency token
- conflict translated to application-level error

This protects aggregate boundaries without distributed locking.

---

## EF Core Usage

EF Core is the primary persistence technology in V1.

Conventions:

- DbContext may be used directly in slices
- no generic repository layer is required
- projections should be expressed directly in LINQ
- write handlers load aggregates explicitly
- read queries use `AsNoTracking()`

---

## Summary

The DomusMind data model is aggregate-oriented for writes, projection-oriented for reads, ULID-based for identity, and append-only for domain events.

The goal is simple persistence aligned with the domain and efficient query execution.

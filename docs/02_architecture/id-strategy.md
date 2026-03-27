# DomusMind - Identifier Strategy

## Purpose

Identifiers uniquely reference domain entities and aggregates across the entire system.

IDs must support:

- distributed systems
- event-driven architecture
- offline clients
- long-lived data
- cross-system integrations

Identifiers are part of the **domain contract**.

---

# Identifier Principles

## Global Uniqueness

All aggregate identifiers must be globally unique.

This allows entities to be created by:

- mobile clients
- web clients
- backend services
- integrations

without coordination.

---

## Client-Safe Generation

IDs must be safely generated on clients.

This supports:

- offline-first mobile apps
- optimistic UI
- local event creation

Clients may create IDs before server persistence.

---

## Immutability

Identifiers are immutable.

Once assigned, an ID must never change.

IDs must not encode mutable information.

---

# ID Format

DomusMind uses **ULID** (Universally Unique Lexicographically Sortable Identifier).

Reasons:

- globally unique
- sortable by creation time
- URL safe
- shorter than UUID
- widely supported across platforms

Example:

```

01J6T8N9Z4F7KQ3M2C8Y6W1A5B

```

---

# Domain Identifier Types

Each aggregate has its own strongly typed identifier.

Examples:

```

FamilyId
MemberId
EventId
TaskId
RoutineId
PropertyId
DocumentId
InventoryItemId
MealPlanId

```

Identifiers must not be interchangeable.

---

# Implementation Guidelines (.NET)

Domain IDs should be implemented as **value objects**.

Example:

```csharp
public readonly record struct FamilyId(Ulid Value);
```

Benefits:

* strong typing
* compile-time safety
* explicit domain semantics

---

# API Representation

Identifiers are serialized as **strings**.

Example:

```json
{
  "familyId": "01J6T8N9Z4F7KQ3M2C8Y6W1A5B"
}
```

This ensures compatibility across:

* REST
* GraphQL
* messaging systems
* JavaScript clients

---

# Client Generation

Clients may generate IDs using ULID libraries.

Supported platforms:

* .NET
* JavaScript / TypeScript
* mobile environments

This supports offline event creation and synchronization.

---

# Event Identifiers

Domain events use their own identifier:

```
EventId
```

Event IDs are independent from aggregate IDs.

Events also include:

```
AggregateId
OccurredAt
```

---

# Database Storage

Identifiers should be stored as:

```
CHAR(26)
```

or equivalent fixed-length string.

Benefits:

* efficient indexing
* predictable storage size
* lexicographical ordering

---

# Stability Requirement

Identifiers form part of the **long-term data contract**.

Changing ID formats or semantics must be avoided.

IDs must remain stable across:

* system versions
* deployments
* integrations


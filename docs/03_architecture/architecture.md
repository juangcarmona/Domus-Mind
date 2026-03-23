# DomusMind — System Architecture

## Overview

DomusMind is a **domain-centric, API-first platform** that models and operates the state of a household system.

The architecture prioritizes:

- domain clarity
- extensibility
- long-term evolvability
- multiple interaction surfaces

DomusMind is not designed around a single application.
It is a **household operating platform**.

---

# Architectural Principles

## Domain First

The domain model defines the system.

Technology, persistence, and user interfaces are secondary concerns.

All system capabilities emerge from the domain model and its invariants.

---

## Domain Independence

The domain model must never depend on:

- API contracts
- persistence models
- messaging formats
- infrastructure services
- AI services

The domain layer contains only business concepts and rules.

---

## API as the System Boundary

All interaction with DomusMind occurs through a **capability-oriented API**.

Clients may include:

- mobile applications
- web interfaces
- messaging platforms
- automation systems
- external integrations

The API exposes **domain capabilities**, not database entities.

---

## Vertical Slice Architecture

System capabilities are implemented as **vertical slices**.

Each slice contains everything required to deliver a feature:

- request
- validation
- application logic
- persistence interaction
- API endpoint

Example slices:

```
create-family
add-member
schedule-event
create-task
create-routine
```

Slices represent **domain capabilities**, not technical layers.

---

## Bounded Contexts

The system is divided into bounded contexts aligned with the domain.

Each context owns its internal model and invariants.

Primary contexts in V1 include:

- Family
- Calendar
- Tasks

Areas are **not** a bounded context, aggregate root, or standalone module.
They are a lightweight supporting concept used inside Calendar, Timeline, and Tasks read models.

Contexts collaborate through **domain events and explicit contracts**.

---

## Event-Driven Collaboration

Domain events represent meaningful state changes.

Examples:

```
FamilyCreated
MemberAdded
EventScheduled
RoutineTriggered
InventoryItemDepleted
ContractRenewalApproaching
```

Events allow contexts and features to react without tight coupling.

---

# High-Level System Structure

```
        +-----------------------+
        |        Clients        |
        |-----------------------|
        | Mobile Apps           |
        | Web Applications      |
        | Messaging Interfaces  |
        | Automation Systems    |
        | External Integrations |
        +-----------+-----------+
                    |
                    v
          +-------------------+
          |    DomusMind API  |
          +-------------------+
                    |
                    v
      +---------------------------+
      |  Application Layer        |
      |---------------------------|
      | Vertical Feature Slices   |
      | Read Models + Projections |
      +---------------------------+
                    |
                    v
      +---------------------------+
      |      Domain Layer         |
      |---------------------------|
      | Aggregates                |
      | Entities                  |
      | Value Objects             |
      | Domain Events             |
      | Domain Services           |
      +---------------------------+
                    |
                    v
      +---------------------------+
      |   Infrastructure Layer    |
      |---------------------------|
      | Persistence               |
      | Messaging                 |
      | Integrations              |
      | AI Services               |
      +---------------------------+
```

---

# Core Layers

## Domain Layer

Contains the **household domain model**.

Defines:

- aggregates
- entities
- value objects
- domain services
- domain events

Examples:

```
Family
Member
Pet
Event
Task
Routine
Property
Document
InventoryItem
MealPlan
```

The domain layer contains **no infrastructure dependencies**.

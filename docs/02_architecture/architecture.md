# DomusMind - System Architecture

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
assign-responsibility
schedule-event
create-routine
record-property-expense
generate-shopping-list

```

Slices represent **domain capabilities**, not technical layers.

---

## Bounded Contexts

The system is divided into bounded contexts aligned with the domain.

Each context owns its internal model and invariants.

Primary contexts include:

- Family
- Responsibilities
- Calendar
- Tasks
- Household
- Properties
- Administration
- Food
- Pets
- Finance
- AI Interpretation

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
ResponsibilityDomain
Event
Routine
Property
Document
InventoryItem
MealPlan

```

The domain layer contains **no infrastructure dependencies**.

---

## Application Layer

Implements system use cases through **feature slices**.

Responsibilities:

- orchestrate domain behavior
- enforce application rules
- coordinate aggregates
- publish domain events

Example structure:

```

features/
family/
create-family
add-member
calendar/
schedule-event
responsibilities/
assign-owner
tasks/
create-routine
food/
generate-shopping-list

```

---

## Infrastructure Layer

Provides technical implementations for domain abstractions.

Examples:

- persistence
- event publishing
- messaging adapters
- external service integrations
- AI processing

Infrastructure must not contain domain logic.

---

## Interface Layer

Exposes system capabilities to external actors.

Possible interfaces:

- REST API
- GraphQL API
- messaging adapters
- web applications
- mobile applications

Interfaces translate external input into application commands.

---

# Deployment Model

DomusMind supports multiple deployment models.

### Self-Hosted

Families deploy DomusMind locally.

Examples:

- home servers
- NAS devices
- container platforms

### Private Cloud

Deployment within a privately managed environment.

### Managed Cloud

Centralized hosting supporting multiple families.

---

# Interaction Surfaces

The system supports multiple simultaneous interfaces.

Examples:

- mobile applications
- web dashboards
- messaging systems (Telegram)
- home automation platforms
- voice assistants
- external APIs

All surfaces interact through the **same system API**.

---

# AI Integration

AI is not part of the core domain.

AI operates as a **supporting capability**.

Possible uses:

- natural language interpretation
- document extraction
- recommendation systems
- predictive reminders

AI converts external input into structured domain information.

---

# Extensibility

New capabilities can be introduced through:

- new bounded contexts
- new vertical slices
- new event consumers
- new interface adapters

The domain model remains the stable center of the system.

---

# Architectural Goal

DomusMind is not an application.

It is a **platform that models and operates family life as a structured system**.

The architecture ensures:

- coherent domain model
- independent feature evolution
- multiple interface support
- long-term system stability

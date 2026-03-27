# DomusMind - Application Model

## Purpose

This document defines the **application execution model** used by DomusMind.

The application layer translates system capabilities into domain behavior.

All interactions follow a **Command / Query execution model**.

The application layer also produces **read models used by product surfaces**.

---

# Core Concepts

## Commands

Commands represent **intent to change system state**.

Examples:

```
CreateFamily
IdentifySelf
AddMember
ScheduleEvent
CreateTask
AssignPrimaryOwner
CreateRoutine
UpdateHouseholdSettings
```

Properties:

- mutate domain state
- handled by a single handler
- operate on a single aggregate
- may emit domain events

Contract:

```
ICommand<TResponse>
```

Handler:

```
ICommandHandler<TCommand, TResponse>
```

Commands are **explicit system capabilities**.

---

## Queries

Queries represent **read operations**.

Examples:

```
GetFamily
GetFamilyMembers
GetFamilyTimeline
GetWeeklyHouseholdGrid
GetMemberActivity
GetResponsibilityBalance
```

Properties:

- do not modify domain state
- may read multiple aggregates
- optimized for read models
- may compose data from multiple modules

Contract:

```
IQuery<TResponse>
```

Handler:

```
IQueryHandler<TQuery, TResponse>
```

Queries often produce **projections tailored for UI surfaces**.

Those surfaces may use product language that differs from domain names, but the application layer must preserve explicit domain semantics in commands, queries, and events.

---

# Read Models

DomusMind exposes several **coordination projections**.

These are not aggregates.

They are **computed views over multiple domain concepts**.

Examples:

```
FamilyTimeline
EnrichedFamilyTimeline
WeeklyHouseholdGrid
ResponsibilityBalance
MemberActivity
```

Examples of product-language mappings:

```
ResponsibilityDomain -> Area
PrimaryOwner -> Owner
SecondaryOwner -> Support
```

Read models may aggregate data from:

```
Events
Tasks
Routines
Members
Responsibilities
```

They exist **only in the application layer**.

For example, planning and timeline surfaces may show Areas as optional accountability context for plans and tasks, while the underlying domain behavior remains rooted in the Responsibility context.

---

# Domain Events

Domain events represent **facts that occurred in the domain**.

Examples:

```
FamilyCreated
MemberAdded
EventScheduled
EventRescheduled
TaskAssigned
TaskCompleted
RoutinePaused
ResponsibilityTransferred
```

Contract:

```
IDomainEvent
```

Handlers:

```
IDomainEventHandler<TEvent>
```

Domain events allow **modules to react to changes without direct coupling**.

---

# Dispatchers

Dispatchers route execution requests.

Interfaces:

```
ICommandDispatcher
IQueryDispatcher
IDomainEventDispatcher
```

Responsibilities:

- resolve handlers
- execute handlers
- publish domain events

Dispatchers use the system dependency injection container.

---

# Execution Flow

Typical command execution:

```
Client
↓
API Endpoint
↓
Command Dispatcher
↓
Command Handler
↓
Aggregate
↓
Domain Events
↓
Event Handlers
```

Typical query execution:

```
Client
↓
API Endpoint
↓
Query Dispatcher
↓
Query Handler
↓
Read Model
↓
Response
```

---

# Validation

Validation occurs before command execution.

Typical flow:

```
Command
↓
Validator
↓
Handler
```

Rules:

- validation failures stop execution
- validators do not mutate domain state

Contract:

```
IValidator<TCommand>
```

---

# Transaction Boundary

Commands execute inside a **single transaction scope**.

Rules:

- one command modifies one aggregate
- domain events are emitted after state change
- persistence occurs before event publication
- event handlers run after successful commit

Queries do not open transaction scopes unless required.

---

# Relationship With Vertical Slices

Each vertical slice contains:

```
Command or Query
Validator
Handler
Endpoint
```

Example:

```
features/calendar/schedule-event

ScheduleEventCommand
ScheduleEventValidator
ScheduleEventHandler
ScheduleEventEndpoint
```

Example query slice:

```
features/calendar/get-weekly-household-grid

GetWeeklyHouseholdGridQuery
GetWeeklyHouseholdGridHandler
GetWeeklyHouseholdGridEndpoint
```

Each slice is independent and self-contained.

---

# Architectural Role

The application model ensures:

- explicit system capabilities
- strict separation between domain and UI projections
- event-driven module interaction
- independent vertical slices
- predictable execution flow

It forms the **execution backbone of the DomusMind platform**.

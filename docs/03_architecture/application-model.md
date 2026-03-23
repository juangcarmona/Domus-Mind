# DomusMind — Application Model

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
GetHouseholdAreas
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
HouseholdAreas
MemberActivity
```

Read models may aggregate data from:

```
Events
Tasks
Routines
Members
Areas
```

They exist **only in the application layer**.

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

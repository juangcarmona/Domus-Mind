# ADR-001 - Internal Application Mediator

## Status

Accepted

---

## Context

DomusMind implements a modular monolith with:

- vertical slices
- bounded contexts as modules
- command/query separation
- domain events

A mediator is required to dispatch:

- commands
- queries
- domain events

External libraries such as MediatR were considered.

---

## Decision

DomusMind will implement a **minimal internal application mediator**.

Core contracts:

```

ICommand<TResponse>
IQuery<TResponse>

ICommandHandler<TCommand, TResponse>
IQueryHandler<TQuery, TResponse>

IDomainEvent
IDomainEventHandler<TEvent>

ICommandDispatcher
IQueryDispatcher
IDomainEventDispatcher

```

Dispatching is implemented using the native DI container.

No external mediator framework is required.

---

## Rationale

Reasons for this decision:

- avoid external architectural dependency
- maintain full control of dispatch pipeline
- simplify debugging and tracing
- reduce abstraction layers
- guarantee long-term stability

The internal mediator provides only the behavior required by DomusMind.

---

## Consequences

### Positive

- simpler architecture
- no external dependency
- explicit domain contracts
- predictable behavior
- easier documentation for AI-assisted development

### Negative

- small amount of infrastructure code required
- pipeline behaviors must be implemented internally if needed

---

## Alternatives Considered

### MediatR

Rejected due to:

- external dependency
- license changes
- unnecessary abstraction for project needs

### Direct handler invocation

Rejected because:

- breaks architectural boundaries
- complicates testing
- increases coupling between slices

---

## Result

DomusMind uses a **minimal internal mediator** as part of its core architecture.

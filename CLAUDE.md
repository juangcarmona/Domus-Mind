# CLAUDE.md

## Purpose

DomusMind is a real system with an existing design.
Preserve the documented architecture.
Do not redesign the domain.
Do not introduce generic boilerplate patterns that conflict with the docs.

## Working mode

Before changing code:
1. Read the relevant architecture and spec documents.
2. Identify the bounded context and slice involved.
3. Preserve layer boundaries.
4. Make the smallest safe change that keeps the solution coherent.
5. Prefer concrete code and file edits over theoretical advice.

## Source of truth

Read these first when working on backend scaffolding:

### Core architecture
- `docs/03_architecture/architecture.md`
- `docs/03_architecture/application-model.md`
- `docs/03_architecture/aggregate-design.md`
- `docs/03_architecture/event-processing.md`
- `docs/03_architecture/id-strategy.md`

### ADRs
- `docs/03_architecture/decision-records/ADR-001-internal-application-mediator.md`
- `docs/03_architecture/decision-records/ADR-002-authentication-and-identity.md`

### Implementation rules
- `docs/06_slices/slice-conventions.md`
- `docs/07_interfaces/api.md`
- `docs/08_data/data-model.md`
- `docs/09_security/security.md`

### Domain/context references
- `docs/04_domain/domain-overview.md`
- `docs/04_domain/domain-events.md`
- `docs/04_domain/ubiquitous-language.md`
- `docs/05_contexts/context-map.md`
- `docs/05_contexts/family-context.md`
- `docs/05_contexts/responsibility-context.md`
- `docs/05_contexts/calendar-context.md`
- `docs/05_contexts/tasks-context.md`

### System + feature specs
- `specs/system/system-spec.md`
- `specs/contexts/*.md`
- `specs/features/**/*.md`

## Mandatory architectural rules

- DomusMind is a domain-centric, API-first modular monolith.
- The domain model is the stable center.
- The API uses ASP.NET Core Controllers, not Minimal APIs.
- The API is REST-based and documented with Swagger/OpenAPI.
- API contracts use explicit models under `Model.*`.
- Domain entities must never be exposed directly through the API.
- Mapping is explicit. Do not introduce AutoMapper.
- EF Core is used directly. Do not introduce generic repositories.
- Read queries should prefer projection-based queries and `AsNoTracking()`.
- Commands and queries are explicit.
- One command modifies one aggregate only.
- Cross-module collaboration happens through domain events.
- Domain events are persisted in an append-only event log.
- Authentication is local to the modular monolith.
- Authentication identity and family/member domain identity are separate concepts.
- The domain must remain framework-agnostic.
- Do not push infrastructure concerns into the domain.
- Do not collapse the modular monolith into a single project.
- Do not redesign into generic Clean Architecture boilerplate.

## Current backend structure

Backend root:
- `src/backend/DomusMind.Api`
- `src/backend/DomusMind.Application`
- `src/backend/DomusMind.Contracts`
- `src/backend/DomusMind.Domain`
- `src/backend/DomusMind.Infrastructure`

Tests live under:
- `tests/backend/*`

## Layer responsibilities

### DomusMind.Domain
Contains:
- aggregates
- entities
- value objects
- domain events
- domain rules

Must not contain:
- EF Core
- ASP.NET Core
- JWT/auth framework code
- infrastructure services
- API contracts

### DomusMind.Application
Contains:
- commands
- queries
- handlers
- validators
- dispatcher abstractions
- orchestration across domain + persistence boundaries

### DomusMind.Contracts
Contains:
- API request models
- API response models
- shared API error models

### DomusMind.Infrastructure
Contains:
- EF Core DbContext
- persistence mappings
- migrations
- event log persistence
- auth implementation
- current-user accessors
- clock/system services
- dispatcher implementations

### DomusMind.Api
Contains:
- controllers
- HTTP transport mapping
- auth/swagger wiring
- exception and problem-details mapping

## Slice rules

Represent capabilities as vertical slices under bounded contexts.

Examples:
- `Features/Family/CreateFamily`
- `Features/Family/AddMember`
- `Features/Responsibilities/AssignPrimaryOwner`
- `Features/Calendar/ScheduleEvent`
- `Features/Tasks/CreateTask`

Each slice should contain only what it needs, typically:
- command or query
- validator
- handler
- response or mapping when needed

Controllers stay thin.
Handlers do not call other handlers.
Cross-module reactions happen through domain events.

## Internal mediator rule

Use the internal application mediator from ADR-001.
Do not add MediatR or similar frameworks unless the user explicitly changes the ADR.

Core contracts:
- `ICommand<TResponse>`
- `IQuery<TResponse>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`
- `IDomainEvent`
- `IDomainEventHandler<TEvent>`
- `ICommandDispatcher`
- `IQueryDispatcher`
- `IDomainEventDispatcher`

## Execution discipline

When asked to implement something:
1. Locate the relevant docs.
2. State which docs govern the change.
3. Identify the target bounded context and slice.
4. Preserve existing names and concepts from the ubiquitous language.
5. Make minimal edits.
6. Keep code compilable.
7. Do not invent missing business rules when the docs are silent; stop and report the gap.

## Output expectations

When proposing work:
- be concrete
- name exact files
- show minimal code
- avoid broad refactors
- avoid placeholder abstractions not required by the docs

When uncertain:
- read more docs first
- report the uncertainty clearly
- do not guess domain behavior
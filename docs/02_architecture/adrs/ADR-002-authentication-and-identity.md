# ADR-002 - Authentication and Identity Strategy

## Status

Accepted

---

## Context

DomusMind is designed as:

- local-first
- self-hosted capable
- API-first
- modular monolith

The system requires authentication for:

- household access
- administrative actions
- secure API usage
- future invitation and membership flows

Identity is required, but identity management is **not** the core business domain of DomusMind. DomusMind models household operations, not enterprise IAM.

---

## Decision

DomusMind will implement **built-in local authentication** as an **internal module of the modular monolith**.

Authentication will **not** be implemented as a separate microservice in V1.

The identity/authentication module will remain logically isolated, with its own models, persistence area, and application flows.

---

## Decision Drivers

- minimize operational complexity
- preserve local-first deployment
- reduce infrastructure dependencies
- keep installation simple for self-hosted families
- retain clear future path toward external identity integration if needed

---

## Options Considered

### Option 1 - Built-in Local Authentication inside the main API

Description:

- authentication implemented inside DomusMind
- same deployable unit as the main backend
- logically isolated as an internal module

Pros:

- simplest operational model
- best fit for self-hosted deployment
- no external dependency
- fast to implement in .NET
- easiest developer experience for V1

Cons:

- DomusMind owns password/session security implementation
- future federation/OIDC support must be added later
- some security features may need later expansion

Assessment:

**Best fit for V1.**

---

### Option 2 - Separate Identity Microservice

Description:

- authentication extracted into a dedicated service
- DomusMind API consumes it as an external dependency

Pros:

- clearer service isolation
- future reuse across multiple products possible
- easier long-term extraction if platform scope grows

Cons:

- unnecessary distributed complexity
- more deployment units
- more secrets/configuration
- harder self-hosted experience
- no clear V1 benefit

Assessment:

Rejected for V1 due to accidental complexity.

---

### Option 3 - External Self-Hosted Identity Provider

Examples:

- Keycloak
- authentik

Pros:

- standards support
- ready-made authentication features
- good path toward OIDC / federation

Cons:

- extra operational burden
- more moving parts
- heavier than required for V1
- weak fit for a minimal local-first installation

Assessment:

Valuable future option, not the V1 default.

---

## Rationale

DomusMind V1 should optimize for:

- simplicity
- low deployment friction
- local-first usability
- clear architecture

A separate identity service would introduce distributed-system concerns without solving a real V1 problem.

An external IdP would also increase installation and operational complexity for self-hosted deployments.

Built-in local authentication provides the best balance between security, simplicity, and speed of delivery.

---

## Decision Details

The selected approach requires:

- internal Identity module
- separation between `User` and `Member`
- identity persistence isolated from household domain persistence
- authentication endpoints exposed by the main API
- future-friendly abstraction for current authenticated user context

Conceptual distinction:

- `User` = authenticable system identity
- `Member` = household domain entity

These concepts must not be merged.

---

## Consequences

### Positive

- simpler deployment
- better self-hosted experience
- fewer runtime dependencies
- faster implementation
- easier debugging and tracing
- better alignment with modular monolith architecture

### Negative

- DomusMind owns auth implementation
- future external identity integration will require additional work
- advanced IAM features are deferred

---

## Follow-Up Rules

- authentication remains an internal module, not a bounded context of the household domain
- `User` and `Member` must remain separate models
- the rest of the system must depend on an abstraction such as `ICurrentUserContext`
- future OIDC support may be added later without changing the core household domain

---

## Result

DomusMind uses **built-in local authentication inside the modular monolith**.

It does **not** use a separate identity microservice in V1.
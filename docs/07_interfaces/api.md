# DomusMind — API

## Purpose

This document defines the external HTTP API conventions for DomusMind.

The API is the main system boundary for clients and integrations. It exposes domain capabilities through REST endpoints backed by controllers.

---

## Style

DomusMind uses:

- HTTP REST API
- ASP.NET Core Controllers
- OpenAPI / Swagger
- JSON request and response bodies

Minimal APIs are not the default style.

---

## API Principles

- the API exposes capabilities, not database tables
- endpoints use domain language
- controllers remain thin
- handlers execute application behavior
- the API is stateless
- read endpoints should use efficient projection queries
- write endpoints should target one command and one aggregate boundary

---

## Endpoint Conventions

Examples:

- `POST /api/families`
- `POST /api/families/{familyId}/members`
- `POST /api/responsibility-domains`
- `POST /api/responsibility-domains/{id}/primary-owner`
- `POST /api/events`
- `POST /api/tasks`
- `POST /api/routines`
- `GET /api/families/{familyId}/timeline` *(deprecated — use `/timeline/enriched`)*

Rules:

- use plural resource names where appropriate
- use nested routes only when ownership is explicit
- commands use `POST`, `PUT`, `PATCH`, or `DELETE` as appropriate
- queries use `GET`

---

## Current Authentication Surface

DomusMind currently exposes built-in local authentication endpoints.

Implemented endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `POST /api/auth/change-password`
- `GET /api/auth/me`

These endpoints manage authentication identity (`User`), not household domain identity (`Member`).

Successful login returns:

- `accessToken`
- `refreshToken`

Protected endpoints use:

- `Authorization: Bearer <access-token>`

Swagger / OpenAPI must document authentication requirements and allow bearer token testing during development.

---

## Model Naming Conventions

DomusMind uses explicit request and response models.

Rules:

- request models end with `Request`
- response models end with `Response`
- collection items may use descriptive names
- timeline or composite items may omit suffix when clearer

Examples:

Requests:

- `CreateFamilyRequest`
- `AddMemberRequest`
- `ScheduleEventRequest`
- `AssignTaskRequest`
- `LoginRequest`
- `RegisterUserRequest`

Responses:

- `FamilyResponse`
- `MemberResponse`
- `EventResponse`
- `TaskResponse`
- `LoginResponse`
- `RefreshTokenResponse`

Composite models:

- `FamilyTimelineItem`
- `ResponsibilityMatrixItem`

API models live under:

`Model.*`

Domain entities live under:

`Domain.*`

The API contract must never expose domain entities directly.

---

## Model Evolution

API models are versioned implicitly.

Rules:

- new optional fields may be added
- existing fields must not change meaning
- breaking changes require versioning
- clients must tolerate additional fields

---

## Controllers

Controllers are transport adapters.

Responsibilities:

- receive HTTP requests
- bind request models
- dispatch commands or queries
- translate results into HTTP responses

Controllers must not contain:

- domain logic
- persistence logic
- cross-slice orchestration

---

## Models

DomusMind uses **API models**, not DTO terminology.

Rules:

- API-exposed models live in `Model.*`
- domain entities live in `Domain.*`
- API models do not use `Dto` suffix
- exposed models should have clear domain-aligned names

The API model is a contract, not a domain entity.

---

## Mapping

DomusMind does not use AutoMapper.

Rules:

- mapping is explicit
- mapping remains close to the slice
- query projections should be expressed directly in EF Core queries when possible

This keeps contracts visible and avoids hidden conventions.

---

## Queries

Query endpoints should prefer:

- EF Core projections
- `AsNoTracking()`
- direct materialization into API models or read models

This is the default approach for stateless API reads.

Generic repositories are not required. The application may query directly through the EF Core DbContext using module-aware boundaries.

---

## Commands

Write endpoints dispatch commands through the internal mediator.

Typical flow:

`Controller → Command → Handler → Aggregate → Domain Events`

Commands must:

- target one capability
- modify one aggregate
- return explicit results

---

## Responses

Conventions:

- `200 OK` for successful reads
- `201 Created` for successful creation
- `204 No Content` for successful operations without body
- `400 Bad Request` for validation errors
- `404 Not Found` when target resource does not exist
- `409 Conflict` for invariant or concurrency violations
- `401 Unauthorized` when authentication is missing
- `403 Forbidden` when access is denied

---

## Error Shape

Errors should use a consistent JSON structure.

Example:

```json
{
  "code": "family.not_found",
  "message": "Family was not found.",
  "details": {}
}
````

Validation errors may include field-level details.

---

## Versioning

Initial versioning strategy:

* URL versioning is optional in V1
* OpenAPI document version must exist
* breaking API changes should be avoided
* versioning becomes explicit when external clients require it

---

## Documentation

Swagger / OpenAPI is required.

It must document:

* endpoints
* request models
* response models
* authentication requirements
* error responses

Swagger is part of the development workflow and integration experience.

---

## Summary

DomusMind exposes a REST API based on controllers, explicit models, explicit mapping, Swagger, and efficient EF Core projections.

The API remains thin, capability-oriented, and aligned with the domain model.
---
applyTo: "src/backend/**/*.cs"
---

# Backend Instructions

## Architecture

Follow the existing layered structure:
- Domain: business rules only
- Application: commands, queries, handlers
- Contracts: request/response DTOs
- Infrastructure: persistence, auth, integrations
- API: controllers

Do not move logic across layers.

## Rules

- controllers are thin (no business logic)
- one handler = one responsibility
- do not introduce generic repositories
- do not introduce broad service/helper classes
- do not expose domain entities through API contracts

## Size and structure

- file target: <300 lines
- class target: focused, single purpose
- method target: small and readable

Split code when it mixes responsibilities.

## Changes

- prefer modifying existing slices over creating new ones
- follow existing naming and folder conventions
- add or update tests when behavior changes
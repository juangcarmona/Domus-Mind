# DomusMind — Copilot Instructions

This repository is documentation-driven.

Before making non-trivial changes:
- read the relevant files in `docs/` and `specs/`
- identify the owning area (backend, web app, public site)
- find an existing pattern and follow it

## Working rules

- prefer extending existing code over introducing new abstractions
- keep changes small and localized
- keep files focused and small (target <300 lines)
- keep methods/components small and single-purpose
- avoid multi-responsibility classes or components
- avoid generic “service”, “manager”, “helper” patterns unless already established
- avoid introducing new architectural layers

## Structure awareness

Respect current structure and conventions:
- backend is layered (Domain / Application / Contracts / Infrastructure / API)
- web app uses feature-based structure and shared state
- docs and specs are the source of truth for behavior and terminology

Do not reorganize structure unless explicitly required.

## Backend constraints

- controllers must remain thin
- business logic belongs in domain/application, not controllers
- do not expose domain entities through API contracts
- do not introduce generic repositories or mapping frameworks unless already used

## Web app constraints

- avoid large components (split when mixing data, state, and rendering)
- reuse existing API/state patterns
- avoid duplicating forms or logic across features

## Docs and specs

- do not invent new terminology if existing terms exist in docs/specs
- if behavior changes, update the corresponding docs/specs
- do not create parallel or conflicting explanations

## Validation

Before finishing:
- build affected projects
- run relevant tests
- check for obvious regressions
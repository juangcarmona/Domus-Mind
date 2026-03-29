# DomusMind - Deployment Modes

## Purpose

Define how DomusMind supports two deployment modes without splitting into two products.

DomusMind is one product with:

- `SingleInstance`
- `CloudHosted`

Both modes must run the same core system.

---

## Product Invariant

The following must never diverge by deployment mode:

- domain model
- aggregates
- commands and queries
- handlers
- endpoints
- feature slices
- migrations
- read models
- UI components
- API contracts
- core auth/session primitives

If any of these fork, DomusMind becomes two products.

---

## Deployment Modes

### SingleInstance

One installation, one household.

Intended for:

- self-hosted deployments
- local-first usage
- single-family ownership

Constraints:

- only one household may exist in the installation
- household bootstrap may happen locally
- multi-household behavior is disabled by policy, not by domain fork

### CloudHosted

One shared cloud deployment, many households.

Intended for:

- Azure-hosted shared service
- family-scoped authorization
- managed operations

Constraints:

- multiple households may coexist in the same deployment
- household creation may be restricted by policy
- invitations and abuse controls may be enabled by policy

---

## Allowed Variation

These may differ by deployment mode:

- onboarding flow constraints
- household creation policy
- invitation policy
- environment configuration
- storage provider
- email provider
- rate limits
- abuse controls
- backup strategy
- observability profile
- admin and support tooling

---

## Architectural Rule

Deployment mode must be resolved only in:

- composition root
- configuration
- infrastructure adapters
- policy layer
- UI flow guards

Deployment mode must never be resolved inside:

- domain entities
- aggregates
- handlers
- feature logic
- endpoint duplication
- migrations
- read model definitions

---

## Required Seams

DomusMind should define explicit seams for:

- deployment mode context
- household provisioning policy
- invitation policy
- abuse protection policy
- email capability
- storage capability
- observability profile
- backup strategy

These seams must be configuration-driven and testable.

---

## Summary

DomusMind supports two deployment modes but remains one codebase, one model, one set of slices, and one API.

Deployment mode changes hosting constraints and operational policy only.
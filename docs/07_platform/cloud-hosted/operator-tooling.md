# CloudHosted - Operator and Admin Tooling

## Status

Accepted — Minimum operator scope for first rollout

---

## Purpose

CloudHosted requires a minimum operator surface to provision access, inspect state, and respond to failure or abuse, without exposing household member data beyond what is necessary.

Operator tooling is not a household product feature. It must not modify household aggregate state through shortcuts.

---

## Operator Capabilities Required Before Scale

The following capabilities must be available before CloudHosted accepts its first real household.

### Account and access management

- view list of registered accounts (email, creation date, enabled/disabled status)
- disable or suspend an account
- re-enable a suspended account
- view which household(s) an account belongs to

### Invitation management

- create an invitation for a specific person
- view issued invitations (pending, accepted, expired)
- revoke a pending invitation

### Household inspection

- view list of households (id, name, creation date, member count)
- view members of a specific household

### Bootstrap and configuration

- confirm system is initialized (via `/api/setup/status`)
- confirm active deployment mode
- inspect current platform deployment capabilities (via `/api/platform/deployment-mode`)

### Auth and incident response

- inspect failed login frequency per account (via Application Insights)
- disable a repeatedly-abusing account
- view active refresh tokens for an account and invalidate if needed

### Data access and deletion

- manually export account and household data on request
- manually delete an account and household data on explicit user request
- maintain a written record of such requests and actions

---

## Implementation Posture for First Rollout

For first rollout, operator tooling may be:

- a thin internal admin area accessible only to the operator
- a protected set of API endpoints under a separate auth surface (`/api/admin/*`)
- or direct database operations with a documented runbook

Whichever form it takes, it must:

- require authenticated operator access
- not be accessible to normal household users
- not bypass family-scoped authorization in ways that silently mix household data

---

## Out of Scope for First Rollout

- no public-facing support portal
- no self-service account deletion
- no automated abuse detection workflows
- no SLA management tooling
- no customer support ticketing integration
- no metrics dashboards beyond Application Insights
- no multi-operator access control distinctions (single operator role at first)

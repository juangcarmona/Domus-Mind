# CloudHosted - Signup Policy

## Status

Accepted — First rollout baseline

---

## Access Model

CloudHosted has no open signup.

No account is created without operator authorization. No unauthenticated path leads to a fully provisioned account.

---

## First Rollout Access Flow

1. Operator creates an invitation for a known person.
2. Person receives the invite through a trusted channel.
3. Person uses the invite to complete account registration.
4. Operator manually creates the household for the new user, or grants the user access to an existing one.
5. Family domain membership is established through normal household flows.

This model is manual at first and designed to remain safe and auditable at low volume.

---

## Account and Household Relationship

Account creation and household membership are separate steps.

Having an account does not automatically create or join a household. That step is explicit.

For the first rollout:

- a user may belong to exactly one household
- belonging to multiple households simultaneously is deferred

This is a policy constraint, not a domain constraint. It is enforced through household creation policy and provisioning control, not by changing the domain model.

---

## Disabled / Rejected Access

A rejected or suspended user:

- cannot authenticate
- loses access to all household features
- retains account state in storage (not deleted, unless a data deletion is requested)

The operator disables an account through operator tooling. The affected household is not automatically notified.

Account re-enablement is also an operator action.

---

## Policy vs Domain

The following must not enter domain logic, handlers, or feature slices:

- deployment mode checks
- invite token validation
- account provisioning decisions
- account suspension state

These concerns belong to the platform/auth/infrastructure layer only.

The domain operates on authenticated family membership. The platform ensures only authorized identities reach the domain.

---

## Non-Goals for First Rollout

- no self-service account registration
- no social login or external identity provider integration
- no waiting list
- no freemium tier or usage-gated access
- no household-member-initiated account provisioning without operator involvement

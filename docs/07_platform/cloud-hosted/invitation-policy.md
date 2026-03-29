# CloudHosted - Invitation Policy

## Status

Accepted — First rollout baseline

---

## Access Model

CloudHosted uses invite-only access.

There is no open signup. No household or account is created without a prior invitation from an authorized party.

---

## Who May Issue Invitations

For the first rollout:

- the operator issues invitations manually
- there is no self-service invite from household members yet
- household members do not independently invite new accounts without operator involvement

Household members may be added to an existing household through the family domain model (AddMember), but this does not grant an independent account to the new person without operator action.

---

## Invitation Scope

An invitation is account-scoped.

It grants the ability to create an account in the deployment.

Household membership is established separately after account creation, through the household flows.

An invitation does not implicitly create a household.

---

## Acceptance Rules

- an invitation is single-use
- an invitation must be accepted to complete account creation
- an accepted invitation cannot be reused
- a declined or unused invitation remains valid until it expires

---

## Expiry and Revocation

- invitations expire after a defined period (default: 7 days)
- the operator may revoke an invitation before acceptance
- once accepted, revocation of an invitation has no effect on the account; the account must be managed separately

---

## Email Dependency

Email delivery is **not required** for the first rollout.

In the absence of email delivery:

- the operator delivers the invite link or code through a trusted out-of-band channel
- the system must still generate and persist the invitation token
- the acceptance endpoint must work independently of email delivery

Email delivery will be wired once an email provider is configured. The invitation flow must not break when email is absent.

---

## Non-Goals for First Rollout

- no household member can send invitations directly without operator involvement
- no public invite link or referral model
- no bulk invite tooling
- no automated re-invite on expiry
- no email templates or branded email flows

---

## Policy vs Domain Separation

Invitation state lives in the platform/auth layer, not in the household domain.

The household domain does not know about invitation tokens. The domain knows only about family membership, which is established after account creation.

Authorization does not bypass family-scoped rules once a user has an account.

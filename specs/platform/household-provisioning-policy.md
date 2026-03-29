# Spec - Household Provisioning Policy

## Purpose

Define how DomusMind decides whether a new household may be created in the current deployment mode.

This is a policy seam, not a domain fork.

---

## Policy

`IHouseholdProvisioningPolicy`

Core responsibility:

- determine whether household creation is allowed
- enforce deployment-mode constraints before household bootstrap flows continue

---

## Modes

### SingleInstance

Rules:

- if no household exists, creation is allowed
- if one household already exists, creation is rejected
- the installation is bound to one household only

### CloudHosted

Rules:

- household creation may be allowed or restricted by configuration
- many households may exist in one deployment
- invitation or approval requirements may be layered on top by policy

---

## Inputs

Required:

- deployment mode
- current household count
- environment policy settings

Optional future inputs:

- authenticated user
- invitation state
- bootstrap token

---

## Outputs

Return a policy result with:

- `allowed`
- `reasonCode`
- `message`

Example reason codes:

- `allowed`
- `single_instance_already_bound`
- `household_creation_disabled`
- `invitation_required`
- `invalid_configuration`

---

## Rules

- must not modify domain state
- must not duplicate family aggregate logic
- must be testable in isolation
- must be invoked from onboarding or bootstrap flow
- must not live inside handlers for normal household features

---

## Success Criteria

The product can enforce different household creation constraints per deployment mode while keeping:

- one domain model
- one set of slices
- one API

---

## Summary

Household creation constraints belong to policy.

They must be explicit, configuration-driven, and separate from household domain behavior.
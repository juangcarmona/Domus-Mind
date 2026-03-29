# Spec - Get Current Deployment Mode

## Purpose

Expose the active deployment mode and relevant runtime capabilities to the app.

This is a platform capability, not a household-domain feature.

---

## Slice

- Module: Platform
- Slice: `get-current-deployment-mode`
- Query: `GetCurrentDeploymentMode`

---

## Inputs

None.

---

## Result

Return:

- `deploymentMode`
- `canCreateHousehold`
- `requiresInvitation`
- `supportsEmail`
- `supportsAdminTools`
- `supportsCloudBackups`
- `abuseProtectionLevel`

Optional future fields:

- `storageProvider`
- `supportContactMode`

---

## Behavior

The system reads configuration and policy state and returns the effective deployment capabilities for the current runtime.

It must not:

- inspect domain aggregates
- depend on household data
- mutate any state

---

## Success Criteria

The result allows the UI and onboarding flow to adapt without forking feature logic.

---

## Failure Cases

- deployment mode not configured
- invalid configuration combination

---

## Notes

This slice exists to make deployment constraints explicit and testable.

It must stay small, read-only, and configuration-driven.
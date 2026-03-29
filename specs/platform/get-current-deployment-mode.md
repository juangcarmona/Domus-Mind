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

- `deploymentMode` — the active mode (`SingleInstance` or `CloudHosted`)
- `canCreateHousehold` — whether a new household can be created right now (policy + state)
- `requiresInvitation` — whether a valid invitation is required to proceed with signup
- `supportsEmail` — whether email sending is available
- `supportsAdminTools` — whether admin tooling is available

Future fields (not yet implemented):

- `supportsCloudBackups`
- `abuseProtectionLevel`
- `storageProvider`
- `supportContactMode`

---

## Behavior

The system reads configuration and calls the provisioning policy to return the effective deployment capabilities for the current runtime.

- `canCreateHousehold` is derived from the provisioning policy, which may read current installation state (e.g. whether a household already exists in `SingleInstance` mode)
- `requiresInvitation` reflects `RequireInvitationForSignup` from deployment config — the requirement for an invitation to complete signup, not merely whether the invitation feature is enabled
- this slice must not modify any aggregate or domain state

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
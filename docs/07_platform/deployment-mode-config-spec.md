# DomusMind - Deployment Mode Configuration Spec

## Purpose

Define the configuration contract required to run DomusMind in different deployment modes.

---

## Core Setting

### DeploymentMode

Allowed values:

- `SingleInstance`
- `CloudHosted`

This setting is required.

---

## Configuration Surface

### Household Provisioning

- `AllowHouseholdCreation`

Expected behavior:

- `SingleInstance` — exactly one household; enforced by DB unique constraint
- `CloudHosted` — operator-controlled; `AllowHouseholdCreation` governs self-service creation

### Invitations

- `InvitationsEnabled`
- `RequireInvitationForSignup`

### Email

- `EmailEnabled`
- `EmailProvider`

### Abuse Protection

- `RateLimitingEnabled`
- `CaptchaEnabled`
- `SignupProtectionLevel`

### Storage

- `StorageProvider`
- `UseLocalPersistentStorage`

### Observability

- `StructuredLoggingEnabled`
- `MetricsEnabled`
- `TracingEnabled`

### Backup

- `BackupEnabled`
- `BackupProfile`

### Support Tooling

- `AdminToolsEnabled`
- `SupportToolsEnabled`

---

## Rules

- configuration must not alter domain behavior
- configuration may enable or restrict operational flows
- all values must be resolved at startup
- invalid combinations must fail fast
- `SingleInstance` + `InvitationsEnabled = true` is invalid
- `SingleInstance` + `RequireInvitationForSignup = true` is invalid
- `RequireInvitationForSignup = true` requires `InvitationsEnabled = true`

---

## Recommended Typed Options

A single strongly typed options object should expose:

- deployment mode
- household creation policy
- invitation settings
- email settings
- abuse protection settings
- storage settings
- observability settings
- backup settings
- support tooling settings

---

## Summary

Deployment mode is selected by configuration.

Configuration changes policies and infrastructure behavior, never product logic.
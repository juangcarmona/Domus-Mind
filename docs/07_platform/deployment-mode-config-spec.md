# DomusMind - Deployment Mode Configuration Spec

## Purpose

Define the configuration contract required to run DomusMind in different deployment modes.

---

## Core Setting

### DeploymentMode

Allowed values:

- `SingleInstance` — one household, one operator, no self-service signup
- `CloudHosted` — multi-household, operator-configured access control

This setting is required.

---

## Canonical Config Contract

The following table defines all `Deployment:*` keys, their types, defaults, and which modes support them.

| Key | Type | Default | SingleInstance | CloudHosted | Notes |
|-----|------|---------|----------------|-------------|-------|
| `Mode` | string | — | required | required | Case-insensitive |
| `AllowHouseholdCreation` | bool | true | supported | supported | Governs self-service family creation |
| `InvitationsEnabled` | bool | false | **invalid** | supported | Must be false in SingleInstance |
| `RequireInvitationForSignup` | bool | false | **invalid** | supported | Requires InvitationsEnabled=true |
| `EmailEnabled` | bool | false | supported | supported | |
| `AdminToolsEnabled` | bool | false | supported | supported | Enables /admin surface |
| `MaxHouseholdsPerDeployment` | int | 0 | 0 or 1 only | 0 or ≥2 | 0 = unlimited; 1 invalid in CloudHosted |

---

## Configuration Surface

### Household Provisioning

- `AllowHouseholdCreation`
- `MaxHouseholdsPerDeployment`

Expected behavior:

- `SingleInstance` — exactly one household; `MaxHouseholdsPerDeployment` must be 0 or 1
- `CloudHosted` — operator-controlled; `AllowHouseholdCreation` governs self-service creation; `MaxHouseholdsPerDeployment` caps total families (0 = unlimited, value 1 is invalid — use SingleInstance)

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

## Validation Rules

All rules are enforced eagerly at startup via `DeploymentSettings.Validate()`. Invalid combinations throw `InvalidOperationException` and prevent startup.

| Rule | Error |
|------|-------|
| `Mode` not in `[SingleInstance, CloudHosted]` | Invalid Deployment:Mode value |
| `SingleInstance` + `InvitationsEnabled = true` | InvitationsEnabled is not supported in SingleInstance mode |
| `SingleInstance` + `RequireInvitationForSignup = true` | RequireInvitationForSignup is not supported in SingleInstance mode |
| `RequireInvitationForSignup = true` + `InvitationsEnabled = false` | RequireInvitationForSignup requires InvitationsEnabled |
| `MaxHouseholdsPerDeployment < 0` | MaxHouseholdsPerDeployment cannot be negative |
| `SingleInstance` + `MaxHouseholdsPerDeployment > 1` | MaxHouseholdsPerDeployment must be 0 or 1 |
| `CloudHosted` + `MaxHouseholdsPerDeployment == 1` | MaxHouseholdsPerDeployment = 1 is not valid for CloudHosted mode |

---

## Observability

At startup, the API logs all effective settings as structured properties:

```
DomusMind startup: Mode={DeploymentMode} AllowHouseholdCreation={...} InvitationsEnabled={...}
  RequireInvitationForSignup={...} AdminToolsEnabled={...} MaxHouseholdsPerDeployment={...}
  BootstrapAdminEnabled={...} BootstrapAdminEmailConfigured={...}
```

Every household provisioning evaluation is logged at `Information` level with the decision and all relevant context:

```
Household creation {Decision}: {ReasonCode} | DeploymentMode={...} AllowHouseholdCreation={...}
  InvitationsEnabled={...} RequireInvitationForSignup={...} MaxHouseholdsPerDeployment={...}
```

Both log events flow to Application Insights when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set.

### Useful Kusto queries

Configuration drift detection:
```kusto
traces
| where message startswith "DomusMind startup:"
| extend mode = tostring(customDimensions["DeploymentMode"])
| summarize count() by mode, bin(timestamp, 1d)
```

Household creation denials:
```kusto
traces
| where message startswith "Household creation Denied:"
| extend reason = tostring(customDimensions["ReasonCode"])
| summarize count() by reason, bin(timestamp, 1h)
```

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
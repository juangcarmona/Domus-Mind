# ADR-003 - First CloudHosted Azure Baseline

## Status

Accepted

---

## Context

DomusMind supports two deployment modes:

* SingleInstance
* CloudHosted

Both modes must preserve the same:

* domain model
* handlers
* endpoints
* UI components
* read models
* core auth/session logic
* migrations
* feature slices

Only operational and policy concerns may vary by deployment mode, such as:

* onboarding constraints
* household creation policy
* invitation policy
* environment configuration
* storage provider
* email provider
* rate limits
* abuse controls
* backup strategy
* observability
* admin/support tooling

DomusMind CloudHosted must support real households with private household data while keeping cost and operator burden low.

---

## Decision

The first real CloudHosted baseline will use:

* **Azure App Service on Linux** as the hosting target
* **Azure Database for PostgreSQL Flexible Server** as the database
* **West Europe** as the initial Azure region
* **Azure Key Vault** for secrets
* **Application Insights + Azure Monitor** for observability
* **manual household creation + invite-only access** as the initial signup policy

CloudHosted will remain a **single shared deployment** with **family-scoped authorization**.

It will **not** introduce a separate CloudHosted code path in domain or slice logic.

---

## Decision Drivers

* keep one codebase and one artifact set
* minimize operational complexity
* choose cheap but credible production infrastructure
* protect private household data
* preserve future ability to keep CloudHosted permanently
* avoid accidental SaaS re-architecture
* avoid divergence from SingleInstance behavior

---

## Options Considered

### Option 1 - Azure App Service on Linux

Description:

* one managed web hosting target
* serves API and static web app
* simple configuration, deployment, TLS, logging, and managed identity support

Pros:

* lowest operator burden
* straightforward deployment model
* good fit for always-available web/API application
* simpler than container-native hosting for V1
* easy path to add staging slots later by moving to Standard tier

Cons:

* always-on cost floor
* less elastic than container-native scale models
* deployment slots require higher tier than Basic

Assessment:

**Selected as the best first CloudHosted baseline.**

---

### Option 2 - Azure Container Apps

Description:

* container-native hosting with revisions and autoscaling

Pros:

* flexible scaling model
* revision-based deployment behavior
* scale-to-zero capability

Cons:

* more moving parts
* more operational complexity
* worse fit for a simple, always-available early hosted product
* unnecessary platform sophistication for first baseline

Assessment:

Rejected as the first baseline due to higher operational complexity.

---

## Rationale

The first hosted version of DomusMind needs:

* simple deployment
* predictable operation
* low monthly cost
* safe handling of private household data
* minimal platform complexity

Azure App Service on Linux provides the simplest credible production baseline for this shape.

Azure Database for PostgreSQL Flexible Server provides managed PostgreSQL with backups, restore capability, encryption, and low-friction operations.

Invite-only access reduces abuse risk and support burden while multi-household boundaries are still being hardened.

This baseline is intentionally conservative.

It optimizes for:

* correctness
* operability
* low friction

It does not optimize for theoretical scale.

---

## Decision Details

The selected baseline includes:

* App Service Linux as the application host
* PostgreSQL Flexible Server as the primary data store
* West Europe as the first region
* Key Vault for secrets such as database credentials, signing keys, and provider secrets
* non-secret runtime configuration in application settings
* Application Insights and Azure Monitor alerts
* invite-only access
* manual household creation for the first hosted households
* backup retention and restore runbook as part of production readiness

Initial operating posture:

* no open signup
* no email-critical flows required for launch
* no Kubernetes
* no microservices
* no deployment-mode-specific domain behavior

Admin/support implications:

* CloudHosted requires an **operator surface**
* this may begin as a thin internal admin or super-admin area, or controlled manual operations
* support capabilities must remain outside household domain behavior
* support tooling must not bypass family-scoped authorization rules casually

---

## Consequences

### Positive

* low infrastructure complexity
* cheap initial production baseline
* easier deployment and tracing
* credible operational posture for early real households
* preserves one product model across deployment modes
* allows CloudHosted to continue long-term if adoption justifies it

### Negative

* some support actions remain manual at first
* rollout safety is limited until moving to a tier with deployment slots
* invite and support flows need admin tooling sooner rather than later
* abuse protection remains policy-heavy until productized further

---

## Follow-Up Rules

* CloudHosted must not fork domain behavior from SingleInstance
* family-scoped authorization must remain explicit in all hosted flows
* admin/support tooling must stay operational, not domain-defining
* open signup is deferred until abuse controls and support tooling are stronger
* email remains optional for first hosted rollout
* future platform upgrades must not force changes to slices, handlers, or read models

---

## Result

DomusMind CloudHosted starts on a **simple Azure managed baseline**:

* **Azure App Service on Linux**
* **Azure Database for PostgreSQL Flexible Server**
* **West Europe**
* **Key Vault**
* **Application Insights + Azure Monitor**
* **manual household creation + invite-only access**

This is the first real hosted baseline for early production use.

It is intentionally cheap, safe, and operationally restrained.

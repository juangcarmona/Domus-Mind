# CloudHosted - Deployment Flow

## Status

Accepted — Azure App Service baseline for first rollout

---

## Target Runtime Topology

| Component | Service | Notes |
|---|---|---|
| Application | Azure App Service on Linux | B1 tier; serves API + static web |
| Database | Azure Database for PostgreSQL Flexible Server | Managed, automated backups, VNet or firewall-restricted |
| Secrets | Azure Key Vault | DB credentials, JWT signing key, provider secrets |
| Config | App Service Application Settings | Non-secret runtime config; mapped to .NET config keys |
| Observability | Application Insights + Azure Monitor | Structured telemetry, alert rules |
| Region | West Europe | Single region for first rollout |

There is no separate web container, CDN, or separate static host. The API serves the built SPA.

---

## Config and Secrets Ownership

**Key Vault** holds:

- `DB_PASSWORD` — PostgreSQL user password
- `JWT_SECRET` — JWT signing key (minimum 32 chars)
- any future provider secrets (email, third-party API keys)

**App Service Application Settings** hold:

- `Deployment__Mode` = `CloudHosted`
- `Deployment__AllowHouseholdCreation` — policy setting
- `Deployment__InvitationsEnabled` = `true`
- `Deployment__RequireInvitationForSignup` = `true`
- `Deployment__AdminToolsEnabled` = `true`
- `Jwt__Issuer` — token issuer claim
- `Jwt__Audience` — token audience claim
- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ConnectionStrings__domusmind` — references Key Vault secret via Key Vault reference syntax

App Service uses the Key Vault reference syntax (`@Microsoft.KeyVault(...)`) to resolve secrets at runtime. Secrets are never written directly into application settings.

---

## Deployment Order

First-time provisioning:

1. Provision PostgreSQL Flexible Server
2. Provision Key Vault; store DB credentials and JWT signing key
3. Provision App Service; configure application settings and Key Vault references
4. Configure App Insights; set `APPLICATIONINSIGHTS_CONNECTION_STRING` in application settings
5. Deploy the `domusmind` image or app package
6. Application starts; EF Core migrations run at startup
7. Call `GET /api/setup/status` — should return `{ isInitialized: false }`
8. Call `POST /api/setup/initialize` to create the initial operator account
9. Verify `GET /api/platform/deployment-mode` returns expected capabilities
10. Verify health via App Service health check and Application Insights live metrics

Subsequent deployments:

1. Deploy the new image or app package to App Service
2. App Service replaces the running instance
3. Application starts; EF Core applies any new migrations
4. Verify health check passes

---

## Migration Expectations

Migrations run automatically at API startup via `dbContext.Database.Migrate()`.

Rules:

- all V1 schema changes are additive only — no destructive changes without a MAJOR version bump
- a failed migration at startup will cause the app instance to fail its health check
- the operator must verify migration state after any deployment that includes schema changes

For the first rollout, a single App Service instance handles migrations at startup. If horizontal scaling is introduced later, migration must move to a pre-deployment step or an init container equivalent.

---

## Health Check Expectations

App Service must be configured with a health check path.

Recommended: `GET /api/setup/status`

This endpoint is unauthenticated and returns a 200 with the initialization state. A successful response confirms the application is running and the database is reachable.

Alternatively, a dedicated `/health` or `/api/health` endpoint may be introduced. Until then, the setup status endpoint serves this purpose.

---

## Rollback Posture

There is no deployment slot on the Basic B1 tier.

Rollback procedure for B1:

- redeploy the previous image or app package version
- App Service replaces the running instance with the previous version
- if the new version introduced a migration, the previous version may not be compatible with the new schema
- for safe rollback across schema changes, a database point-in-time restore may be required

For zero-downtime deployments and proper slot-based swap rollback, move to Standard tier and use a staging slot. This is deferred past the initial rollout.

---

## Non-Goals for First Rollout

- no Kubernetes or container orchestration
- no deployment slots / blue-green on B1
- no separate database migration step or init container
- no CDN or global edge distribution
- no multi-region active-active setup
- no automated rollback pipeline

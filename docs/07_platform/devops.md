# DomusMind - DevOps & Operations

## Purpose

This document is the canonical reference for how DomusMind is built, packaged, distributed, deployed, and operated.

DomusMind is one product with two deployment modes: `SingleInstance` and `CloudHosted`. Both modes use the same artifacts, the same database schema, the same migrations, the same API surface, and the same domain model. Deployment mode changes only operational policy and infrastructure behavior.

CloudHosted operator policy is detailed in `docs/07_platform/cloud-hosted/`:

- [deployment-flow.md](cloud-hosted/deployment-flow.md) — Azure runtime topology and deployment order
- [invitation-policy.md](cloud-hosted/invitation-policy.md) — invite-only access model
- [signup-policy.md](cloud-hosted/signup-policy.md) — no open signup; manual provisioning
- [operator-tooling.md](cloud-hosted/operator-tooling.md) — minimum operator capabilities
- [abuse-protection.md](cloud-hosted/abuse-protection.md) — closed-cohort baseline and rate limiting
- [backup-restore-policy.md](cloud-hosted/backup-restore-policy.md) — managed PostgreSQL backup posture

SingleInstance operator guide: [single-instance-operator-guide.md](single-instance-operator-guide.md) — fresh install, upgrade, backup, restore, and troubleshooting.

---

## Deployment Principles

- one product, one codebase, one set of artifacts
- deployment mode is a runtime configuration choice, not a code fork
- all schema changes and migrations apply equally to both modes
- no endpoint, handler, aggregate, or read model differs by deployment mode
- configuration drives policy; policy never enters domain or application logic

---

## Runtime Topology

### SingleInstance

For self-hosted installations: home servers, NAS devices, mini PCs.

Services:

| Service | Image | Role |
|---|---|---|
| `postgres` | `postgres:17-alpine` | Database |
| `domusmind` | `ghcr.io/<owner>/domusmind:<version>` | API + static web |

- one household only — enforced by household provisioning policy
- ingress is the `domusmind` container on a configurable host port
- postgres is not exposed to the host network
- HTTPS is handled by an external reverse proxy

### CloudHosted

For managed cloud deployments supporting multiple households.

The same two services run on cloud infrastructure. Additional operational concerns:

- external managed database (e.g. Azure Database for PostgreSQL, Amazon RDS)
- TLS terminated at load balancer or ingress controller
- email and notification providers enabled and configured
- abuse protection and rate limiting enabled

Both modes share the same `docker-compose.yml` structure as a baseline. Cloud deployments may adapt the compose file or deploy via container orchestration (Kubernetes, Azure Container Apps, etc.) using the same image.

---

## Stack Composition

DomusMind is deployed as a Docker Compose stack.

The production stack includes two services:

| Service | Image | Purpose |
|---|---|---|
| `postgres` | `postgres:17-alpine` | Relational database (family state, events, auth) |
| `domusmind` | `ghcr.io/<owner>/domusmind` | ASP.NET Core app serving API and static web |

The DomusMind app is the ingress surface. PostgreSQL is internal to the Docker network - it is never exposed to the host unless explicitly configured.

---

## Inner Loop (Development)

.NET Aspire **orchestrates the local development environment only**. It is not used in production.

- Aspire starts the API, web app, PostgreSQL, and pgAdmin
- Aspire injects connection strings and service references automatically
- Aspire provides the developer dashboard, health checks, and structured telemetry
- `AppHost.cs` is the source of truth for local service topology

Aspire is not used in production or staging. Docker Compose replaces it.

### Keeping Compose Aligned with AppHost

`dotnet aspire publish --publisher docker-compose` generates a Docker Compose baseline from the AppHost definition. Run this whenever a new service is added to `AppHost.cs`. Manual adjustments (image tags, environment variable names, health check tuning) are applied on top of the generated baseline.

```
AppHost.cs → aspire publish → docker-compose.yml (baseline) → manual overlay → release artifact
```

---

## Artifact Strategy

### Image

One Docker image: `ghcr.io/<owner>/domusmind`

The image contains the compiled ASP.NET Core API and the built web application served as static files from the API. No separate web container is needed in production.

### Tags

| Tag | Meaning |
|---|---|
| `1.2.3` | Exact release — recommended for production |
| `1.2` | Latest patch on that minor version |
| `latest` | Latest stable release |
| `edge` | Latest successful `main` build — not for production |
| `1.1.0-alpha.1` | Prereleases — not for production |

### Release Assets

Each GitHub Release publishes:
- `docker-compose.yml`
- `.env.example`
- `CHANGELOG.md` entry

---

## Versioning

DomusMind uses semantic versioning: `MAJOR.MINOR.PATCH`

| Segment | When it changes |
|---|---|
| MAJOR | Breaking change requiring manual user action (schema rename, `.env` restructure) |
| MINOR | New capabilities; migrations run automatically; safe update |
| PATCH | Bug fixes; no schema changes |

---

## Configuration Contract

All environment-specific and sensitive values live in a `.env` file on the host. This file is never committed to source control. See `deploy/.env.example` for the full configuration surface.

Required for all modes:

| Variable | Description |
|---|---|
| `DB_USER` | PostgreSQL username |
| `DB_PASSWORD` | PostgreSQL password — strong, generated per installation |
| `JWT_SECRET` | JWT signing key — minimum 32 characters, generated per installation |
| `JWT_ISSUER` | Token issuer claim |
| `VERSION` | Image tag to run |
| `APP_PORT` | Host port for the DomusMind container |

Deployment mode:

| Variable | Values |
|---|---|
| `DEPLOYMENT_MODE` | `SingleInstance` \| `CloudHosted` |

Policy variables (e.g. `ALLOW_HOUSEHOLD_CREATION`, `INVITATIONS_ENABLED`) are resolved at startup. Invalid combinations fail fast.

Secrets must never be committed to source control. In CI, use GitHub Actions secrets. In production, use environment variables or a secrets manager appropriate to the hosting environment.

---

## Bootstrap Rules

DomusMind uses a two-path first-run model. Both paths are single-use. Once the system is initialized, all bootstrap paths become permanent no-ops.

### Primary path — UI setup wizard

The default path for all installations.

```
GET  /api/setup/status       → { isInitialized: false }  (unauthenticated)
POST /api/setup/initialize   → 201 Created                (unauthenticated, one-time only)
```

- no configuration required
- the endpoint is permanently routeable but server-gated
- calling `POST /api/setup/initialize` a second time returns `409 Conflict`
- initialization is atomic: admin user created and system marked initialized in one request

### Fallback path — headless bootstrap

For scripted or CI environments where the UI is not available.

- disabled by default (`BootstrapAdmin__Enabled = false`)
- no-op if the system is already initialized
- activated via environment variables in `docker-compose.yml`
- must be disabled after first use

---

## Data and Persistence

- EF Core is the primary persistence technology
- PostgreSQL is the only supported database in V1
- state lives in the `postgres_data` Docker volume
- all schema changes are additive only in V1 — no drops, renames, or destructive changes
- migrations run automatically at API startup via `dbContext.Database.Migrate()`
- breaking schema changes are versioned as MAJOR releases and documented explicitly

### Backup

All data lives in the `postgres_data` volume.

Minimum recommended backup:

```bash
docker exec domusmind-postgres-1 \
  pg_dump -U $DB_USER domusmind \
  > backups/domusmind_$(date +%Y%m%d_%H%M%S).sql
```

Restore:

```bash
docker exec -i domusmind-postgres-1 \
  psql -U $DB_USER -d domusmind \
  < backups/domusmind_20260321_120000.sql
```

Automated backup scheduling is delegated to the host OS or a dedicated backup container.

---

## Security Baseline

- passwords are hashed; plaintext passwords are never stored or logged
- JWT signing key is validated at startup; invalid or weak keys fail fast
- token claims contain user identity only, not domain state
- refresh tokens are stored server-side and rotated on renewal
- HTTPS must be provided by an external reverse proxy in production
- the postgres container is not exposed outside the Docker network
- Swagger is available in development; access may be restricted in production by environment

---

## Observability Baseline

Structured logging, metrics, and tracing are controlled by configuration.

`SingleInstance` typically runs with structured logging only. `CloudHosted` should enable metrics and tracing.

No observability framework is bundled in V1. The application emits structured logs to stdout; operators route these to their preferred sink.

---

## Release Pipeline

**Trigger:** tag push matching `v*.*.*`

**Steps:**

1. Restore and build the .NET solution
2. Run backend tests
3. Build the `domusmind` Docker image (includes frontend build)
4. Push image to `ghcr.io/<owner>/domusmind` with version tags
5. Generate release notes from commit log since previous tag
6. Publish GitHub Release with `docker-compose.yml`, `.env.example`, and changelog entry

**CI tool:** GitHub Actions

`main` branch builds (non-tagged) produce the `edge` image.

---

## Deployment Flows

### SingleInstance — first install

```bash
# 1. Download docker-compose.yml and .env.example from the GitHub Release
# 2. cp .env.example .env  →  fill in DB_PASSWORD, JWT_SECRET, VERSION, APP_PORT
#    Set DEPLOYMENT_MODE=SingleInstance
# 3. docker compose up -d
# 4. Open http://localhost:<APP_PORT> and complete the setup wizard
```

### SingleInstance — update

```bash
# 1. Read the release notes (check for new .env variables or migration notes)
# 2. Update VERSION in .env; add any new required variables
# 3. docker compose pull
# 4. docker compose up -d
# Migrations run automatically at startup.
```

### CloudHosted — first deploy

```bash
# 1. Configure managed database; update DB_USER, DB_PASSWORD, and connection string
# 2. Set DEPLOYMENT_MODE=CloudHosted and policy variables
# 3. Deploy using docker-compose.yml or equivalent container orchestration
# 4. Complete setup via the API (UI or headless bootstrap)
```

### CloudHosted — update

```bash
# 1. Pull the new image tag
# 2. Apply any new environment variables from .env.example
# 3. Restart the service; migrations run automatically at startup
```

---

## Reverse Proxy

DomusMind serves plain HTTP. HTTPS is terminated upstream.

- `https://domusmind.example.com` → `http://<host>:<APP_PORT>`
- API and web are on the same origin (`/api` prefix for API routes)

Compatible proxies: Caddy, Nginx Proxy Manager, Traefik, HAProxy.

---

## Non-Goals

- DomusMind does not implement its own backup strategy in V1
- DomusMind does not provide a CLI setup tool in V1
- DomusMind does not support databases other than PostgreSQL in V1
- DomusMind does not run multiple API containers in V1 (startup migration assumes single writer)
- DomusMind does not provide built-in auto-update integration
- Deployment mode does not alter domain behavior, aggregates, handlers, slices, migrations, or API contracts

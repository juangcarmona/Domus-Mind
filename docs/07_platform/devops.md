# DomusMind - DevOps & Application Lifecycle Management

## Purpose

This document defines how DomusMind is built, versioned, distributed, updated, and operated in self-hosted environments.

DomusMind is designed to run at home - on a mini PC, a NAS, a home server, or any machine capable of running Docker. It must be trivial to install, safe to update, and straightforward to maintain with no DevOps expertise.

This document defines the application lifecycle for that model: from code commit to a running household installation.

---

## Stack Composition

DomusMind is deployed as a Docker Compose stack.

The production stack includes two services:

| Service | Image | Purpose |
|---|---|---|
| `postgres` | `postgres:16-alpine` | Relational database (family state, events, auth) |
| `domusmind` | `ghcr.io/<owner>/domusmind` | ASP.NET Core app serving API and static web |

The DomusMind app is the ingress surface. PostgreSQL is internal to the Docker network - it is never exposed to the host unless explicitly configured.

---

## .NET Aspire Role: Inner Loop Only

.NET Aspire **orchestrates local development only**. It is not used in production.

In local development (current state):
- Aspire starts all services - API, web, PostgreSQL, pgAdmin
- Aspire injects connection strings and service references automatically
- Aspire provides the developer dashboard, health checks, and structured telemetry
- The AppHost definition is the source of truth for the service topology

In production:
- Aspire does not run
- Docker Compose replaces Aspire's orchestration role
- Service dependencies, health checks, environment wiring, and volumes are defined in `docker-compose.yml`

### Keeping Compose Aligned with AppHost

`dotnet aspire publish --publisher docker-compose` generates a Docker Compose baseline from the AppHost definition. This command should be run whenever a new service is added to `AppHost.cs`, to regenerate a canonical starting point. Manual adjustments (image tags, environment variable names, health check tuning) are applied on top of the generated baseline.

The flow is:

```
AppHost.cs → aspire publish → docker-compose.yml (baseline) → manual overlay → release artifact
```

This ensures the production Compose file is never drafted from scratch and always reflects the actual topology.

---

## Docker Compose Definition

The canonical `docker-compose.yml` released with each version:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: domusmind
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER} -d domusmind"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  domusmind:
    image: ghcr.io/${IMAGE_OWNER}/domusmind:${VERSION:-latest}
    environment:
      ConnectionStrings__domusmind: Host=postgres;Database=domusmind;Username=${DB_USER};Password=${DB_PASSWORD}
      Jwt__SigningKey: ${JWT_SECRET}
      Jwt__Issuer: ${JWT_ISSUER:-domusmind}
      ASPNETCORE_URLS: http://0.0.0.0:8080
    depends_on:
      postgres:
        condition: service_healthy
    ports:
      - "${APP_PORT:-24365}:8080"
    restart: unless-stopped

volumes:
  postgres_data:
```

The `docker-compose.yml` and a `.env.example` file are published as release assets on every GitHub Release.

---

## Configuration

All sensitive and environment-specific values live in a `.env` file on the host machine. This file is never committed to source control.

Users copy `.env.example` to `.env` and fill in their values.

Required variables:

| Variable | Description | Example |
|---|---|---|
| `DB_USER` | PostgreSQL username | `domusmind` |
| `DB_PASSWORD` | PostgreSQL password | *(generate a strong password)* |
| `JWT_SECRET` | JWT signing secret, minimum 32 characters | *(generate randomly)* |
| `JWT_ISSUER` | Token issuer identifier | `domusmind` |
| `VERSION` | Image version to run | `1.0.0` or `latest` |
| `APP_PORT` | Host port for the DomusMind app | `24365` |

The `JWT_SECRET` must be generated per installation. DomusMind may provide a setup helper command in a future release (see Open Decisions).

---

## Database Migrations

EF Core migrations are applied automatically at API startup.

The API runs `dbContext.Database.Migrate()` during the startup sequence, before accepting traffic. This is safe for single-instance deployments.

Migration rules for V1:

- All schema changes are **additive only** - no column drops, no renames, no destructive changes
- Every release that includes a migration must document it in the release notes
- Breaking schema changes (if ever required) will be explicitly versioned as a MAJOR release with a documented migration note and an upgrade path

If horizontal scaling is introduced in a future version, the startup migration will be replaced with a dedicated `migrate` Compose target that runs as a one-shot init container before the API starts.

---

## Versioning Strategy

DomusMind uses semantic versioning: `MAJOR.MINOR.PATCH`

| Segment | When it changes |
|---|---|
| MAJOR | Breaking changes requiring manual user action (schema rename, .env restructure) |
| MINOR | New capabilities added; migrations run automatically; safe update |
| PATCH | Bug fixes, no schema changes |

Docker images are tagged with:

- `v1.2.3` — exact release (immutable, prefixed with `v`)
- `1.2.3` — same commit, no prefix (immutable)
- `sha-<shortsha>` — commit-pinned reference

Mutable aliases such as `latest` and `1.2` are not published for releases. Users pin to an exact version in `.env`.

---

## Release Pipeline

**Trigger:** manual `workflow_dispatch` from the Actions tab on the `main` branch.

**Inputs:**

| Input | Type | Description |
|---|---|---|
| `version` | string, required | Semver string without leading `v` — e.g. `0.1.4` or `0.1.4-beta.1` |
| `prerelease` | boolean, required | Whether to mark the GitHub Release as a pre-release |

**Steps:**

1. Validate `version` matches a semver pattern (supports optional prerelease and build metadata).
2. Fail if not on `main`.
3. Fail if the tag `v<version>` already exists.
4. Run backend restore, build, and test gate.
5. Create and push annotated git tag `v<version>`.
6. Generate `src/web/app/src/generated/version.ts` with version, release date, short SHA, and prerelease flag.
7. Build the `domusmind` Docker image (with the generated version metadata baked in) and push to GHCR with three immutable tags: `v<version>`, `<version>`, `sha-<shortsha>`.
8. Validate the `docker-compose.yml` resolves correctly.
9. Publish a GitHub Release with:
   - auto-generated release notes from commit history
   - `docker-compose.yml`
   - `.env.example`
   - `deploy/README.md`

**CI tool:** GitHub Actions (`.github/workflows/release.yml`)

### Publishing a pre-release

```
Actions → release → Run workflow
  version:    0.1.4-beta.1
  prerelease: true (checked)
```

This creates tag `v0.1.4-beta.1`, a GitHub pre-release, and GHCR images tagged
`v0.1.4-beta.1`, `0.1.4-beta.1`, and `sha-<shortsha>`.

### Publishing a stable release

```
Actions → release → Run workflow
  version:    0.1.4
  prerelease: false (unchecked)
```

This creates tag `v0.1.4`, a normal GitHub Release, and GHCR images tagged
`v0.1.4`, `0.1.4`, and `sha-<shortsha>`.

Main branch builds (non-release) produce images tagged `:edge` via the separate
`docker-edge` workflow for integration testing.

---

## Release Metadata in the Web App

During each release build the workflow generates `src/web/app/src/generated/version.ts`
with the following exports, which are baked into the compiled web bundle:

```ts
export const APP_VERSION       = "v0.1.4-beta.1";
export const APP_RELEASE_DATE  = "2026-03-31";
export const APP_COMMIT_SHA    = "abc1234";
export const APP_IS_PRERELEASE = true;
```

These are consumed by the **About** section in Settings (`Settings → About`).

---

## User Update Flow

A home user updating from one release to the next:

```bash
# 1. Download the new docker-compose.yml from the GitHub Release
#    (or replace in place if using a fixed path)

# 2. Read the release notes
#    Check for new required .env variables or migration notes

# 3. Update .env if new variables were introduced

# 4. Pull the new images
docker compose pull

# 5. Restart the stack
docker compose up -d

# Migrations run automatically on app startup.
# The stack is live within seconds.
```

No CLI tooling, no package manager, no cloud account.

---

## Reverse Proxy Integration

DomusMind is designed to sit behind a reverse proxy for HTTPS termination and external access. The stack itself serves plain HTTP from a single app container.

Recommended pattern:
- `https://domusmind.home.example.com` → `http://[host]:24365` (or your configured `APP_PORT`)
- API remains available under `/api` on the same origin

Compatible reverse proxies: Nginx Proxy Manager, Caddy, Traefik, HAProxy, Home Assistant Nginx Add-on.

HTTPS termination at the reverse proxy is strongly recommended, especially when exposing DomusMind outside the LAN.

---

## Backup Strategy

All persistent state lives in the `postgres_data` Docker volume.

Minimum recommended backup:

```bash
docker exec domusmind-postgres-1 \
  pg_dump -U $DB_USER domusmind \
  > ~/backups/domusmind_$(date +%Y%m%d_%H%M%S).sql
```

This can be automated with a cron job on the host. A built-in backup mechanism is out of scope for V1 and delegated to the host OS or a dedicated backup container.

Restore:

```bash
docker exec -i domusmind-postgres-1 \
  psql -U $DB_USER -d domusmind \
  < ~/backups/domusmind_20260321_120000.sql
```

---

## Release Cadence

| Phase | Cadence | Focus |
|---|---|---|
| V1 | On-demand | Feature completeness, first installations |
| V1.1 | Monthly | Stability, operational hardening, no new contexts |
| V2+ | Quarterly | Capability expansion |

Patch releases are unscheduled and released as needed.

---

## Open Decisions

| # | Question | Notes |
|---|---|---|
| OD-1 | Should the API run migrations on startup or as a separate `migrate` Compose service? | Startup is simpler for V1; init container is safer at scale |
| OD-2 | Should images be on Docker Hub (public, no auth to pull) or ghcr.io (GitHub-linked)? | ghcr.io is free for public repos; Docker Hub has wider tooling support |
| OD-3 | Should DomusMind provide a `domusmind-setup` CLI to generate `.env` and validate the config? | Reduces friction for non-technical users |
| OD-4 | First-run bootstrap: is there an admin registration page, or does the first `CreateFamily` call auto-promote the user? | Relevant to the initial onboarding UX |
| OD-5 | Should the web app be served from its own nginx container or should the API serve the built SPA? | Resolved: API serves the built SPA in self-hosted packaging |
| OD-6 | Should DomusMind publish a Watchtower-compatible label scheme for automatic update notifications? | Optional convenience for technically-minded home users |

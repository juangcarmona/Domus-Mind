# DomusMind - DevOps & Application Lifecycle Management

## Purpose

This document defines how DomusMind is built, versioned, released, deployed, updated, and operated in self-hosted environments.

DomusMind is packaged for a single self-hosted household installation. The goal is boring, predictable operation:

- exact versions
- simple upgrades
- clear runtime identity
- low operational friction

---

## Runtime Model

DomusMind runs as a Docker Compose stack.

Default production stack:

| Service | Image | Purpose |
|---|---|---|
| `postgres` | `postgres:17-alpine` | Relational database |
| `domusmind` | `ghcr.io/<owner>/domusmind` | ASP.NET Core app serving API and web app |

Rules:

- PostgreSQL stays internal to the Compose network
- the app is the only exposed service
- self-hosted runtime stays single-instance and simple
- Aspire is for local development only, not production

---

## Configuration

All installation-specific values live in `.env` on the host.

Required variables:

| Variable | Description |
|---|---|
| `IMAGE_REGISTRY` | Container registry |
| `IMAGE_OWNER` | Image namespace |
| `VERSION` | Exact image version to run |
| `DB_USER` | PostgreSQL username |
| `DB_PASSWORD` | PostgreSQL password |
| `JWT_SECRET` | JWT signing secret |
| `JWT_ISSUER` | Token issuer |
| `JWT_AUDIENCE` | Token audience |
| `APP_PORT` | Host port |

Rules:

- `.env` is never committed
- exact versions only
- no floating release aliases
- `JWT_SECRET` must be generated per installation

---

## Database Migrations

EF Core migrations run automatically at app startup.

Rules:

- additive schema changes by default
- migration notes required when schema changes are included
- breaking schema changes require a MAJOR release and explicit upgrade notes

---

## Versioning

DomusMind uses semantic versioning with explicit prerelease lanes.

Allowed shapes:

- `MAJOR.MINOR.PATCH-beta.N`
- `MAJOR.MINOR.PATCH-rc.N`
- `MAJOR.MINOR.PATCH`

Examples:

- `1.0.0-beta.1`
- `1.0.0-rc.1`
- `1.0.0`

Meaning:

| Lane | Meaning |
|---|---|
| `beta` | testing release |
| `rc` | release candidate |
| stable | intended upgrade target |

Bump rules:

| Segment | When it changes |
|---|---|
| MAJOR | breaking upgrade or required manual action |
| MINOR | significant capability increment |
| PATCH | bug fix, UX refinement, packaging or operational hardening |

---

## Distribution Policy

There is one public distribution path:

- manual release from `main`

There is no public floating `edge` release lane.

Public releases publish immutable GHCR image tags:

- `v<version>`
- `<version>`
- `sha-<shortsha>`

Examples:

- `ghcr.io/<owner>/domusmind:v1.0.1-beta.2`
- `ghcr.io/<owner>/domusmind:1.0.1-beta.2`
- `ghcr.io/<owner>/domusmind:sha-abc1234`

Mutable tags such as `latest` or `edge` are not used for releases.

---

## Release Pipeline

Canonical workflow: `.github/workflows/release.yml`

Trigger:

- manual `workflow_dispatch` from `main`

Inputs:

| Input | Description |
|---|---|
| `version` | SemVer string without leading `v` |
| `prerelease` | mark GitHub Release as prerelease |

Flow:

1. validate version
2. reject duplicate tag
3. run validation gates
4. create annotated tag `v<version>`
5. generate release metadata for the app
6. build and push immutable GHCR images
7. validate Compose packaging
8. publish GitHub Release with deploy assets

Release assets:

- `deploy/docker-compose.yml`
- `deploy/.env.example`
- `deploy/README.md`

---

## Runtime Version Traceability

Each release must be traceable across:

1. git tag
2. GitHub Release
3. immutable image tags
4. OCI image labels
5. in-app version metadata

Release builds generate app metadata such as:

```ts
export const APP_VERSION       = "v1.0.1-beta.2";
export const APP_RELEASE_DATE  = "2026-04-02";
export const APP_COMMIT_SHA    = "abc1234";
export const APP_IS_PRERELEASE = true;
````

This should be visible in:

* Settings → About
* backend runtime version endpoint
* startup logs

---

## Recommended Pipelines

### `backend-ci.yml`

Backend validation only:

* restore
* build
* test

### `webapp-ci.yml`

Web app validation only:

* install
* build
* optional lint/test

### `public-site-ci.yml`

Public site validation only.

### `public-site-cd.yml`

Public site deployment only.

### `mobile-ci.yml`

Mobile validation only.

### `release.yml`

Only public release publisher.

### Reusable validation workflows

Recommended next step:

* reusable backend validation workflow
* reusable web app validation workflow

These should be called by CI and release to remove duplication.

---

## Update Flow

Typical self-hosted update:

```bash
# 1. Read release notes
# 2. Update .env if required
# 3. Set VERSION to the target release
docker compose pull
docker compose up -d
```

Migrations run automatically at startup.

---

## Reverse Proxy

DomusMind is designed to sit behind a reverse proxy for HTTPS and external access.

Pattern:

```text
https://domusmind.example.com -> http://host:24365
```

Forwarded headers must be configured correctly.

---

## Backup

All persistent state lives in PostgreSQL.

Minimum backup example:

```bash
docker exec <postgres-container> \
  pg_dump -U $DB_USER domusmind \
  > ~/backups/domusmind_$(date +%Y%m%d_%H%M%S).sql
```

Restore example:

```bash
docker exec -i <postgres-container> \
  psql -U $DB_USER -d domusmind \
  < ~/backups/domusmind_YYYYMMDD_HHMMSS.sql
```

---

## Operational Rules

* exact versions only
* one public release path
* no public `edge`
* self-hosted runtime stays simple
* release notes required
* migration notes required when schema changes exist
* runtime version must be visible in the app
* CI validates; release publishes

---

## Current Focus

Current focus is V1 hardening:

* operational stability
* upgrade confidence
* coherent major surfaces
* packaging reliability
* bug fixing and UX refinement

---

## Summary

DomusMind DevOps optimizes for:

* predictable self-hosting
* exact release identity
* simple upgrades
* reproducible packaging
* low operational overhead

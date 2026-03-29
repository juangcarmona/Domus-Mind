# DomusMind — Manual Release Runbook (CloudHosted)

This document covers the manual release process for the CloudHosted deployment mode.
All steps are performed through GitHub Actions workflows and the Azure CLI.

---

## Release principles

- **Immutable tags only.** Every production deployment references an explicit semver tag (e.g. `1.0.0`). Mutable tags (`latest`, `edge`) are never used as deployment sources.
- **Explicit deploy target.** The deployment workflow requires a tag and an environment. Nothing is inferred from branch state.
- **Rollback by previous tag.** Redeploying a previous immutable tag restores the prior version deterministically.
- **Infra is separate from app release.** Bicep provisioning (`deploy/bicep/`) is a one-time or configuration-change operation. Normal app releases do not touch infra.

---

## Prerequisites

Before running a release, confirm:

| Item | Where |
|------|-------|
| Code merged to `main` | GitHub — check branch protection |
| GHCR write access | Granted via `GITHUB_TOKEN` (no extra secret required) |
| Azure credentials configured | GitHub repo secrets: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP` |
| CloudHosted infra already deployed | `deploy/bicep/main.bicep` — run once separately |
| App Service name known | Convention: `domusmind-prod` (or override via workflow input) |

The Azure credentials must be the OIDC federated identity for the service principal — no client secrets stored in GitHub. See [Azure login action docs](https://github.com/azure/login) for setup.

---

## Step 1 — Build and publish the release image

**Workflow:** `.github/workflows/release-image.yml`

1. Go to **Actions → release-image → Run workflow**.
2. Enter the release tag, e.g. `1.0.0`.
3. Run.

**What it does:**
- Validates the tag (must be semver; rejects `latest` and `edge`)
- Builds from `src/backend/DomusMind.Api/Dockerfile` with repo root as context
- Pushes two immutable tags to GHCR:
  - `ghcr.io/<owner>/domusmind:1.0.0`
  - `ghcr.io/<owner>/domusmind:sha-<short-sha>`
- Prints both references in the workflow summary

**Verify:**
- Workflow summary shows both tags
- `ghcr.io/<owner>/domusmind` package page shows the new tag

---

## Step 2 — Deploy the image to CloudHosted

**Workflow:** `.github/workflows/deploy-cloudhosted.yml`

1. Go to **Actions → deploy-cloudhosted → Run workflow**.
2. Fill in inputs:
   - **image_tag**: the immutable tag published in Step 1 (e.g. `1.0.0`)
   - **environment**: `prod` (default) or another named environment
   - **web_app_name**: leave blank to use the convention `domusmind-<environment>`, or override explicitly
3. Run.

**What it does:**
- Validates that the tag is not `latest` or `edge`
- Derives or accepts the App Service name
- Logs in to Azure via OIDC
- Updates the App Service container image to the specified tag
- Restarts the App Service
- Polls `GET /api/setup/status` until healthy (up to ~3 minutes)
- Fails with a clear error if the health check does not pass

**Verify:**
- Workflow step summary shows the environment, app name, and deployed image
- Health endpoint responds: `https://domusmind-prod.azurewebsites.net/api/setup/status`

---

## Rollback

To roll back to a previous release:

1. Go to **Actions → deploy-cloudhosted → Run workflow**.
2. Set **image_tag** to the previous immutable tag (e.g. `0.9.0`).
3. Run.

The workflow follows the same deploy + health check path. No special rollback mode is needed.

> Note: Rollback restores the application binary only. If a migration ran against the database, the schema change remains. Verify database compatibility before rolling back across a migration boundary.

---

## GitHub Actions secrets required

| Secret | Purpose |
|--------|---------|
| `AZURE_CLIENT_ID` | OIDC federated credential client ID |
| `AZURE_TENANT_ID` | Azure AD tenant |
| `AZURE_SUBSCRIPTION_ID` | Target subscription |
| `AZURE_RESOURCE_GROUP` | Resource group containing the App Service |

`GITHUB_TOKEN` is used automatically by the release-image workflow for GHCR push — no additional secret needed.

---

## Non-goals

- No `latest` tag in any production workflow
- No auto-promotion from `main` to production
- No staging slots or canary delivery (not yet)
- No infra provisioning in the app release workflow — use `deploy/bicep/` for that
- No database migration management in these workflows — migrations run automatically at app startup

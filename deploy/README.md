# DomusMind - Deployment

This folder contains the operator-facing deployment artifacts for DomusMind.

| File | Purpose |
|---|---|
| `docker-compose.yml` | Production stack definition |
| `.env.example` | Configuration contract template |
| `manual-release.md` | Manual release and rollback runbook (CloudHosted) |
| `bicep/main.bicep` | Azure infra baseline (CloudHosted) |

DomusMind is one product with two deployment modes: `SingleInstance` and `CloudHosted`. Both modes use the same image, the same schema, and the same API. Mode selection and policy are configuration-only.

See [`docs/07_platform/devops.md`](../docs/07_platform/devops.md) for the canonical DevOps reference.

---

## SingleInstance

For self-hosted installations: home servers, NAS devices, mini PCs.

- one household per installation — enforced by policy
- two services: `postgres` and `domusmind`
- HTTPS handled by an external reverse proxy
- state lives in the `postgres_data` Docker volume

### First install

```bash
# 1. Copy the template and fill in required values
cp .env.example .env

# Required: DB_PASSWORD, JWT_SECRET, VERSION, APP_PORT
# Set: DEPLOYMENT_MODE=SingleInstance

# 2. Start the stack
docker compose up -d

# 3. Open the web UI and complete the setup wizard
#    http://localhost:<APP_PORT>
```

### Update

```bash
# 1. Read the release notes — check for new .env variables or migration notes
# 2. Update VERSION in .env; add any new required variables
docker compose pull
docker compose up -d
# Migrations run automatically at startup.
```

---

## CloudHosted

For managed cloud deployments supporting multiple households.

Differences from SingleInstance:
- `DEPLOYMENT_MODE=CloudHosted`
- external managed database recommended (Azure Database for PostgreSQL, Amazon RDS, etc.)
- policy variables enabled: `ALLOW_HOUSEHOLD_CREATION`, `INVITATIONS_ENABLED`, `RATE_LIMITING_ENABLED`
- observability variables enabled: `METRICS_ENABLED`, `TRACING_ENABLED`

The same `docker-compose.yml` applies as a baseline. Cloud deployments may adapt it for their container platform (Kubernetes, Azure Container Apps, etc.) while using the same image.

```bash
# 1. Configure external database; update DB_USER, DB_PASSWORD, connection string
# 2. Set DEPLOYMENT_MODE=CloudHosted and policy/observability variables
# 3. Deploy via docker-compose.yml or container platform tooling
# 4. Complete setup via the API (UI wizard or headless bootstrap)
```

### CloudHosted on Azure (Bicep)

The `bicep/` folder contains the canonical Azure baseline for CloudHosted.

What it provisions: App Service (Linux B1), PostgreSQL Flexible Server, Key Vault, Application Insights, Log Analytics Workspace.

See [`docs/07_platform/cloud-hosted/deployment-flow.md`](../docs/07_platform/cloud-hosted/deployment-flow.md) for the full deployment policy.

#### Prerequisites

- Azure CLI installed and logged in (`az login`)
- A resource group already created (`az group create -n <rg> -l westeurope`)
- `dbAdminPassword` and `jwtSigningKey` values ready
  - generate: `openssl rand -hex 32`
  - minimum length for `jwtSigningKey`: 32 characters

#### Deploy

```bash
cd deploy/bicep

# Deploy with inline secure parameters (do not commit passwords to source control)
az deployment group create \
  --resource-group <rg-name> \
  --template-file main.bicep \
  --parameters main.parameters.json \
  --parameters dbAdminPassword='<strong-password>' jwtSigningKey='<32-char-key>'
```

To use a bootstrap Key Vault for secrets instead of inline values, populate the `reference` blocks in `main.parameters.json` with your bootstrap Key Vault details before deploying.

#### After provisioning

1. Run `az deployment group show` or check Azure portal outputs for the web app hostname and PostgreSQL FQDN.
2. Verify the web app is reachable: `curl https://<hostname>/api/setup/status`
3. Complete first-run setup via the UI wizard or: `POST https://<hostname>/api/setup/initialize`
4. Enable `alwaysOn: true` on the App Service if you want warm start (requires Standard tier for slot support).

#### Secrets management

| Secret | How it reaches the app |
|---|---|
| `db-password` | Stored in Key Vault; resolved via Key Vault reference in App Settings |
| `jwt-signing-key` | Stored in Key Vault; resolved via Key Vault reference in App Settings |
| Any future provider secrets | Add to Key Vault and reference in App Settings |

The web app's system-assigned managed identity is granted `Key Vault Secrets User` automatically by the Bicep deployment.

#### Update / redeploy

```bash
# Change appImage parameter to the new version tag and redeploy
az deployment group create \
  --resource-group <rg-name> \
  --template-file main.bicep \
  --parameters main.parameters.json \
  --parameters dbAdminPassword='<password>' jwtSigningKey='<key>' appImage='ghcr.io/juangcarmona/domusmind:1.1.0'
```

Migrations run automatically at startup. Back up the database before upgrading.

---

## Required Configuration

Copy `.env.example` to `.env` and fill in every required value.

| Variable | Required | Notes |
|---|---|---|
| `DB_USER` | Yes | PostgreSQL username |
| `DB_PASSWORD` | Yes | Strong random password |
| `JWT_SECRET` | Yes | Minimum 32 characters — `openssl rand -hex 32` |
| `JWT_ISSUER` | No | Defaults to `domusmind` |
| `VERSION` | Yes | Image tag — use a semver tag for production |
| `APP_PORT` | No | Defaults to `24365` |
| `DEPLOYMENT_MODE` | Yes | `SingleInstance` or `CloudHosted` |
| `ALLOW_HOUSEHOLD_CREATION` | No | Defaults to `true` |
| `MAX_HOUSEHOLDS_PER_DEPLOYMENT` | No | Defaults to `1` (SingleInstance) |
| `INVITATIONS_ENABLED` | No | Defaults to `false` |

Invalid combinations (e.g. `SingleInstance` with `MaxHouseholdsPerDeployment > 1`) fail at startup.

---

## Startup Order

1. `postgres` starts and passes its health check
2. `domusmind` starts after postgres is healthy
3. `domusmind` runs EF Core migrations at startup
4. API begins accepting traffic

No manual migration step is required.

---

## Migrations

- migrations run automatically at API startup (`dbContext.Database.Migrate()`)
- all schema changes in V1 are additive only — no drops, renames, or destructive changes
- every release with a migration documents it in the changelog
- back up the `postgres_data` volume before upgrading

---

## Bootstrap

**Default path:** open `http://localhost:<APP_PORT>` after first start. The setup wizard is served at the root URL and is available once.

**Headless path:** for scripted or CI environments without UI access. Disabled by default. Enable by uncommenting `BootstrapAdmin__` lines in `docker-compose.yml` and setting credentials in `.env`. Once the system is initialized this path becomes a permanent no-op.

---

## Backup

All state lives in the `postgres_data` volume.

```bash
docker exec domusmind-postgres-1 \
  pg_dump -U $DB_USER domusmind \
  > backups/domusmind_$(date +%Y%m%d_%H%M%S).sql
```

Back up before every upgrade.

---

## Reverse Proxy

DomusMind serves plain HTTP on `APP_PORT`. Terminate TLS upstream.

```
https://domusmind.example.com → http://<host>:<APP_PORT>
```

Compatible proxies: Caddy, Nginx Proxy Manager, Traefik, HAProxy.

---

## Troubleshooting

```bash
# View application logs
docker compose logs -f domusmind

# Check stack status
docker compose ps

# Validate config before starting
docker compose config
```

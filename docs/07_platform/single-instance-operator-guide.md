# SingleInstance - Operator Guide

## What SingleInstance means

One installation, one household.

DomusMind `SingleInstance` is designed for a single family running their own server. It uses the same application artifact, the same domain model, and the same API as `CloudHosted`. Deployment mode is a configuration choice, not a product fork.

---

## Installation requirements

- Docker Engine 24+ with the Compose plugin (`docker compose version`)
- A host machine with persistent storage (home server, NAS, mini PC, VM)
- A reverse proxy for HTTPS (Caddy, Nginx Proxy Manager, Traefik, HAProxy)

---

## Fresh install

```bash
# 1. Download docker-compose.yml and .env.example from the release
#    https://github.com/juangcarmona/domusmind/releases

# 2. Create your .env
cp .env.example .env

# 3. Fill in required values in .env:
#    DB_PASSWORD   — strong random password (openssl rand -hex 32)
#    JWT_SECRET    — minimum 32 characters (openssl rand -hex 32)
#    VERSION       — release tag, e.g. 1.0.0
#
#    DEPLOYMENT_MODE is already set to SingleInstance in .env.example

# 4. Start the stack
docker compose up -d

# 5. Open the web UI at http://localhost:<APP_PORT>
#    The setup wizard runs on first visit.
#    Create the initial administrator account through the UI.
```

The application runs EF Core migrations at startup. The database schema is created on first run.

---

## What happens on first run

1. `postgres` starts and passes its health check.
2. `domusmind` starts after postgres is healthy.
3. The app runs `dbContext.Database.Migrate()` — creates the schema.
4. `AuthSeedService` runs — no-op unless `BootstrapAdmin__Enabled = true` in compose.
5. `GET /api/setup/status` returns `{ isInitialized: false }`.
6. The operator opens the UI and completes the setup wizard.
7. `POST /api/setup/initialize` creates the admin account and marks the system as initialized.
8. `GET /api/setup/status` permanently returns `{ isInitialized: true }`.

---

## Restart behavior

Restarting the stack does not change any data.

```bash
docker compose restart
docker compose up -d  # after a host reboot
```

On restart:
- `dbContext.Database.Migrate()` runs again — safe, no-op if schema is current
- `AuthSeedService` runs again — no-op because the system is already initialized
- `POST /api/setup/initialize` returns `409 Conflict` if called again — no-op

Restarts are fully safe and idempotent.

---

## Upgrade

```bash
# 1. Read the release notes — check for new required .env variables or migration notes
# 2. Back up the database before applying a schema-changing update
# 3. Update VERSION in .env to the new release tag
docker compose pull
docker compose up -d
# Migrations run automatically at startup.
```

Upgrade rules:
- all V1 schema changes are additive only — no destructive changes without a MAJOR version bump
- MAJOR version upgrades may require a manual migration step — read the release notes
- if startup fails after upgrade, check `docker compose logs -f domusmind` for migration errors

---

## One-household enforcement

`SingleInstance` allows exactly one household.

- the first household creation succeeds
- any further household creation attempts return `409 Conflict` with body:
  ```json
  { "code": "household.creation_not_allowed", "reasonCode": "single_instance_already_bound", "error": "..." }
  ```
- this is enforced at two levels: a policy pre-check (read) and a DB unique constraint on the `singleton_key` column of the `families` table (write-level guarantee that closes the race between the check and the insert)
- no special configuration is required beyond `Deployment__Mode=SingleInstance`

This behavior is tested in `HouseholdProvisioningPolicyTests`.

---

## Persistent data

All state lives in the `postgres_data` Docker volume.

```bash
# Verify the volume exists and is in use
docker volume inspect domusmind_postgres_data

# Where Docker stores the volume on the host (Linux default)
/var/lib/docker/volumes/domusmind_postgres_data/_data
```

The `postgres_data` volume is the only storage that must survive. If the volume is lost, all household data is lost.

---

## Backup

Back up the `postgres_data` volume before every upgrade and on a regular schedule.

### Minimum backup (pg_dump)

```bash
docker exec domusmind-postgres-1 \
  pg_dump -U $DB_USER domusmind \
  > backups/domusmind_$(date +%Y%m%d_%H%M%S).sql
```

Store the backup file somewhere outside the Docker host — external drive, NAS share, cloud storage.

### Volume-level backup

```bash
docker run --rm \
  -v domusmind_postgres_data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/domusmind_volume_$(date +%Y%m%d).tar.gz -C /data .
```

### Automating backups

Use a cron job on the host:

```cron
0 3 * * * docker exec domusmind-postgres-1 pg_dump -U domusmind domusmind > /backups/domusmind_$(date +\%Y\%m\%d).sql
```

---

## Restore

### From pg_dump

```bash
# Stop the app (optional but recommended to prevent writes during restore)
docker compose stop domusmind

# Restore to the running postgres instance
docker exec -i domusmind-postgres-1 \
  psql -U $DB_USER -d domusmind \
  < backups/domusmind_20260321_120000.sql

# Restart the app
docker compose start domusmind
```

### From volume backup

```bash
docker compose down

docker run --rm \
  -v domusmind_postgres_data:/data \
  -v $(pwd)/backups:/backup \
  alpine sh -c "cd /data && tar xzf /backup/domusmind_volume_20260321.tar.gz"

docker compose up -d
```

### Restore risks

- restoring from a `pg_dump` may not restore exact sequence states — verify household data after restore
- restoring a volume backup overwrites all current data in the volume
- if a restore rolls back past a schema migration, the current app version may fail at startup; pin to the matching release tag or restore from a more recent backup

---

## Headless bootstrap (scripted / CI / recovery)

For installs where the UI setup wizard is not available:

```bash
# In docker-compose.yml, uncomment and set:
#   BootstrapAdmin__Enabled:  true
#   BootstrapAdmin__Email:    ${BOOTSTRAP_ADMIN_EMAIL}
#   BootstrapAdmin__Password: ${BOOTSTRAP_ADMIN_PASSWORD}

# In .env:
#   BOOTSTRAP_ADMIN_EMAIL=admin@example.com
#   BOOTSTRAP_ADMIN_PASSWORD=<strong-password>

docker compose up -d
# Admin account is created at startup.
# Disable the BootstrapAdmin block after the first run.
```

The bootstrap path is a permanent no-op once the system is initialized. It cannot override or re-initialize an already-initialized system.

---

## Troubleshooting

```bash
# View application logs
docker compose logs -f domusmind

# Check stack and health status
docker compose ps

# Verify database connectivity (from inside the stack network)
docker exec domusmind-postgres-1 pg_isready -U $DB_USER -d domusmind

# Check initialized state
curl http://localhost:<APP_PORT>/api/setup/status

# Check deployment mode
curl http://localhost:<APP_PORT>/api/platform/deployment-mode
```

---

## Non-goals for SingleInstance

- no horizontal scaling
- no deployment slot / zero-downtime swap
- no automated backup scheduling in the app
- no invitation system
- no multi-household support

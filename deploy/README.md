# DomusMind — Self-hosted Installation

DomusMind runs as a Docker Compose stack: **postgres** and **domusmind**.

## Prerequisites

- Docker Engine 24+ with the Compose plugin (`docker compose version`)
- A GitHub Container Registry token or access to the GHCR namespace where images are published

## Quick start

```bash
# 1. Download the release assets
#    (or unzip the release archive — docker-compose.yml and .env.example are included)

# 2. Create your .env file
cp .env.example .env

# 3. Edit .env and fill in at minimum:
#    IMAGE_OWNER  — GitHub username or org that published the release
#    DB_PASSWORD  — a strong random password for the database
#    JWT_SECRET   — at least 32 random characters (openssl rand -hex 32)
#    VERSION      — the release tag without the leading v, e.g. 1.0.0

# 4. (First run only) Enable the bootstrap admin in .env:
#    Uncomment BootstrapAdmin__ lines in docker-compose.yml,
#    set BOOTSTRAP_ADMIN_EMAIL and BOOTSTRAP_ADMIN_PASSWORD in .env.
#    The admin account is created on startup.
#    Disable this again after your first login.

# 5. Start the stack
docker compose up -d

# 6. Open the app
#    http://localhost:${APP_PORT}
```

## Image tags

| Tag | Source |
|-----|--------|
| `edge` | Latest successful `main` build |
| `1.0.0` (exact) | Stable release |
| `1.0` (minor) | Latest patch in the 1.0 line |
| `latest` | Latest stable release |
| `1.1.0-alpha.1` | Prerelease — not suitable for production |

## Updates

```bash
docker compose pull
docker compose up -d
```

Migrations run automatically on app startup. Back up your `postgres_data`
volume before upgrading.

## Volumes

| Volume | Contents |
|--------|----------|
| `postgres_data` | All application data |

Back up with:
```bash
docker run --rm \
  -v domusmind_postgres_data:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/postgres_data_$(date +%Y%m%d).tar.gz -C /data .
```

## Troubleshooting

```bash
# View logs
docker compose logs -f domusmind

# Check configuration before starting
docker compose config
```

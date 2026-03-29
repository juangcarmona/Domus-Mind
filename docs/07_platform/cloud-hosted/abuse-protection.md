# CloudHosted - Abuse Protection Baseline

## Status

Accepted — Closed-cohort baseline for first rollout

---

## Closed-Cohort Assumption

The first CloudHosted rollout operates as a closed cohort.

All valid accounts are known to the operator. There is no anonymous or self-provisioned access path.

This assumption is the primary abuse protection layer. It does not scale to public access.

---

## Auth Rate Limiting

Authentication endpoints must apply rate limiting to prevent credential stuffing and brute force attempts.

Minimum baseline:

| Endpoint | Limit |
|---|---|
| `POST /api/auth/login` | max 10 attempts per IP per minute |
| `POST /api/auth/register` | blocked in production (no open signup) |
| `POST /api/auth/refresh` | max 20 attempts per IP per minute |
| `POST /api/setup/initialize` | callable once; permanently no-op after initialization |

Rate limit violations return `429 Too Many Requests`.

Rate limiting is implemented in infrastructure, not in domain or slice logic.

---

## Invite-Only Posture

- no public signup endpoint is active in production
- `POST /api/auth/register` is either disabled or protected behind operator-only access
- invitation tokens are single-use, time-limited, and validated server-side
- unauthenticated access is limited to the setup, login, token refresh, and deployment-mode endpoints

---

## Audit Logging for Auth Events

The following events must be logged with sufficient context (user identifier, IP, timestamp, outcome):

- successful login
- failed login (including reason: bad credentials, account disabled)
- token refresh
- logout
- account creation
- account disable/suspension

Logs are routed to Application Insights. Operators can query for repeated failures or unusual access patterns.

---

## Account Suspension

The operator may disable an account immediately if abuse is suspected. Disabled accounts receive `401 Unauthorized` on all authenticated endpoints.

Suspended accounts are not automatically notified (email is not required for first rollout).

---

## What Is Deferred

The following protection measures are deferred until volume and risk justify them:

- automated suspicious-activity detection
- CAPTCHA or proof-of-work challenges
- geo-blocking
- device fingerprinting
- anomaly-based alerting
- IP allowlisting
- multi-factor authentication

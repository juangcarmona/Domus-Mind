# DomusMind — Security

## Purpose

This document defines the baseline security model for DomusMind V1.

Security in V1 focuses on:

- authentication
- authorization
- family data isolation
- secret handling
- secure defaults

This document reflects both the intended model and the currently implemented authentication baseline.

---

## Authentication

DomusMind V1 uses **local authentication implemented inside the modular monolith**.

Rules:

- authentication is part of the application boundary
- user identity is managed internally
- external identity providers are optional future integrations

Important distinction:

- `User` is an authentication concept
- `Member` is a family domain concept

These must remain separate.

Current implemented baseline:

- local user registration and login
- password hashing and verification
- JWT bearer access tokens
- refresh tokens persisted in the database
- authenticated user resolution from request context

```mermaid
sequenceDiagram
Client->>API: POST /api/auth/login
API->>AuthUserRepository: load user by email
API->>PasswordHasher: verify password
PasswordHasher-->>API: valid / invalid
API->>AccessTokenGenerator: generate JWT
API->>RefreshTokenStore: create refresh token
API-->>Client: accessToken + refreshToken
````

---

## Authorization

Authorization must be family-scoped.

Rules:

* a user may access only the families they belong to or administer
* commands and queries must enforce family isolation
* cross-family access is forbidden by default

Current state:

* authentication is implemented
* family-scoped authorization exists as an application/infrastructure seam
* full family membership enforcement is not yet complete across the whole product surface

V1 authorization can remain simple, but isolation must be strict.

---

## Session Model

DomusMind V1 uses token-based authentication.

Implemented mechanism:

* JWT bearer access tokens
* short-lived access tokens
* refresh tokens stored server-side
* refresh token rotation on renewal

Rules:

* tokens must contain user identity
* tokens must not embed domain state
* authorization checks must verify family membership

Authentication identity and domain identity remain separate:

User → authentication identity
Member → family domain identity

```mermaid
sequenceDiagram
Client->>API: GET /api/auth/me with Bearer token
API->>JWT Middleware: validate token
JWT Middleware-->>API: ClaimsPrincipal
API->>CurrentUserAccessor: resolve current user
CurrentUserAccessor-->>API: user identity
API-->>Client: authenticated response
```

---

## Authorization Model

Authorization is family-scoped.

Typical rules:

* a user may belong to multiple families
* commands must verify membership in the target family
* administrative operations require elevated privileges

Authorization enforcement occurs at:

* API boundary
* application slice handlers

Domain entities remain authorization-agnostic.

---

## Domain Boundary

Authentication and authorization must not pollute the core domain model.

Rules:

* domain entities do not depend on auth framework types
* access checks occur in API, application, or policy boundaries
* family membership in the domain is not the same as login identity

---

## Secret Management

V1 secret handling principles:

* secrets must never be committed to source control
* CI secrets are stored in GitHub Actions secrets
* local development secrets should use environment variables or local secret storage
* no cloud secret manager is required yet

Examples of secrets:

* database connection strings
* signing keys
* notification credentials
* external integration tokens

Current implemented baseline:

* JWT signing key is validated at startup
* invalid or weak signing key configuration fails fast

---

## Transport Security

Production deployments must use HTTPS.

Rules:

* no plaintext auth traffic
* secure cookies or bearer tokens only
* Swagger access may be restricted outside development

---

## Password and Credential Handling

Rules:

* passwords must be hashed using a modern password hashing algorithm
* plaintext passwords must never be stored or logged
* credential reset flows must be explicit and auditable

Current implemented baseline:

* password hashing is performed by infrastructure auth services
* bootstrap admin creation stores only hashed password values

---

## Bootstrap Identity

DomusMind uses a **two-path first-run model** backed by a server-enforced initialization state persisted in the `system_initialization` table. Once the system is initialized, all bootstrap paths become permanent no-ops.

---

### Primary path — UI-driven setup

The default path for all installations is the setup endpoint, which enables a guided first-run wizard.

```
GET  /api/setup/status       → { isInitialized: false }   (unauthenticated)
POST /api/setup/initialize   → 201 Created                 (unauthenticated, usable once)
GET  /api/setup/status       → { isInitialized: true }    (all subsequent calls)
```

Rules:

* the setup endpoint is permanently routeable but server-gated
* `POST /api/setup/initialize` returns `409 Conflict` once initialization is complete
* initialization is atomic: the admin user is created and the system is marked initialized in the same request
* no special configuration is required for this path

```mermaid
sequenceDiagram
Client->>API: GET /api/setup/status
API->>SystemInitializationRepository: IsInitializedAsync
SystemInitializationRepository-->>API: false
API-->>Client: { isInitialized: false }

Client->>API: POST /api/setup/initialize { email, password }
API->>InitializeSystemCommandHandler: Handle
InitializeSystemCommandHandler->>SystemInitializationRepository: IsInitializedAsync
SystemInitializationRepository-->>InitializeSystemCommandHandler: false
InitializeSystemCommandHandler->>PasswordHasher: hash password
InitializeSystemCommandHandler->>AuthUserRepository: AddAsync + SaveChangesAsync
InitializeSystemCommandHandler->>SystemInitializationRepository: MarkInitializedAsync
API-->>Client: 201 { userId, email }

Client->>API: POST /api/setup/initialize (again)
API-->>Client: 409 Conflict { code: "setup.already_initialized" }
```

---

### Fallback path — headless / recovery bootstrap

For headless deployments (containers, CI, scripted recovery), a configuration-driven fallback bootstrap is available.

Rules:

* **disabled by default** in production (`BootstrapAdmin:Enabled = false`)
* **no-op if the system is already initialized** — the flag has no effect on initialized systems
* must never log plaintext passwords
* intended only for recovery scenarios or environments where UI-driven setup is not possible
* disable in configuration once initialization is complete

Configuration:

```yaml
BootstrapAdmin:
  Enabled: true
  Email:   admin@example.com
  Password: <strong-password>
```

```mermaid
sequenceDiagram
AppStartup->>AuthSeedService: SeedAdminAsync
AuthSeedService->>SystemInitializationRepository: IsInitializedAsync
alt system already initialized
    AuthSeedService-->>AppStartup: skip (no-op)
else not initialized and Enabled = true
    AuthSeedService->>PasswordHasher: hash configured password
    AuthSeedService->>AuthUserRepository: AddAsync + SaveChangesAsync
    AuthSeedService->>SystemInitializationRepository: MarkInitializedAsync
end
```

---

## Auditability

Security-relevant actions should be traceable.

Examples:

* login
* failed login
* family creation
* ownership transfer
* permission-sensitive changes

Domain events and application logs together provide the baseline audit trail.

---

## Data Protection

Household data is sensitive.

Minimum expectations:

* family-level isolation
* least privilege by default
* secure persistence configuration
* controlled exposure in logs and telemetry

Sensitive fields should not be unnecessarily returned or logged.

---

## Future Considerations

Possible later additions:

* external identity providers
* MFA
* device/session management
* per-family roles
* encrypted secret stores
* fine-grained audit dashboards

These are not required for V1.

---

## Summary

DomusMind V1 uses local authentication, JWT bearer access tokens, persisted refresh tokens, strict family-level isolation as the target authorization model, internal secret handling discipline, and secure defaults.

The key architectural rule remains: authentication identity and household domain identity are separate concepts.
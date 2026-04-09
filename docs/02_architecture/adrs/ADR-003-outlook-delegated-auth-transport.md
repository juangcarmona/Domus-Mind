# ADR-003 - Outlook Delegated Auth Transport

## Status

Accepted

---

## Context

Phase 1 Outlook ingestion requires delegated access to a member's Microsoft account.

DomusMind must decide how the interactive OAuth completion flow crosses the web app and API boundary.

The decision must preserve these requirements:

- Microsoft Graph delegated access
- server-side storage of provider refresh material
- no provider access or refresh tokens in browser storage
- API-first modular-monolith boundary
- support for the existing Settings/Profile connect flow

The key transport question is:

> How does the browser hand interactive Outlook authorization back to the backend so the backend can own token exchange and long-lived sync state?

---

## Decision

DomusMind will use **authorization code handoff from the web client to the backend, with backend-owned code exchange and server-side token storage**.

Phase 1 transport shape:

- the user starts `Connect Outlook` from Settings/Profile
- the browser completes the interactive Microsoft sign-in and consent flow
- Microsoft redirects back to a DomusMind web callback route
- the web client posts the short-lived authorization code and redirect URI to the API
- the API exchanges the code for provider tokens
- the API stores provider refresh material server-side
- the browser never stores provider access tokens or refresh tokens

PKCE should be used in the interactive browser flow when supported by the chosen client registration.

---

## Decision Drivers

- keep long-lived provider credentials out of the browser
- preserve API ownership of integration state
- keep the web app implementation simple for phase 1
- avoid introducing a second callback-only backend endpoint flow unless needed later
- fit the existing API contract and Settings UX already defined for this feature

---

## Options Considered

### Option 1 - Web client hands authorization code to backend

Description:

- browser receives short-lived authorization code after provider redirect
- browser posts the code to the API
- API performs token exchange and persistence

Pros:

- keeps provider tokens server-side
- simple fit for SPA or web-app shell
- aligns with existing API-first contracts
- easiest phase 1 delivery path

Cons:

- authorization code passes through the browser callback route
- UI must preserve redirect URI and correlation state correctly

Assessment:

**Accepted for phase 1.**

### Option 2 - Backend-owned OAuth callback endpoint

Description:

- provider redirects directly to an API callback endpoint
- API exchanges the code and redirects back to the UI

Pros:

- reduces browser exposure to authorization-code payload handling
- centralizes connect-state handling in the backend

Cons:

- more moving parts in callback routing
- more complex local and deployed redirect handling
- not required for phase 1 product value

Assessment:

Viable future refinement, not required now.

### Option 3 - Browser-managed provider tokens

Description:

- browser obtains provider tokens directly
- browser calls Graph or forwards tokens to the API

Pros:

- rapid prototype path

Cons:

- conflicts with server-owned long-lived sync design
- increases token exposure risk
- weak fit for background refresh and catch-up sync

Assessment:

Rejected.

---

## Rationale

Phase 1 needs a pragmatic transport that locks in the important boundary:

- browser handles interactive consent
- backend handles durable integration credentials and sync

The accepted approach preserves that boundary without introducing extra transport complexity before it is needed.

DomusMind is not trying to make the browser an Outlook client.
It is collecting delegated consent so the backend can ingest calendar data safely over time.

---

## Consequences

### Positive

- provider refresh material remains server-side only
- Settings/Profile connect flow stays straightforward
- manual sync and background sync use the same stored credential model
- API contract stays explicit and easy to document

### Negative

- frontend callback handling must preserve state correctly
- short-lived authorization code appears in the browser callback flow before handoff
- a future native-mobile client may require a different transport wrapper around the same backend exchange

---

## Follow-Up Rules

- never persist provider access or refresh tokens in browser storage
- always exchange authorization code on the backend
- encrypt persisted refresh material at rest
- validate redirect URI and correlation state before code exchange
- reject incomplete scope grants such as missing `Calendars.Read` or `offline_access`

---

## Result

DomusMind phase 1 Outlook integration uses **browser-to-backend authorization-code handoff with backend-owned token exchange and server-side credential storage**.
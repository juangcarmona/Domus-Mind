# DomusMind - External Calendar API

Status: Phase 1 Contract
Audience: Engineering / Product / API Design
Scope: Outlook pull-based calendar ingestion and member agenda projection
Depends on:
  - docs/06_interfaces/api.md
  - docs/06_interfaces/external-calendar-contract-model-catalog.md
  - docs/04_contexts/calendar.md
  - docs/02_architecture/adrs/ADR-003-outlook-delegated-auth-transport.md
  - specs/features/calendar/connect-outlook-account.md
  - specs/features/calendar/configure-external-calendar-connection.md
  - specs/features/calendar/sync-external-calendar-connection.md
  - specs/features/calendar/disconnect-external-calendar-connection.md
  - specs/read-models/calendar/view-member-agenda.md

---

# Purpose

This document defines the HTTP contract for phase 1 external calendar ingestion.

It covers:

- member-scoped external calendar connection management
- Outlook delegated-connect completion
- per-connection configuration
- manual sync
- multi-connection sync orchestration
- member agenda read access to imported entries

This contract does not change native `Event` APIs.
Imported external entries remain a separate read-only model.

---

# Authorization and Scope

All endpoints in this document are authenticated.

Rules:

- the caller must be authorized for the target `familyId`
- the caller may manage only the permitted member's connection surface
- phase 1 assumes members manage their own provider credentials
- household managers may read connection summary only if a later authorization rule explicitly allows it

DomusMind remains the OAuth token holder.
The API stores provider auth metadata server-side.

---

# Resource Map

Primary resource:

- `ExternalCalendarConnection`

Supporting projections:

- `ExternalCalendarConnectionSummaryResponse`
- `ExternalCalendarConnectionDetailResponse`
- `MemberAgendaResponse`

Workflow endpoint:

- member-level `Sync calendars`

---

# Endpoint Catalog

## 1. List member connections

`GET /api/families/{familyId}/members/{memberId}/external-calendar-connections`

Purpose:

- populate Settings/Profile connection management UI
- show sync status summary per connection

Response: `200 OK`

```json
[
  {
    "connectionId": "01J...",
    "memberId": "01J...",
    "provider": "microsoft",
    "providerLabel": "Outlook",
    "accountEmail": "ana@example.com",
    "accountDisplayLabel": "Work Outlook",
    "selectedCalendarCount": 2,
    "forwardHorizonDays": 90,
    "scheduledRefreshEnabled": true,
    "scheduledRefreshIntervalMinutes": 60,
    "lastSuccessfulSyncUtc": "2026-04-07T09:12:00Z",
    "status": "healthy",
    "isSyncInProgress": false,
    "lastErrorCode": null,
    "lastErrorMessage": null
  }
]
```

Failure cases:

- `404 Not Found` when family or member does not exist
- `403 Forbidden` when the caller cannot view the target member's connections

## 2. Get connection detail

`GET /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}`

Purpose:

- show selected calendars
- show available provider calendars for editing
- show sync configuration and last result

Response: `200 OK`

```json
{
  "connectionId": "01J...",
  "memberId": "01J...",
  "provider": "microsoft",
  "providerLabel": "Outlook",
  "accountEmail": "ana@example.com",
  "accountDisplayLabel": "Work Outlook",
  "tenantId": "contoso-tenant",
  "forwardHorizonDays": 90,
  "scheduledRefreshEnabled": true,
  "scheduledRefreshIntervalMinutes": 60,
  "lastSuccessfulSyncUtc": "2026-04-07T09:12:00Z",
  "lastSyncAttemptUtc": "2026-04-07T09:12:00Z",
  "status": "healthy",
  "isSyncInProgress": false,
  "feeds": [
    {
      "calendarId": "AQMk...",
      "calendarName": "Calendar",
      "isSelected": true,
      "lastSuccessfulSyncUtc": "2026-04-07T09:12:00Z",
      "windowStartUtc": "2026-04-06T00:00:00Z",
      "windowEndUtc": "2026-07-06T23:59:59Z"
    }
  ],
  "availableCalendars": [
    {
      "calendarId": "AQMk...",
      "calendarName": "Calendar",
      "isDefault": true,
      "isSelected": true
    },
    {
      "calendarId": "AQMk...secondary",
      "calendarName": "School",
      "isDefault": false,
      "isSelected": false
    }
  ],
  "lastErrorCode": null,
  "lastErrorMessage": null
}
```

## 3. Connect Outlook account

`POST /api/families/{familyId}/members/{memberId}/external-calendar-connections/outlook`

Purpose:

- complete the Outlook delegated connect flow
- create one `ExternalCalendarConnection`

Request: `ConnectOutlookAccountRequest`

```json
{
  "authorizationCode": "provider-auth-code",
  "redirectUri": "https://app.domusmind.local/settings/profile/integrations/callback",
  "accountDisplayLabel": "Work Outlook"
}
```

Rules:

- the API exchanges the authorization code with Microsoft Graph
- the API validates that `Calendars.Read` and `offline_access` were granted
- the API stores provider auth metadata server-side
- the API discovers provider calendars
- the API may preselect the default calendar when none is specified yet

Response: `201 Created`

Headers:

- `Location: /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}`

Body: `ExternalCalendarConnectionDetailResponse`

Failure cases:

- `400 Bad Request` invalid authorization code or redirect URI
- `403 Forbidden` caller cannot manage this member's provider credentials
- `404 Not Found` family or member not found
- `409 Conflict` duplicate active connection for the same member and Outlook account

## 4. Configure connection

`PUT /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}`

Purpose:

- select provider calendars
- set sync horizon
- set scheduled refresh behavior

Request: `ConfigureExternalCalendarConnectionRequest`

```json
{
  "selectedCalendars": [
    {
      "calendarId": "AQMk...",
      "calendarName": "Calendar",
      "isSelected": true
    },
    {
      "calendarId": "AQMk...secondary",
      "calendarName": "School",
      "isSelected": true
    }
  ],
  "forwardHorizonDays": 90,
  "scheduledRefreshEnabled": true,
  "scheduledRefreshIntervalMinutes": 60
}
```

Response: `200 OK`

```json
{
  "connectionId": "01J...",
  "selectedCalendarCount": 2,
  "forwardHorizonDays": 90,
  "scheduledRefreshEnabled": true,
  "scheduledRefreshIntervalMinutes": 60,
  "status": "requires_rehydration"
}
```

Failure cases:

- `400 Bad Request` unsupported horizon or invalid interval
- `404 Not Found` connection not found
- `409 Conflict` configuration cannot be applied because a sync is already in progress

## 5. Sync one connection

`POST /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}/sync`

Purpose:

- run manual `Sync now` for one connection

Request: `SyncExternalCalendarConnectionRequest`

```json
{
  "reason": "manual"
}
```

Response: `200 OK`

```json
{
  "connectionId": "01J...",
  "selectedFeedCount": 2,
  "syncedFeedCount": 2,
  "importedEntryCount": 14,
  "updatedEntryCount": 3,
  "deletedEntryCount": 1,
  "status": "synchronized",
  "lastSuccessfulSyncUtc": "2026-04-07T10:14:00Z"
}
```

Failure cases:

- `404 Not Found` connection not found
- `409 Conflict` sync already in progress
- `409 Conflict` provider authorization no longer refreshable
- `400 Bad Request` invalid request body

## 6. Sync all connections for one member

`POST /api/families/{familyId}/members/{memberId}/external-calendar-connections/sync`

Purpose:

- implement the Settings/Profile `Sync calendars` action
- fan out one sync request per eligible connection

Request: `SyncMemberExternalCalendarConnectionsRequest`

```json
{
  "reason": "manual"
}
```

Response: `202 Accepted`

```json
{
  "memberId": "01J...",
  "requestedConnectionCount": 3,
  "acceptedConnectionCount": 3,
  "skippedConnectionCount": 0,
  "status": "accepted"
}
```

Rules:

- this is an orchestration endpoint, not a multi-aggregate command
- the implementation dispatches one sync operation per connection
- response does not imply all connections have finished syncing

## 7. Disconnect connection

`DELETE /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}`

Purpose:

- disconnect one provider account
- remove imported read-only entries owned by that connection

Response: `204 No Content`

Failure cases:

- `404 Not Found` connection not found
- `409 Conflict` connection is in a non-interruptible sync section

## 8. View Agenda in member scope

`GET /api/families/{familyId}/members/{memberId}/agenda?mode=day&windowStartUtc=2026-04-07T00:00:00Z&windowEndUtc=2026-04-07T23:59:59Z`

Purpose:

- return the member-scoped Agenda read model including imported external entries when relevant

Response: `200 OK`

```json
{
  "memberId": "01J...",
  "mode": "day",
  "windowStartUtc": "2026-04-07T00:00:00Z",
  "windowEndUtc": "2026-04-07T23:59:59Z",
  "items": [
    {
      "type": "event",
      "title": "Dentist",
      "startsAtUtc": "2026-04-07T09:00:00Z",
      "endsAtUtc": "2026-04-07T10:00:00Z",
      "allDay": false,
      "status": "scheduled",
      "isReadOnly": false,
      "eventId": "01J..."
    },
    {
      "type": "external-calendar-entry",
      "title": "Work stand-up",
      "startsAtUtc": "2026-04-07T11:00:00Z",
      "endsAtUtc": "2026-04-07T11:30:00Z",
      "allDay": false,
      "status": "confirmed",
      "isReadOnly": true,
      "connectionId": "01J...",
      "calendarId": "AQMk...",
      "externalEventId": "AAMk...",
      "provider": "microsoft",
      "providerLabel": "Outlook",
      "openInProviderUrl": "https://outlook.office.com/calendar/item/..."
    }
  ]
}
```

---

# API Models

Suggested request models:

- `ConnectOutlookAccountRequest`
- `ConfigureExternalCalendarConnectionRequest`
- `SyncExternalCalendarConnectionRequest`
- `SyncMemberExternalCalendarConnectionsRequest`

Suggested response models:

- `ExternalCalendarConnectionSummaryResponse`
- `ExternalCalendarConnectionDetailResponse`
- `ExternalCalendarFeedResponse`
- `AvailableExternalCalendarResponse`
- `SyncExternalCalendarConnectionResponse`
- `SyncMemberExternalCalendarConnectionsResponse`
- `MemberAgendaResponse`
- `MemberAgendaItem`

All models live under `Model.Calendar` or another explicit API-model namespace, not under `Domain`.

The exact file-by-file Contracts catalog is defined in `docs/06_interfaces/external-calendar-contract-model-catalog.md`.

---

# Error Codes

Recommended API error codes:

- `calendar.connection_not_found`
- `calendar.connection_duplicate`
- `calendar.connection_sync_in_progress`
- `calendar.connection_invalid_provider_scope`
- `calendar.connection_provider_auth_failed`
- `calendar.connection_provider_refresh_failed`
- `calendar.connection_invalid_horizon`
- `calendar.connection_invalid_refresh_interval`
- `calendar.connection_provider_calendar_not_found`
- `calendar.member_agenda_invalid_window`

---

# OpenAPI Requirements

Swagger / OpenAPI must document:

- bearer authentication requirement on all endpoints
- request and response models for each endpoint
- `201`, `202`, `204`, `400`, `403`, `404`, and `409` responses where applicable
- the read-only meaning of `external-calendar-entry` items in `MemberAgendaResponse`

The contract should make the separation explicit:

- native household events are writable through Event APIs
- imported Outlook entries are read-only and surfaced only through Agenda member-scope and connection queries
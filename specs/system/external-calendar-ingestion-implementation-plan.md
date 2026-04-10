Status: Draft Implementation Plan
Audience: Backend / Architecture / Product
Scope: Phase 1 Outlook pull-based ingestion in the Calendar module
Depends on:
  - docs/04_contexts/calendar.md
  - docs/06_interfaces/external-calendar-api.md
  - docs/07_platform/data-model.md
  - docs/07_platform/security.md
  - specs/features/calendar/connect-outlook-account.md
  - specs/features/calendar/configure-external-calendar-connection.md
  - specs/features/calendar/sync-external-calendar-connection.md
  - specs/features/calendar/refresh-external-calendar-feeds.md
  - specs/features/calendar/disconnect-external-calendar-connection.md

---

# Purpose

This document defines the backend implementation plan for phase 1 Outlook ingestion.

It covers:

- project placement across Domain / Application / Contracts / Infrastructure / API
- persistence model for connections, feeds, entries, and sync state
- background worker design
- sync orchestration and concurrency control
- recommended implementation sequence

---

# Architectural Placement

## DomusMind.Domain

Add the `ExternalCalendarConnection` aggregate and related value objects.

Domain-owned responsibilities:

- connection identity
- member identity reference
- provider identity
- selected feed configuration
- sync horizon setting
- scheduled refresh setting
- sync status transitions
- connection lifecycle events

Keep out of the domain:

- Microsoft Graph SDK types
- OAuth token exchange
- EF Core persistence models
- background worker timers
- raw provider payload JSON

## DomusMind.Application

Add vertical slices for:

- `connect-outlook-account`
- `configure-external-calendar-connection`
- `sync-external-calendar-connection`
- `disconnect-external-calendar-connection`
- queries for connection summary/detail and member agenda projection

Application-owned responsibilities:

- authorization checks
- aggregate loading and persistence
- orchestration of provider calls through interfaces
- mapping provider responses into integration records
- dispatch of integration-specific events

## DomusMind.Contracts

Add explicit request and response models for the HTTP contract defined in `docs/06_interfaces/external-calendar-api.md`.

## DomusMind.Infrastructure

Add:

- EF Core mappings for connection/feed/entry storage
- encrypted provider auth metadata storage
- Microsoft Graph client adapter
- background refresh worker
- connection-level sync lease acquisition

## DomusMind.Api

Add thin controllers/endpoints only.

The API layer:

- binds request models
- dispatches commands and queries
- returns explicit response models
- does not implement provider or scheduling logic

---

# Persistence Model

## Table Set

Recommended phase 1 tables:

### `external_calendar_connections`

Owns one connection per provider account.

Suggested columns:

- `Id`
- `FamilyId`
- `MemberId`
- `Provider`
- `ProviderAccountId`
- `AccountEmail`
- `AccountDisplayLabel`
- `TenantId`
- `ForwardHorizonDays`
- `ScheduledRefreshEnabled`
- `ScheduledRefreshIntervalMinutes`
- `Status`
- `LastSuccessfulSyncUtc`
- `LastSyncAttemptUtc`
- `LastSyncFailureUtc`
- `LastErrorCode`
- `LastErrorMessage`
- `NextScheduledSyncUtc`
- `SyncLeaseId`
- `SyncLeaseExpiresAtUtc`
- `Version`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### `external_calendar_feeds`

Owns selected provider calendars under one connection.

Suggested columns:

- `Id`
- `ConnectionId`
- `ProviderCalendarId`
- `CalendarName`
- `IsDefault`
- `IsSelected`
- `WindowStartUtc`
- `WindowEndUtc`
- `LastDeltaToken`
- `LastSuccessfulSyncUtc`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### `external_calendar_entries`

Stores imported occurrences for read-only projection.

Suggested columns:

- `Id`
- `ConnectionId`
- `FeedId`
- `Provider`
- `ExternalEventId`
- `ICalUId`
- `SeriesMasterId`
- `Title`
- `StartsAtUtc`
- `EndsAtUtc`
- `OriginalTimezone`
- `IsAllDay`
- `Location`
- `ParticipantSummaryJson`
- `Status`
- `RawPayloadHash`
- `ProviderModifiedAtUtc`
- `IsDeleted`
- `OpenInProviderUrl`
- `LastSeenAtUtc`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### `external_calendar_sync_runs` (recommended)

Optional but recommended for observability and diagnosis.

Suggested columns:

- `Id`
- `ConnectionId`
- `Reason`
- `StartedAtUtc`
- `CompletedAtUtc`
- `Outcome`
- `ImportedEntryCount`
- `UpdatedEntryCount`
- `DeletedEntryCount`
- `ErrorCode`
- `ErrorMessage`

## Key Constraints

- unique active connection by `(MemberId, Provider, ProviderAccountId)`
- unique feed by `(ConnectionId, ProviderCalendarId)`
- unique entry by `(FeedId, ExternalEventId)`

## Indexes

Recommended indexes:

- `external_calendar_connections(MemberId, Provider)`
- `external_calendar_connections(NextScheduledSyncUtc)`
- `external_calendar_connections(SyncLeaseExpiresAtUtc)`
- `external_calendar_feeds(ConnectionId, IsSelected)`
- `external_calendar_entries(ConnectionId, StartsAtUtc)`
- `external_calendar_entries(FeedId, StartsAtUtc, EndsAtUtc)`
- `external_calendar_entries(FeedId, ICalUId)`

## Persistence Rules

- `ExternalCalendarConnection` remains the aggregate root
- selected feed configuration persists with the connection boundary
- imported entries remain module-owned integration rows, not native `Event` rows
- deleting or disconnecting a connection removes or tombstones only integration rows owned by that connection

---

# Provider Auth Metadata Handling

Store provider auth metadata server-side only.

Recommended fields:

- refresh token or equivalent delegated refresh material
- access token expiry metadata if cached
- granted scope set
- provider account subject id

Security rules:

- encrypt refresh material at rest
- never expose tokens through API models
- log token lifecycle events without token values
- clear stored auth metadata on disconnect

The encrypted blob may live in `external_calendar_connections` for phase 1 if that keeps the model simple.
If encryption infrastructure prefers separation, use a connection-owned companion table.

---

# Sync Execution Model

## Initial sync

For each selected feed:

1. compute active window from now - 1 day to now + configured forward horizon
2. call Microsoft Graph `calendarView`
3. map returned occurrences into `external_calendar_entries`
4. continue until the terminal delta token is reached
5. persist the delta token on the feed row
6. update connection/feed sync timestamps and status

## Incremental sync

1. load the feed's stored delta token
2. call Microsoft Graph delta on the same window identity
3. apply additions, updates, and deletions
4. replace the stored token
5. update `LastSuccessfulSyncUtc` and `NextScheduledSyncUtc`

## Recovery path

If the provider token is invalid or the horizon changed:

1. clear entries for the affected feed and active window
2. clear the stored delta token
3. rerun initial bounded sync

---

# Concurrency and Lease Strategy

The system must prevent concurrent sync on the same connection.

Recommended strategy:

- acquire a database-backed lease on `external_calendar_connections`
- set `SyncLeaseId` and `SyncLeaseExpiresAtUtc` using an atomic conditional update
- release the lease on success or failure
- allow expired leases to be reclaimed safely

Why this approach:

- works in a single-node monolith now
- stays safe if the app scales to multiple nodes later
- avoids relying on in-memory locks only

---

# Background Worker Design

## Worker responsibility

Use one dedicated hosted background service for calendar refresh only.

The worker must:

- wake on a short internal cadence such as every 5 minutes
- query for connections where `ScheduledRefreshEnabled = true` and `NextScheduledSyncUtc <= now`
- process connections in small batches
- acquire a per-connection lease before sync
- invoke the same application-level sync path used by manual sync

## Scheduling model

- default refresh interval: 60 minutes
- per-connection configurable interval from settings
- set `NextScheduledSyncUtc` after each completed sync
- apply jitter of 1 to 5 minutes to avoid herd behavior

## Startup behavior

- worker starts automatically with app startup
- perform an initial scan after a short delay so startup remains predictable

## Failure handling

- one failed connection must not block the batch
- repeated failures update connection error state and remain visible in Settings
- the worker should continue scanning healthy connections

---

# Catch-up Trigger Plan

## User login

After successful local login:

- resolve the current user's member identities
- detect stale external calendar connections for those members
- enqueue non-blocking catch-up sync requests

Login itself should not wait for remote provider sync to complete.

## Agenda open (Member scope)

When `GetMemberAgenda` detects stale connections:

- return the best current read model
- mark stale state in the connection summary if exposed
- enqueue a non-blocking catch-up sync when no lease is active

This keeps the surface responsive while still honoring the user's expectation of a fresh view.

---

# Backend Slice Outline

## Query slices

- `get-member-external-calendar-connections`
- `get-external-calendar-connection-detail`
- `get-member-agenda`

## Command slices

- `connect-outlook-account`
- `configure-external-calendar-connection`
- `sync-external-calendar-connection`
- `disconnect-external-calendar-connection`

## Workflow entry points

- `sync-member-external-calendar-connections`
- `refresh-external-calendar-feeds`

The workflow entry points orchestrate per-connection commands.
They do not bypass aggregate boundaries.

---

# Recommended Delivery Sequence

1. Add persistence schema and EF mappings for connections, feeds, and entries.
2. Add the `ExternalCalendarConnection` aggregate and value objects.
3. Implement connection list/detail queries for Settings UI.
4. Implement `connect-outlook-account` with server-side token storage and provider calendar discovery.
5. Implement `configure-external-calendar-connection`.
6. Implement `sync-external-calendar-connection` with full bounded sync first, then delta refresh.
7. Implement `get-member-agenda` projection including read-only external entries.
8. Add the member-level `Sync calendars` orchestration endpoint.
9. Add the hosted background worker and lease-based stale refresh processing.
10. Add login and Agenda-open catch-up triggers.

---

# Test Plan

Minimum test coverage:

- aggregate tests for connection configuration and horizon-change rules
- handler tests for connect/configure/sync/disconnect
- provider adapter tests around initial sync, delta sync, and invalid token recovery
- query tests for member agenda projection rules
- background worker tests for stale selection, lease handling, and batch isolation
- API tests for authorization and contract shape

---

# Non-Goals in This Plan

- webhook subscriptions
- provider write-back
- attendee mutation
- unbounded history import
- conversion of external entries into native `Event` aggregates
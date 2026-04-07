# Spec - Sync External Calendar Connection

## Purpose

Synchronize one external calendar connection by importing or refreshing entries from each selected Outlook calendar feed.

This is the manual `Sync now` capability for one connection.

## Context

- Module: Calendar
- Aggregate: `ExternalCalendarConnection`
- Slice: `sync-external-calendar-connection`
- Command: `SyncExternalCalendarConnection`

## Inputs

Required:

- `connectionId`

Optional:

- `reason` where allowed values include `manual`, `catch_up`, `scheduled`
- `requestedByUserId`

## Preconditions

- connection must exist
- connection must be active
- at least one feed must be selected or the sync becomes a no-op success
- valid delegated access must be available or refreshable
- another sync for the same connection must not already be running
- command modifies one `ExternalCalendarConnection` aggregate boundary while updating its sync state

## Sync Behavior

For each selected feed, the system computes the active window from connection settings.

### First sync or rehydration

- call Microsoft Graph `calendarView` for the bounded window
- store returned occurrences as `ExternalCalendarEntry`
- continue following the provider cursor until the terminal delta token is reached
- persist the final delta token with the feed and active window identity

### Incremental sync

- use the stored delta token for the feed and current window identity
- apply additions, updates, and deletions
- replace the stored token with the newest token
- update `lastSuccessfulSyncUtc`

### Recovery path

If the delta token is invalid, the window changed, or local sync state is corrupted:

- clear local entries for the affected feed and active window
- discard the stored token
- perform a fresh bounded sync

## State Changes

On success, the system updates:

- connection sync status
- per-feed last successful sync time
- per-feed delta token
- imported external entries for the active window

On failure, the system updates:

- connection sync status
- last failure metadata

## Invariants

- imported entries remain read-only
- imported entries are keyed by provider identity, not by native `EventId`
- sync cursor identity includes connection, feed, and active window
- one connection sync must not run concurrently with itself

## Events

Emit:

- `ExternalCalendarConnectionSyncCompleted`
- `ExternalCalendarConnectionSyncFailed`

## Success Result

Return:

- `connectionId`
- `selectedFeedCount`
- `syncedFeedCount`
- `importedEntryCount`
- `updatedEntryCount`
- `deletedEntryCount`
- `status = synchronized`

## Failure Cases

- connection not found
- access token refresh failed
- provider API unavailable
- provider delta token invalid and recovery failed
- concurrency block because another sync is already running

## Notes

The UI-level `Sync calendars` action for a member with several accounts must fan out into one `SyncExternalCalendarConnection` request per connection.

Phase 1 does not use provider webhooks.
# Spec - Connect Outlook Account

## Purpose

Connect one Microsoft Outlook account to one DomusMind member for read-only calendar ingestion.

This capability establishes a delegated external calendar connection without creating native household plans.

## Context

- Module: Calendar
- Aggregate: `ExternalCalendarConnection`
- Slice: `connect-outlook-account`
- Command: `ConnectOutlookAccount`

## Inputs

Required:

- `connectionId`
- `memberId`
- `provider = microsoft`
- delegated authorization result from Microsoft Graph

Optional:

- `accountEmail`
- `accountDisplayLabel`
- `tenantId`
- `defaultSelectedCalendarId`

## Preconditions

- target member must exist
- delegated authorization must succeed
- granted scopes must include `Calendars.Read` and `offline_access`
- the same member must not already have an active connection to the same Outlook account
- command modifies one `ExternalCalendarConnection` aggregate boundary

## State Changes

On success, the system creates a new `ExternalCalendarConnection` with:

- stable connection identity
- member association
- provider account identity
- delegated auth metadata required for refresh
- default sync settings
- sync status = `pending_initial_sync`
- discovered provider calendars available for later selection

Phase 1 default sync settings:

- window start offset = now - 1 day
- window end offset = now + 90 days
- scheduled refresh enabled
- scheduled refresh interval = 60 minutes

If a default provider calendar is available and no explicit selection is supplied, the implementation may preselect the provider default calendar.

## Invariants

- one connection belongs to exactly one member
- one connection points to exactly one provider account
- connection provider is immutable after creation
- connection creation must not create native `Event` aggregates

## Events

Emit:

- `ExternalCalendarConnectionConnected`

## Success Result

Return:

- `connectionId`
- `memberId`
- `provider = microsoft`
- `accountEmail`
- `accountDisplayLabel`
- `status = pending_initial_sync`

## Failure Cases

- member not found
- authorization denied or failed
- required scopes missing
- duplicate active connection for the same member and Outlook account
- provider calendar discovery failed after connection creation attempt

## Notes

This capability establishes long-lived delegated access for pull-based sync.

It does not run the full sync itself.
Initial import occurs through `sync-external-calendar-connection`.
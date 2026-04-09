# Spec - Configure External Calendar Connection

## Purpose

Configure which provider calendars are included for one external calendar connection and which sync horizon applies.

This capability defines what is imported, not how imported entries behave in DomusMind.

## Context

- Module: Calendar
- Aggregate: `ExternalCalendarConnection`
- Slice: `configure-external-calendar-connection`
- Command: `ConfigureExternalCalendarConnection`

## Inputs

Required:

- `connectionId`
- `selectedCalendars[]`

Optional:

- `forwardHorizonDays` where allowed values are `30 | 90 | 180 | 365`
- `scheduledRefreshEnabled`
- `scheduledRefreshIntervalMinutes`

Each selected calendar item includes:

- `calendarId`
- `calendarName`
- `isSelected`

## Preconditions

- connection must exist
- connection must belong to the target member context
- selected provider calendars must belong to the connected Outlook account
- configured horizon must be one of the supported values
- scheduled refresh interval must not be less than the system minimum
- command modifies one `ExternalCalendarConnection` aggregate boundary

## State Changes

On success, the system updates:

- selected feed set
- sync horizon configuration
- scheduled refresh settings
- sync status when a full rehydration is required

If the selected calendars change:

- removed feeds stop projecting imported entries
- removed feeds clear stored delta state
- newly selected feeds require initial bounded load

If the horizon changes:

- existing delta cursors for affected feeds are discarded
- the next sync must perform a fresh bounded load for the new window
- previously stored entries outside the new window may be removed during rehydration

## Invariants

- feed identities are unique inside the connection
- unselected feeds do not ingest entries
- delta state is valid only for the active feed and horizon
- configuration changes must not create native `Event` aggregates

## Events

Emit:

- `ExternalCalendarConnectionConfigured`

## Success Result

Return:

- `connectionId`
- `selectedCalendarCount`
- `forwardHorizonDays`
- `scheduledRefreshEnabled`
- `scheduledRefreshIntervalMinutes`
- `status`

## Failure Cases

- connection not found
- provider calendars not found on the connected account
- unsupported horizon value
- invalid refresh interval
- stale or invalid provider authorization preventing validation

## Notes

Phase 1 default horizon is now - 1 day to now + 90 days.

Global household or member-level `Sync calendars` actions must compose repeated per-connection sync requests.
They must not bypass the one-aggregate boundary of this model.
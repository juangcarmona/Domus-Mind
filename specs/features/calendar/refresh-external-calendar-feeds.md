# Spec - Refresh External Calendar Feeds

## Purpose

Refresh stale external calendar connections in the background so Member Agenda stays current without requiring manual sync every time.

This capability is an application workflow backed by a dedicated background worker.

## Context

- Module: Calendar
- Workflow: `refresh-external-calendar-feeds`
- Trigger: hosted background worker and lightweight stale-state catch-up triggers
- Uses: `SyncExternalCalendarConnection`

## Triggers

Primary trigger:

- scheduled refresh every 60 minutes by default

Additional catch-up triggers:

- user login when the member's connection state is stale
- opening Agenda in Member scope when relevant connection state is stale
- application startup after the worker begins processing

## Preconditions

- background worker must be enabled
- candidate connections must be active
- candidate connections must have at least one selected feed
- candidate connections must not already be syncing

## Workflow Behavior

The workflow:

- finds connections whose last successful sync is older than the configured threshold
- processes connections in small batches
- applies a small jitter to avoid burst alignment at scale
- invokes `SyncExternalCalendarConnection` once per eligible connection
- records per-connection success or failure without blocking other connections

## State Changes

The workflow does not change native planning aggregates.

It updates:

- per-connection sync status
- per-feed sync timestamps
- imported external entries through per-connection sync execution

## Invariants

- a connection is never synced concurrently with itself
- stale detection respects each connection's configured refresh interval
- batch failure must not stop unrelated connections from refreshing
- background refresh must not create native `Event` aggregates

## Success Result

The workflow succeeds when:

- eligible stale connections are evaluated
- non-blocked connections are synchronized or marked with failure state
- Agenda catch-up triggers can safely skip already-fresh connections

## Failure Cases

- worker startup failure
- provider outage causing repeated sync failures
- connection auth refresh failure
- batch selection or persistence failure

## Notes

Phase 1 keeps this workflow simple.

It uses pull-based refresh only.
Webhook subscriptions are explicitly out of scope unless hourly polling proves insufficient.
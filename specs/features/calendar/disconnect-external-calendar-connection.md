# Spec - Disconnect External Calendar Connection

## Purpose

Disconnect one external calendar connection from DomusMind and remove its imported read-only agenda data.

## Context

- Module: Calendar
- Aggregate: `ExternalCalendarConnection`
- Slice: `disconnect-external-calendar-connection`
- Command: `DisconnectExternalCalendarConnection`

## Inputs

Required:

- `connectionId`

Optional:

- `requestedByUserId`

## Preconditions

- connection must exist
- requester must be allowed to manage the target member's connection
- connection must not be in a non-interruptible sync section
- command modifies one `ExternalCalendarConnection` aggregate boundary

## State Changes

On success, the system:

- marks the connection as disconnected or removes it according to persistence policy
- removes delegated auth metadata used for future refresh
- clears selected feed configuration
- removes or tombstones imported `ExternalCalendarEntry` records owned by the connection
- removes the connection from Member Agenda projection

## Invariants

- disconnecting a connection must not modify native `Event` aggregates
- imported entries owned by the connection must stop appearing in Agenda immediately after disconnect
- disconnect must leave no active delta cursor for the connection

## Events

Emit:

- `ExternalCalendarConnectionDisconnected`

## Success Result

Return:

- `connectionId`
- `status = disconnected`

## Failure Cases

- connection not found
- unauthorized requester
- connection is locked by an in-flight sync that cannot be safely interrupted
- persistence failure while clearing imported state

## Notes

Phase 1 disconnect removes DomusMind's local integration state.

It does not attempt provider-side calendar mutation or account-side cleanup beyond revoking or discarding locally held delegated access material where supported.
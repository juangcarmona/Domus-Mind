Status: Draft Backend Slice Map
Audience: Backend / Architecture / Product
Scope: File and folder plan for phase 1 Outlook ingestion implementation
Depends on:
  - specs/system/external-calendar-ingestion-implementation-plan.md
  - docs/06_interfaces/external-calendar-contract-model-catalog.md
  - docs/02_architecture/adrs/ADR-003-outlook-delegated-auth-transport.md

---

# Purpose

This document maps the phase 1 external calendar feature into concrete backend projects and folders.

It is intentionally file-oriented so implementation can start without inventing placement.

---

# Project Map

## `src/backend/DomusMind.Contracts`

Folder:

- `Calendar/`

New files:

- `Calendar/ConnectOutlookAccountRequest.cs`
- `Calendar/ConfigureExternalCalendarConnectionRequest.cs`
- `Calendar/ExternalCalendarSelectionRequest.cs`
- `Calendar/SyncExternalCalendarConnectionRequest.cs`
- `Calendar/SyncMemberExternalCalendarConnectionsRequest.cs`
- `Calendar/ExternalCalendarConnectionSummaryResponse.cs`
- `Calendar/ExternalCalendarConnectionDetailResponse.cs`
- `Calendar/ExternalCalendarFeedResponse.cs`
- `Calendar/AvailableExternalCalendarResponse.cs`
- `Calendar/ConfigureExternalCalendarConnectionResponse.cs`
- `Calendar/SyncExternalCalendarConnectionResponse.cs`
- `Calendar/SyncMemberExternalCalendarConnectionsResponse.cs`
- `Calendar/MemberAgendaResponse.cs`
- `Calendar/MemberAgendaItem.cs`

Why here:

- current contract organization already groups calendar-facing models under `Calendar/`
- phase 1 does not need a new top-level Contracts module

## `src/backend/DomusMind.Application`

Folder root:

- `Features/Calendar/`

New command slices:

- `Features/Calendar/ConnectOutlookAccount/ConnectOutlookAccountCommand.cs`
- `Features/Calendar/ConnectOutlookAccount/ConnectOutlookAccountCommandHandler.cs`
- `Features/Calendar/ConfigureExternalCalendarConnection/ConfigureExternalCalendarConnectionCommand.cs`
- `Features/Calendar/ConfigureExternalCalendarConnection/ConfigureExternalCalendarConnectionCommandHandler.cs`
- `Features/Calendar/SyncExternalCalendarConnection/SyncExternalCalendarConnectionCommand.cs`
- `Features/Calendar/SyncExternalCalendarConnection/SyncExternalCalendarConnectionCommandHandler.cs`
- `Features/Calendar/DisconnectExternalCalendarConnection/DisconnectExternalCalendarConnectionCommand.cs`
- `Features/Calendar/DisconnectExternalCalendarConnection/DisconnectExternalCalendarConnectionCommandHandler.cs`

New query slices:

- `Features/Calendar/GetMemberExternalCalendarConnections/GetMemberExternalCalendarConnectionsQuery.cs`
- `Features/Calendar/GetMemberExternalCalendarConnections/GetMemberExternalCalendarConnectionsQueryHandler.cs`
- `Features/Calendar/GetExternalCalendarConnectionDetail/GetExternalCalendarConnectionDetailQuery.cs`
- `Features/Calendar/GetExternalCalendarConnectionDetail/GetExternalCalendarConnectionDetailQueryHandler.cs`
- `Features/Calendar/GetMemberAgenda/GetMemberAgendaQuery.cs`
- `Features/Calendar/GetMemberAgenda/GetMemberAgendaQueryHandler.cs`

New orchestration slices:

- `Features/Calendar/SyncMemberExternalCalendarConnections/SyncMemberExternalCalendarConnectionsCommand.cs`
- `Features/Calendar/SyncMemberExternalCalendarConnections/SyncMemberExternalCalendarConnectionsCommandHandler.cs`

New application abstractions:

- `Abstractions/Integrations/Calendar/IExternalCalendarProviderClient.cs`
- `Abstractions/Integrations/Calendar/ExternalCalendarProviderAccount.cs`
- `Abstractions/Integrations/Calendar/ExternalCalendarProviderCalendar.cs`
- `Abstractions/Integrations/Calendar/ExternalCalendarProviderEvent.cs`
- `Abstractions/Integrations/Calendar/ExternalCalendarProviderDeltaPage.cs`
- `Abstractions/Integrations/Calendar/IExternalCalendarAuthService.cs`
- `Abstractions/Integrations/Calendar/IExternalCalendarSyncLeaseService.cs`

Supporting files:

- `Features/Calendar/CalendarException.cs` may absorb new external-calendar error codes if that remains the current module pattern

## `src/backend/DomusMind.Domain`

Recommended folder:

- `Calendar/ExternalConnections/`

New files:

- `Calendar/ExternalConnections/ExternalCalendarConnection.cs`
- `Calendar/ExternalConnections/ExternalCalendarFeed.cs`
- `Calendar/ExternalConnections/ExternalCalendarConnectionId.cs`
- `Calendar/ExternalConnections/ExternalCalendarProvider.cs`
- `Calendar/ExternalConnections/SyncHorizon.cs`
- `Calendar/ExternalConnections/ExternalCalendarConnectionStatus.cs`
- `Calendar/ExternalConnections/Events/ExternalCalendarConnectionConnected.cs`
- `Calendar/ExternalConnections/Events/ExternalCalendarConnectionConfigured.cs`
- `Calendar/ExternalConnections/Events/ExternalCalendarConnectionSyncCompleted.cs`
- `Calendar/ExternalConnections/Events/ExternalCalendarConnectionSyncFailed.cs`
- `Calendar/ExternalConnections/Events/ExternalCalendarConnectionDisconnected.cs`

Do not add `ExternalCalendarEntry` as a native household aggregate.
It remains integration storage and projection data.

## `src/backend/DomusMind.Infrastructure`

Recommended folders:

- `Persistence/Configurations/Calendar/`
- `Persistence/Entities/Calendar/`
- `Integrations/Calendar/Microsoft/`
- `BackgroundJobs/Calendar/`

New persistence files:

- `Persistence/Entities/Calendar/ExternalCalendarConnectionEntity.cs`
- `Persistence/Entities/Calendar/ExternalCalendarFeedEntity.cs`
- `Persistence/Entities/Calendar/ExternalCalendarEntryEntity.cs`
- `Persistence/Entities/Calendar/ExternalCalendarSyncRunEntity.cs`
- `Persistence/Configurations/Calendar/ExternalCalendarConnectionConfiguration.cs`
- `Persistence/Configurations/Calendar/ExternalCalendarFeedConfiguration.cs`
- `Persistence/Configurations/Calendar/ExternalCalendarEntryConfiguration.cs`
- `Persistence/Configurations/Calendar/ExternalCalendarSyncRunConfiguration.cs`

New integration files:

- `Integrations/Calendar/Microsoft/MicrosoftGraphCalendarClient.cs`
- `Integrations/Calendar/Microsoft/MicrosoftGraphCalendarAuthService.cs`
- `Integrations/Calendar/Microsoft/MicrosoftGraphCalendarMapper.cs`

New worker / infra files:

- `BackgroundJobs/Calendar/ExternalCalendarRefreshWorker.cs`
- `BackgroundJobs/Calendar/ExternalCalendarRefreshOptions.cs`
- `BackgroundJobs/Calendar/ExternalCalendarConnectionLeaseService.cs`

Optional infrastructure seam:

- `Security/ProtectedData/` or existing secret-protection area for encrypted provider refresh material

## `src/backend/DomusMind.Api`

Current controller layout is flat under `Controllers/`.

Recommended new controller files:

- `Controllers/ExternalCalendarConnectionsController.cs`
- `Controllers/MemberAgendaController.cs`

Controller ownership:

### `ExternalCalendarConnectionsController.cs`

Route root:

- `api/families/{familyId:guid}/members/{memberId:guid}/external-calendar-connections`

Endpoints:

- `GET /`
- `GET /{connectionId:guid}`
- `POST /outlook`
- `PUT /{connectionId:guid}`
- `POST /{connectionId:guid}/sync`
- `POST /sync`
- `DELETE /{connectionId:guid}`

### `MemberAgendaController.cs`

Route root:

- `api/families/{familyId:guid}/members/{memberId:guid}/agenda`

Endpoints:

- `GET /`

Why separate controllers:

- keeps `FamiliesController` from absorbing more unrelated profile/integration behavior
- keeps member-agenda read concerns separated from connection-management commands

## `tests/backend`

Recommended test project placement:

- `tests/backend/DomusMind.Application.Tests/Features/Calendar/...`
- `tests/backend/DomusMind.Infrastructure.Tests/Integrations/Calendar/...`
- `tests/backend/DomusMind.Api.Tests/Controllers/ExternalCalendarConnectionsControllerTests.cs`
- `tests/backend/DomusMind.Api.Tests/Controllers/MemberAgendaControllerTests.cs`

---

# Minimal Route-to-Slice Map

| Route | API Controller | Application Slice | Contracts |
|---|---|---|---|
| `GET /api/families/{familyId}/members/{memberId}/external-calendar-connections` | `ExternalCalendarConnectionsController` | `GetMemberExternalCalendarConnections` | `ExternalCalendarConnectionSummaryResponse` |
| `GET /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}` | `ExternalCalendarConnectionsController` | `GetExternalCalendarConnectionDetail` | `ExternalCalendarConnectionDetailResponse` |
| `POST /api/families/{familyId}/members/{memberId}/external-calendar-connections/outlook` | `ExternalCalendarConnectionsController` | `ConnectOutlookAccount` | `ConnectOutlookAccountRequest`, `ExternalCalendarConnectionDetailResponse` |
| `PUT /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}` | `ExternalCalendarConnectionsController` | `ConfigureExternalCalendarConnection` | `ConfigureExternalCalendarConnectionRequest`, `ConfigureExternalCalendarConnectionResponse` |
| `POST /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}/sync` | `ExternalCalendarConnectionsController` | `SyncExternalCalendarConnection` | `SyncExternalCalendarConnectionRequest`, `SyncExternalCalendarConnectionResponse` |
| `POST /api/families/{familyId}/members/{memberId}/external-calendar-connections/sync` | `ExternalCalendarConnectionsController` | `SyncMemberExternalCalendarConnections` | `SyncMemberExternalCalendarConnectionsRequest`, `SyncMemberExternalCalendarConnectionsResponse` |
| `DELETE /api/families/{familyId}/members/{memberId}/external-calendar-connections/{connectionId}` | `ExternalCalendarConnectionsController` | `DisconnectExternalCalendarConnection` | no body |
| `GET /api/families/{familyId}/members/{memberId}/agenda` | `MemberAgendaController` | `GetMemberAgenda` | `MemberAgendaResponse` |

---

# Implementation Guardrails

- keep controller files thin
- keep one handler per slice
- keep one aggregate mutation per command
- keep provider-specific code in Infrastructure only
- keep imported entries out of native `Event` persistence and controllers
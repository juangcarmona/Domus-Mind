# DomusMind - External Calendar Contracts Model Catalog

Status: Phase 1 Contract Catalog
Audience: Backend / API / OpenAPI
Scope: Exact request and response model shapes for phase 1 Outlook ingestion
Owns: Contract file names, namespaces, field lists, and model composition
Depends on:
  - docs/06_interfaces/api.md
  - docs/06_interfaces/external-calendar-api.md
  - docs/04_contexts/calendar.md

---

# Purpose

This document defines the exact Contracts catalog for phase 1 external calendar ingestion.

The goal is simple:

- one model name
- one target file
- one field list
- no invented shapes during backend implementation

This catalog follows the existing backend contract conventions in `src/backend/DomusMind.Contracts/Calendar/`.

---

# Target Namespace and Folder

All phase 1 external calendar contracts should live under:

- folder: `src/backend/DomusMind.Contracts/Calendar/`
- namespace: `DomusMind.Contracts.Calendar`

No new top-level Contracts module is required in phase 1.

---

# Request Models

## `ConnectOutlookAccountRequest`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ConnectOutlookAccountRequest.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ConnectOutlookAccountRequest(
    string AuthorizationCode,
    string RedirectUri,
    string? AccountDisplayLabel,
    string? ConnectState);
```

Field rules:

- `AuthorizationCode`: provider authorization code returned by Microsoft identity platform
- `RedirectUri`: exact redirect URI used in the interactive connect flow
- `AccountDisplayLabel`: optional user-defined label shown in Settings
- `ConnectState`: optional correlation value if the UI flow uses one

## `ConfigureExternalCalendarConnectionRequest`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ConfigureExternalCalendarConnectionRequest.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ConfigureExternalCalendarConnectionRequest(
    IReadOnlyCollection<ExternalCalendarSelectionRequest> SelectedCalendars,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes);
```

Field rules:

- `SelectedCalendars`: complete selected/unselected feed list from the edit form
- `ForwardHorizonDays`: allowed values `30`, `90`, `180`, `365`
- `ScheduledRefreshEnabled`: enables background refresh selection for the connection
- `ScheduledRefreshIntervalMinutes`: default `60` in phase 1

## `ExternalCalendarSelectionRequest`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ExternalCalendarSelectionRequest.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarSelectionRequest(
    string CalendarId,
    string CalendarName,
    bool IsSelected);
```

## `SyncExternalCalendarConnectionRequest`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/SyncExternalCalendarConnectionRequest.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record SyncExternalCalendarConnectionRequest(
    string Reason);
```

Field rules:

- `Reason`: allowed values `manual`, `catch_up`, `scheduled`

## `SyncMemberExternalCalendarConnectionsRequest`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/SyncMemberExternalCalendarConnectionsRequest.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record SyncMemberExternalCalendarConnectionsRequest(
    string Reason);
```

---

# Response Models

## `ExternalCalendarConnectionSummaryResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ExternalCalendarConnectionSummaryResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarConnectionSummaryResponse(
    Guid ConnectionId,
    Guid MemberId,
    string Provider,
    string ProviderLabel,
    string AccountEmail,
    string? AccountDisplayLabel,
    int SelectedCalendarCount,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    DateTime? LastSuccessfulSyncUtc,
    string Status,
    bool IsSyncInProgress,
    string? LastErrorCode,
    string? LastErrorMessage);
```

## `ExternalCalendarConnectionDetailResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ExternalCalendarConnectionDetailResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarConnectionDetailResponse(
    Guid ConnectionId,
    Guid MemberId,
    string Provider,
    string ProviderLabel,
    string AccountEmail,
    string? AccountDisplayLabel,
    string? TenantId,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    DateTime? LastSuccessfulSyncUtc,
    DateTime? LastSyncAttemptUtc,
    string Status,
    bool IsSyncInProgress,
    IReadOnlyCollection<ExternalCalendarFeedResponse> Feeds,
    IReadOnlyCollection<AvailableExternalCalendarResponse> AvailableCalendars,
    string? LastErrorCode,
    string? LastErrorMessage);
```

## `ExternalCalendarFeedResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ExternalCalendarFeedResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarFeedResponse(
    string CalendarId,
    string CalendarName,
    bool IsSelected,
    DateTime? LastSuccessfulSyncUtc,
    DateTime? WindowStartUtc,
    DateTime? WindowEndUtc);
```

## `AvailableExternalCalendarResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/AvailableExternalCalendarResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record AvailableExternalCalendarResponse(
    string CalendarId,
    string CalendarName,
    bool IsDefault,
    bool IsSelected);
```

## `ConfigureExternalCalendarConnectionResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/ConfigureExternalCalendarConnectionResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record ConfigureExternalCalendarConnectionResponse(
    Guid ConnectionId,
    int SelectedCalendarCount,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    string Status);
```

## `SyncExternalCalendarConnectionResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/SyncExternalCalendarConnectionResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record SyncExternalCalendarConnectionResponse(
    Guid ConnectionId,
    int SelectedFeedCount,
    int SyncedFeedCount,
    int ImportedEntryCount,
    int UpdatedEntryCount,
    int DeletedEntryCount,
    string Status,
    DateTime? LastSuccessfulSyncUtc);
```

## `SyncMemberExternalCalendarConnectionsResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/SyncMemberExternalCalendarConnectionsResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record SyncMemberExternalCalendarConnectionsResponse(
    Guid MemberId,
    int RequestedConnectionCount,
    int AcceptedConnectionCount,
    int SkippedConnectionCount,
    string Status);
```

## `MemberAgendaResponse`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/MemberAgendaResponse.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record MemberAgendaResponse(
    Guid MemberId,
    string Mode,
    DateTime WindowStartUtc,
    DateTime WindowEndUtc,
    IReadOnlyCollection<MemberAgendaItem> Items);
```

## `MemberAgendaItem`

Target file:

- `src/backend/DomusMind.Contracts/Calendar/MemberAgendaItem.cs`

Recommended shape:

```csharp
namespace DomusMind.Contracts.Calendar;

public sealed record MemberAgendaItem(
    string Type,
    string Title,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    bool AllDay,
    string Status,
    bool IsReadOnly,
    Guid? EventId,
    Guid? TaskId,
    Guid? RoutineId,
    Guid? ConnectionId,
    string? CalendarId,
    string? ExternalEventId,
    string? Provider,
    string? ProviderLabel,
    string? OpenInProviderUrl,
    string? Location,
    string? ParticipantSummary,
    DateTime? SourceLastModifiedUtc);
```

Field rules:

- native items populate `EventId`, `TaskId`, or `RoutineId`
- imported items populate `ConnectionId`, `CalendarId`, `ExternalEventId`, and provider fields
- `IsReadOnly = true` for imported external entries only in phase 1

---

# Optional Auxiliary Response

## `DisconnectExternalCalendarConnectionResponse`

Phase 1 should prefer `204 No Content`.

Do not add a response model unless implementation later needs body content.

---

# Enumerated Value Expectations

The Contracts layer may keep these as `string` in phase 1 for compatibility with the current API style.

Expected string sets:

- `Provider`: `microsoft`
- `ProviderLabel`: `Outlook`
- `Reason`: `manual | catch_up | scheduled`
- `Status` for connections: `healthy | syncing | needs_attention | auth_expired | rehydrating | disconnected`
- `Type` for agenda items: `event | task | routine | external-calendar-entry`

If the backend later introduces typed enums in contracts, the API documentation must remain backward-compatible.

---

# OpenAPI Mapping Rule

These exact model names should appear in Swagger / OpenAPI.

The API layer should not invent anonymous transport shapes for these endpoints.
Use explicit Contracts types throughout controller action signatures and response annotations.
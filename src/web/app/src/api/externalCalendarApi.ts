import { request } from "./request";

// --- response types ---

export interface ExternalCalendarFeedSummary {
  calendarId: string;
  calendarName: string;
  isSelected: boolean;
  lastSuccessfulSyncUtc: string | null;
  windowStartUtc: string | null;
  windowEndUtc: string | null;
}

export interface ExternalCalendarConnectionSummary {
  connectionId: string;
  memberId: string;
  provider: string;
  providerLabel: string;
  accountEmail: string;
  accountDisplayLabel: string | null;
  selectedCalendarCount: number;
  forwardHorizonDays: number;
  scheduledRefreshEnabled: boolean;
  scheduledRefreshIntervalMinutes: number;
  lastSuccessfulSyncUtc: string | null;
  lastSyncAttemptUtc: string | null;
  lastSyncFailureUtc: string | null;
  status: string;
  isSyncInProgress: boolean;
  importedEntryCount: number;
  lastErrorCode: string | null;
  lastErrorMessage: string | null;
}

export interface AvailableCalendar {
  calendarId: string;
  calendarName: string;
  isDefault: boolean;
  isSelected: boolean;
}

export interface ExternalCalendarConnectionDetail extends ExternalCalendarConnectionSummary {
  tenantId: string | null;
  feeds: ExternalCalendarFeedSummary[];
  availableCalendars: AvailableCalendar[];
}

export interface ConnectOutlookAccountRequest {
  authorizationCode: string;
  redirectUri: string;
  accountDisplayLabel?: string;
  connectState?: string;
}

export interface ConnectOutlookAccountResponse {
  connectionId: string;
  memberId: string;
  provider: string;
  providerAccountId: string;
  accountEmail: string;
}

export interface CalendarSelectionItem {
  calendarId: string;
  calendarName: string;
  isSelected: boolean;
}

export interface ConfigureConnectionRequest {
  selectedCalendars: CalendarSelectionItem[];
  forwardHorizonDays: number;
  scheduledRefreshEnabled: boolean;
  scheduledRefreshIntervalMinutes: number;
}

export interface ConfigureConnectionResponse {
  connectionId: string;
  selectedCalendarCount: number;
  forwardHorizonDays: number;
  scheduledRefreshEnabled: boolean;
  scheduledRefreshIntervalMinutes: number;
  status: string;
}

export interface SyncConnectionResponse {
  connectionId: string;
  status: string;
  syncedFeedCount: number;
  syncedEntryCount: number;
  syncCompletedAtUtc: string | null;
}

export interface GetExternalCalendarEntryResponse {
  entryId: string;
  title: string;
  date: string;
  time: string | null;
  endDate: string | null;
  endTime: string | null;
  isAllDay: boolean;
  status: string;
  location: string | null;
  calendarName: string | null;
  providerLabel: string | null;
  openInProviderUrl: string | null;
}

// --- API functions ---

const baseUrl = (familyId: string, memberId: string) =>
  `/api/families/${familyId}/members/${memberId}/external-calendar-connections`;

export const externalCalendarApi = {
  getOutlookAuthUrl: (familyId: string, memberId: string, redirectUri: string) =>
    request<{ authUrl: string }>(
      `${baseUrl(familyId, memberId)}/outlook/auth-url?redirectUri=${encodeURIComponent(redirectUri)}`,
    ),

  listConnections: (familyId: string, memberId: string) =>
    request<ExternalCalendarConnectionSummary[]>(baseUrl(familyId, memberId)),

  getConnectionDetail: (familyId: string, memberId: string, connectionId: string) =>
    request<ExternalCalendarConnectionDetail>(`${baseUrl(familyId, memberId)}/${connectionId}`),

  connectOutlook: (familyId: string, memberId: string, body: ConnectOutlookAccountRequest) =>
    request<ConnectOutlookAccountResponse>(`${baseUrl(familyId, memberId)}/outlook`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  configureConnection: (
    familyId: string,
    memberId: string,
    connectionId: string,
    body: ConfigureConnectionRequest,
  ) =>
    request<ConfigureConnectionResponse>(`${baseUrl(familyId, memberId)}/${connectionId}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  syncConnection: (familyId: string, memberId: string, connectionId: string) =>
    request<SyncConnectionResponse>(`${baseUrl(familyId, memberId)}/${connectionId}/sync`, {
      method: "POST",
      body: JSON.stringify({ reason: "manual" }),
    }),

  syncAllConnections: (familyId: string, memberId: string) =>
    request<unknown>(`${baseUrl(familyId, memberId)}/sync`, {
      method: "POST",
      body: JSON.stringify({ reason: "manual" }),
    }),

  disconnectConnection: (familyId: string, memberId: string, connectionId: string) =>
    request<unknown>(`${baseUrl(familyId, memberId)}/${connectionId}`, { method: "DELETE" }),

  getExternalEntry: (familyId: string, memberId: string, entryId: string) =>
    request<GetExternalCalendarEntryResponse>(
      `/api/families/${familyId}/members/${memberId}/external-calendar-entries/${entryId}`,
    ),
};

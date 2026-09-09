import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchConnections,
  fetchConnectionDetail,
  configureConnection,
  syncConnection,
  disconnectConnection,
  clearDetail,
} from "../../../store/externalCalendarSlice";
import type { ExternalCalendarConnectionDetail } from "../../../api/externalCalendarApi";
import { ConnectionStatusBadge } from "./ConnectionStatusBadge";
import { ConnectOutlookPanel } from "./ConnectOutlookPanel";
import { CalendarFeedSelector } from "./CalendarFeedSelector";

export function ConnectedCalendarsSection() {
  const { t } = useTranslation("settings");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();

  const family = useAppSelector((s) => s.household.family);
  const members = useAppSelector((s) => s.household.members);
  const connections = useAppSelector((s) => s.externalCalendar.connections);
  const detail = useAppSelector((s) => s.externalCalendar.detail);
  const loadStatus = useAppSelector((s) => s.externalCalendar.status);

  // Find the member that corresponds to the currently authenticated user.
  const myMember = members.find((m) => m.isCurrentUser);

  const [showConnectPanel, setShowConnectPanel] = useState(false);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [disconnecting, setDisconnecting] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [configuring, setConfiguring] = useState(false);

  useEffect(() => {
    if (family && myMember) {
      dispatch(fetchConnections({ familyId: family.familyId, memberId: myMember.memberId }));
    }
  }, [dispatch, family, myMember]);

  useEffect(() => {
    if (expandedId && family && myMember) {
      dispatch(
        fetchConnectionDetail({
          familyId: family.familyId,
          memberId: myMember.memberId,
          connectionId: expandedId,
        }),
      );
    } else {
      dispatch(clearDetail());
    }
  }, [dispatch, expandedId, family, myMember]);

  if (!family || !myMember) return null;

  async function handleSync(connectionId: string) {
    if (!family || !myMember) return;
    setError(null);
    try {
      await dispatch(
        syncConnection({ familyId: family.familyId, memberId: myMember.memberId, connectionId }),
      ).unwrap();
      await dispatch(fetchConnections({ familyId: family.familyId, memberId: myMember.memberId }));
      if (expandedId === connectionId) {
        await dispatch(
          fetchConnectionDetail({
            familyId: family.familyId,
            memberId: myMember.memberId,
            connectionId,
          }),
        );
      }
    } catch {
      setError(t("connectedCalendars.syncError"));
    }
  }

  async function handleDisconnect(connectionId: string) {
    if (!family || !myMember) return;
    if (!window.confirm(t("connectedCalendars.disconnectConfirm"))) return;
    setDisconnecting(connectionId);
    setError(null);
    try {
      await dispatch(
        disconnectConnection({
          familyId: family.familyId,
          memberId: myMember.memberId,
          connectionId,
        }),
      ).unwrap();
      if (expandedId === connectionId) setExpandedId(null);
    } catch {
      setError(t("connectedCalendars.disconnectError"));
    } finally {
      setDisconnecting(null);
    }
  }

  async function handleSaveFeedSelection(
    connectionId: string,
    detail: ExternalCalendarConnectionDetail,
    selectedIds: Set<string>,
    forwardHorizonDays: number,
  ) {
    if (!family || !myMember) return;
    setConfiguring(true);
    setError(null);
    try {
      await dispatch(
        configureConnection({
          familyId: family.familyId,
          memberId: myMember.memberId,
          connectionId,
          body: {
            selectedCalendars: detail.availableCalendars.map((c) => ({
              calendarId: c.calendarId,
              calendarName: c.calendarName,
              isSelected: selectedIds.has(c.calendarId),
            })),
            forwardHorizonDays,
            scheduledRefreshEnabled: detail.scheduledRefreshEnabled,
            scheduledRefreshIntervalMinutes: detail.scheduledRefreshIntervalMinutes,
          },
        }),
      ).unwrap();
      dispatch(
        fetchConnectionDetail({
          familyId: family.familyId,
          memberId: myMember.memberId,
          connectionId,
        }),
      );
      dispatch(fetchConnections({ familyId: family.familyId, memberId: myMember.memberId }));
    } catch {
      setError(t("connectedCalendars.configureError"));
    } finally {
      setConfiguring(false);
    }
  }

  function formatDate(iso: string | null) {
    if (!iso) return "—";
    return new Date(iso).toLocaleString();
  }

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">{t("connectedCalendars.title")}</h2>

      <p className="settings-description">
        {t("connectedCalendars.description")}
      </p>

      {error && <p className="form-error">{error}</p>}

      {loadStatus === "loading" && connections.length === 0 && (
        <p className="settings-loading">{tCommon("loading")}</p>
      )}

      {connections.length > 0 && (
        <div className="calendar-connections-list">
          {connections.map((conn) => {
            const isExpanded = expandedId === conn.connectionId;
            const isSyncing = conn.isSyncInProgress || conn.status === "syncing";
            const isDisconnecting = disconnecting === conn.connectionId;

            return (
              <div key={conn.connectionId} className="calendar-connection-card">
                <div
                  className="calendar-connection-header"
                  role="button"
                  tabIndex={0}
                  onClick={() => setExpandedId(isExpanded ? null : conn.connectionId)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      setExpandedId(isExpanded ? null : conn.connectionId);
                    }
                  }}
                >
                  <div className="calendar-connection-info">
                    <span className="calendar-connection-provider-icon">📅</span>
                    <div>
                      <span className="calendar-connection-label">
                        {conn.accountDisplayLabel ?? conn.accountEmail}
                      </span>
                      {conn.accountDisplayLabel && (
                        <span className="calendar-connection-email">{conn.accountEmail}</span>
                      )}
                    </div>
                  </div>
                  <div className="calendar-connection-meta">
                    <ConnectionStatusBadge status={conn.status} />
                    <span className="calendar-connection-feed-count">
                      {conn.selectedCalendarCount} {t("connectedCalendars.calendarsSelected")}
                    </span>
                    <span className="calendar-connection-chevron">{isExpanded ? "▲" : "▼"}</span>
                  </div>
                </div>

                {isExpanded && (
                  <div className="calendar-connection-body">
                    <div className="calendar-connection-dates">
                      <span className="settings-field-label">{t("connectedCalendars.lastSyncAttempt", "Last sync attempt")}</span>
                      <span className="settings-field-value">{formatDate(conn.lastSyncAttemptUtc)}</span>
                    </div>
                    <div className="calendar-connection-dates">
                      <span className="settings-field-label">{t("connectedCalendars.lastSync")}</span>
                      <span className="settings-field-value">{formatDate(conn.lastSuccessfulSyncUtc)}</span>
                    </div>
                    <div className="calendar-connection-dates">
                      <span className="settings-field-label">{t("connectedCalendars.lastFailure", "Last failure")}</span>
                      <span className="settings-field-value">{formatDate(conn.lastSyncFailureUtc)}</span>
                    </div>

                    {conn.lastErrorMessage && (
                      <p className="settings-description">{conn.lastErrorMessage}</p>
                    )}

                    {conn.importedEntryCount === 0 && (
                      <p className="settings-empty">
                        {t("connectedCalendars.noImportedEventsYet", "No imported events yet")}
                      </p>
                    )}

                    {detail?.connectionId === conn.connectionId && (
                      <CalendarFeedSelector
                        detail={detail}
                        saving={configuring}
                        onSave={(selectedIds, horizonDays) =>
                          handleSaveFeedSelection(conn.connectionId, detail, selectedIds, horizonDays)
                        }
                      />
                    )}

                    <div className="calendar-connection-actions">
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() => handleSync(conn.connectionId)}
                        disabled={isSyncing}
                        type="button"
                      >
                        {isSyncing ? t("connectedCalendars.syncing") : t("connectedCalendars.syncNow")}
                      </button>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => handleDisconnect(conn.connectionId)}
                        disabled={isDisconnecting}
                        type="button"
                      >
                        {isDisconnecting
                          ? t("connectedCalendars.disconnecting")
                          : t("connectedCalendars.disconnect")}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {connections.length === 0 && loadStatus !== "loading" && (
        <p className="settings-empty">{t("connectedCalendars.noConnections")}</p>
      )}

      {!showConnectPanel && (
        <button
          className="btn btn-primary"
          style={{ marginTop: "1rem" }}
          onClick={() => setShowConnectPanel(true)}
          type="button"
        >
          {t("connectedCalendars.connectOutlook")}
        </button>
      )}

      {showConnectPanel && (
        <ConnectOutlookPanel
          familyId={family.familyId}
          memberId={myMember.memberId}
          onConnected={() => {
            setShowConnectPanel(false);
            dispatch(fetchConnections({ familyId: family.familyId, memberId: myMember.memberId }));
          }}
          onCancel={() => setShowConnectPanel(false)}
        />
      )}
    </section>
  );
}

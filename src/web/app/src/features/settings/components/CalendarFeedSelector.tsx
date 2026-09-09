import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { ExternalCalendarConnectionDetail } from "../../../api/externalCalendarApi";

const HORIZON_OPTIONS = [30, 90, 180, 365] as const;

interface Props {
  detail: ExternalCalendarConnectionDetail;
  saving: boolean;
  onSave: (selectedIds: Set<string>, horizonDays: number) => void;
}

export function CalendarFeedSelector({ detail, saving, onSave }: Props) {
  const { t } = useTranslation("settings");

  const [selected, setSelected] = useState<Set<string>>(
    () => new Set(detail.feeds.filter((f) => f.isSelected).map((f) => f.calendarId)),
  );
  const [horizonDays, setHorizonDays] = useState(detail.forwardHorizonDays);
  const [dirty, setDirty] = useState(false);

  // Re-sync selection state whenever detail is refreshed after a save or initial load.
  useEffect(() => {
    setSelected(new Set(detail.feeds.filter((f) => f.isSelected).map((f) => f.calendarId)));
    setHorizonDays(detail.forwardHorizonDays);
    setDirty(false);
  }, [detail]);

  const calendars = detail.availableCalendars.length > 0
    ? detail.availableCalendars
    : detail.feeds.map((f) => ({
        calendarId: f.calendarId,
        calendarName: f.calendarName,
        isDefault: false,
        isSelected: f.isSelected,
      }));

  function toggle(calendarId: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(calendarId)) next.delete(calendarId);
      else next.add(calendarId);
      return next;
    });
    setDirty(true);
  }

  function handleHorizonChange(days: number) {
    setHorizonDays(days);
    setDirty(true);
  }

  return (
    <div className="calendar-feed-selector">
      <h4 className="calendar-feed-selector-title">{t("connectedCalendars.selectCalendars")}</h4>

      {calendars.length === 0 ? (
        <p className="settings-empty">{t("connectedCalendars.noCalendarsAvailable")}</p>
      ) : (
        <ul className="calendar-feed-list">
          {calendars.map((cal) => (
            <li key={cal.calendarId} className="calendar-feed-item">
              <label className="calendar-feed-label">
                <input
                  type="checkbox"
                  className="calendar-feed-checkbox"
                  checked={selected.has(cal.calendarId)}
                  onChange={() => toggle(cal.calendarId)}
                />
                <span className="calendar-feed-name">
                  {cal.calendarName}
                  {cal.isDefault && (
                    <span className="calendar-feed-default-badge">
                      {t("connectedCalendars.default")}
                    </span>
                  )}
                </span>
              </label>
            </li>
          ))}
        </ul>
      )}

      <div className="calendar-horizon-selector">
        <label className="settings-field-label">{t("connectedCalendars.forwardHorizon")}</label>
        <div className="calendar-horizon-options">
          {HORIZON_OPTIONS.map((days) => (
            <button
              key={days}
              type="button"
              className={`calendar-horizon-btn${horizonDays === days ? " active" : ""}`}
              onClick={() => handleHorizonChange(days)}
            >
              {days} {t("connectedCalendars.days")}
            </button>
          ))}
        </div>
      </div>

      {dirty && (
        <div className="calendar-feed-actions">
          <button
            type="button"
            className="btn btn-primary btn-sm"
            disabled={saving}
            onClick={() => {
              onSave(selected, horizonDays);
              setDirty(false);
            }}
          >
            {saving ? t("connectedCalendars.saving") : t("connectedCalendars.saveSelection")}
          </button>
        </div>
      )}
    </div>
  );
}

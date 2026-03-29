import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { toIsoDate } from "../../today/utils/dateUtils";

type AgendaView = "day" | "week" | "month";

interface AgendaHeaderProps {
  memberName: string;
  selectedDate: string; // ISO YYYY-MM-DD
  view: AgendaView;
  onViewChange: (view: AgendaView) => void;
  onPrev: () => void;
  onNext: () => void;
  onToday: () => void;
}

/**
 * Compact, responsive header for the member agenda surface.
 *
 * Three distinct rows (never collapse on narrow widths):
 *   Row 1 — back button + member name
 *   Row 2 — ‹  date  today?  ›
 *   Row 3 — Day / Week / Month tab strip
 */
export function AgendaHeader({
  memberName,
  selectedDate,
  view,
  onViewChange,
  onPrev,
  onNext,
  onToday,
}: AgendaHeaderProps) {
  const { t, i18n } = useTranslation("agenda");
  const navigate = useNavigate();

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  const dateLabel = new Date(selectedDate + "T00:00:00").toLocaleDateString(
    i18n.language,
    { weekday: "short", day: "numeric", month: "short", year: "numeric" },
  );

  return (
    <div className="agenda-header">
      {/* Row 1: back + member name */}
      <div className="agenda-identity">
        <button
          className="btn btn-ghost btn-sm agenda-back-btn"
          onClick={() => navigate(-1)}
          type="button"
          aria-label={t("nav.back")}
        >
          ‹ {t("nav.back")}
        </button>
        <span className="agenda-member-name">{memberName}</span>
      </div>

      {/* Row 2: date navigation */}
      <div className="agenda-date-nav">
        <button
          className="btn btn-ghost btn-sm agenda-nav-btn"
          onClick={onPrev}
          type="button"
          aria-label={t(`nav.prev${view.charAt(0).toUpperCase() + view.slice(1)}`)}
        >
          ‹
        </button>
        <div className="agenda-date-center">
          <span className="agenda-date-text">{dateLabel}</span>
          {!isToday && (
            <button
              className="btn btn-ghost btn-sm agenda-today-btn"
              onClick={onToday}
              type="button"
            >
              {t("nav.today")}
            </button>
          )}
        </div>
        <button
          className="btn btn-ghost btn-sm agenda-nav-btn"
          onClick={onNext}
          type="button"
          aria-label={t(`nav.next${view.charAt(0).toUpperCase() + view.slice(1)}`)}
        >
          ›
        </button>
      </div>

      {/* Row 3: view tabs */}
      <div className="agenda-view-tabs" role="tablist">
        {(["day", "week", "month"] as AgendaView[]).map((v) => (
          <button
            key={v}
            role="tab"
            aria-selected={view === v}
            className={`agenda-view-tab${view === v ? " agenda-view-tab--active" : ""}`}
            onClick={() => onViewChange(v)}
            type="button"
          >
            {t(`views.${v}`)}
          </button>
        ))}
      </div>
    </div>
  );
}

// Re-export type for consumers
export type { AgendaView };

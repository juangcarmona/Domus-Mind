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
 * Header for the member agenda surface.
 * Shows identity, date navigation, and view switcher (Day / Week / Month).
 * Visual flavor matches Today/Week surfaces.
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
    { weekday: "long", day: "numeric", month: "long", year: "numeric" },
  );

  return (
    <div className="agenda-header">
      <div className="agenda-header-top">
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

      <div className="agenda-header-nav">
        <button
          className="btn btn-ghost btn-sm agenda-nav-btn"
          onClick={onPrev}
          type="button"
          aria-label={t(`nav.prev${view.charAt(0).toUpperCase() + view.slice(1)}`)}
        >
          ‹
        </button>

        <div className="agenda-date-label">
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

      <div className="agenda-view-tabs">
        {(["day", "week", "month"] as AgendaView[]).map((v) => (
          <button
            key={v}
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

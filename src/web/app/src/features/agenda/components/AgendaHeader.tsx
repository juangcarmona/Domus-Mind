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
 * Row 1: back ‹ · member name
 * Row 2: ‹ · date (centered) · today? · ›
 * Row 3: day / week / month underline tabs
 *
 * On wide screens rows 1 and 2 collapse into one flex row.
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
      {/* ---- Row 1+2: identity + date navigation ---- */}
      <div className="agenda-header-main">
        {/* Back + member name */}
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

        {/* Date nav: prev · date · today · next */}
        <div className="agenda-date-nav">
          <button
            className="btn btn-ghost btn-sm agenda-nav-btn"
            onClick={onPrev}
            type="button"
            aria-label={t(`nav.prev${view.charAt(0).toUpperCase() + view.slice(1)}`)}
          >
            ‹
          </button>
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
          <button
            className="btn btn-ghost btn-sm agenda-nav-btn"
            onClick={onNext}
            type="button"
            aria-label={t(`nav.next${view.charAt(0).toUpperCase() + view.slice(1)}`)}
          >
            ›
          </button>
        </div>
      </div>

      {/* ---- Row 3: view tabs ---- */}
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

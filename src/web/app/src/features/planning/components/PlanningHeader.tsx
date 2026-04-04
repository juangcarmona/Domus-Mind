import { useTranslation } from "react-i18next";
import { weekRangeFor } from "../../agenda/utils/agendaDateGrid";
import { toIsoDate } from "../../today/utils/dateUtils";

export type PlanningView = "week" | "day" | "month";

interface PlanningHeaderProps {
  selectedDate: string; // ISO YYYY-MM-DD
  view: PlanningView;
  firstDayOfWeek: string | null;
  onViewChange: (view: PlanningView) => void;
  onPrev: () => void;
  onNext: () => void;
  onToday: () => void;
}

/**
 * Compact, responsive header for the Planning surface.
 *
 * Three stable rows:
 *   Row 1 — "Planning" title + primary "Add plan" action
 *   Row 2 — ‹  date range  today?  ›
 *   Row 3 — Week / Day / Month view tab strip
 */
export function PlanningHeader({
  selectedDate,
  view,
  firstDayOfWeek,
  onViewChange,
  onPrev,
  onNext,
  onToday,
}: PlanningHeaderProps) {
  const { t, i18n } = useTranslation("agenda");
  const { t: tNav } = useTranslation("nav");

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  function getDateRangeLabel(): string {
    const locale = i18n.language;
    if (view === "week") {
      const { weekStart, weekEnd } = weekRangeFor(selectedDate, firstDayOfWeek);
      const start = new Date(weekStart + "T00:00:00").toLocaleDateString(locale, {
        day: "numeric",
        month: "short",
      });
      const end = new Date(weekEnd + "T00:00:00").toLocaleDateString(locale, {
        day: "numeric",
        month: "short",
        year: "numeric",
      });
      return `${start} – ${end}`;
    }
    if (view === "day") {
      return new Date(selectedDate + "T00:00:00").toLocaleDateString(locale, {
        weekday: "short",
        day: "numeric",
        month: "short",
        year: "numeric",
      });
    }
    // month
    return new Date(selectedDate + "T00:00:00").toLocaleDateString(locale, {
      month: "long",
      year: "numeric",
    });
  }

  function getPrevAriaLabel(): string {
    if (view === "week") return t("nav.prevWeek");
    if (view === "day") return t("nav.prevDay");
    return t("nav.prevMonth");
  }

  function getNextAriaLabel(): string {
    if (view === "week") return t("nav.nextWeek");
    if (view === "day") return t("nav.nextDay");
    return t("nav.nextMonth");
  }

  return (
    <header className="planning-header">
      {/* Row 1: title */}
      <div className="planning-identity">
        <span className="planning-title">{tNav("planning")}</span>
      </div>

      {/* Row 2: date navigation */}
      <div className="planning-date-nav">
        <button
          type="button"
          className="btn btn-ghost btn-sm planning-nav-btn"
          onClick={onPrev}
          aria-label={getPrevAriaLabel()}
        >
          ‹
        </button>
        <div className="planning-date-center">
          <span className="planning-date-text">{getDateRangeLabel()}</span>
          {!isToday && (
            <button
              type="button"
              className="btn btn-ghost btn-sm planning-today-btn"
              onClick={onToday}
            >
              {t("nav.today")}
            </button>
          )}
        </div>
        <button
          type="button"
          className="btn btn-ghost btn-sm planning-nav-btn"
          onClick={onNext}
          aria-label={getNextAriaLabel()}
        >
          ›
        </button>
      </div>

      {/* Row 3: view tabs */}
      <div className="planning-view-tabs" role="tablist" aria-label="Planning view">
        {(["week", "day", "month"] as PlanningView[]).map((v) => (
          <button
            key={v}
            type="button"
            role="tab"
            aria-selected={view === v}
            className={`planning-view-tab${view === v ? " planning-view-tab--active" : ""}`}
            onClick={() => onViewChange(v)}
          >
            {t(`views.${v}`)}
          </button>
        ))}
      </div>
    </header>
  );
}

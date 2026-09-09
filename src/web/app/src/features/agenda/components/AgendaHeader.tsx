import { useTranslation } from "react-i18next";
import { weekRangeFor } from "../utils/agendaDateGrid";
import { toIsoDate } from "../../agenda-today/utils/dateUtils";
import { MemberAvatar } from "../../settings/components/avatar/MemberAvatar";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

export type AgendaView = "day" | "week" | "month";

export interface AgendaHeaderMember {
  memberId: string;
  name: string;
  avatarInitial?: string;
  avatarIconId?: number | null;
  avatarColorId?: number | null;
}

interface AgendaHeaderProps {
  /**
   * "household" for the household scope, or a memberId string for member scope.
   */
  scope: "household" | string;
  members: AgendaHeaderMember[];
  householdLabel: string;
  selectedDate: string; // ISO YYYY-MM-DD
  view: AgendaView;
  firstDayOfWeek: string | null;
  onScopeChange: (scope: "household" | string) => void;
  onViewChange: (view: AgendaView) => void;
  onPrev: () => void;
  onNext: () => void;
  onToday: () => void;
}

/**
 * Unified compact header for the Agenda surface.
 *
 * Three rows:
 *   Row 1 — Household | member scope selector pills
 *   Row 2 — ‹  date range  today?  ›
 *   Row 3 — Day / Week / Month tab strip
 */
export function AgendaHeader({
  scope,
  members,
  householdLabel,
  selectedDate,
  view,
  firstDayOfWeek,
  onScopeChange,
  onViewChange,
  onPrev,
  onNext,
  onToday,
}: AgendaHeaderProps) {
  const { t, i18n } = useTranslation("agenda");

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
    <header className="agenda-header">
      {/* Row 1: scope selector */}
      <div className="agenda-scope-row" role="group" aria-label="Scope">
        <button
          type="button"
          className={`agenda-scope-pill agenda-scope-pill--icon-only${scope === "household" ? " agenda-scope-pill--active" : ""}`}
          aria-pressed={scope === "household"}
          aria-label={householdLabel}
          title={householdLabel}
          onClick={() => onScopeChange("household")}
        >
          <HouseholdLogo className="agenda-scope-pill-icon" />
        </button>
        {members.map((m) => (
          <button
            key={m.memberId}
            type="button"
            className={`agenda-scope-pill${scope === m.memberId ? " agenda-scope-pill--active" : ""}`}
            aria-pressed={scope === m.memberId}
            onClick={() => onScopeChange(m.memberId)}
          >
            <MemberAvatar
              initial={m.avatarInitial ?? m.name[0]?.toUpperCase() ?? "?"}
              avatarIconId={m.avatarIconId}
              avatarColorId={m.avatarColorId}
              size={18}
            />
            <span>{m.name}</span>
          </button>
        ))}
      </div>

      {/* Row 2: date navigation */}
      <div className="agenda-date-nav">
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={onPrev}
          aria-label={getPrevAriaLabel()}
        >
          ‹
        </button>
        <div className="agenda-date-center">
          <span className="agenda-date-text">{getDateRangeLabel()}</span>
          {!isToday && (
            <button
              type="button"
              className="btn btn-ghost btn-sm agenda-today-btn"
              onClick={onToday}
            >
              {t("nav.today")}
            </button>
          )}
        </div>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={onNext}
          aria-label={getNextAriaLabel()}
        >
          ›
        </button>
      </div>

      {/* Row 3: view tabs */}
      <div className="agenda-view-tabs" role="tablist" aria-label="View">
        {(["day", "week", "month"] as AgendaView[]).map((v) => (
          <button
            key={v}
            type="button"
            role="tab"
            aria-selected={view === v}
            className={`agenda-view-tab${view === v ? " agenda-view-tab--active" : ""}`}
            onClick={() => onViewChange(v)}
          >
            {t(`views.${v}`)}
          </button>
        ))}
      </div>
    </header>
  );
}

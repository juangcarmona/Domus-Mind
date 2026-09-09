import { useTranslation } from "react-i18next";
import type { WeeklyGridCell, WeeklyGridMember } from "../../agenda-today/types";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { buildMemberEntries, sortEntries } from "../../agenda-today/utils/todayPanelHelpers";
import { toIsoDate } from "../../agenda-today/utils/dateUtils";
import { MemberSelectedDaySummary } from "./MemberSelectedDaySummary";
import { MemberSelectedDayUntimedSection } from "./MemberSelectedDayUntimedSection";
import { MemberSelectedDayTimedSection } from "./MemberSelectedDayTimedSection";

interface MemberWeekViewProps {
  member: WeeklyGridMember;
  sharedCells?: WeeklyGridCell[];
  /** ISO YYYY-MM-DD — the currently selected day (must be within the loaded week). */
  selectedDate: string;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
  /** Called when the user taps a day in the strip — stays in week view. */
  onDaySelect?: (date: string) => void;
  /** Called when the user explicitly drills into a day (tapping the detail header). */
  onDayClick?: (date: string) => void;
  /** Called when an empty timeline slot is clicked. */
  onSlotClick?: (time: string) => void;
}

/**
 * Week-level member agenda view.
 *
 * Top row: compact 7-day strip. Tapping a day selects it.
 * Bottom panel: the selected day's entries in a scannable list.
 *
 * This replaces the old flat "all 7 days stacked" layout with a more
 * focused strip-then-detail model matching the household week view grammar.
 */
export function MemberWeekView({
  member,
  sharedCells,
  selectedDate,
  onItemClick,
  onDaySelect,
  onDayClick,
  onSlotClick,
}: MemberWeekViewProps) {
  const { t, i18n } = useTranslation("agenda");
  const today = toIsoDate(new Date());

  const days = member.cells.map((cell) => cell.date.slice(0, 10));

  if (days.length === 0) {
    return (
      <div className="mweek-view">
        <span className="mday-empty">{t("week.empty")}</span>
      </div>
    );
  }

  // Per-day entry counts for load dots in the strip.
  const countByDay: Record<string, number> = {};
  for (const day of days) {
    const sharedCell = sharedCells?.find((c) => c.date.slice(0, 10) === day) ?? null;
    countByDay[day] = buildMemberEntries(member, day, sharedCell).length;
  }

  const activeDate = days.includes(selectedDate) ? selectedDate : days[0];
  const sharedCellForActiveDate = sharedCells?.find((c) => c.date.slice(0, 10) === activeDate) ?? null;
  const allEntries: CalendarEntry[] = sortEntries(buildMemberEntries(member, activeDate, sharedCellForActiveDate));
  const untimedEntries = allEntries.filter((e) => e.time === null);
  const timedEntries   = allEntries.filter((e) => e.time !== null);

  const activeDateLabel = new Date(activeDate + "T00:00:00").toLocaleDateString(i18n.language, {
    weekday: "short",
    day: "numeric",
    month: "short",
  });

  return (
    <div className="mweek-view">
      {/* ── Day strip ── */}
      <div className="mweek-strip" role="group" aria-label={t("week.dayStrip", "Week days")}>
        {days.map((day) => {
          const d = new Date(day + "T00:00:00");
          const isSelected = day === activeDate;
          const isToday = day === today;
          const count = countByDay[day] ?? 0;
          const dayName = d.toLocaleDateString(i18n.language, { weekday: "narrow" });
          const dayNum = d.getDate();
          const loadTier = count === 0 ? null : count <= 2 ? "low" : count <= 5 ? "medium" : "high";

          return (
            <button
              key={day}
              type="button"
              className={[
                "mweek-strip-day",
                isSelected ? "mweek-strip-day--selected" : "",
                isToday ? "mweek-strip-day--today" : "",
              ].filter(Boolean).join(" ")}
              aria-current={isSelected ? "date" : undefined}
              aria-label={d.toLocaleDateString(i18n.language, {
                weekday: "long",
                day: "numeric",
                month: "short",
              })}
              onClick={() => onDaySelect?.(day)}
            >
              <span className="mweek-strip-name">{dayName}</span>
              <span className="mweek-strip-num">{dayNum}</span>
              {loadTier && (
                <span className={`mweek-strip-load mweek-strip-load--${loadTier}`} aria-hidden="true" />
              )}
            </button>
          );
        })}
      </div>

      {/* ── Selected-day detail ── */}
      <div className="mweek-detail">
        <div className="mweek-detail-header">
          <button
            type="button"
            className="mweek-detail-date-btn btn btn-ghost btn-sm"
            onClick={() => onDayClick?.(activeDate)}
            title={t("day.drillIn", "Open day view")}
          >
            {activeDateLabel}
          </button>
        </div>

        <div className="mday-sections">
          <MemberSelectedDaySummary entries={allEntries} />

          <MemberSelectedDayUntimedSection
            entries={untimedEntries}
            onItemClick={onItemClick}
          />

          <MemberSelectedDayTimedSection
            timedEntries={timedEntries}
            selectedDate={activeDate}
            onItemClick={onItemClick}
            onSlotClick={onSlotClick}
            embedded
          />

          {allEntries.length === 0 && (
            <span className="mday-empty">{t("day.nothingScheduled")}</span>
          )}
        </div>
      </div>
    </div>
  );
}



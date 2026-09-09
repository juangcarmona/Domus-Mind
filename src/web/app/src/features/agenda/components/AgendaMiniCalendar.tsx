import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { DAY_ORDER, addMonths } from "../../agenda-today/utils/dateUtils";
import { buildCalendarGrid, weekRangeFor } from "../utils/agendaDateGrid";
import type { AgendaView } from "./AgendaHeader";

interface AgendaMiniCalendarProps {
  selectedDate: string;        // ISO YYYY-MM-DD — controlled by the page
  today: string;               // ISO YYYY-MM-DD
  view: AgendaView;            // determines range-highlight mode
  firstDayOfWeek: string | null;
  onSelectDate: (date: string) => void;
}

const WEEKDAY_KEYS = [
  "month.weekdays.sun",
  "month.weekdays.mon",
  "month.weekdays.tue",
  "month.weekdays.wed",
  "month.weekdays.thu",
  "month.weekdays.fri",
  "month.weekdays.sat",
] as const;

/**
 * Compact month-grid calendar navigator.
 *
 * - Clicking a day calls onSelectDate (never changes the active view).
 * - Day view:   only the selected day is highlighted.
 * - Week view:  the full selected week is highlighted as a band; selected day is stronger.
 * - Month view: selected day is highlighted; month header reads as the context.
 * - Today is always clearly marked.
 * - Days outside the displayed month are dimmed but visible.
 * - Mini calendar has its own month navigation; syncs to selectedDate changes from the parent.
 */
export function AgendaMiniCalendar({
  selectedDate,
  today,
  view,
  firstDayOfWeek,
  onSelectDate,
}: AgendaMiniCalendarProps) {
  const { t, i18n } = useTranslation("agenda");

  // Mini calendar has its own displayed month (can drift from selectedDate).
  const [displayMonth, setDisplayMonth] = useState<string>(selectedDate);

  // When the parent changes selectedDate, snap the displayed month only if
  // selectedDate has moved into a different month. This keeps the mini calendar
  // stable during same-month day navigation and during manual month browsing.
  useEffect(() => {
    if (selectedDate.slice(0, 7) !== displayMonth.slice(0, 7)) {
      setDisplayMonth(selectedDate);
    }
    // Intentionally excludes displayMonth to avoid re-triggering when the
    // user navigates the mini calendar's own chevrons.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDate]);

  const anchorDate = new Date(displayMonth + "T00:00:00");
  const year = anchorDate.getFullYear();
  const month = anchorDate.getMonth();

  const effectiveFirstDay = firstDayOfWeek ?? "monday";
  const firstDayIdx = Math.max(0, DAY_ORDER.indexOf(effectiveFirstDay.toLowerCase()));

  const weeks = buildCalendarGrid(year, month, effectiveFirstDay);

  const orderedWeekdayKeys = [
    ...WEEKDAY_KEYS.slice(firstDayIdx),
    ...WEEKDAY_KEYS.slice(0, firstDayIdx),
  ];

  // Week range for band-highlighting in Week view.
  const { weekStart, weekEnd } =
    view === "week"
      ? weekRangeFor(selectedDate, firstDayOfWeek)
      : { weekStart: "", weekEnd: "" };

  const monthLabel = new Date(year, month, 1).toLocaleDateString(i18n.language, {
    month: "long",
    year: "numeric",
  });

  function classForDay(iso: string): string {
    const isCurrentMonth = new Date(iso + "T00:00:00").getMonth() === month;
    const isToday = iso === today;
    const isSelected = iso === selectedDate;

    // Week-range band
    const inWeekBand = view === "week" && iso >= weekStart && iso <= weekEnd;

    return [
      "amc-day",
      !isCurrentMonth && "amc-day--other",
      isToday && "amc-day--today",
      isSelected && "amc-day--selected",
      inWeekBand && !isSelected && "amc-day--week",
      inWeekBand && isSelected && "amc-day--week-selected",
    ]
      .filter(Boolean)
      .join(" ");
  }

  return (
    <div className="agenda-mini-cal" aria-label="Date navigator">
      {/* Month nav */}
      <div className="amc-nav">
        <button
          type="button"
          className="btn btn-ghost btn-sm amc-nav-btn"
          onClick={() => setDisplayMonth(addMonths(displayMonth, -1))}
          aria-label={t("nav.prevMonth")}
        >
          ‹
        </button>
        <span className="amc-month-label">{monthLabel}</span>
        <button
          type="button"
          className="btn btn-ghost btn-sm amc-nav-btn"
          onClick={() => setDisplayMonth(addMonths(displayMonth, 1))}
          aria-label={t("nav.nextMonth")}
        >
          ›
        </button>
      </div>

      {/* Weekday header */}
      <div className="amc-grid">
        {orderedWeekdayKeys.map((key) => (
          <div key={key} className="amc-weekday">
            {(t(key as never) as string).slice(0, 1)}
          </div>
        ))}

        {/* Day cells */}
        {weeks.flat().map((iso) => {
          const dayNum = new Date(iso + "T00:00:00").getDate();
          return (
            <button
              key={iso}
              type="button"
              className={classForDay(iso)}
              onClick={() => onSelectDate(iso)}
              aria-label={iso}
              aria-pressed={iso === selectedDate}
            >
              {dayNum}
            </button>
          );
        })}
      </div>
    </div>
  );
}

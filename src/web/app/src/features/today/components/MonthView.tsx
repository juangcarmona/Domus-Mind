import { useTranslation } from "react-i18next";
import type { DayTypeSummary } from "../types";

interface MonthViewProps {
  selectedDate: string; // ISO YYYY-MM-DD - day to highlight as selected
  today: string; // ISO YYYY-MM-DD
  firstDayOfWeek?: string | null;
  displayAnchor?: string; // ISO YYYY-MM-DD - which month to show (defaults to selectedDate)
  /** Per-day item-type summary for density pips (event/task/routine). */
  daySummary?: Record<string, DayTypeSummary>;
  onSelectDay: (date: string) => void;
  onPrevMonth: () => void;
  onNextMonth: () => void;
}

const DAY_ORDER = [
  "sunday",
  "monday",
  "tuesday",
  "wednesday",
  "thursday",
  "friday",
  "saturday",
];

function toIsoDate(d: Date): string {
  return d.toISOString().slice(0, 10);
}

/**
 * Build the calendar grid for a given year+month.
 * Returns an array of weeks; each week is 7 date strings (may include dates
 * from adjacent months to fill the grid).
 */
function buildCalendarGrid(
  year: number,
  month: number, // 0-indexed
  firstDayOfWeek: string,
): string[][] {
  const firstDayIdx = Math.max(
    0,
    DAY_ORDER.indexOf(firstDayOfWeek.toLowerCase()),
  );

  // First day of this month
  const firstOfMonth = new Date(year, month, 1);
  // Last day of this month
  const lastOfMonth = new Date(year, month + 1, 0);

  // Pad start: how many days before the 1st to show
  let startPad = firstOfMonth.getDay() - firstDayIdx;
  if (startPad < 0) startPad += 7;

  // Collect all dates to show
  const dates: string[] = [];
  for (let i = -startPad; i <= lastOfMonth.getDate() - 1; i++) {
    const d = new Date(year, month, 1 + i);
    dates.push(toIsoDate(d));
  }
  // Pad end to complete last row
  const remaining = dates.length % 7 === 0 ? 0 : 7 - (dates.length % 7);
  for (let i = 1; i <= remaining; i++) {
    const d = new Date(year, month + 1, i);
    dates.push(toIsoDate(d));
  }

  // Split into weeks
  const weeks: string[][] = [];
  for (let i = 0; i < dates.length; i += 7) {
    weeks.push(dates.slice(i, i + 7));
  }
  return weeks;
}

export function MonthView({
  selectedDate,
  today,
  firstDayOfWeek,
  displayAnchor,
  daySummary,
  onSelectDay,
  onPrevMonth,
  onNextMonth,
}: MonthViewProps) {
  const { t, i18n } = useTranslation("today");

  // Use displayAnchor to decide which month to show, falling back to selectedDate
  const anchorIso = displayAnchor ?? selectedDate;
  const anchor = new Date(anchorIso + "T00:00:00");
  const year = anchor.getFullYear();
  const month = anchor.getMonth();

  const effectiveFirstDay = firstDayOfWeek ?? "monday";
  const firstDayIdx = Math.max(
    0,
    DAY_ORDER.indexOf(effectiveFirstDay.toLowerCase()),
  );

  const weeks = buildCalendarGrid(year, month, effectiveFirstDay);

  // Ordered weekday labels starting from firstDayOfWeek
  const weekdayKeys: (keyof ReturnType<typeof t>)[] = [
    "month.weekdays.sun",
    "month.weekdays.mon",
    "month.weekdays.tue",
    "month.weekdays.wed",
    "month.weekdays.thu",
    "month.weekdays.fri",
    "month.weekdays.sat",
  ];
  const orderedWeekdayKeys = [
    ...weekdayKeys.slice(firstDayIdx),
    ...weekdayKeys.slice(0, firstDayIdx),
  ];

  const monthLabel = new Date(year, month, 1).toLocaleDateString(i18n.language, {
    month: "long",
    year: "numeric",
  });

  return (
    <div className="coord-month">
      <div className="coord-month-nav">
        <button
          className="btn btn-ghost btn-sm coord-nav-btn"
          onClick={onPrevMonth}
          type="button"
          aria-label={t("nav.prevMonth")}
        >
          ‹
        </button>
        <span className="coord-month-label">{monthLabel}</span>
        <button
          className="btn btn-ghost btn-sm coord-nav-btn"
          onClick={onNextMonth}
          type="button"
          aria-label={t("nav.nextMonth")}
        >
          ›
        </button>
      </div>

      <div className="coord-month-grid">
        {/* Weekday header row */}
        <div className="coord-month-weekdays">
          {orderedWeekdayKeys.map((key) => (
            <div key={key as string} className="coord-month-weekday">
              {t(key as never)}
            </div>
          ))}
        </div>

        {/* Calendar rows */}
        {weeks.map((week, wi) => (
          <div key={wi} className="coord-month-row">
            {week.map((iso) => {
              const isCurrentMonth = new Date(iso + "T00:00:00").getMonth() === month;
              const isToday = iso === today;
              const isSelected = iso === selectedDate;

              const cellClass = [
                "coord-month-cell",
                !isCurrentMonth ? "coord-month-cell--other" : "",
                isToday ? "coord-month-cell--today" : "",
                isSelected ? "coord-month-cell--selected" : "",
              ]
                .filter(Boolean)
                .join(" ");

              const dayNum = new Date(iso + "T00:00:00").getDate();

              return (
                <button
                  key={iso}
                  className={cellClass}
                  onClick={() => onSelectDay(iso)}
                  type="button"
                  aria-label={iso}
                  aria-pressed={isSelected}
                >
                  <span className="coord-month-day-num">{dayNum}</span>
                  {(() => {
                    const s = daySummary?.[iso];
                    if (!s || (s.events === 0 && s.tasks === 0 && s.routines === 0)) return null;
                    return (
                      <div className="month-cell-pips">
                        {s.events > 0 && (
                          <span className="month-cell-pip month-cell-pip--event">
                            <span className="month-cell-pip-glyph">◆</span>{s.events}
                          </span>
                        )}
                        {s.tasks > 0 && (
                          <span className="month-cell-pip month-cell-pip--task">
                            <span className="month-cell-pip-glyph">□</span>{s.tasks}
                          </span>
                        )}
                        {s.routines > 0 && (
                          <span className="month-cell-pip month-cell-pip--routine" aria-hidden="true" />
                        )}
                      </div>
                    );
                  })()}
                </button>
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
}

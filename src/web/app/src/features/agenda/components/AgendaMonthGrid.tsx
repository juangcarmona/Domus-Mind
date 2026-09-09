import React from "react";
import { useTranslation } from "react-i18next";
import { DAY_ORDER } from "../../agenda-today/utils/dateUtils";
import { buildCalendarGrid } from "../utils/agendaDateGrid";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import type { DayTypeSummary } from "../../agenda-today/types";

// ----------------------------------------------------------------
// Compact entry preview
// ----------------------------------------------------------------

interface MonthEntryPreviewProps {
  topEntry: CalendarEntry;
  overflow: number;
}

function MonthEntryPreview({ topEntry, overflow }: MonthEntryPreviewProps) {
  const style = topEntry.color
    ? ({ "--wg-item-accent": topEntry.color } as React.CSSProperties)
    : undefined;
  return (
    <div className="amg-preview" style={style}>
      <span className="amg-preview-title">{topEntry.title}</span>
      {overflow > 0 && (
        <span className="amg-preview-overflow">+{overflow}</span>
      )}
    </div>
  );
}

// ----------------------------------------------------------------
// AgendaMonthGrid
// ----------------------------------------------------------------

interface AgendaMonthGridProps {
  selectedDate: string;
  today: string;
  firstDayOfWeek?: string | null;
  displayAnchor?: string;
  daySummary?: Record<string, DayTypeSummary>;
  dayTopEntry?: Record<string, CalendarEntry>;
  onSelectDay: (date: string) => void;
  onPrevMonth: () => void;
  onNextMonth: () => void;
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

export function AgendaMonthGrid({
  selectedDate,
  today,
  firstDayOfWeek,
  displayAnchor,
  daySummary,
  dayTopEntry,
  onSelectDay,
  onPrevMonth,
  onNextMonth,
}: AgendaMonthGridProps) {
  const { t, i18n } = useTranslation("today");

  const anchorIso = displayAnchor ?? selectedDate;
  const anchor = new Date(anchorIso + "T00:00:00");
  const year = anchor.getFullYear();
  const month = anchor.getMonth();

  const effectiveFirstDay = firstDayOfWeek ?? "monday";
  const firstDayIdx = Math.max(0, DAY_ORDER.indexOf(effectiveFirstDay.toLowerCase()));

  const weeks = buildCalendarGrid(year, month, effectiveFirstDay);

  const orderedWeekdayKeys = [
    ...WEEKDAY_KEYS.slice(firstDayIdx),
    ...WEEKDAY_KEYS.slice(0, firstDayIdx),
  ];

  const monthLabel = new Date(year, month, 1).toLocaleDateString(i18n.language, {
    month: "long",
    year: "numeric",
  });

  return (
    <div className="coord-month">
      <div className="coord-month-nav">
        <button
          className="btn btn-ghost btn-sm"
          onClick={onPrevMonth}
          type="button"
          aria-label={t("nav.prevMonth")}
        >
          ‹
        </button>
        <span className="coord-month-label">{monthLabel}</span>
        <button
          className="btn btn-ghost btn-sm"
          onClick={onNextMonth}
          type="button"
          aria-label={t("nav.nextMonth")}
        >
          ›
        </button>
      </div>

      <div className="coord-month-grid amg-month-grid">
        <div className="coord-month-weekdays">
          {orderedWeekdayKeys.map((key) => (
            <div key={key} className="coord-month-weekday">
              {t(key as never)}
            </div>
          ))}
        </div>

        {weeks.map((week, wi) => (
          <div key={wi} className="coord-month-row">
            {week.map((iso) => {
              const isCurrentMonth =
                new Date(iso + "T00:00:00").getMonth() === month;
              const isToday = iso === today;
              const isSelected = iso === selectedDate;
              const topEntry = dayTopEntry?.[iso];
              const s = daySummary?.[iso];
              const total = s ? s.events + s.tasks + s.routines : 0;
              const overflow = topEntry ? Math.max(0, total - 1) : 0;

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
                  {topEntry ? (
                    <MonthEntryPreview topEntry={topEntry} overflow={overflow} />
                  ) : total > 0 ? (
                    <div className="month-cell-pips">
                      {s!.events > 0 && (
                        <span className="month-cell-pip month-cell-pip--event">
                          <span className="month-cell-pip-glyph">◆</span>
                          {s!.events}
                        </span>
                      )}
                      {s!.tasks > 0 && (
                        <span className="month-cell-pip month-cell-pip--task">
                          <span className="month-cell-pip-glyph">□</span>
                          {s!.tasks}
                        </span>
                      )}
                      {s!.routines > 0 && (
                        <span
                          className="month-cell-pip month-cell-pip--routine"
                          aria-hidden="true"
                        />
                      )}
                    </div>
                  ) : null}
                </button>
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
}

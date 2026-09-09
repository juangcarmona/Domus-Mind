import { useState, useEffect } from "react";
import { AgendaMonthGrid } from "./AgendaMonthGrid";
import { useAgendaMonthCache } from "../hooks/useAgendaMonthCache";
import { toIsoDate, addMonths } from "../../agenda-today/utils/dateUtils";
import { useAppSelector } from "../../../store/hooks";
import { CalendarEntryItem } from "../../agenda-today/components/shared/CalendarEntryItem";
import { buildMemberEntries, sortEntries } from "../../agenda-today/utils/todayPanelHelpers";
import { useTranslation } from "react-i18next";
import type { WeeklyGridCell, WeeklyGridMember } from "../../agenda-today/types";

interface MemberMonthViewProps {
  /** Member ID to scope the month calendar. Pass null for the shared/collective subject. */
  memberId: string | null;
  selectedDate: string; // ISO YYYY-MM-DD
  firstDayOfWeek: string | null;
  /**
   * The loaded member row from the week grid for the currently selected date.
   * Used to show the selected-day detail below the calendar.
   * May be null while the grid is loading.
   */
  memberRow?: WeeklyGridMember | null;
  sharedCells?: WeeklyGridCell[];
  gridLoading?: boolean;
  onSelectDay: (date: string) => void;
  onItemClick?: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

/**
 * Month calendar for a single member's agenda.
 *
 * Top: month grid for scanning and date selection.
 * Bottom: selected-day entry list sourced from the weekly grid.
 */
export function MemberMonthView({
  memberId,
  selectedDate,
  firstDayOfWeek,
  memberRow,
  sharedCells,
  gridLoading,
  onSelectDay,
  onItemClick,
}: MemberMonthViewProps) {
  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const { t } = useTranslation("agenda");

  const todayIso = toIsoDate(new Date());

  const [monthAnchor, setMonthAnchor] = useState<string>(selectedDate);
  useEffect(() => {
    if (selectedDate.slice(0, 7) !== monthAnchor.slice(0, 7)) {
      setMonthAnchor(selectedDate);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDate]);

  const { daySummary, dayTopEntry } = useAgendaMonthCache(
    familyId,
    memberId,
    monthAnchor,
    firstDayOfWeek,
    /* active */ true,
  );

  const sharedCellForDate = sharedCells?.find((c) => c.date.slice(0, 10) === selectedDate) ?? null;
  const dayEntries = memberRow
    ? sortEntries(buildMemberEntries(memberRow, selectedDate, sharedCellForDate))
    : [];
  const hasEntries = dayEntries.length > 0;

  return (
    <div className="member-month-view">
      <AgendaMonthGrid
        selectedDate={selectedDate}
        today={todayIso}
        firstDayOfWeek={firstDayOfWeek}
        displayAnchor={monthAnchor}
        daySummary={daySummary}
        dayTopEntry={dayTopEntry}
        onSelectDay={onSelectDay}
        onPrevMonth={() => setMonthAnchor(addMonths(monthAnchor, -1))}
        onNextMonth={() => setMonthAnchor(addMonths(monthAnchor, 1))}
      />

      {/* Selected-day detail below calendar */}
      <div className="member-month-day-detail">
        {gridLoading && (
          <span className="mday-empty">{t("loading")}</span>
        )}
        {!gridLoading && !hasEntries && (
          <span className="mday-empty">{t("day.nothingScheduled")}</span>
        )}
        {!gridLoading && hasEntries && onItemClick && (
          <div className="mday-entry-list">
            {dayEntries.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
          </div>
        )}
        {!gridLoading && hasEntries && !onItemClick && (
          <div className="mday-entry-list">
            {dayEntries.map((entry) => (
              <CalendarEntryItem key={entry.id} entry={entry} onClick={() => {}} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}


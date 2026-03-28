import { useState, useEffect } from "react";
import { AgendaMonthGrid } from "./AgendaMonthGrid";
import { useAgendaMonthCache } from "../hooks/useAgendaMonthCache";
import { toIsoDate, addMonths } from "../../today/utils/dateUtils";
import { useAppSelector } from "../../../store/hooks";

interface MemberMonthViewProps {
  memberId: string;
  selectedDate: string; // ISO YYYY-MM-DD
  firstDayOfWeek: string | null;
  onSelectDay: (date: string) => void;
}

/**
 * Month calendar for a single member's agenda.
 *
 * Reuses the existing MonthView calendar grid from the Today feature.
 * Per-day density pips are scoped to this member only via useAgendaMonthCache.
 *
 * Month navigation is local; selecting a day calls onSelectDay on the parent
 * which may switch the view back to Day.
 */
export function MemberMonthView({
  memberId,
  selectedDate,
  firstDayOfWeek,
  onSelectDay,
}: MemberMonthViewProps) {
  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";

  const todayIso = toIsoDate(new Date());

  // Month anchor navigated independently of selectedDate.
  const [monthAnchor, setMonthAnchor] = useState<string>(selectedDate);
  useEffect(() => {
    setMonthAnchor(selectedDate);
  }, [selectedDate]);

  const { daySummary, dayTopEntry } = useAgendaMonthCache(
    familyId,
    memberId,
    monthAnchor,
    firstDayOfWeek,
    /* active */ true,
  );

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
    </div>
  );
}


import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse, WeeklyGridCell as GridCell } from "../types";
import { WeekHeader } from "./WeekHeader";
import { WeeklyGridRow } from "./WeeklyGridRow";
import { WeeklyGridCell as WGCell } from "./WeeklyGridCell";
import { TodaySummary } from "./TodaySummary";

interface WeeklyGridProps {
  grid: WeeklyGridResponse;
  selectedDate?: string; // Optional: highlight selected day column
  onDayClick?: (date: string) => void; // Optional: handle day header click
  suppressTodaySummary?: boolean; // When true, don't render TodaySummary above the grid
}

function SharedRow({
  cells,
  label,
  today,
}: {
  cells: GridCell[];
  label: string;
  today: string;
}) {
  return (
    <div className="wg-row wg-row--shared">
      <div className="wg-member-label">
        <span className="wg-member-name">{label}</span>
      </div>
      {cells.map((cell) => (
        <WGCell
          key={cell.date}
          cell={cell}
          isToday={cell.date.slice(0, 10) === today}
        />
      ))}
    </div>
  );
}

export function WeeklyGrid({ grid, selectedDate, onDayClick, suppressTodaySummary }: WeeklyGridProps) {
  const { t } = useTranslation("week");
  const now = new Date();
  const todayIso = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
  const members = grid.members ?? [];
  const sharedCells = grid.sharedCells ?? [];
  const hasSharedContent = sharedCells.some(
    (c) =>
      (c.events?.length ?? 0) > 0 ||
      (c.tasks?.length ?? 0) > 0 ||
      (c.routines?.length ?? 0) > 0,
  );

  const days: string[] =
    members.length > 0
      ? (members[0].cells ?? []).map((c) => c.date.slice(0, 10))
      : sharedCells.length > 0
        ? sharedCells.map((c) => c.date.slice(0, 10))
        : Array.from({ length: 7 }, (_, i) => {
            const d = new Date(grid.weekStart);
            d.setDate(d.getDate() + i);
            return d.toISOString().slice(0, 10);
          });

  const isCurrentWeek = days.includes(todayIso);

  return (
    <>
      {isCurrentWeek && !suppressTodaySummary && <TodaySummary grid={grid} today={todayIso} />}
      <div className="weekly-grid">
        <WeekHeader
          days={days}
          today={todayIso}
          selectedDate={selectedDate}
          onDayClick={onDayClick}
        />
        {hasSharedContent && (
          <SharedRow cells={sharedCells} label={t("household")} today={todayIso} />
        )}
        {members.map((member) => (
          <WeeklyGridRow key={member.memberId} member={member} today={todayIso} />
        ))}
      </div>
    </>
  );
}

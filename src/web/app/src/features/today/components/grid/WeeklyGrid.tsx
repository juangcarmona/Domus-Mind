import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse, WeeklyGridCell as GridCell } from "../../types";
import { WeekHeader } from "./WeekHeader";
import { WeeklyGridRow } from "./WeeklyGridRow";
import { WeeklyGridCell as WGCell } from "./WeeklyGridCell";

interface WeeklyGridProps {
  grid: WeeklyGridResponse;
  selectedDate?: string; // Optional: highlight selected day column
  onDayClick?: (date: string) => void; // Optional: handle day header click
  onItemClick?: (type: "event" | "task" | "routine", id: string) => void;
}

function SharedRow({
  cells,
  label,
  today,
  onItemClick,
}: {
  cells: GridCell[];
  label: string;
  today: string;
  onItemClick?: (type: "event" | "task" | "routine", id: string) => void;
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
          onItemClick={onItemClick}
        />
      ))}
    </div>
  );
}

export function WeeklyGrid({
  grid,
  selectedDate,
  onDayClick,
  onItemClick,
}: WeeklyGridProps) {
  const { t } = useTranslation("today");
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

  // Compute per-day total item count across shared + all member cells
  const dayCounts: Record<string, number> = {};
  for (const day of days) {
    const shared = sharedCells.find((c) => c.date.slice(0, 10) === day);
    let count = 0;
    if (shared) {
      count += (shared.events?.length ?? 0)
        + (shared.tasks?.filter((t) => t.status === "Active").length ?? 0)
        + (shared.routines?.length ?? 0);
    }
    for (const member of members) {
      const cell = (member.cells ?? []).find((c) => c.date.slice(0, 10) === day);
      if (cell) {
        count += (cell.events?.length ?? 0)
          + (cell.tasks?.filter((t) => t.status === "Active").length ?? 0)
          + (cell.routines?.length ?? 0);
      }
    }
    dayCounts[day] = count;
  }

  return (
    <>
      <div className="weekly-grid">
        <WeekHeader
          days={days}
          today={todayIso}
          selectedDate={selectedDate}
          onDayClick={onDayClick}
          dayCounts={dayCounts}
        />
        {hasSharedContent && (
          <SharedRow
            cells={sharedCells}
            label={t("day.household")}
            today={todayIso}
            onItemClick={onItemClick}
          />
        )}
        {members.map((member) => (
          <WeeklyGridRow
            key={member.memberId}
            member={member}
            today={todayIso}
            onItemClick={onItemClick}
          />
        ))}
      </div>
    </>
  );
}

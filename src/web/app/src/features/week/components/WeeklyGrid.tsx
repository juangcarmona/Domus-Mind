import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse, WeeklyGridCell as GridCell } from "../types";
import { WeekHeader } from "./WeekHeader";
import { WeeklyGridRow } from "./WeeklyGridRow";
import { WeeklyGridCell as WGCell } from "./WeeklyGridCell";

interface WeeklyGridProps {
  grid: WeeklyGridResponse;
}

function SharedRow({ cells, label }: { cells: GridCell[]; label: string }) {
  return (
    <div className="wg-row wg-row--shared">
      <div className="wg-member-label">
        <span className="wg-member-name">{label}</span>
      </div>
      {cells.map((cell) => (
        <WGCell key={cell.date} cell={cell} />
      ))}
    </div>
  );
}

export function WeeklyGrid({ grid }: WeeklyGridProps) {
  const { t } = useTranslation("week");
  const members = grid.members ?? [];
  const sharedCells = grid.sharedCells ?? [];
  const hasSharedContent = sharedCells.some((c) => (c.routines?.length ?? 0) > 0);

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

  return (
    <div className="weekly-grid">
      <WeekHeader days={days} />
      {hasSharedContent && (
        <SharedRow cells={sharedCells} label={t("household")} />
      )}
      {members.map((member) => (
        <WeeklyGridRow key={member.memberId} member={member} />
      ))}
    </div>
  );
}

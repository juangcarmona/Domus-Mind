import type { WeeklyGridCell as WeeklyGridCellType } from "../../types";
import { weeklyGridItemMappers } from "./weeklyGridItemMappers";

interface WeeklyGridCellProps {
  cell: WeeklyGridCellType;
  isToday?: boolean;
  compact?: boolean;
  onItemClick?: (type: "event" | "task" | "routine", id: string) => void;
}

export function WeeklyGridCell({ cell, isToday, onItemClick }: WeeklyGridCellProps) {
  const hasItems =
    (cell.events?.length ?? 0) > 0 ||
    (cell.tasks?.length ?? 0) > 0 ||
    (cell.routines?.length ?? 0) > 0;

  const classes = [
    "wg-cell",
    !hasItems ? "wg-cell--empty" : "",
    isToday ? "wg-cell--today" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classes}>
      {(cell.events ?? []).map((e) =>
        weeklyGridItemMappers.eventToItem(e, () => onItemClick?.("event", e.eventId)),
      )}
      {(cell.tasks ?? []).map((t) =>
        weeklyGridItemMappers.taskToItem(t, () => onItemClick?.("task", t.taskId)),
      )}
      {(cell.routines ?? []).map((r) =>
        weeklyGridItemMappers.routineToItem(r, () =>
          onItemClick?.("routine", r.routineId)
        ),
      )}
    </div>
  );
}

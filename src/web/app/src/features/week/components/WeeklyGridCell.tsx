import type { WeeklyGridCell as WeeklyGridCellType } from "../types";
import { eventToItem, taskToItem, routineToItem } from "./WeeklyGridItem";

interface WeeklyGridCellProps {
  cell: WeeklyGridCellType;
  isToday?: boolean;
}

export function WeeklyGridCell({ cell, isToday }: WeeklyGridCellProps) {
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
      {(cell.events ?? []).map((e) => eventToItem(e))}
      {(cell.tasks ?? []).map((t) => taskToItem(t))}
      {(cell.routines ?? []).map((r) => routineToItem(r))}
    </div>
  );
}

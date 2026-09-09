import type { WeeklyGridCell as WeeklyGridCellType } from "../../types";
import { normalizeCellItems } from "../../utils/calendarEntry";
import { CalendarEntryItem } from "../shared/CalendarEntryItem";

interface WeeklyGridCellProps {
  cell: WeeklyGridCellType;
  isToday?: boolean;
  compact?: boolean;
  onItemClick?: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

export function WeeklyGridCell({ cell, isToday, onItemClick }: WeeklyGridCellProps) {
  const entries = normalizeCellItems(cell);
  const hasItems = entries.length > 0;

  const classes = [
    "wg-cell",
    !hasItems ? "wg-cell--empty" : "",
    isToday ? "wg-cell--today" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classes}>
      {entries.map((entry) => (
        <CalendarEntryItem
          key={entry.id}
          entry={entry}
          onClick={() => onItemClick?.(entry.sourceType, entry.id)}
        />
      ))}
    </div>
  );
}


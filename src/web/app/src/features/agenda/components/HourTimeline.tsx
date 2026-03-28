import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { CalendarEntryItem } from "../../today/components/shared/CalendarEntryItem";

// Default visible hour range (inclusive).
const HOUR_START = 6;
const HOUR_END = 22;

interface HourTimelineProps {
  /** Entries that have a time value (HH:mm). */
  timedEntries: CalendarEntry[];
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

/**
 * Vertical hourly timeline for timed calendar entries.
 *
 * Each hour slot shows entries whose time falls within that hour.
 * The visible range is HOUR_START–HOUR_END; entries outside that range
 * still appear by clamping to the nearest boundary slot.
 *
 * Structure is ready for slot-based creation/rescheduling: each hour
 * cell has a data-hour attribute and a clickable background area.
 * Drag-and-drop is explicitly out of scope for V1.
 */
export function HourTimeline({ timedEntries, onItemClick }: HourTimelineProps) {
  // Bucket entries by hour.
  const byHour: Map<number, CalendarEntry[]> = new Map();
  for (let h = HOUR_START; h <= HOUR_END; h++) {
    byHour.set(h, []);
  }

  for (const entry of timedEntries) {
    if (!entry.time) continue;
    const [hStr] = entry.time.split(":");
    const hour = Math.max(HOUR_START, Math.min(HOUR_END, parseInt(hStr, 10)));
    byHour.get(hour)!.push(entry);
  }

  return (
    <div className="hour-timeline">
      {Array.from({ length: HOUR_END - HOUR_START + 1 }, (_, i) => {
        const hour = HOUR_START + i;
        const entries = byHour.get(hour) ?? [];
        const label = `${String(hour).padStart(2, "0")}:00`;

        return (
          <div
            key={hour}
            className={`ht-slot${entries.length > 0 ? " ht-slot--has-items" : ""}`}
            data-hour={hour}
          >
            <div className="ht-slot-label" aria-hidden="true">
              {label}
            </div>
            <div className="ht-slot-body">
              {entries.map((entry) => (
                <CalendarEntryItem
                  key={entry.id}
                  entry={entry}
                  onClick={() => onItemClick(entry.sourceType, entry.id)}
                />
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}

import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { ENTRY_GLYPH } from "../../today/utils/calendarEntry";

// Visible hour range (inclusive). Entries outside this range clamp to a boundary slot.
const HOUR_START = 6;
const HOUR_END = 22;

interface HourTimelineProps {
  /** All timed entries for the day (entry.time is non-null). */
  timedEntries: CalendarEntry[];
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
  /**
   * Called when an empty hour slot background is clicked.
   * Plumbed for future "create at time" flow (hour is 0–23).
   * No-ops if not provided.
   */
  onHourClick?: (hour: number) => void;
}

/**
 * Vertical hourly time grid for timed calendar entries.
 *
 * Renders a fixed set of hour rows (HOUR_START–HOUR_END).
 * - Empty rows stay visible so the day always looks like a calendar.
 * - Items with the same hour are stacked in the slot body.
 * - Each empty slot is clickable (onHourClick) for future create-at-time flow.
 * - Drag-and-drop is explicitly out of scope for V1.
 *
 * Items are rendered inline (not minute-positioned) in V1.
 * The slot structure is ready for minute-offset positioning later:
 * add a --ht-minute-offset CSS custom prop on the item element.
 */
export function HourTimeline({ timedEntries, onItemClick, onHourClick }: HourTimelineProps) {
  // Bucket entries by hour.
  const byHour = new Map<number, CalendarEntry[]>();
  for (let h = HOUR_START; h <= HOUR_END; h++) {
    byHour.set(h, []);
  }
  for (const entry of timedEntries) {
    if (!entry.time) continue;
    const [hStr] = entry.time.split(":");
    const hour = Math.max(HOUR_START, Math.min(HOUR_END, parseInt(hStr, 10)));
    byHour.get(hour)!.push(entry);
  }

  const hasAnyItems = timedEntries.length > 0;

  return (
    <div className="hour-timeline" aria-label="Hourly schedule">
      {/* Subtle empty-state banner inside the grid when no timed entries exist */}
      {!hasAnyItems && (
        <div className="ht-empty-banner" aria-hidden="true">
          {/* Shown as a centered note overlaid on the empty grid */}
        </div>
      )}

      {Array.from({ length: HOUR_END - HOUR_START + 1 }, (_, i) => {
        const hour = HOUR_START + i;
        const entries = byHour.get(hour) ?? [];
        const hasItems = entries.length > 0;
        const label = `${String(hour).padStart(2, "0")}:00`;

        return (
          <div
            key={hour}
            className={`ht-slot${hasItems ? " ht-slot--has-items" : " ht-slot--empty"}`}
            data-hour={hour}
          >
            {/* Time label column */}
            <div className="ht-label" aria-hidden="true">
              {label}
            </div>

            {/* Slot content — clickable background for future create-at-time */}
            <div
              className={`ht-body${onHourClick && !hasItems ? " ht-body--clickable" : ""}`}
              onClick={!hasItems && onHourClick ? () => onHourClick(hour) : undefined}
              role={!hasItems && onHourClick ? "button" : undefined}
              tabIndex={!hasItems && onHourClick ? 0 : undefined}
              onKeyDown={
                !hasItems && onHourClick
                  ? (e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        onHourClick(hour);
                      }
                    }
                  : undefined
              }
            >
              {entries.map((entry) => (
                <TimelineItem
                  key={entry.id}
                  entry={entry}
                  onItemClick={onItemClick}
                />
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}

// ----------------------------------------------------------------
// TimelineItem — entry rendered inside a timeline slot.
// Slightly wider layout than the compact wg-item pill used in Today.
// Preserves glyph + time + title grammar and user-defined color.
// ----------------------------------------------------------------

interface TimelineItemProps {
  entry: CalendarEntry;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

function TimelineItem({ entry, onItemClick }: TimelineItemProps) {
  const glyph = ENTRY_GLYPH[entry.displayType];
  const timeStr = entry.time ? entry.time : null;

  const style = entry.color
    ? ({ ["--wg-item-accent" as string]: entry.color } as React.CSSProperties)
    : undefined;

  const typeClass =
    entry.displayType === "overdue" ? "wg-item--overdue" : `wg-item--${entry.sourceType}`;

  const classes = [
    "wg-item",
    "ht-item",
    typeClass,
    entry.isCompleted ? "wg-item--completed" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div
      className={classes}
      style={style}
      title={`${glyph}${timeStr ? ` ${timeStr}` : ""} ${entry.title}`}
      onClick={() => onItemClick(entry.sourceType, entry.id)}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onItemClick(entry.sourceType, entry.id);
        }
      }}
    >
      <span className="wg-item-glyph" aria-hidden="true">
        {glyph}
        {timeStr && <span className="ht-item-time"> {timeStr}</span>}
      </span>
      <span className="wg-item-title">{entry.title}</span>
    </div>
  );
}


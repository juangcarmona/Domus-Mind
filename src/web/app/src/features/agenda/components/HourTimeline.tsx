import React from "react";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { ENTRY_GLYPH } from "../../today/utils/calendarEntry";

// ----------------------------------------------------------------
// 24-hour, 30-minute slot grid
// ----------------------------------------------------------------

/** Total slots = 48 (0:00 through 23:30). Slot index = hour * 2 + (half ? 1 : 0). */
const SLOT_COUNT = 48;

/** Day-phase boundary: slots 12 (06:00) through 43 (21:30) inclusive are "day". */
const DAY_SLOT_START = 6 * 2;   // 12
const DAY_SLOT_END   = 21 * 2 + 1; // 43

/**
 * Height in rem of a single 30-minute slot. Must match CSS `--ht-slot-h` on `.hour-timeline`.
 * Used to compute absolute top/height for duration blocks.
 */
const SLOT_H_REM = 1.5;

/** Convert "HH:MM" to fractional 30-min slot units from midnight. */
function timeToSlots(time: string): number {
  const [hStr, mStr] = time.split(":");
  const h = Math.min(23, Math.max(0, parseInt(hStr, 10)));
  const m = Math.min(59, Math.max(0, parseInt(mStr ?? "0", 10)));
  return (h * 60 + m) / 30;
}

function slotIndex(hour: number, minute: number): number {
  return hour * 2 + (minute >= 30 ? 1 : 0);
}

function slotLabel(slotIdx: number): string {
  const hour = Math.floor(slotIdx / 2);
  const half = slotIdx % 2 === 1;
  return half
    ? `${String(hour).padStart(2, "0")}:30`
    : `${String(hour).padStart(2, "0")}:00`;
}

function isNightSlot(slotIdx: number): boolean {
  return slotIdx < DAY_SLOT_START || slotIdx > DAY_SLOT_END;
}

/** Returns true when an entry should be rendered as an absolute-positioned duration block. */
function isDurationEvent(entry: CalendarEntry): boolean {
  return entry.sourceType === "event" && !!entry.time && !!entry.endTime;
}

// ----------------------------------------------------------------

interface HourTimelineProps {
  /** All timed entries for the day (entry.time is non-null). */
  timedEntries: CalendarEntry[];
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
  /**
   * Called when an empty slot background is clicked.
   * Receives "HH:MM" (the nearest :00 or :30 slot start).
   */
  onSlotClick?: (time: string) => void;
  /** When true, renders a current-time indicator line. */
  isToday?: boolean;
  /** Current time as total minutes from midnight. Used only when isToday is true. */
  nowMinutes?: number;
}

/**
 * Vertical 24-hour, 30-minute-slot time grid.
 *
 * - 48 slots total: 00:00, 00:30, 01:00 … 23:30
 * - Day phase (06:00–21:59): normal background; Night phase: dimmed
 * - Events with both `time` and `endTime` are rendered as absolute-positioned
 *   duration blocks spanning their full time range
 * - All other timed entries are bucketed to nearest :00/:30 slot
 * - Empty slots are optionally clickable for "create at time" action
 */
export function HourTimeline({ timedEntries, onItemClick, onSlotClick, isToday, nowMinutes }: HourTimelineProps) {
  // Separate duration events (absolute blocks) from point-in-time items (slot-bucketed).
  const durationEvents: CalendarEntry[] = [];
  const pointItems: CalendarEntry[] = [];

  for (const entry of timedEntries) {
    if (isDurationEvent(entry)) {
      durationEvents.push(entry);
    } else {
      pointItems.push(entry);
    }
  }

  // Bucket point-in-time items by slot index.
  const bySlot = new Map<number, CalendarEntry[]>();
  for (let i = 0; i < SLOT_COUNT; i++) bySlot.set(i, []);

  for (const entry of pointItems) {
    if (!entry.time) continue;
    const [hStr, mStr] = entry.time.split(":");
    const h = Math.min(23, Math.max(0, parseInt(hStr, 10)));
    const m = parseInt(mStr ?? "0", 10);
    const idx = slotIndex(h, m);
    bySlot.get(idx)!.push(entry);
  }

  return (
    <div className="hour-timeline" aria-label="Hourly schedule">
      {/* Slot rail: provides the time grid background and point-in-time items */}
      {Array.from({ length: SLOT_COUNT }, (_, idx) => {
        const isHalf = idx % 2 === 1;
        const entries = bySlot.get(idx) ?? [];
        const hasItems = entries.length > 0;
        const night = isNightSlot(idx);
        const label = slotLabel(idx);
        const clickable = !hasItems && !!onSlotClick;

        return (
          <div
            key={idx}
            className={[
              "ht-slot",
              isHalf ? "ht-slot--half" : "ht-slot--hour",
              night ? "ht-slot--night" : "ht-slot--day",
              hasItems ? "ht-slot--has-items" : "ht-slot--empty",
            ].join(" ")}
            data-slot={label}
          >
            {/* Time label column */}
            <div className={`ht-label${isHalf ? " ht-label--half" : ""}`} aria-hidden="true">
              {!isHalf ? label : <span className="ht-label-tick" />}
            </div>

            {/* Slot body */}
            <div
              className={`ht-body${clickable ? " ht-body--clickable" : ""}`}
              onClick={clickable ? () => onSlotClick!(label) : undefined}
              role={clickable ? "button" : undefined}
              tabIndex={clickable ? 0 : undefined}
              onKeyDown={
                clickable
                  ? (e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        onSlotClick!(label);
                      }
                    }
                  : undefined
              }
            >
              {entries.map((entry) => (
                <TimelineItem key={entry.id} entry={entry} onItemClick={onItemClick} />
              ))}
            </div>
          </div>
        );
      })}

      {/* Duration blocks: absolutely positioned over the slot rail */}
      {durationEvents.map((entry) => (
        <DurationBlock key={entry.id} entry={entry} onItemClick={onItemClick} />
      ))}

      {/* Current-time indicator — only shown when viewing today */}
      {isToday && nowMinutes !== undefined && (
        <div
          className="ht-now-line"
          style={{ top: `${(nowMinutes / 30) * SLOT_H_REM}rem` }}
          aria-hidden="true"
        >
          <div className="ht-now-dot" />
        </div>
      )}
    </div>
  );
}

// ----------------------------------------------------------------
// TimelineItem
// ----------------------------------------------------------------

interface TimelineItemProps {
  entry: CalendarEntry;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

function TimelineItem({ entry, onItemClick }: TimelineItemProps) {
  const glyph = ENTRY_GLYPH[entry.displayType];
  const timeStr = entry.time ?? null;

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

// ----------------------------------------------------------------
// DurationBlock — absolutely-positioned event spanning start→end time
// ----------------------------------------------------------------

interface DurationBlockProps {
  entry: CalendarEntry;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

function DurationBlock({ entry, onItemClick }: DurationBlockProps) {
  const startSlots = timeToSlots(entry.time!);
  const endSlots = timeToSlots(entry.endTime!);
  const topRem = startSlots * SLOT_H_REM;
  const heightRem = Math.max(SLOT_H_REM, (endSlots - startSlots) * SLOT_H_REM);

  const style: React.CSSProperties = {
    position: "absolute",
    top: `${topRem}rem`,
    height: `${heightRem}rem`,
    left: "2.6rem",   // aligns with ht-body (after the 2.6rem label column)
    right: "0.4rem",
    zIndex: 1,
    ...(entry.color ? { ["--wg-item-accent" as string]: entry.color } : {}),
  };

  const glyph = ENTRY_GLYPH[entry.displayType];

  const classes = [
    "wg-item",
    "ht-item",
    "ht-duration-block",
    `wg-item--${entry.sourceType}`,
    entry.isCompleted ? "wg-item--completed" : "",
  ]
    .filter(Boolean)
    .join(" ");

  const endStr = entry.endTime ?? null;

  return (
    <div
      className={classes}
      style={style}
      title={`${glyph} ${entry.time}–${endStr} ${entry.title}`}
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
        <span className="ht-item-time"> {entry.time}–{endStr}</span>
      </span>
      <span className="wg-item-title">{entry.title}</span>
    </div>
  );
}

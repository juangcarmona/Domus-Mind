import type { CalendarEntry } from "../../utils/calendarEntry";
import { ENTRY_GLYPH } from "../../utils/calendarEntry";

interface CalendarEntryItemProps {
  entry: CalendarEntry;
  onClick?: () => void;
}

function formatDateTimeRangeForTooltip(entry: CalendarEntry): string | null {
  const startDate =
    "date" in entry && typeof entry.date === "string" ? entry.date : null;

  const startTime = entry.time ?? null;
  const endTime = entry.endTime ?? null;

  if (entry.sourceType === "routine") {
    if (startTime && endTime) return `${startTime}–${endTime}`;
    if (startTime) return startTime;
    return null;
  }

  if (entry.sourceType === "event") {
    if (startDate && startTime && endTime) return `${startDate} ${startTime}–${endTime}`;
    if (startDate && startTime) return `${startDate} ${startTime}`;
    if (startDate) return startDate;
    if (startTime && endTime) return `${startTime}–${endTime}`;
    if (startTime) return startTime;
    return null;
  }

  return null;
}


/**
 * Unified renderer for a single CalendarEntry.
 *
 * Used by both the Today Panel (TodayMemberCard, TodayBoard) and the
 * Weekly Grid (WeeklyGridCell). Keeps visual grammar identical across views.
 *
 * Visual grammar (today-panel.md):
 *   !   □   overdue task
 *   □       pending task
 *   ● HH:mm event / plan
 *   ⟳       routine
 *   ✓       completed
 *
 * Color: uses the entry's own user-defined color via --wg-item-accent.
 * Global color tokens are never introduced; the fallback in CSS is the
 * existing wg-item palette.
 */
export function CalendarEntryItem({ entry, onClick }: CalendarEntryItemProps) {
  const glyph = ENTRY_GLYPH[entry.displayType];

  const detailLabel = formatDateTimeRangeForTooltip(entry);

  const tooltipParts = [
    `${glyph} ${entry.title}`,
    entry.isReadOnly ? `Read-only${entry.sourceLabel ? ` (${entry.sourceLabel})` : ""}` : null,
    detailLabel,
  ].filter(Boolean);

  const style = entry.color
    ? ({ ["--wg-item-accent" as string]: entry.color } as React.CSSProperties)
    : undefined;

  const typeClass =
    entry.displayType === "overdue" ? "wg-item--overdue" : `wg-item--${entry.sourceType}`;

  const classes = [
    "wg-item",
    typeClass,
    entry.isReadOnly ? "wg-item--readonly" : "",
    entry.isCompleted ? "wg-item--completed" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div
      className={classes}
      title={tooltipParts.join(" · ")}
      style={style}
      onClick={onClick}
      role={onClick ? "button" : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={
        onClick
          ? (e) => {
              if (e.key !== "Enter" && e.key !== " ") return;
              if (e.key === " ") e.preventDefault();
              onClick();
            }
          : undefined
      }
    >
      <span className="wg-item-glyph" aria-hidden="true">
        {glyph}
      </span>
      <span className="wg-item-title">{entry.title}</span>
      {entry.isReadOnly && (
        <span className="wg-item-readonly-cue">{entry.sourceLabel ?? "External"}</span>
      )}
    </div>
  );
}
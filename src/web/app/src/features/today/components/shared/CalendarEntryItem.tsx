import type { CalendarEntry } from "../../utils/calendarEntry";
import { ENTRY_GLYPH } from "../../utils/calendarEntry";

interface CalendarEntryItemProps {
  entry: CalendarEntry;
  onClick?: () => void;
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

  // Events show time inline: ● 19:30 Title
  const timeLabel =
    entry.sourceType === "event" && entry.time ? ` ${entry.time}` : null;

  const tooltipParts = [
    `${glyph}${timeLabel ?? ""} ${entry.title}`,
    entry.subtitle,
  ].filter(Boolean);

  const style = entry.color
    ? ({ ["--wg-item-accent" as string]: entry.color } as React.CSSProperties)
    : undefined;

  // Base CSS: wg-item (Weekly Grid compact style is the baseline).
  // displayType "overdue" gets its own emphasis class instead of sourceType.
  // displayType "completed" stacks sourceType + wg-item--completed for coloring.
  const typeClass =
    entry.displayType === "overdue" ? "wg-item--overdue" : `wg-item--${entry.sourceType}`;

  const classes = [
    "wg-item",
    typeClass,
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
        {timeLabel}
      </span>
      <span className="wg-item-title">{entry.title}</span>
      {entry.subtitle && (
        <span className="wg-item-sub">{entry.subtitle}</span>
      )}
    </div>
  );
}

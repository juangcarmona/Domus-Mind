import type { TodayEntry, TodayEntryDisplayType } from "../../utils/todayPanelHelpers";

// Spec-defined glyphs for each display type.
// Overdue tasks get both the overdue indicator (!) and the task glyph (□)
// to convey that the item is a task AND it is overdue.
const GLYPH: Record<TodayEntryDisplayType, string> = {
  overdue: "! □",
  task: "□",
  event: "●",
  routine: "⟳",
  completed: "✓",
};

interface TodayPanelItemProps {
  entry: TodayEntry;
  onClick?: () => void;
}

export function TodayPanelItem({ entry, onClick }: TodayPanelItemProps) {
  const glyph = GLYPH[entry.displayType];

  // Events show the time inline: ● 19:30 Title
  const timeLabel =
    entry.sourceType === "event" && entry.time ? ` ${entry.time}` : null;

  const label = `${glyph}${timeLabel ?? ""} ${entry.title}`;

  const style = entry.color
    ? ({ ["--panel-item-accent" as string]: entry.color } as React.CSSProperties)
    : undefined;

  const classes = [
    "tp-item",
    `tp-item--${entry.displayType}`,
  ]
    .join(" ");

  return (
    <span
      className={classes}
      style={style}
      title={label}
      onClick={onClick}
      role={onClick ? "button" : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={
        onClick
          ? (e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                onClick();
              }
            }
          : undefined
      }
    >
      <span className="tp-item-glyph" aria-hidden="true">
        {glyph}
        {timeLabel}
      </span>
      <span className="tp-item-title">{entry.title}</span>
    </span>
  );
}

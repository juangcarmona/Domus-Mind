type ItemType = "event" | "task" | "routine";

/** Small Unicode glyphs that hint at type without using text labels. */
const TYPE_GLYPH: Record<ItemType, string> = {
  event: "◆",   // plan / scheduled event
  task: "□",    // task / chore (checkbox metaphor)
  routine: "↻", // repeating
};

interface WeeklyGridItemProps {
  type: ItemType;
  title: string;
  time?: string | null;
  status?: string;
  subtitle?: string;
  color?: string | null;
  compact?: boolean;
  onClick?: () => void;
}

export function WeeklyGridItem({
  type,
  title,
  time,
  status,
  subtitle,
  color,
  onClick,
}: WeeklyGridItemProps) {
  const isCompleted = status?.toLowerCase() === "completed";

  const tooltipText = [title, time, subtitle]
    .filter(Boolean)
    .join(" · ");

  const style = color
    ? ({ ["--wg-item-accent" as string]: color } as React.CSSProperties)
    : undefined;

  const classes = [
    "wg-item",
    `wg-item--${type}`,
    isCompleted ? "wg-item--completed" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div
      className={classes}
      title={tooltipText}
      style={style}
      onClick={onClick}
      role={onClick ? "button" : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={
        onClick
          ? (e) => {
              if (e.key !== "Enter" && e.key !== " ") {
                return;
              }
              if (e.key === " ") {
                e.preventDefault();
              }
              onClick();
            }
          : undefined
      }
    >
      <span className="wg-item-glyph" aria-hidden="true">{TYPE_GLYPH[type]}</span>
      <span className="wg-item-title">{title}{time ? ` · ${time}` : ""}</span>
      {subtitle && <span className="wg-item-sub">{subtitle}</span>}
    </div>
  );
}

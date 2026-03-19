type ItemType = "event" | "task" | "routine";

interface WeeklyGridItemProps {
  type: ItemType;
  title: string;
  time?: string | null;
  status?: string;
  subtitle?: string;
  color?: string | null;
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
  const tooltipText = [title, time, subtitle, status ? `(${status})` : ""]
    .filter(Boolean)
    .join(" · ");

  const style = color
    ? ({ ["--wg-item-accent" as string]: color } as React.CSSProperties)
    : undefined;

  return (
    <div
      className={`wg-item wg-item--${type}`}
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
      <span className="wg-item-title">{title} {time && `· ${time}`}</span>
    </div>
  );
}

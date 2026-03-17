import type { WeeklyGridEventItem, WeeklyGridTaskItem, WeeklyGridRoutineItem } from "../types";

type ItemType = "event" | "task" | "routine";

interface WeeklyGridItemProps {
  type: ItemType;
  title: string;
  time?: string | null;
  status?: string;
  subtitle?: string;
  color?: string | null;
}

export function WeeklyGridItem({
  type,
  title,
  time,
  status,
  subtitle,
  color,
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
    >
      <span className="wg-item-title">{title} {time && `· ${time}`}</span>
    </div>
  );
}

export function eventToItem(e: WeeklyGridEventItem) {
  const time = new Date(e.startTime).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  });
  const participantNames = e.participants?.map((p) => p.displayName).join(", ");
  return (
    <WeeklyGridItem
      key={e.eventId}
      type="event"
      title={e.title}
      time={time}
      status={e.status}
      subtitle={participantNames || undefined}
    />
  );
}

export function taskToItem(t: WeeklyGridTaskItem) {
  return (
    <WeeklyGridItem
      key={t.taskId}
      type="task"
      title={t.title}
      status={t.status}
    />
  );
}

export function routineToItem(r: WeeklyGridRoutineItem) {
  return (
    <WeeklyGridItem
      key={`routine-${r.routineId}`}
      type="routine"
      title={r.name}
      time={r.time ?? r.frequency}
      status={r.kind}
      color={r.color}
    />
  );
}

/**
 * @deprecated
 * This file is no longer used. Rendering is now handled by CalendarEntryItem
 * via the shared CalendarEntry model (utils/calendarEntry.ts).
 * WeeklyGridCell uses normalizeCellItems() + CalendarEntryItem directly.
 * This file is kept temporarily to avoid breaking any untracked imports.
 */
import type {
  WeeklyGridEventItem,
  WeeklyGridTaskItem,
  WeeklyGridRoutineItem,
} from "../../types";
import { WeeklyGridItem } from "./WeeklyGridItem";

function eventToItem(e: WeeklyGridEventItem, onClick?: () => void) {
  const time = e.time ?? undefined; // already HH:mm or null
  const participantNames = e.participants?.map((p) => p.displayName).join(", ");
  return (
    <WeeklyGridItem
      key={e.eventId}
      type="event"
      title={e.title}
      time={time}
      status={e.status}
      color={e.color}
      subtitle={participantNames || undefined}
      onClick={onClick}
    />
  );
}

function taskToItem(t: WeeklyGridTaskItem, onClick?: () => void) {
  return (
    <WeeklyGridItem
      key={t.taskId}
      type="task"
      title={t.title}
      status={t.status}
      color={t.color}
      onClick={onClick}
    />
  );
}

function routineToItem(r: WeeklyGridRoutineItem, onClick?: () => void) {
  return (
    <WeeklyGridItem
      key={`routine-${r.routineId}`}
      type="routine"
      title={r.name}
      time={r.time ?? undefined}
      status={r.kind}
      color={r.color}
      onClick={onClick}
    />
  );
}

export const weeklyGridItemMappers = {
  eventToItem,
  taskToItem,
  routineToItem,
};

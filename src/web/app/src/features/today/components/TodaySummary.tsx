import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse, WeeklyGridCell } from "../types";
import { weeklyGridItemMappers } from "./weeklyGridItemMappers";

interface TodaySummaryProps {
  grid: WeeklyGridResponse;
  today: string; // ISO date string yyyy-mm-dd
  onItemClick?: (type: "event" | "task" | "routine", id: string) => void;
}

function SummaryMemberSection({
  name,
  cell,
  onItemClick,
}: {
  name: string;
  cell: WeeklyGridCell;
  onItemClick?: (type: "event" | "task" | "routine", id: string) => void;
}) {
  const { t } = useTranslation("today");
  const events = cell.events ?? [];
  const tasks = cell.tasks ?? [];
  const routines = cell.routines ?? [];
  const isEmpty = events.length === 0 && tasks.length === 0 && routines.length === 0;

  return (
    <div className="today-summary-member">
      <div className="today-summary-member-name">{name}</div>
      {isEmpty ? (
        <span className="today-summary-empty">{t("day.todayEmpty")}</span>
      ) : (
        <div className="today-summary-items">
          {events.map((e) =>
            weeklyGridItemMappers.eventToItem(e, () => onItemClick?.("event", e.eventId)),
          )}
          {tasks.map((t) =>
            weeklyGridItemMappers.taskToItem(t, () => onItemClick?.("task", t.taskId)),
          )}
          {routines.map((r) =>
            weeklyGridItemMappers.routineToItem(r, () =>
              onItemClick?.("routine", r.routineId),
            ),
          )}
        </div>
      )}
    </div>
  );
}

export function TodaySummary({ grid, today, onItemClick }: TodaySummaryProps) {
  const { t, i18n } = useTranslation("today");

  const todayDate = new Date(today);
  const todayLabel = todayDate.toLocaleDateString(i18n.language, {
    weekday: "long",
    day: "numeric",
    month: "long",
  });

  // Find today's shared cell (household routines)
  const sharedTodayCell = (grid.sharedCells ?? []).find(
    (c) => c.date.slice(0, 10) === today,
  );
  const hasSharedItems = (sharedTodayCell?.routines?.length ?? 0) > 0;

  // Find today's cell per member
  const memberCells = (grid.members ?? []).map((m) => ({
    member: m,
    cell: m.cells.find((c) => c.date.slice(0, 10) === today) ?? {
      date: today,
      events: [],
      tasks: [],
      routines: [],
    },
  }));

  const hasAnyContent =
    hasSharedItems ||
    memberCells.some(
      ({ cell }) =>
        (cell.events?.length ?? 0) > 0 ||
        (cell.tasks?.length ?? 0) > 0 ||
        (cell.routines?.length ?? 0) > 0,
    );

  return (
    <div className="today-summary">
      <div className="today-summary-header">
        <span className="today-summary-label">{t("nav.today")}</span>
        <span className="today-summary-date">{todayLabel}</span>
      </div>
      {!hasAnyContent ? (
        <p className="today-summary-empty">{t("day.todayNothingScheduled")}</p>
      ) : (
        <div className="today-summary-body">
          {hasSharedItems && sharedTodayCell && (
            <SummaryMemberSection
              name={t("day.household")}
              cell={sharedTodayCell}
              onItemClick={onItemClick}
            />
          )}
          {memberCells.map(({ member, cell }) => (
            <SummaryMemberSection
              key={member.memberId}
              name={member.name}
              cell={cell}
              onItemClick={onItemClick}
            />
          ))}
        </div>
      )}
    </div>
  );
}

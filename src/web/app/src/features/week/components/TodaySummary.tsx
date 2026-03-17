import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse, WeeklyGridCell } from "../types";
import { eventToItem, taskToItem, routineToItem } from "./WeeklyGridItem";

interface TodaySummaryProps {
  grid: WeeklyGridResponse;
  today: string; // ISO date string yyyy-mm-dd
}

function SummaryMemberSection({
  name,
  cell,
}: {
  name: string;
  cell: WeeklyGridCell;
}) {
  const { t } = useTranslation("week");
  const events = cell.events ?? [];
  const tasks = cell.tasks ?? [];
  const routines = cell.routines ?? [];
  const isEmpty = events.length === 0 && tasks.length === 0 && routines.length === 0;

  return (
    <div className="today-summary-member">
      <div className="today-summary-member-name">{name}</div>
      {isEmpty ? (
        <span className="today-summary-empty">{t("todayEmpty")}</span>
      ) : (
        <div className="today-summary-items">
          {events.map((e) => eventToItem(e))}
          {tasks.map((t) => taskToItem(t))}
          {routines.map((r) => routineToItem(r))}
        </div>
      )}
    </div>
  );
}

export function TodaySummary({ grid, today }: TodaySummaryProps) {
  const { t, i18n } = useTranslation("week");

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
        <span className="today-summary-label">{t("today")}</span>
        <span className="today-summary-date">{todayLabel}</span>
      </div>
      {!hasAnyContent ? (
        <p className="today-summary-empty">{t("todayNothingScheduled")}</p>
      ) : (
        <div className="today-summary-body">
          {hasSharedItems && sharedTodayCell && (
            <SummaryMemberSection
              name={t("household")}
              cell={sharedTodayCell}
            />
          )}
          {memberCells.map(({ member, cell }) => (
            <SummaryMemberSection
              key={member.memberId}
              name={member.name}
              cell={cell}
            />
          ))}
        </div>
      )}
    </div>
  );
}

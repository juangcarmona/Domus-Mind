import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse, WeeklyGridCell } from "../../types";
import { weeklyGridItemMappers } from "../grid/weeklyGridItemMappers";

const ACTOR_ROLES = new Set(["Adult", "Child"]);
const ROLE_SORT_ORDER: Record<string, number> = { Adult: 0, Child: 1 };

interface TodayBoardProps {
  grid: WeeklyGridResponse | null;
  selectedDate: string; // ISO YYYY-MM-DD
  loading: boolean;
  error: string | null;
  isToday: boolean;
  onPrevDay: () => void;
  onNextDay: () => void;
  onToday: () => void;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

function DayMemberSection({
  name,
  cell,
  onItemClick,
}: {
  name: string;
  cell: WeeklyGridCell;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
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
            weeklyGridItemMappers.eventToItem(e, () => onItemClick("event", e.eventId)),
          )}
          {tasks.map((t) =>
            weeklyGridItemMappers.taskToItem(t, () => onItemClick("task", t.taskId)),
          )}
          {routines.map((r) =>
            weeklyGridItemMappers.routineToItem(r, () =>
              onItemClick("routine", r.routineId),
            ),
          )}
        </div>
      )}
    </div>
  );
}

export function TodayBoard({
  grid,
  selectedDate,
  loading,
  error,
  isToday,
  onPrevDay,
  onNextDay,
  onToday,
  onItemClick,
}: TodayBoardProps) {
  const { t, i18n } = useTranslation("today");
  const { t: tCommon } = useTranslation("common");

  const dateLabel = new Date(selectedDate + "T00:00:00").toLocaleDateString(
    i18n.language,
    { weekday: "long", day: "numeric", month: "long" },
  );

  if (loading) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }
  if (error) {
    return <p className="error-msg">{error}</p>;
  }
  if (!grid) {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  const members = grid.members ?? [];
  const sharedCells = grid.sharedCells ?? [];

  const sharedCell = sharedCells.find((c) => c.date.slice(0, 10) === selectedDate);
  const hasSharedItems =
    (sharedCell?.events?.length ?? 0) > 0 ||
    (sharedCell?.tasks?.length ?? 0) > 0 ||
    (sharedCell?.routines?.length ?? 0) > 0;

  const memberDays = members.map((member) => ({
    member,
    cell: member.cells.find((c) => c.date.slice(0, 10) === selectedDate) ?? {
      date: selectedDate,
      events: [],
      tasks: [],
      routines: [],
    },
  }));

  const actorDays = memberDays
    .filter(({ member }) => ACTOR_ROLES.has(member.role))
    .sort(
      (a, b) =>
        (ROLE_SORT_ORDER[a.member.role] ?? 9) -
        (ROLE_SORT_ORDER[b.member.role] ?? 9),
    );

  const hasAnyContent =
    hasSharedItems ||
    actorDays.some(
      ({ cell }) =>
        (cell.events?.length ?? 0) > 0 ||
        (cell.tasks?.length ?? 0) > 0 ||
        (cell.routines?.length ?? 0) > 0,
    );

  return (
    <div className="today-summary coord-day-panel">
      <div className="today-summary-header coord-day-header">
        <button
          className="btn btn-ghost btn-sm coord-nav-btn"
          onClick={onPrevDay}
          type="button"
          aria-label={t("nav.prevDay")}
        >
          ‹
        </button>
        <div className="coord-day-header-label">
          <span className="today-summary-label">
            {isToday ? t("nav.today") : t("day.title")}
          </span>
          <span className="today-summary-date">{dateLabel}</span>
        </div>
        <div className="coord-day-header-right">
          {!isToday && (
            <button
              className="btn btn-ghost btn-sm coord-today-btn"
              onClick={onToday}
              type="button"
            >
              {t("nav.today")}
            </button>
          )}
          <button
            className="btn btn-ghost btn-sm coord-nav-btn"
            onClick={onNextDay}
            type="button"
            aria-label={t("nav.nextDay")}
          >
            ›
          </button>
        </div>
      </div>

      {members.length === 0 && (
        <p className="today-summary-empty">{t("day.noMembers")}</p>
      )}

      {members.length > 0 && !hasAnyContent && (
        <p className="today-summary-empty">{t("day.empty")}</p>
      )}

      {members.length > 0 && hasAnyContent && (
        <>
          <div className="today-summary-body">
            {actorDays.map(({ member, cell }) => (
              <DayMemberSection
                key={member.memberId}
                name={member.name}
                cell={cell}
                onItemClick={onItemClick}
              />
            ))}
          </div>
          {hasSharedItems && sharedCell && (
            <div className="today-household">
              <div className="today-household-label">{t("day.household")}</div>
              <div className="today-summary-items">
                {sharedCell.events?.map((e) =>
                  weeklyGridItemMappers.eventToItem(e, () =>
                    onItemClick("event", e.eventId),
                  ),
                )}
                {sharedCell.tasks?.map((task) =>
                  weeklyGridItemMappers.taskToItem(task, () =>
                    onItemClick("task", task.taskId),
                  ),
                )}
                {sharedCell.routines?.map((r) =>
                  weeklyGridItemMappers.routineToItem(r, () =>
                    onItemClick("routine", r.routineId),
                  ),
                )}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

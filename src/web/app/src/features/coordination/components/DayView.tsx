import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../week/types";
import { eventToItem, taskToItem, routineToItem } from "../../week/components/WeeklyGridItem";

interface DayViewProps {
  grid: WeeklyGridResponse | null;
  selectedDate: string; // ISO YYYY-MM-DD
  loading: boolean;
  error: string | null;
}

function MemberDaySection({
  name,
  role,
  events,
  tasks,
  routines,
  emptyLabel,
}: {
  name: string;
  role?: string;
  events: React.ReactNode[];
  tasks: React.ReactNode[];
  routines: React.ReactNode[];
  emptyLabel: string;
}) {
  const isEmpty = events.length === 0 && tasks.length === 0 && routines.length === 0;

  return (
    <div className="coord-day-member">
      <div className="coord-day-member-label">
        <span className="coord-day-member-name">{name}</span>
        {role && <span className="coord-day-member-role">{role}</span>}
      </div>
      <div className="coord-day-member-items">
        {isEmpty ? (
          <span className="coord-day-empty-member">{emptyLabel}</span>
        ) : (
          <>
            {events}
            {tasks}
            {routines}
          </>
        )}
      </div>
    </div>
  );
}

export function DayView({ grid, selectedDate, loading, error }: DayViewProps) {
  const { t, i18n } = useTranslation("coordination");
  const { t: tWeek } = useTranslation("week");
  const { t: tCommon } = useTranslation("common");

  const dateLabel = new Date(selectedDate + "T00:00:00").toLocaleDateString(
    i18n.language,
    { weekday: "long", day: "numeric", month: "long", year: "numeric" },
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

  // Extract each member's cell for the selected date
  const memberDays = members.map((member) => ({
    member,
    cell: member.cells.find((c) => c.date.slice(0, 10) === selectedDate) ?? {
      date: selectedDate,
      events: [],
      tasks: [],
      routines: [],
    },
  }));

  // Shared/household cell for the selected date
  const sharedCell = sharedCells.find((c) => c.date.slice(0, 10) === selectedDate);
  const hasSharedItems = (sharedCell?.routines?.length ?? 0) > 0;

  const hasAnyContent =
    hasSharedItems ||
    memberDays.some(
      ({ cell }) =>
        (cell.events?.length ?? 0) > 0 ||
        (cell.tasks?.length ?? 0) > 0 ||
        (cell.routines?.length ?? 0) > 0,
    );

  const nothingLabel = t("day.empty");
  const emptyMemberLabel = tWeek("todayEmpty");

  return (
    <div className="coord-day-view">
      <div className="coord-day-header">
        <h2 className="coord-day-title">{dateLabel}</h2>
      </div>

      {members.length === 0 && (
        <p className="empty-note">{t("day.noMembers")}</p>
      )}

      {members.length > 0 && !hasAnyContent && (
        <div className="empty-state">
          <p>{nothingLabel}</p>
        </div>
      )}

      {(hasSharedItems || members.length > 0) && hasAnyContent && (
        <div className="coord-day-sections">
          {hasSharedItems && sharedCell && (
            <MemberDaySection
              name={t("day.household")}
              events={[]}
              tasks={[]}
              routines={(sharedCell.routines ?? []).map((r) => routineToItem(r))}
              emptyLabel={emptyMemberLabel}
            />
          )}
          {memberDays.map(({ member, cell }) => (
            <MemberDaySection
              key={member.memberId}
              name={member.name}
              role={member.role}
              events={(cell.events ?? []).map((e) => eventToItem(e))}
              tasks={(cell.tasks ?? []).map((t) => taskToItem(t))}
              routines={(cell.routines ?? []).map((r) => routineToItem(r))}
              emptyLabel={emptyMemberLabel}
            />
          ))}
        </div>
      )}
    </div>
  );
}

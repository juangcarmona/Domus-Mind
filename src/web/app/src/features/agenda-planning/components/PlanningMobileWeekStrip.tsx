import { useTranslation } from "react-i18next";
import { useAppSelector } from "../../../store/hooks";
import { MemberAvatar } from "../../settings/components/avatar/MemberAvatar";
import { CalendarEntryItem } from "../../agenda-today/components/shared/CalendarEntryItem";
import {
  buildMemberEntries,
  buildSharedEntries,
  sortEntries,
} from "../../agenda-today/utils/todayPanelHelpers";
import { HouseholdLogo } from "../../../components/HouseholdLogo";
import type { WeeklyGridResponse } from "../../agenda-today/types";
import { toIsoDate } from "../../agenda-today/utils/dateUtils";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";

interface PlanningMobileWeekStripProps {
  grid: WeeklyGridResponse | null;
  loading: boolean;
  error: string | null;
  selectedDate: string; // ISO YYYY-MM-DD
  onDaySelect: (date: string) => void;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

const ACTOR_ROLES = new Set(["Adult", "Child", "Caregiver"]);
const ROLE_SORT_ORDER: Record<string, number> = { Adult: 0, Caregiver: 1, Child: 2 };

/**
 * Mobile week composition for Planning.
 *
 * Two parts:
 *   1. A compact day-strip: 7 buttons (Mon–Sun) with load indicators.
 *      Tapping a day selects it.
 *   2. The selected day's content shown below as flat member-row list.
 *
 * This preserves the "week view" mental model on mobile without a 7-column grid.
 */
export function PlanningMobileWeekStrip({
  grid,
  loading,
  error,
  selectedDate,
  onDaySelect,
  onItemClick,
}: PlanningMobileWeekStripProps) {
  const { t } = useTranslation("agenda");
  const { t: tCommon } = useTranslation("common");
  const { i18n } = useTranslation();

  if (loading) {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }
  if (error) {
    return <p className="error-msg">{error}</p>;
  }
  if (!grid) {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  const today = toIsoDate(new Date());

  // Derive the 7 days from grid
  const days: string[] =
    grid.members.length > 0
      ? (grid.members[0].cells ?? []).map((c) => c.date.slice(0, 10))
      : grid.sharedCells.map((c) => c.date.slice(0, 10));

  // Per-day item counts for load dots
  const dayCounts: Record<string, number> = {};
  for (const day of days) {
    const shared = grid.sharedCells.find((c) => c.date.slice(0, 10) === day);
    let count = 0;
    if (shared) {
      count += (shared.events?.length ?? 0)
        + (shared.tasks?.length ?? 0)
        + (shared.routines?.length ?? 0)
        + (shared.listItems?.length ?? 0);
    }
    for (const member of grid.members) {
      const cell = member.cells.find((c) => c.date.slice(0, 10) === day);
      if (cell) {
        count += (cell.events?.length ?? 0)
          + (cell.tasks?.length ?? 0)
          + (cell.routines?.length ?? 0)
          + (cell.listItems?.length ?? 0);
      }
    }
    dayCounts[day] = count;
  }

  // Selected day content
  const actorMembers = (grid.members ?? [])
    .filter((m) => ACTOR_ROLES.has(m.role))
    .sort((a, b) => (ROLE_SORT_ORDER[a.role] ?? 9) - (ROLE_SORT_ORDER[b.role] ?? 9));

  const sharedForDay = buildSharedEntries(grid.sharedCells, selectedDate);
  const sortedShared = sortEntries(sharedForDay);
  const hasSharedContent = sortedShared.length > 0;

  return (
    <div className="pmws">
      {/* ---- Day strip ---- */}
      <div className="pmws-strip">
        {days.map((day) => {
          const d = new Date(day + "T00:00:00");
          const isSelected = day === selectedDate;
          const isToday = day === today;
          const count = dayCounts[day] ?? 0;

          const dayName = d.toLocaleDateString(i18n.language, { weekday: "narrow" });
          const dayNum = d.getDate();
          const loadTier = count === 0 ? null : count <= 2 ? "low" : count <= 5 ? "medium" : "high";

          return (
            <button
              key={day}
              type="button"
              className={[
                "pmws-day",
                isSelected ? "pmws-day--selected" : "",
                isToday ? "pmws-day--today" : "",
              ].filter(Boolean).join(" ")}
              onClick={() => onDaySelect(day)}
              aria-current={isSelected ? "date" : undefined}
              aria-label={d.toLocaleDateString(i18n.language, { weekday: "long", day: "numeric", month: "short" })}
            >
              <span className="pmws-day-name">{dayName}</span>
              <span className="pmws-day-num">{dayNum}</span>
              {loadTier && (
                <span className={`pmws-load pmws-load--${loadTier}`} aria-hidden="true" />
              )}
            </button>
          );
        })}
      </div>

      {/* ---- Selected day content ---- */}
      <div className="pmws-day-content">
        {/* Shared / household row */}
        {hasSharedContent && (
          <div className="pmws-section">
            <div className="pmws-section-label">
              <HouseholdLogo size={16} />
            </div>
            <div className="pmws-section-entries">
              {sortedShared.map((entry) => (
                <CalendarEntryItem
                  key={entry.id}
                  entry={entry}
                  onClick={() => onItemClick(entry.sourceType, entry.id)}
                />
              ))}
            </div>
          </div>
        )}

        {/* Member rows */}
        {actorMembers.map((member) => {
          const entries = sortEntries(buildMemberEntries(member, selectedDate));
          if (entries.length === 0) return null;
          return (
            <MemberDaySection
              key={member.memberId}
              memberId={member.memberId}
              name={member.name}
              entries={entries}
              onItemClick={onItemClick}
            />
          );
        })}

        {/* Empty state */}
        {!hasSharedContent && actorMembers.every((m) =>
          sortEntries(buildMemberEntries(m, selectedDate)).length === 0
        ) && (
          <p className="pmws-empty">{t("week.empty")}</p>
        )}
      </div>
    </div>
  );
}

function MemberDaySection({
  memberId,
  name,
  entries,
  onItemClick,
}: {
  memberId: string;
  name: string;
  entries: CalendarEntry[];
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}) {
  const householdMember = useAppSelector((s) =>
    s.household.members.find((m) => m.memberId === memberId),
  );
  const displayName = householdMember?.preferredName || name;

  return (
    <div className="pmws-section">
      <div className="pmws-section-label">
        <MemberAvatar
          initial={householdMember?.avatarInitial ?? displayName[0]?.toUpperCase() ?? "?"}
          avatarIconId={householdMember?.avatarIconId}
          avatarColorId={householdMember?.avatarColorId}
          size={20}
        />
      </div>
      <div className="pmws-section-entries">
        {entries.map((entry) => (
          <CalendarEntryItem
            key={entry.id}
            entry={entry}
            onClick={() => onItemClick(entry.sourceType, entry.id)}
          />
        ))}
      </div>
    </div>
  );
}

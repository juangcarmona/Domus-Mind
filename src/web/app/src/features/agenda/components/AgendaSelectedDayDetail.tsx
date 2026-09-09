import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../agenda-today/types";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import {
  buildSharedEntries,
  buildMemberEntries,
  sortEntries,
} from "../../agenda-today/utils/todayPanelHelpers";
import { CalendarEntryItem } from "../../agenda-today/components/shared/CalendarEntryItem";
import { HouseholdLogo } from "../../../components/HouseholdLogo";
import { MemberAvatar } from "../../settings/components/avatar/MemberAvatar";
import { useAppSelector } from "../../../store/hooks";

// ----------------------------------------------------------------
// Member section
// ----------------------------------------------------------------

interface MemberSectionProps {
  memberId: string;
  name: string;
  entries: CalendarEntry[];
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

function MemberSection({ memberId, name, entries, onItemClick }: MemberSectionProps) {
  const member = useAppSelector((s) => s.household.members.find((m) => m.memberId === memberId));
  const displayName = member?.preferredName || name;

  return (
    <div className="asdd-section">
      <div className="asdd-section-icon">
        <MemberAvatar
          initial={member?.avatarInitial ?? displayName[0]?.toUpperCase() ?? "?"}
          avatarIconId={member?.avatarIconId}
          avatarColorId={member?.avatarColorId}
          size={20}
        />
      </div>
      <div className="asdd-section-entries">
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

// ----------------------------------------------------------------
// AgendaSelectedDayDetail
// ----------------------------------------------------------------

const ACTOR_ROLES = new Set(["Adult", "Child", "Caregiver"]);
const ROLE_SORT: Record<string, number> = { Adult: 0, Caregiver: 1, Child: 2 };

interface AgendaSelectedDayDetailProps {
  /**
   * Weekly grid for the week containing selectedDate.
   * Pass null while loading — a compact loading state will be shown.
   */
  grid: WeeklyGridResponse | null;
  /** ISO YYYY-MM-DD */
  selectedDate: string;
  loading?: boolean;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

/**
 * Shared selected-day detail panel.
 *
 * Shows household-shared entries + per-member entries for the given date,
 * sourced from the weekly grid. Used below the month calendar (household and member scope).
 *
 * Does not manage data fetching — the parent page is responsible for keeping
 * `grid` and `loading` current as `selectedDate` changes.
 */
export function AgendaSelectedDayDetail({
  grid,
  selectedDate,
  loading,
  onItemClick,
}: AgendaSelectedDayDetailProps) {
  const { t } = useTranslation("agenda");

  if (loading) {
    return <div className="asdd asdd--loading"><span className="asdd-loading-text">{t("loading")}</span></div>;
  }

  if (!grid) return null;

  const sharedEntries = sortEntries(buildSharedEntries(grid.sharedCells, selectedDate));
  const hasShared = sharedEntries.length > 0;

  const actorMembers = (grid.members ?? [])
    .filter((m) => ACTOR_ROLES.has(m.role))
    .sort((a, b) => (ROLE_SORT[a.role] ?? 9) - (ROLE_SORT[b.role] ?? 9));

  const memberSections = actorMembers
    .map((m) => ({ member: m, entries: sortEntries(buildMemberEntries(m, selectedDate)) }))
    .filter(({ entries }) => entries.length > 0);

  if (!hasShared && memberSections.length === 0) {
    return (
      <div className="asdd asdd--empty">
        <span className="asdd-empty-text">{t("day.nothingScheduled")}</span>
      </div>
    );
  }

  return (
    <div className="asdd">
      {hasShared && (
        <div className="asdd-section">
          <div className="asdd-section-icon">
            <HouseholdLogo size={16} />
          </div>
          <div className="asdd-section-entries">
            {sharedEntries.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
          </div>
        </div>
      )}
      {memberSections.map(({ member, entries }) => (
        <MemberSection
          key={member.memberId}
          memberId={member.memberId}
          name={member.name}
          entries={entries}
          onItemClick={onItemClick}
        />
      ))}
    </div>
  );
}

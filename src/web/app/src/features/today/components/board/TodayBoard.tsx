import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../types";
import {
  buildMemberEntries,
  buildSharedEntries,
  splitForDisplay,
} from "../../utils/todayPanelHelpers";
import { TodayMemberCard } from "./TodayMemberCard";
import { TodayPanelItem } from "./TodayPanelItem";

// Only show People roles in the Today Panel. Pets are excluded (V1 scope).
const ACTOR_ROLES = new Set(["Adult", "Child", "Caregiver"]);
const ROLE_SORT_ORDER: Record<string, number> = { Adult: 0, Caregiver: 1, Child: 2 };

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

  // Only one card may be expanded at a time. null = all collapsed.
  const [expandedMemberId, setExpandedMemberId] = useState<string | null>(null);

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

  // Actor members ordered by role priority, then by the order the backend returns them.
  const actorMembers = members
    .filter((m) => ACTOR_ROLES.has(m.role))
    .sort(
      (a, b) =>
        (ROLE_SORT_ORDER[a.role] ?? 9) - (ROLE_SORT_ORDER[b.role] ?? 9),
    );

  // Build normalised entry lists.
  const memberEntries = actorMembers.map((member) => ({
    member,
    entries: buildMemberEntries(member, selectedDate),
  }));

  const sharedEntries = buildSharedEntries(sharedCells, selectedDate);
  const sharedDisplayState = splitForDisplay(sharedEntries);

  function handleCardToggle(memberId: string) {
    setExpandedMemberId((prev) => (prev === memberId ? null : memberId));
  }

  return (
    <div className="today-summary coord-day-panel">
      {/* ---- Header with day navigation ---- */}
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

      {/* ---- No members yet ---- */}
      {members.length === 0 && (
        <p className="today-summary-empty">{t("day.noMembers")}</p>
      )}

      {/* ---- Member cards (one per person) ---- */}
      {actorMembers.length > 0 && (
        <div className="tp-member-list">
          {memberEntries.map(({ member, entries }) => (
            <TodayMemberCard
              key={member.memberId}
              name={member.name}
              entries={entries}
              isExpanded={expandedMemberId === member.memberId}
              onToggle={() => handleCardToggle(member.memberId)}
              onItemClick={onItemClick}
            />
          ))}
        </div>
      )}

      {/* ---- Household row (shared / unassigned items only) ---- */}
      <div className="today-household tp-household-row">
        <div className="today-household-label">{t("day.household")}</div>
        {sharedDisplayState.isEmpty ? (
          <span className="today-summary-empty tp-card-empty">
            {t("day.nothingToday")}
          </span>
        ) : (
          <div className="today-household-chips">
            {sharedDisplayState.activeItems.map((entry) => (
              <TodayPanelItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
            {sharedDisplayState.completedItems.map((entry) => (
              <TodayPanelItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

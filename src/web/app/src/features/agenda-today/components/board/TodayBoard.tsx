import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../types";
import {
  buildMemberEntries,
  buildSharedEntries,
  splitForDisplay,
} from "../../utils/todayPanelHelpers";
import { CalendarEntryItem } from "../shared/CalendarEntryItem";
import { TodayMemberCell } from "./TodayMemberCell";
import { HouseholdLogo } from "../../../../components/HouseholdLogo";
import { useIsMobile } from "../../../../hooks/useIsMobile";

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
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
  onMemberClick: (memberId: string) => void;
  /** Called when the user clicks the shared/household row to open the shared agenda. */
  onSharedClick?: () => void;
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
  onMemberClick,
  onSharedClick,
}: TodayBoardProps) {
  const { t, i18n } = useTranslation("today");
  const { t: tCommon } = useTranslation("common");
  const isMobile = useIsMobile();

  // One member expanded at a time (in place).
  const [expandedMemberId, setExpandedMemberId] = useState<string | null>(null);

  function handleMemberToggle(memberId: string) {
    setExpandedMemberId((prev) => (prev === memberId ? null : memberId));
  }

  // Swipe gesture: left = next day, right = prev day.
  const touchStartX = useRef<number | null>(null);

  function handleTouchStart(e: React.TouchEvent) {
    touchStartX.current = e.touches[0].clientX;
  }

  function handleTouchEnd(e: React.TouchEvent) {
    if (touchStartX.current === null) return;
    const dx = e.changedTouches[0].clientX - touchStartX.current;
    if (Math.abs(dx) > 50) {
      if (dx < 0) onNextDay();
      else onPrevDay();
    }
    touchStartX.current = null;
  }

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

  return (
    <div
      className="today-board"
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
    >
      {/* ---- Header with day navigation ---- */}
      <div className="today-board-header">
        {!isMobile ? (
          <button
            className="btn btn-ghost btn-sm"
            onClick={onPrevDay}
            type="button"
            aria-label={t("nav.prevDay")}
          >
            ‹
          </button>
        ) : (
          /* Swipe affordance: muted chevron hints, not interactive buttons */
          <span className="tp-swipe-hint" aria-hidden="true">‹</span>
        )}
        <div className="today-board-header-center">
          <span className="today-board-title">
            {isToday ? t("nav.today") : t("day.title")}
          </span>
          <span className="today-board-date">{dateLabel}</span>
        </div>
        <div className="today-board-header-right">
          {!isToday && (
            <button
              className="btn btn-ghost btn-sm"
              onClick={onToday}
              type="button"
            >
              {t("nav.today")}
            </button>
          )}
          {!isMobile ? (
            <button
              className="btn btn-ghost btn-sm"
              onClick={onNextDay}
              type="button"
              aria-label={t("nav.nextDay")}
            >
              ›
            </button>
          ) : (
            <span className="tp-swipe-hint" aria-hidden="true">›</span>
          )}
        </div>
      </div>

      {/* ---- No members yet ---- */}
      {members.length === 0 && (
        <p className="today-board-empty">{t("day.noMembers")}</p>
      )}

      {/* ---- Compact member snapshot grid (one cell per person) ---- */}
      {actorMembers.length > 0 && (
        <div className="tp-member-grid">
          {memberEntries.map(({ member, entries }) => (
            <TodayMemberCell
              key={member.memberId}
              memberId={member.memberId}
              name={member.name}
              entries={entries}
              isExpanded={expandedMemberId === member.memberId}
              onToggle={() => handleMemberToggle(member.memberId)}
              onMemberClick={onMemberClick}
              onItemClick={onItemClick}
            />
          ))}
        </div>
      )}

      {/* ---- Household section divider ---- */}
      <div className="tp-section-divider" aria-hidden="true">
        <span className="tp-section-label">{t("day.household")}</span>
      </div>

      {/* ---- Household row (shared / unassigned items) ---- */}
      <div className="tp-cell tp-cell--household">
        {/* Left zone: logo → navigates to shared agenda */}
        <div
          className="tp-cell-left"
          role={onSharedClick ? "button" : undefined}
          tabIndex={onSharedClick ? 0 : undefined}
          aria-label={onSharedClick ? t("day.household") : undefined}
          onClick={onSharedClick}
          onKeyDown={onSharedClick ? (e) => {
            if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onSharedClick(); }
          } : undefined}
        >
          <HouseholdLogo size={24} />
        </div>
        {/* Right zone: shared items */}
        <div className="tp-cell-right">
          {sharedDisplayState.isEmpty ? (
            <span className="tp-cell-empty">{t("day.nothingToday")}</span>
          ) : (
            <>
              {sharedDisplayState.activeItems.map((entry) => (
                <CalendarEntryItem
                  key={entry.id}
                  entry={entry}
                  onClick={() => onItemClick(entry.sourceType, entry.id)}
                />
              ))}
              {sharedDisplayState.completedItems.map((entry) => (
                <CalendarEntryItem
                  key={entry.id}
                  entry={entry}
                  onClick={() => onItemClick(entry.sourceType, entry.id)}
                />
              ))}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

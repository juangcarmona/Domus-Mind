import { useTranslation } from "react-i18next";
import type { CalendarEntry } from "../../utils/calendarEntry";
import { splitForDisplay } from "../../utils/todayPanelHelpers";
import { CalendarEntryItem } from "../shared/CalendarEntryItem";
import { useAppSelector } from "../../../../store/hooks";
import { MemberAvatar } from "../../../settings/components/avatar/MemberAvatar";

interface TodayMemberCellProps {
  memberId: string;
  name: string;
  entries: CalendarEntry[];
  /** Whether this member row is currently expanded (shows full list). */
  isExpanded: boolean;
  /** Called to toggle expanded state. Caller manages single-expanded invariant. */
  onToggle: () => void;
  onMemberClick: (memberId: string) => void;
  onItemClick: (sourceType: "event" | "task" | "routine", id: string) => void;
}

/**
 * Compact member row for the Today panel.
 *
 * Layout has two independent zones:
 *   Left  (tp-cell-left)  — avatar + name; tapping navigates to member agenda.
 *   Right (tp-cell-right) — entry chips; tapping expands/collapses the row.
 *
 * Collapsed: shows max 2 active items + "+N" overflow badge.
 * Expanded:  shows all active items, then completed items at low emphasis.
 *
 * Desktop: rendered as a card in the auto-fit grid (tp-member-grid).
 * Mobile:  rendered as a flat row with left/right zones side-by-side.
 */
export function TodayMemberCell({
  memberId,
  name,
  entries,
  isExpanded,
  onToggle,
  onMemberClick,
  onItemClick,
}: TodayMemberCellProps) {
  const { t } = useTranslation("today");
  const { visibleCollapsed, overflowCount, activeItems, completedItems, isEmpty } = splitForDisplay(entries);

  const householdMember = useAppSelector((s) =>
    s.household.members.find((m) => m.memberId === memberId),
  );
  const displayName = householdMember?.preferredName || name;

  return (
    <div className={`tp-cell${isExpanded ? " tp-cell--expanded" : ""}`}>
      {/* ---- Left zone: avatar + name → navigates to member agenda ---- */}
      <div
        className="tp-cell-left"
        role="button"
        tabIndex={0}
        aria-label={displayName}
        onClick={() => onMemberClick(memberId)}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            onMemberClick(memberId);
          }
        }}
      >
        <MemberAvatar
          initial={householdMember?.avatarInitial ?? displayName[0]?.toUpperCase() ?? "?"}
          avatarIconId={householdMember?.avatarIconId}
          avatarColorId={householdMember?.avatarColorId}
          size={26}
        />
        <span className="tp-cell-name" title={displayName}>{displayName}</span>
      </div>

      {/* ---- Right zone: entries — tap empty space or +N to expand ---- */}
      <div
        className="tp-cell-right"
        onClick={isEmpty ? undefined : (e) => {
          // Tapping an entry item opens the inspector; do not toggle expansion.
          if ((e.target as HTMLElement).closest(".wg-item")) return;
          onToggle();
        }}
        onKeyDown={isEmpty ? undefined : (e) => {
          if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onToggle(); }
        }}
        role={isEmpty ? undefined : "button"}
        tabIndex={isEmpty ? undefined : 0}
        aria-expanded={isExpanded}
      >
        {isEmpty ? (
          <span className="tp-cell-empty">{t("day.nothingToday")}</span>
        ) : isExpanded ? (
          /* ---- Expanded: full entry list ---- */
          <>
            {activeItems.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
            {completedItems.length > 0 && (
              <div className="tp-cell-completed-section">
                {completedItems.map((entry) => (
                  <CalendarEntryItem
                    key={entry.id}
                    entry={entry}
                    onClick={() => onItemClick(entry.sourceType, entry.id)}
                  />
                ))}
              </div>
            )}
          </>
        ) : (
          /* ---- Collapsed: max 2 items + overflow badge ---- */
          <>
            {visibleCollapsed.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
            {overflowCount > 0 && (
              <span
                className="tp-cell-overflow"
                role="button"
                tabIndex={0}
                onClick={(e) => { e.stopPropagation(); onToggle(); }}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onToggle(); }
                }}
              >
                +{overflowCount}
              </span>
            )}
          </>
        )}
      </div>
    </div>
  );
}


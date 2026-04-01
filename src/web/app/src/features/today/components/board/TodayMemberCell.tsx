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
  onMemberClick: (memberId: string) => void;
  onItemClick: (sourceType: "event" | "task" | "routine", id: string) => void;
}

/**
 * Compact member row for the Today panel.
 *
 * Layout has two independent zones:
 *   Left  (tp-cell-left)  — avatar + name; tapping navigates to member agenda.
 *   Right (tp-cell-right) — up to 2 entry chips; each tapping opens edit modal.
 *
 * Desktop: rendered as a card in the auto-fit grid (tp-member-grid).
 * Mobile:  rendered as a flat row with left/right zones side-by-side.
 */
export function TodayMemberCell({
  memberId,
  name,
  entries,
  onMemberClick,
  onItemClick,
}: TodayMemberCellProps) {
  const { t } = useTranslation("today");
  const { visibleCollapsed, overflowCount, isEmpty } = splitForDisplay(entries);

  const householdMember = useAppSelector((s) =>
    s.household.members.find((m) => m.memberId === memberId),
  );
  const displayName = householdMember?.preferredName || name;

  return (
    <div className="tp-cell">
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

      {/* ---- Right zone: entry chips ---- */}
      <div className="tp-cell-right">
        {isEmpty ? (
          <span className="tp-cell-empty">{t("day.nothingToday")}</span>
        ) : (
          <>
            {visibleCollapsed.map((entry) => (
              <CalendarEntryItem
                key={entry.id}
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            ))}
            {overflowCount > 0 && (
              <span className="tp-cell-overflow">+{overflowCount}</span>
            )}
          </>
        )}
      </div>
    </div>
  );
}


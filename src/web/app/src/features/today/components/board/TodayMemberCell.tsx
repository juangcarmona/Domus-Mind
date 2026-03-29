import { useTranslation } from "react-i18next";
import type { CalendarEntry } from "../../utils/calendarEntry";
import { splitForDisplay } from "../../utils/todayPanelHelpers";
import { CalendarEntryItem } from "../shared/CalendarEntryItem";

interface TodayMemberCellProps {
  memberId: string;
  name: string;
  entries: CalendarEntry[];
  onMemberClick: (memberId: string) => void;
  onItemClick: (sourceType: "event" | "task" | "routine", id: string) => void;
}

/**
 * Compact, non-expandable snapshot cell for a single household member.
 *
 * Shows up to 2 entries (collapsed view from splitForDisplay).
 * Clicking the cell navigates to the member agenda page (via onMemberClick).
 * Clicking an entry item opens the entity edit modal without triggering navigation.
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

  return (
    <div
      className="tp-cell"
      role="button"
      tabIndex={0}
      aria-label={name}
      onClick={() => onMemberClick(memberId)}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onMemberClick(memberId);
        }
      }}
    >
      <div className="tp-cell-header">
        <span className="tp-cell-name">{name}</span>
        {overflowCount > 0 && (
          <span className="tp-cell-overflow">+{overflowCount}</span>
        )}
      </div>

      <div className="tp-cell-entries">
        {isEmpty ? (
          <span className="tp-cell-empty">{t("day.nothingToday")}</span>
        ) : (
          visibleCollapsed.map((entry) => (
            // Stop propagation so entry clicks open the edit modal
            // rather than also triggering member navigation.
            <div
              key={entry.id}
              className="tp-cell-entry-wrap"
              onClick={(e) => e.stopPropagation()}
            >
              <CalendarEntryItem
                entry={entry}
                onClick={() => onItemClick(entry.sourceType, entry.id)}
              />
            </div>
          ))
        )}
      </div>
    </div>
  );
}

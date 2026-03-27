import { useTranslation } from "react-i18next";
import type { TodayEntry } from "../../utils/todayPanelHelpers";
import { splitForDisplay } from "../../utils/todayPanelHelpers";
import { TodayPanelItem } from "./TodayPanelItem";

interface TodayMemberCardProps {
  name: string;
  entries: TodayEntry[];
  isExpanded: boolean;
  onToggle: () => void;
  onItemClick: (sourceType: "event" | "task" | "routine", id: string) => void;
}

export function TodayMemberCard({
  name,
  entries,
  isExpanded,
  onToggle,
  onItemClick,
}: TodayMemberCardProps) {
  const { t } = useTranslation("today");
  const { visibleCollapsed, overflowCount, activeItems, completedItems, isEmpty } =
    splitForDisplay(entries);

  return (
    <div
      className={`tp-card${isExpanded ? " tp-card--expanded" : ""}`}
      aria-expanded={isExpanded}
    >
      {/* ---- Card header (tap to toggle) ---- */}
      <button className="tp-card-header" onClick={onToggle} type="button">
        <span className="tp-card-name">{name}</span>
        {overflowCount > 0 && !isExpanded && (
          <span className="tp-card-overflow">+{overflowCount}</span>
        )}
        <span className="tp-card-chevron" aria-hidden="true">
          {isExpanded ? "▾" : "▸"}
        </span>
      </button>

      {/* ---- Collapsed view ---- */}
      {!isExpanded && (
        <div className="tp-card-collapsed">
          {isEmpty ? (
            <span className="tp-card-empty">{t("day.nothingToday")}</span>
          ) : (
            <div className="tp-card-inline">
              {visibleCollapsed.map((entry) => (
                <TodayPanelItem
                  key={entry.id}
                  entry={entry}
                  onClick={() => onItemClick(entry.sourceType, entry.id)}
                />
              ))}
              {overflowCount > 0 && (
                <span className="tp-card-more">+{overflowCount}</span>
              )}
            </div>
          )}
        </div>
      )}

      {/* ---- Expanded view ---- */}
      {isExpanded && (
        <div className="tp-card-expanded-body">
          {isEmpty ? (
            <span className="tp-card-empty">{t("day.nothingToday")}</span>
          ) : (
            <>
              <div className="tp-card-active-list">
                {activeItems.map((entry) => (
                  <TodayPanelItem
                    key={entry.id}
                    entry={entry}
                    onClick={() => onItemClick(entry.sourceType, entry.id)}
                  />
                ))}
              </div>
              {completedItems.length > 0 && (
                <div className="tp-card-completed-list" aria-label={t("day.completedSection")}>
                  {completedItems.map((entry) => (
                    <TodayPanelItem
                      key={entry.id}
                      entry={entry}
                      onClick={() => onItemClick(entry.sourceType, entry.id)}
                    />
                  ))}
                </div>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
}

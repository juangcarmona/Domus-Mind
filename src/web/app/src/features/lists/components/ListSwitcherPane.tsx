import { useTranslation } from "react-i18next";
import type { SharedListSummary } from "../../../api/types/listTypes";

interface ListSwitcherPaneProps {
  lists: SharedListSummary[];
  activeListId: string | null;
  areaNamesById?: Record<string, string>;
  onSelect: (listId: string) => void;
  onNewList: () => void;
}

/**
 * ListSwitcherPane — compact left pane showing all household lists.
 *
 * Each row shows: list name, unchecked count, and optional area/plan cues.
 * Active list is visually indicated.
 * Designed for desktop desktop left rail pattern.
 *
 * On mobile, this pane is hidden and replaced by a sheet trigger in the surface header.
 */
export function ListSwitcherPane({
  lists,
  activeListId,
  areaNamesById,
  onSelect,
  onNewList,
}: ListSwitcherPaneProps) {
  const { t } = useTranslation("lists");

  return (
    <aside className="list-switcher-pane">
      <div className="list-switcher-header">
        <button
          type="button"
          className="list-switcher-new-btn"
          onClick={onNewList}
          aria-label={t("newList")}
          title={t("newList")}
        >
          +
        </button>
      </div>

      {lists.length === 0 ? (
        <p className="list-switcher-empty">{t("emptyHeadline")}</p>
      ) : (
        <ul className="list-switcher-list" role="listbox" aria-label={t("title")}>
          {lists.map((list) => {
            const allDone = list.itemCount > 0 && list.uncheckedCount === 0;
            const sub = list.itemCount === 0
              ? null
              : allDone
              ? t("allDone")
              : t("uncheckedCount", { count: list.uncheckedCount });

            const areaCue = list.areaId ? areaNamesById?.[list.areaId] : null;
            const showPlanCue = list.linkedEntityType?.toLowerCase() === "event";
            return (
              <li key={list.id}>
                <button
                  type="button"
                  role="option"
                  aria-selected={list.id === activeListId}
                  className={`list-switcher-row${list.id === activeListId ? " list-switcher-row--active" : ""}`}
                  onClick={() => onSelect(list.id)}
                >
                  <span className="list-switcher-row-name">{list.name}</span>
                  {sub && (
                    <span className="list-switcher-row-sub">{sub}</span>
                  )}
                  {(areaCue || showPlanCue) && (
                    <span className="list-switcher-row-cues">
                      {areaCue && <span className="list-switcher-row-cue">{areaCue}</span>}
                      {showPlanCue && <span className="list-switcher-row-cue">{t("planLabel")}</span>}
                    </span>
                  )}
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </aside>
  );
}

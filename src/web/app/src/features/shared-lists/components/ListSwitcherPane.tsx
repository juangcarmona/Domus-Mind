import { useTranslation } from "react-i18next";
import type { SharedListSummary } from "../../../api/types/sharedListTypes";

interface ListSwitcherPaneProps {
  lists: SharedListSummary[];
  activeListId: string | null;
  onSelect: (listId: string) => void;
  onNewList: () => void;
}

/**
 * ListSwitcherPane — compact left pane showing all household lists.
 *
 * Each row shows: color marker, list name, unchecked count.
 * Active list is visually indicated.
 * Designed for desktop desktop left rail pattern.
 *
 * On mobile, this pane is hidden and replaced by a sheet trigger in the surface header.
 */
export function ListSwitcherPane({
  lists,
  activeListId,
  onSelect,
  onNewList,
}: ListSwitcherPaneProps) {
  const { t } = useTranslation("sharedLists");

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
          {lists.map((list) => (
            <li key={list.id}>
              <button
                type="button"
                role="option"
                aria-selected={list.id === activeListId}
                className={`list-switcher-row${list.id === activeListId ? " list-switcher-row--active" : ""}`}
                onClick={() => onSelect(list.id)}
              >
                <span className="list-switcher-row-name">{list.name}</span>
                {list.uncheckedCount > 0 && (
                  <span className="list-switcher-row-count">
                    {list.uncheckedCount}
                  </span>
                )}
                {list.uncheckedCount === 0 && list.itemCount > 0 && (
                  <span className="list-switcher-row-done" aria-label={t("allDone")}>✓</span>
                )}
              </button>
            </li>
          ))}
        </ul>
      )}
    </aside>
  );
}

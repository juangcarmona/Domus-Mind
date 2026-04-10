import { useTranslation } from "react-i18next";
import type { SharedListSummary } from "../../../api/types/listTypes";

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
              ? "all done"
              : `${list.uncheckedCount} remaining`;
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
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </aside>
  );
}

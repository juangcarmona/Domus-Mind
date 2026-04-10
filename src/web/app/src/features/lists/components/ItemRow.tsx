import { useTranslation } from "react-i18next";
import { useAppDispatch } from "../../../store/hooks";
import {
  optimisticRemoveItem,
  removeSharedListItem,
} from "../../../store/listsSlice";
import type { SharedListItemDetail } from "../../../api/types/listTypes";

interface ItemRowProps {
  item: SharedListItemDetail;
  listId: string;
  dragHandleProps?: Record<string, unknown>;
  reorderMode?: boolean;
  selectedItemId?: string | null;
  onSelect?: (item: SharedListItemDetail, listId: string) => void;
  onToggle?: (item: SharedListItemDetail) => void;
}

export function ItemRow({
  item,
  listId,
  dragHandleProps,
  reorderMode = false,
  selectedItemId,
  onSelect,
  onToggle,
}: ItemRowProps) {
  const { t } = useTranslation("lists");
  const dispatch = useAppDispatch();
  const isSelected = selectedItemId === item.itemId;

  function handleRowClick() {
    if (reorderMode) return;
    onSelect?.(item, listId);
  }

  function handleCheckboxClick(e: React.MouseEvent) {
    e.stopPropagation();
    onToggle?.(item);
  }

  function handleRemove(e: React.MouseEvent) {
    e.stopPropagation();
    dispatch(optimisticRemoveItem({ itemId: item.itemId }));
    dispatch(removeSharedListItem({ listId, itemId: item.itemId }));
  }

  return (
    <div
      className={`shared-list-item${item.checked ? " shared-list-item--checked" : ""}${isSelected ? " shared-list-item--selected" : ""}`}
    >
      {/* Main row */}
      <div className="shared-list-item__main" onClick={handleRowClick}>
        {!item.checked && dragHandleProps && (
          <button
            type="button"
            className="shared-list-item__drag-handle"
            aria-label={t("dragToReorder")}
            tabIndex={-1}
            onClick={(e) => e.stopPropagation()}
            {...dragHandleProps}
          >
            <span className="shared-list-item__drag-icon" aria-hidden="true">⠿</span>
          </button>
        )}

        <button
          type="button"
          className="shared-list-item__checkbox-btn"
          aria-label={item.checked ? t("uncheckItem") : t("checkItem")}
          onClick={handleCheckboxClick}
          tabIndex={-1}
        >
          <input
            type="checkbox"
            className="shared-list-item__checkbox"
            checked={item.checked}
            readOnly
            aria-label={item.name}
            tabIndex={-1}
          />
        </button>

        <div className="shared-list-item__body">
          <span className="shared-list-item__name">{item.name}</span>
          {(item.quantity || item.note) && (
            <span className="shared-list-item__meta-hint">
              {item.quantity && <span className="shared-list-item__tag">{item.quantity}</span>}
              {item.note && <span className="shared-list-item__note-preview">{item.note}</span>}
            </span>
          )}
        </div>

        <button
          type="button"
          className="shared-list-item__remove"
          onClick={handleRemove}
          aria-label={t("removeItem")}
          tabIndex={-1}
        >
          ×
        </button>
      </div>
    </div>
  );
}

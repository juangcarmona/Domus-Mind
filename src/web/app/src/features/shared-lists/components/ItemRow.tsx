import { useState, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch } from "../../../store/hooks";
import {
  optimisticToggleItem,
  optimisticRenameItem,
  optimisticRemoveItem,
  updateSharedListItem,
  toggleSharedListItem,
  removeSharedListItem,
} from "../../../store/sharedListsSlice";
import type { SharedListItemDetail } from "../../../api/types/sharedListTypes";

interface ItemRowProps {
  item: SharedListItemDetail;
  listId: string;
  dragHandleProps?: Record<string, unknown>;
  reorderMode?: boolean;
}

export function ItemRow({ item, listId, dragHandleProps, reorderMode = false }: ItemRowProps) {
  const { t } = useTranslation("sharedLists");
  const dispatch = useAppDispatch();
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(item.name);
  const [expanded, setExpanded] = useState(false);
  const [qtyDraft, setQtyDraft] = useState(item.quantity ?? "");
  const [noteDraft, setNoteDraft] = useState(item.note ?? "");
  const [savingMeta, setSavingMeta] = useState(false);

  const hasDetails = !!(item.quantity || item.note);

  function startEditing(e: React.MouseEvent) {
    e.stopPropagation();
    setDraft(item.name);
    setEditing(true);
  }

  function commitRename() {
    const trimmed = draft.trim();
    if (trimmed && trimmed !== item.name) {
      dispatch(optimisticRenameItem({ itemId: item.itemId, name: trimmed }));
      dispatch(updateSharedListItem({ listId, itemId: item.itemId, name: trimmed, quantity: item.quantity, note: item.note }));
    }
    setEditing(false);
  }

  function cancelRename() {
    setDraft(item.name);
    setEditing(false);
  }

  function handleRenameKey(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter") { e.preventDefault(); commitRename(); }
    else if (e.key === "Escape") { e.preventDefault(); cancelRename(); }
  }

  function handleRowClick() {
    if (editing || expanded || reorderMode) return;
    dispatch(optimisticToggleItem({ itemId: item.itemId }));
    dispatch(toggleSharedListItem({ listId, itemId: item.itemId }));
  }

  function handleRemove(e: React.MouseEvent) {
    e.stopPropagation();
    dispatch(optimisticRemoveItem({ itemId: item.itemId }));
    dispatch(removeSharedListItem({ listId, itemId: item.itemId }));
  }

  async function commitMeta() {
    const qty = qtyDraft.trim() || null;
    const note = noteDraft.trim() || null;
    if (qty !== item.quantity || note !== item.note) {
      setSavingMeta(true);
      await dispatch(updateSharedListItem({ listId, itemId: item.itemId, name: item.name, quantity: qty, note }));
      setSavingMeta(false);
    }
    setExpanded(false);
  }

  function handleMetaKey(e: KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) {
    if (e.key === "Escape") { e.preventDefault(); setExpanded(false); }
    if (e.key === "Enter" && e.ctrlKey) { e.preventDefault(); commitMeta(); }
  }

  function toggleExpand(e: React.MouseEvent) {
    e.stopPropagation();
    if (!expanded) {
      setQtyDraft(item.quantity ?? "");
      setNoteDraft(item.note ?? "");
    } else {
      commitMeta();
      return;
    }
    setExpanded(true);
  }

  return (
    <div
      className={`shared-list-item${item.checked ? " shared-list-item--checked" : ""}${editing ? " shared-list-item--editing" : ""}${expanded ? " shared-list-item--expanded" : ""}`}
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

        <input
          type="checkbox"
          className="shared-list-item__checkbox"
          checked={item.checked}
          readOnly
          aria-label={item.name}
          tabIndex={-1}
        />

        {editing ? (
          <input
            className="shared-list-item__rename-input"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={handleRenameKey}
            onBlur={commitRename}
            autoFocus
            aria-label={t("renameItem")}
            onClick={(e) => e.stopPropagation()}
          />
        ) : (
          <span
            className="shared-list-item__name"
            onClick={startEditing}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
              if (e.key === "Enter") { e.preventDefault(); startEditing(e as unknown as React.MouseEvent); }
            }}
          >
            {item.name}
          </span>
        )}

        {(hasDetails || !item.checked) && !editing && (
          <button
            type="button"
            className={`shared-list-item__expand${expanded ? " shared-list-item__expand--open" : ""}`}
            onClick={toggleExpand}
            aria-label={t("expandItem")}
            tabIndex={-1}
          >
            {hasDetails && !expanded ? "•••" : "▾"}
          </button>
        )}

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

      {/* Detail preview row (collapsed but has data) */}
      {hasDetails && !expanded && (
        <div className="shared-list-item__meta-preview" onClick={toggleExpand}>
          {item.quantity && <span className="shared-list-item__tag">{item.quantity}</span>}
          {item.note && <span className="shared-list-item__note-preview">{item.note}</span>}
        </div>
      )}

      {/* Expanded detail edit row */}
      {expanded && (
        <div className="shared-list-item__detail" onClick={(e) => e.stopPropagation()}>
          <input
            className="shared-list-item__detail-input"
            placeholder={t("quantityPlaceholder")}
            value={qtyDraft}
            onChange={(e) => setQtyDraft(e.target.value)}
            onKeyDown={handleMetaKey}
            aria-label={t("quantityLabel")}
          />
          <textarea
            className="shared-list-item__detail-textarea"
            placeholder={t("notePlaceholder")}
            value={noteDraft}
            onChange={(e) => setNoteDraft(e.target.value)}
            onKeyDown={handleMetaKey}
            rows={2}
            aria-label={t("noteLabel")}
          />
          <div className="shared-list-item__detail-actions">
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              onClick={commitMeta}
              disabled={savingMeta}
            >
              {savingMeta ? "…" : t("saveDetails")}
            </button>
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              onClick={() => setExpanded(false)}
            >
              {t("cancel")}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

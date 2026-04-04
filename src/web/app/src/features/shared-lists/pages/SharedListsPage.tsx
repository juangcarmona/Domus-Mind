// Phase 3: SharedListsPage is the canonical split-view Shared Lists surface.
// It replaces the old index-only page and absorbs the detail view from SharedListDetailPage.
// /lists       — enters surface, auto-selects first list
// /lists/:listId — deep-link entry, pre-selects the specified list

import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchFamilySharedLists,
  fetchSharedListDetail,
  addItemToSharedList,
  optimisticReorderItems,
  reorderSharedListItems,
  clearDetail,
  renameSharedList,
  deleteSharedList,
  optimisticToggleItem,
  toggleSharedListItem,
} from "../../../store/sharedListsSlice";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { ListSwitcherPane } from "../components/ListSwitcherPane";
import { CreateSharedListModal } from "../components/CreateSharedListModal";
import { SortableItemRow } from "../components/SortableItemRow";
import { ItemRow } from "../components/ItemRow";
import { EditEntityModal } from "../../editors/components/EditEntityModal";
import type { SharedListItemDetail } from "../../../api/types/sharedListTypes";
import {
  DndContext,
  closestCenter,
  type DragEndEvent,
} from "@dnd-kit/core";
import {
  SortableContext,
  verticalListSortingStrategy,
  arrayMove,
} from "@dnd-kit/sortable";
import "../shared-lists.css";

/** Item selected for inspector / bottom-sheet detail */
interface SelectedItem {
  item: SharedListItemDetail;
  listId: string;
}

export function SharedListsPage() {
  // Route param: may be undefined for /lists, or a listId for /lists/:listId
  const { listId: routeListId } = useParams<{ listId?: string }>();

  const { t } = useTranslation("sharedLists");
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const isMobile = useIsMobile();

  const familyId = useAppSelector((s) => s.household.family?.familyId);
  const lists = useAppSelector((s) => s.sharedLists.lists);
  const listsStatus = useAppSelector((s) => s.sharedLists.listsStatus);
  const detail = useAppSelector((s) => s.sharedLists.detail);
  const detailStatus = useAppSelector((s) => s.sharedLists.detailStatus);

  // Active list id: prefer route param on first mount, then switcher selection
  const [activeListId, setActiveListId] = useState<string | null>(routeListId ?? null);

  // Surface-level UI state
  const [showCreate, setShowCreate] = useState(false);
  const [showSwitcherSheet, setShowSwitcherSheet] = useState(false);
  const [checkedCollapsed, setCheckedCollapsed] = useState(false);
  const [addItemName, setAddItemName] = useState("");
  const [addError, setAddError] = useState<string | null>(null);
  const [renameDraft, setRenameDraft] = useState<string | null>(null);
  const [renameError, setRenameError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [showLinkedEvent, setShowLinkedEvent] = useState(false);
  const [selectedItem, setSelectedItem] = useState<SelectedItem | null>(null);
  const [reorderMode, setReorderMode] = useState(false);
  const [showAddComposer, setShowAddComposer] = useState(false);

  const addInputRef = useRef<HTMLInputElement>(null);

  // ---- Load list index ----
  useEffect(() => {
    if (familyId) {
      dispatch(fetchFamilySharedLists(familyId));
    }
  }, [familyId, dispatch]);

  // ---- Auto-select first list if none active and lists are loaded ----
  useEffect(() => {
    if (activeListId === null && lists.length > 0) {
      setActiveListId(lists[0].id);
    }
  }, [lists, activeListId]);

  // ---- Sync route param if it changes (deep-link navigation) ----
  useEffect(() => {
    if (routeListId && routeListId !== activeListId) {
      setActiveListId(routeListId);
    }
  }, [routeListId]); // eslint-disable-line react-hooks/exhaustive-deps

  // ---- Load detail when active list changes ----
  useEffect(() => {
    if (activeListId) {
      dispatch(fetchSharedListDetail(activeListId));
      setCheckedCollapsed(false);
      setAddItemName("");
      setAddError(null);
      setRenameDraft(null);
      setRenameError(null);
      setSelectedItem(null);
      setReorderMode(false);
      setShowAddComposer(false);
    } else {
      dispatch(clearDetail());
    }
  }, [activeListId, dispatch]);

  // ---- Switcher selection ----
  function handleSelectList(listId: string) {
    setActiveListId(listId);
    setShowSwitcherSheet(false);
    setShowAddComposer(false);
    setReorderMode(false);
    // When a deep-link brought us here, collapse the route back to /lists
    if (routeListId) {
      navigate("/lists", { replace: true });
    }
  }

  // ---- Add item ----
  async function handleAddKey(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Escape") {
      e.preventDefault();
      setShowAddComposer(false);
      setAddItemName("");
      setAddError(null);
      return;
    }
    if (e.key !== "Enter") return;
    const name = addItemName.trim();
    if (!name || !activeListId) return;
    setAddError(null);
    setAddItemName("");
    const result = await dispatch(addItemToSharedList({ listId: activeListId, name }));
    if (!addItemToSharedList.fulfilled.match(result)) {
      setAddError((result.payload as string) ?? t("addError"));
    }
    addInputRef.current?.focus();
  }

  // ---- Rename list ----
  async function handleRenameCommit() {
    const trimmed = (renameDraft ?? "").trim();
    if (!trimmed || !activeListId || trimmed === detail?.name) {
      setRenameDraft(null);
      return;
    }
    setRenameError(null);
    const result = await dispatch(renameSharedList({ listId: activeListId, name: trimmed }));
    if (renameSharedList.fulfilled.match(result)) {
      setRenameDraft(null);
    } else {
      setRenameError((result.payload as string) ?? t("renameError"));
    }
  }

  // ---- Delete list ----
  async function handleDelete() {
    if (!detail || !activeListId) return;
    const confirmed = window.confirm(t("deleteConfirm", { name: detail.name }));
    if (!confirmed) return;
    setDeleting(true);
    const result = await dispatch(deleteSharedList(activeListId));
    if (deleteSharedList.fulfilled.match(result)) {
      setActiveListId(null); // will auto-select first remaining list
    } else {
      setDeleting(false);
    }
  }

  // ---- Drag-and-drop reorder ----
  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    if (!over || active.id === over.id || !activeListId) return;
    const ids = unchecked.map((i) => i.itemId);
    const oldIndex = ids.indexOf(String(active.id));
    const newIndex = ids.indexOf(String(over.id));
    if (oldIndex === -1 || newIndex === -1) return;
    const reordered = arrayMove(ids, oldIndex, newIndex);
    dispatch(optimisticReorderItems({ itemIds: reordered }));
    dispatch(reorderSharedListItems({ listId: activeListId, itemIds: reordered }));
  }

  // ---- Item toggle (from inspector/sheet) ----
  function handleItemToggle(item: SharedListItemDetail) {
    if (!activeListId) return;
    dispatch(optimisticToggleItem({ itemId: item.itemId }));
    dispatch(toggleSharedListItem({ listId: activeListId, itemId: item.itemId }));
  }

  // ---- Derived data ----
  const sorted = detail ? [...detail.items].sort((a, b) => a.order - b.order) : [];
  const unchecked = sorted.filter((i) => !i.checked);
  const checked = sorted.filter((i) => i.checked);
  const activeListSummary = lists.find((l) => l.id === activeListId) ?? null;

  // ---- Inspector / sheet content ----
  const inspectorContent = selectedItem ? (
    <div className="lists-inspector-content">
      <p className="lists-inspector-item-name">{selectedItem.item.name}</p>
      {selectedItem.item.quantity && (
        <p className="lists-inspector-meta">
          <span className="lists-inspector-label">{t("quantityLabel")}</span>{" "}
          {selectedItem.item.quantity}
        </p>
      )}
      {selectedItem.item.note && (
        <p className="lists-inspector-meta">
          <span className="lists-inspector-label">{t("noteLabel")}</span>{" "}
          {selectedItem.item.note}
        </p>
      )}
      <div className="lists-inspector-actions">
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={() => handleItemToggle(selectedItem.item)}
        >
          {selectedItem.item.checked ? "☑ Checked" : "☐ Unchecked"}
        </button>
      </div>
    </div>
  ) : (
    <div className="lists-inspector-hint">
      {detail && detail.listId === activeListId ? (
        <>
          <span className="lists-inspector-stat">
            {unchecked.length} remaining
          </span>
          {checked.length > 0 && (
            <span className="lists-inspector-stat">{checked.length} done</span>
          )}
          <span className="lists-inspector-hint-text">Select an item for details</span>
        </>
      ) : (
        <span className="lists-inspector-stat">No list selected</span>
      )}
    </div>
  );

  return (
    <div className="lists-surface l-surface">
      {/* ── Mobile header: switcher trigger only ── */}
      {isMobile && (
        <header className="lists-surface-header">
          <button
            type="button"
            className="lists-switcher-trigger"
            onClick={(e) => { e.stopPropagation(); setShowSwitcherSheet(true); }}
          >
            <span className="lists-switcher-trigger-name">
              {activeListSummary ? activeListSummary.name : t("title")}
            </span>
            {activeListSummary && activeListSummary.uncheckedCount > 0 && (
              <span className="lists-list-count">{activeListSummary.uncheckedCount}</span>
            )}
            <span className="lists-switcher-trigger-chevron" aria-hidden="true">▾</span>
          </button>
        </header>
      )}

      {/* ── Surface body: switcher | content | inspector ── */}
      <div className="lists-surface-body l-surface-body">
        {/* Left switcher pane — desktop only, hidden on mobile */}
        {!isMobile && (
          <ListSwitcherPane
            lists={lists}
            activeListId={activeListId}
            onSelect={handleSelectList}
            onNewList={() => setShowCreate(true)}
          />
        )}

        {/* Main content pane */}
        <div className="lists-content-pane l-surface-content">
          {listsStatus === "loading" && lists.length === 0 && (
            <div className="loading-wrap">{t("loading")}</div>
          )}

          {listsStatus !== "loading" && lists.length === 0 && (
            <div className="lists-surface-empty">
              <p className="empty-state-headline">{t("emptyHeadline")}</p>
              <p>{t("emptyHint")}</p>
              <button className="btn" onClick={() => setShowCreate(true)}>
                {t("newList")}
              </button>
            </div>
          )}

          {/* Active list content */}
          {activeListId && (
            <>
              {detailStatus === "loading" && !detail && (
                <div className="loading-wrap">{t("loadingDetail")}</div>
              )}

              {detail && detail.listId === activeListId && (
                <>
                  {/* Desktop: list title header inside content pane */}
                  {!isMobile && (
                    <div className="lists-list-header">
                      <div className="lists-list-header-main">
                        {renameDraft !== null ? (
                          <input
                            className="lists-list-rename-input"
                            value={renameDraft}
                            onChange={(e) => setRenameDraft(e.target.value)}
                            onBlur={handleRenameCommit}
                            onKeyDown={(e) => {
                              if (e.key === "Enter") { e.preventDefault(); handleRenameCommit(); }
                              else if (e.key === "Escape") { e.preventDefault(); setRenameDraft(null); }
                            }}
                            autoFocus
                            aria-label={t("renameList")}
                          />
                        ) : (
                          <button
                            type="button"
                            className="lists-list-name-btn"
                            onClick={() => !reorderMode && setRenameDraft(detail.name)}
                            title={reorderMode ? undefined : t("renameList")}
                          >
                            {detail.name}
                            {activeListSummary && activeListSummary.uncheckedCount > 0 && (
                              <span className="lists-list-count">{activeListSummary.uncheckedCount}</span>
                            )}
                          </button>
                        )}
                        {renameError && <p className="error-msg" style={{ margin: 0 }}>{renameError}</p>}
                      </div>
                      <div className="lists-list-header-actions">
                        {!reorderMode && detail.linkedEntityDisplayName && (
                          <button
                            type="button"
                            className="btn btn-ghost btn-sm"
                            onClick={() => setShowLinkedEvent(true)}
                          >
                            {t("goToLinkedEvent")}
                          </button>
                        )}
                        <button
                          type="button"
                          className={`lists-reorder-btn${reorderMode ? " lists-reorder-btn--active" : ""}`}
                          onClick={() => setReorderMode((m) => !m)}
                          title={reorderMode ? t("exitReorder") : t("reorderMode")}
                        >
                          {reorderMode ? t("exitReorder") : "⠿"}
                        </button>
                        {!reorderMode && (
                          <button
                            type="button"
                            className="btn btn-ghost btn-sm lists-delete-btn"
                            onClick={handleDelete}
                            disabled={deleting}
                            title={t("deleteList")}
                          >
                            {deleting ? "…" : "···"}
                          </button>
                        )}
                      </div>
                    </div>
                  )}
                  <div className={`lists-active-body${reorderMode ? " lists-reorder-mode" : ""}`}>
                  <div className="shared-list-items-wrap">
                    {reorderMode && (
                      <div className="lists-reorder-banner">{t("reorderHint")}</div>
                    )}
                    <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
                      <SortableContext
                        items={unchecked.map((i) => i.itemId)}
                        strategy={verticalListSortingStrategy}
                      >
                        {unchecked.map((item) => (
                          <SortableItemRow
                            key={item.itemId}
                            item={item}
                            listId={detail.listId}
                            reorderMode={reorderMode}
                          />
                        ))}
                      </SortableContext>
                    </DndContext>

                    {sorted.length === 0 && (
                      <p className="shared-list-empty-hint">{t("noItems")}</p>
                    )}

                    {checked.length > 0 && (
                      <>
                        <button
                          type="button"
                          className="shared-list-section-label shared-list-section-toggle"
                          onClick={() => setCheckedCollapsed((c) => !c)}
                        >
                          {checkedCollapsed
                            ? t("expandChecked", { count: checked.length })
                            : t("collapseChecked")}
                        </button>
                        {!checkedCollapsed && checked.map((item) => (
                          <ItemRow
                            key={item.itemId}
                            item={item}
                            listId={detail.listId}
                          />
                        ))}
                      </>
                    )}

                    {addError && (
                      <p className="error-msg" style={{ padding: "0 0.75rem 0.5rem" }}>
                        {addError}
                      </p>
                    )}
                  </div>

                  {detail.linkedEntityDisplayName && (
                    <div className="shared-list-linked-info">
                      {t("linkedTo", { name: detail.linkedEntityDisplayName })}
                    </div>
                  )}
                  </div>

                  {/* Inline add composer — sticky bottom of content pane */}
                  {showAddComposer && (
                    <div className="lists-add-composer">
                      <input
                        ref={addInputRef}
                        type="text"
                        className="lists-add-composer-input"
                        placeholder={t("addItemPlaceholder")}
                        value={addItemName}
                        onChange={(e) => setAddItemName(e.target.value)}
                        onKeyDown={handleAddKey}
                        autoFocus
                        aria-label={t("addItem")}
                      />
                      <button
                        type="button"
                        className="lists-add-composer-close"
                        onClick={() => { setShowAddComposer(false); setAddItemName(""); setAddError(null); }}
                        aria-label={t("cancel")}
                      >
                        ✕
                      </button>
                    </div>
                  )}
                </>
              )}
            </>
          )}
        </div>

        {/* Desktop inspector — hidden on mobile via InspectorPanel.css */}
        <InspectorPanel title={selectedItem?.item.name}>
          {inspectorContent}
        </InspectorPanel>
      </div>

      {/* FAB — add item (hidden in reorder mode or when composer is open) */}
      {activeListId && !reorderMode && !showAddComposer && (
        <button
          type="button"
          className="lists-fab"
          aria-label={t("addItem")}
          onClick={() => setShowAddComposer(true)}
        >
          +
        </button>
      )}

      {/* Mobile: list switcher sheet */}
      {isMobile && (
        <BottomSheetDetail
          open={showSwitcherSheet}
          onClose={() => setShowSwitcherSheet(false)}
          title={t("title")}
        >
          <div className="lists-switcher-sheet-body">
            {listsStatus === "loading" && lists.length === 0 ? (
              <p className="lists-switcher-sheet-loading">{t("loading")}</p>
            ) : (
              lists.map((list) => (
                <button
                  key={list.id}
                  type="button"
                  className={`lists-switcher-sheet-row${list.id === activeListId ? " lists-switcher-sheet-row--active" : ""}`}
                  onClick={() => handleSelectList(list.id)}
                >
                  <span className="lists-switcher-sheet-name">{list.name}</span>
                  {list.uncheckedCount > 0 && (
                    <span className="lists-switcher-sheet-count">{list.uncheckedCount}</span>
                  )}
                </button>
              ))
            )}
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              style={{ marginTop: "0.75rem", width: "100%" }}
              onClick={() => { setShowSwitcherSheet(false); setShowCreate(true); }}
            >
              + {t("newList")}
            </button>
          </div>
        </BottomSheetDetail>
      )}

      {/* Create list modal */}
      {showCreate && (
        <CreateSharedListModal onClose={() => setShowCreate(false)} />
      )}

      {/* Linked event modal */}
      {showLinkedEvent && detail?.linkedEntityId && (
        <EditEntityModal
          type="event"
          id={detail.linkedEntityId}
          onClose={() => setShowLinkedEvent(false)}
        />
      )}
    </div>
  );
}

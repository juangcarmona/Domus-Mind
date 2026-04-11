// Lists surface — canonical split-view surface for household lists.
// /lists         — enters surface, auto-selects first list
// /lists/:listId — deep-link entry, pre-selects the specified list

import { useEffect, useRef, useState, useCallback, type KeyboardEvent } from "react";
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
  optimisticRenameItem,
  updateSharedListItem,
  setItemImportance,
  optimisticSetImportance,
  setItemTemporal,
  optimisticSetTemporal,
  clearItemTemporal,
  optimisticClearTemporal,
  removeSharedListItem,
  optimisticRemoveItem,
} from "../../../store/listsSlice";
import { fetchAreas } from "../../../store/areasSlice";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { ContextChip } from "../../../components/ContextChip";
import { ListSwitcherPane } from "../components/ListSwitcherPane";
import { CreateListModal } from "../components/CreateListModal";
import { SortableItemRow } from "../components/SortableItemRow";
import { ItemRow } from "../components/ItemRow";
import { EditEntityModal } from "../../editors/components/EditEntityModal";
import type { SharedListItemDetail } from "../../../api/types/listTypes";
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
import "../lists.css";

// ─── Item selection container ────────────────────────────────────
interface SelectedItem {
  item: SharedListItemDetail;
  listId: string;
}

// ─── View mode ───────────────────────────────────────────────────
type ViewMode = "list" | "grid";

// ─── Temporal helpers ────────────────────────────────────────────
function toLocalInput(iso: string | null): string {
  if (!iso) return "";
  try {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  } catch {
    return "";
  }
}

function fromLocalInput(localStr: string): string | null {
  if (!localStr) return null;
  try {
    return new Date(localStr).toISOString();
  } catch {
    return null;
  }
}

// ─── Recurrence helpers ──────────────────────────────────────────
// Canonical format: "Daily" | "Weekly:1,3,5" | "Monthly" | "Yearly"
// Days of week: 0=Sun, 1=Mon … 6=Sat
const WEEK_DAYS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"] as const;

function parseRepeat(val: string | null): { freq: string; days: number[] } {
  if (!val) return { freq: "", days: [] };
  const [freq, daysPart] = val.split(":");
  const days = daysPart
    ? daysPart.split(",").map(Number).filter((d) => d >= 0 && d <= 6)
    : [];
  return { freq, days };
}

function serializeRepeat(freq: string, days: number[]): string | null {
  if (!freq) return null;
  if (freq === "Weekly" && days.length > 0) {
    return `Weekly:${[...days].sort((a, b) => a - b).join(",")}`;
  }
  return freq;
}

// ─── Icon SVGs (inline, keeps bundle clean) ──────────────────────
const IconGrid = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1" y="1" width="5.5" height="5.5" rx="1" fill="currentColor"/>
    <rect x="9.5" y="1" width="5.5" height="5.5" rx="1" fill="currentColor"/>
    <rect x="1" y="9.5" width="5.5" height="5.5" rx="1" fill="currentColor"/>
    <rect x="9.5" y="9.5" width="5.5" height="5.5" rx="1" fill="currentColor"/>
  </svg>
);

const IconList = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1" y="3" width="14" height="1.5" rx="0.75" fill="currentColor"/>
    <rect x="1" y="7.25" width="14" height="1.5" rx="0.75" fill="currentColor"/>
    <rect x="1" y="11.5" width="14" height="1.5" rx="0.75" fill="currentColor"/>
  </svg>
);

const IconOptions = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle cx="3" cy="8" r="1.3" fill="currentColor"/>
    <circle cx="8" cy="8" r="1.3" fill="currentColor"/>
    <circle cx="13" cy="8" r="1.3" fill="currentColor"/>
  </svg>
);

const IconCalendar = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1.5" y="3" width="13" height="11" rx="1.5" stroke="currentColor" strokeWidth="1.4"/>
    <path d="M1.5 6.5h13" stroke="currentColor" strokeWidth="1.4"/>
    <path d="M5 1.5v3M11 1.5v3" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/>
  </svg>
);

const IconBell = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M8 2a4.5 4.5 0 0 0-4.5 4.5V9.5L2 11h12l-1.5-1.5V6.5A4.5 4.5 0 0 0 8 2z" stroke="currentColor" strokeWidth="1.4" strokeLinejoin="round"/>
    <path d="M6.5 12.5a1.5 1.5 0 0 0 3 0" stroke="currentColor" strokeWidth="1.4"/>
  </svg>
);

const IconRepeat = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M3 5h8.5a2 2 0 0 1 2 2v1" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/>
    <path d="M5.5 2.5L3 5l2.5 2.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M13 11H4.5a2 2 0 0 1-2-2V8" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/>
    <path d="M10.5 13.5L13 11l-2.5-2.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);

const IconTrash = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M2.5 4h11M5.5 4V2.5h5V4M6.5 7v5M9.5 7v5M3.5 4l.8 9.5h7.4L12.5 4" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);

const IconChevronDown = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M4 6l4 4 4-4" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);

export function ListsPage() {
  const { listId: routeListId } = useParams<{ listId?: string }>();
  const { t } = useTranslation("lists");
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const isMobile = useIsMobile();

  const familyId = useAppSelector((s) => s.household.family?.familyId);
  const lists = useAppSelector((s) => s.lists.lists);
  const listsStatus = useAppSelector((s) => s.lists.listsStatus);
  const detail = useAppSelector((s) => s.lists.detail);
  const detailStatus = useAppSelector((s) => s.lists.detailStatus);
  const areas = useAppSelector((s) => s.areas.items);
  const members = useAppSelector((s) => s.household.members);

  const [activeListId, setActiveListId] = useState<string | null>(routeListId ?? null);
  const [showCreate, setShowCreate] = useState(false);
  const [showSwitcherSheet, setShowSwitcherSheet] = useState(false);
  const [checkedCollapsed, setCheckedCollapsed] = useState(true);
  const [renameDraft, setRenameDraft] = useState<string | null>(null);
  const [renameError, setRenameError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [showLinkedEvent, setShowLinkedEvent] = useState(false);
  const [selectedItem, setSelectedItem] = useState<SelectedItem | null>(null);
  const [showMobileAdd, setShowMobileAdd] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<ViewMode>("list");

  // Options menu
  const [showOptions, setShowOptions] = useState(false);
  const optionsMenuRef = useRef<HTMLDivElement>(null);

  // Add-row expansion + quick temporal capture
  const [addExpanded, setAddExpanded] = useState(false);
  const [addOpenPanel, setAddOpenPanel] = useState<"dueDate" | "reminder" | "repeat" | null>(null);
  const [addDueDateDraft, setAddDueDateDraft] = useState("");
  const [addReminderDraft, setAddReminderDraft] = useState("");
  const [addRepeatFreqDraft, setAddRepeatFreqDraft] = useState("");
  const [addRepeatDaysDraft, setAddRepeatDaysDraft] = useState<number[]>([]);

  // Inspector drafts
  const [iNameDraft, setINameDraft] = useState("");
  const [iQtyDraft, setIQtyDraft] = useState("");
  const [iNoteDraft, setINoteDraft] = useState("");
  const [iDueDateDraft, setIDueDateDraft] = useState("");
  const [iReminderDraft, setIReminderDraft] = useState("");
  const [iRepeatFreq, setIRepeatFreq] = useState("");
  const [iRepeatDays, setIRepeatDays] = useState<number[]>([]);
  const [iSaving, setISaving] = useState(false);

  const desktopAddRef = useRef<HTMLInputElement>(null);
  const addInputRef = useRef<HTMLInputElement>(null);

  // ── Data loading ─────────────────────────────────────────────────
  useEffect(() => {
    if (familyId) dispatch(fetchFamilySharedLists(familyId));
  }, [familyId, dispatch]);

  useEffect(() => {
    if (familyId) dispatch(fetchAreas(familyId));
  }, [familyId, dispatch]);

  useEffect(() => {
    if (activeListId === null && lists.length > 0) setActiveListId(lists[0].id);
  }, [lists, activeListId]);

  useEffect(() => {
    if (routeListId && routeListId !== activeListId) setActiveListId(routeListId);
  }, [routeListId]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (activeListId) {
      dispatch(fetchSharedListDetail(activeListId));
      setCheckedCollapsed(true);
      setAddError(null);
      setRenameDraft(null);
      setRenameError(null);
      setSelectedItem(null);
    } else {
      dispatch(clearDetail());
    }
  }, [activeListId, dispatch]);

  // ── Sync inspector drafts ────────────────────────────────────────
  useEffect(() => {
    if (selectedItem) {
      const it = selectedItem.item;
      setINameDraft(it.name);
      setIQtyDraft(it.quantity ?? "");
      setINoteDraft(it.note ?? "");
      setIDueDateDraft(it.dueDate ?? "");
      setIReminderDraft(toLocalInput(it.reminder));
      const { freq, days } = parseRepeat(it.repeat);
      setIRepeatFreq(freq);
      setIRepeatDays(days);
    }
  }, [selectedItem?.item.itemId]); // eslint-disable-line react-hooks/exhaustive-deps

  // ── Click-outside to close options ───────────────────────────────
  useEffect(() => {
    if (!showOptions) return;
    function handleClickOutside(e: MouseEvent) {
      if (optionsMenuRef.current && !optionsMenuRef.current.contains(e.target as Node)) {
        setShowOptions(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [showOptions]);

  // ── Switcher ─────────────────────────────────────────────────────
  function handleSelectList(listId: string) {
    setActiveListId(listId);
    setShowSwitcherSheet(false);
    if (routeListId) navigate("/lists", { replace: true });
  }

  // ── Item selection ───────────────────────────────────────────────
  function handleSelectItem(item: SharedListItemDetail, listId: string) {
    setSelectedItem((prev) =>
      prev?.item.itemId === item.itemId ? null : { item, listId },
    );
  }

  // ── Inspector: base fields ───────────────────────────────────────
  async function commitBaseFields() {
    if (!selectedItem || !activeListId) return;
    const newName = iNameDraft.trim();
    const newQty = iQtyDraft.trim() || null;
    const newNote = iNoteDraft.trim() || null;
    const { item } = selectedItem;
    const nameChanged = newName && newName !== item.name;
    const metaChanged =
      newQty !== (item.quantity ?? null) || newNote !== (item.note ?? null);
    if (!nameChanged && !metaChanged) return;
    setISaving(true);
    if (nameChanged) dispatch(optimisticRenameItem({ itemId: item.itemId, name: newName }));
    await dispatch(updateSharedListItem({
      listId: activeListId,
      itemId: item.itemId,
      name: newName || item.name,
      quantity: newQty,
      note: newNote,
    }));
    setISaving(false);
  }

  // ── Inspector: importance ────────────────────────────────────────
  function handleImportanceToggle() {
    if (!selectedItem || !activeListId) return;
    const next = !selectedItem.item.importance;
    dispatch(optimisticSetImportance({ itemId: selectedItem.item.itemId, importance: next }));
    dispatch(setItemImportance({ listId: activeListId, itemId: selectedItem.item.itemId, importance: next }));
  }

  // ── Inspector: temporal fields ───────────────────────────────────
  const commitTemporalFields = useCallback(async (
    overrides?: { dueDate?: string; reminder?: string; repeat?: string | null },
  ) => {
    if (!selectedItem || !activeListId) return;
    const item = selectedInStoreRef.current ?? selectedItem.item;
    const dueDate = (overrides?.dueDate !== undefined ? overrides.dueDate : iDueDateDraft) || null;
    const reminderLocal = overrides?.reminder !== undefined ? overrides.reminder : iReminderDraft;
    const reminder = fromLocalInput(reminderLocal);
    // Compute repeat from current freq+days state or override
    const repeat = overrides?.repeat !== undefined
      ? overrides.repeat
      : serializeRepeat(iRepeatFreq, iRepeatDays);

    const dudChanged = dueDate !== (item.dueDate ?? null);
    const remChanged = reminder !== (item.reminder ?? null);
    const repChanged = repeat !== (item.repeat ?? null);
    if (!dudChanged && !remChanged && !repChanged) return;

    dispatch(optimisticSetTemporal({ itemId: item.itemId, dueDate, reminder, repeat }));

    if (!dueDate && !reminder && !repeat) {
      dispatch(clearItemTemporal({ listId: activeListId, itemId: item.itemId }));
    } else {
      const payload: { dueDate?: string | null; reminder?: string | null; repeat?: string | null } = {};
      if (dudChanged) payload.dueDate = dueDate;
      if (remChanged) payload.reminder = reminder;
      if (repChanged) payload.repeat = repeat;
      dispatch(setItemTemporal({ listId: activeListId, itemId: item.itemId, ...payload }));
    }
  }, [selectedItem, activeListId, iDueDateDraft, iReminderDraft, iRepeatFreq, iRepeatDays, dispatch]); // eslint-disable-line react-hooks/exhaustive-deps

  function handleClearTemporal() {
    if (!selectedItem || !activeListId) return;
    setIDueDateDraft("");
    setIReminderDraft("");
    setIRepeatFreq("");
    setIRepeatDays([]);
    dispatch(optimisticClearTemporal({ itemId: selectedItem.item.itemId }));
    dispatch(clearItemTemporal({ listId: activeListId, itemId: selectedItem.item.itemId }));
  }

  function handleRepeatFreqChange(freq: string) {
    setIRepeatFreq(freq);
    if (freq !== "Weekly") setIRepeatDays([]);
  }

  function handleRepeatDayToggle(day: number) {
    setIRepeatDays((prev) =>
      prev.includes(day) ? prev.filter((d) => d !== day) : [...prev, day],
    );
  }

  // ── Inspector: remove item ───────────────────────────────────────
  function handleInspectorRemove() {
    if (!selectedItem || !activeListId) return;
    dispatch(optimisticRemoveItem({ itemId: selectedItem.item.itemId }));
    dispatch(removeSharedListItem({ listId: activeListId, itemId: selectedItem.item.itemId }));
    setSelectedItem(null);
  }

  // ── Item toggle ──────────────────────────────────────────────────
  function handleItemToggle(item: SharedListItemDetail) {
    if (!activeListId) return;
    dispatch(optimisticToggleItem({ itemId: item.itemId }));
    dispatch(toggleSharedListItem({ listId: activeListId, itemId: item.itemId }));
  }

  // ── Rename list ──────────────────────────────────────────────────
  async function handleRenameCommit() {
    const trimmed = (renameDraft ?? "").trim();
    if (!trimmed || !activeListId || trimmed === detail?.name) {
      setRenameDraft(null);
      return;
    }
    setRenameError(null);
    const result = await dispatch(renameSharedList({ listId: activeListId, name: trimmed }));
    if (renameSharedList.fulfilled.match(result)) setRenameDraft(null);
    else setRenameError((result.payload as string) ?? t("renameError"));
  }

  // ── Delete list ──────────────────────────────────────────────────
  async function handleDelete() {
    if (!detail || !activeListId) return;
    const confirmed = window.confirm(t("deleteConfirm", { name: detail.name }));
    if (!confirmed) return;
    setDeleting(true);
    const result = await dispatch(deleteSharedList(activeListId));
    if (deleteSharedList.fulfilled.match(result)) setActiveListId(null);
    else setDeleting(false);
  }

  // ── DnD reorder ──────────────────────────────────────────────────
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

  // ── Desktop add ──────────────────────────────────────────────────
  async function handleDesktopAddKey(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Escape") {
      e.preventDefault();
      (e.target as HTMLInputElement).value = "";
      setAddExpanded(false);
      setAddOpenPanel(null);
      setAddDueDateDraft(""); setAddReminderDraft(""); setAddRepeatFreqDraft(""); setAddRepeatDaysDraft([]);
      setAddError(null);
      return;
    }
    if (e.key !== "Enter") return;
    const input = e.target as HTMLInputElement;
    const name = input.value.trim();
    if (!name || !activeListId) return;
    setAddError(null);
    input.value = "";
    const result = await dispatch(addItemToSharedList({ listId: activeListId, name }));
    if (addItemToSharedList.fulfilled.match(result)) {
      const newItemId = result.payload.itemId;
      const repeat = serializeRepeat(addRepeatFreqDraft, addRepeatDaysDraft);
      const reminder = fromLocalInput(addReminderDraft);
      if (addDueDateDraft || reminder || repeat) {
        dispatch(setItemTemporal({
          listId: activeListId,
          itemId: newItemId,
          dueDate: addDueDateDraft || null,
          reminder,
          repeat,
        }));
      }
      setAddDueDateDraft(""); setAddReminderDraft(""); setAddRepeatFreqDraft(""); setAddRepeatDaysDraft([]);
      setAddOpenPanel(null);
    } else {
      setAddError((result.payload as string) ?? t("addError"));
    }
    desktopAddRef.current?.focus();
  }

  function handleAddFocusLeave(e: React.FocusEvent<HTMLDivElement>) {
    if (!e.currentTarget.contains(e.relatedTarget as Node | null)) {
      setAddExpanded(false);
    }
  }

  // ── Mobile add ───────────────────────────────────────────────────
  async function handleMobileAddSubmit(name: string) {
    if (!name || !activeListId) return;
    setAddError(null);
    const result = await dispatch(addItemToSharedList({ listId: activeListId, name }));
    if (!addItemToSharedList.fulfilled.match(result)) {
      setAddError((result.payload as string) ?? t("addError"));
    }
  }

  // ── Derived data ─────────────────────────────────────────────────
  const sorted = detail ? [...detail.items].sort((a, b) => a.order - b.order) : [];
  const unchecked = sorted.filter((i) => !i.checked);
  const checked = sorted.filter((i) => i.checked);
  const activeListSummary = lists.find((l) => l.id === activeListId) ?? null;
  const selectedInStore = selectedItem
    ? detail?.items.find((i) => i.itemId === selectedItem.item.itemId) ?? selectedItem.item
    : null;
  const selectedInStoreRef = useRef(selectedInStore);
  selectedInStoreRef.current = selectedInStore;

  const hasTemporalDraft = Boolean(iDueDateDraft || iReminderDraft || iRepeatFreq);
  const itemHasTemporal = Boolean(
    selectedInStore?.dueDate || selectedInStore?.reminder || selectedInStore?.repeat,
  );

  // Linked context resolved
  const linkedArea = detail?.areaId ? areas.find((a) => a.areaId === detail.areaId) : null;
  const linkedEntityLabel = detail?.linkedEntityDisplayName ?? null;

  function renderLinkedContext(className: string) {
    if (!linkedArea && !linkedEntityLabel) return null;
    return (
      <div className={className}>
        {linkedArea && (
          <ContextChip
            label={linkedArea.name}
          />
        )}
        {linkedEntityLabel && detail?.linkedEntityId && (
          <ContextChip
            label={linkedEntityLabel}
            onClick={() => setShowLinkedEvent(true)}
          />
        )}
      </div>
    );
  }

  // ── Inspector body ───────────────────────────────────────────────
  const inspectorBody = selectedInStore ? (
    <div className="li-inspector">
      <div className="li-inspector__scroll">

        {/* Status toggle */}
        <button
          type="button"
          className={`li-inspector__status${selectedInStore.checked ? " li-inspector__status--done" : ""}`}
          onClick={() => handleItemToggle(selectedInStore)}
        >
          <span className="li-inspector__status-circle" aria-hidden="true" />
          <span>{selectedInStore.checked ? t("markUnchecked") : t("markChecked")}</span>
        </button>

        {/* Title */}
        <input
          className="li-inspector__title"
          value={iNameDraft}
          onChange={(e) => setINameDraft(e.target.value)}
          onBlur={commitBaseFields}
          onKeyDown={(e) => { if (e.key === "Enter") { e.preventDefault(); commitBaseFields(); } }}
          aria-label={t("itemName")}
        />

        {/* Importance */}
        <button
          type="button"
          className={`li-inspector__importance${selectedInStore.importance ? " li-inspector__importance--on" : ""}`}
          onClick={handleImportanceToggle}
        >
          <span className="li-inspector__importance-star" aria-hidden="true">
            {selectedInStore.importance ? "★" : "☆"}
          </span>
          <span>{selectedInStore.importance ? t("removeImportance") : t("setImportance")}</span>
        </button>

        {/* Time section */}
        <div className="li-inspector__section">
          <div className="li-inspector__section-label">{t("timeSection")}</div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-due">{t("dueDateLabel")}</label>
            <input
              id="li-due"
              type="date"
              className="li-inspector__field-input"
              value={iDueDateDraft}
              onChange={(e) => setIDueDateDraft(e.target.value)}
              onBlur={() => commitTemporalFields()}
            />
          </div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-reminder">{t("reminderLabel")}</label>
            <input
              id="li-reminder"
              type="datetime-local"
              className="li-inspector__field-input"
              value={iReminderDraft}
              onChange={(e) => setIReminderDraft(e.target.value)}
              onBlur={() => commitTemporalFields()}
            />
          </div>

          {/* Recurrence — structured picker reusing Agenda frequency vocabulary */}
          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-repeat">{t("repeatLabel")}</label>
            <select
              id="li-repeat"
              className="li-inspector__repeat-select"
              value={iRepeatFreq}
              onChange={(e) => { handleRepeatFreqChange(e.target.value); }}
              onBlur={() => commitTemporalFields()}
              aria-label={t("repeatLabel")}
            >
              <option value="">{t("repeatNone")}</option>
              <option value="Daily">{t("repeatDaily")}</option>
              <option value="Weekly">{t("repeatWeekly")}</option>
              <option value="Monthly">{t("repeatMonthly")}</option>
              <option value="Yearly">{t("repeatYearly")}</option>
            </select>

            {iRepeatFreq === "Weekly" && (
              <div className="li-inspector__repeat-days">
                {WEEK_DAYS.map((label, idx) => (
                  <button
                    key={idx}
                    type="button"
                    className={`li-inspector__day-btn${iRepeatDays.includes(idx) ? " li-inspector__day-btn--on" : ""}`}
                    onClick={() => handleRepeatDayToggle(idx)}
                    onBlur={() => commitTemporalFields()}
                    aria-pressed={iRepeatDays.includes(idx)}
                    aria-label={label}
                  >
                    {label}
                  </button>
                ))}
              </div>
            )}
          </div>

          {(hasTemporalDraft || itemHasTemporal) && (
            <button
              type="button"
              className="li-inspector__clear-temporal"
              onClick={handleClearTemporal}
            >
              {t("clearTemporal")}
            </button>
          )}

          {/* Agenda projection hint */}
          {itemHasTemporal && (
            <p className="li-inspector__agenda-hint">{t("agendaProjectionHint")}</p>
          )}
        </div>

        {/* Context section */}
        <div className="li-inspector__section">
          <div className="li-inspector__section-label">{t("contextSection")}</div>

          {/* List name — read-only */}
          {detail && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("listLabel")}</span>
              <span className="li-inspector__scope-value">{detail.name}</span>
            </div>
          )}

          {/* Scope — selector (V1: Household only) */}
          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-scope">{t("scopeLabel")}</label>
            <select
              id="li-scope"
              className="li-inspector__scope-select"
              value="Household"
              aria-label={t("scopeLabel")}
            >
              <option value="Household">{t("scopeHousehold")}</option>
            </select>
          </div>

          {/* Area */}
          {linkedArea && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("areaLabel")}</span>
              <ContextChip label={linkedArea.name} />
            </div>
          )}

          {/* Linked plan */}
          {linkedEntityLabel && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("planLabel")}</span>
              <ContextChip label={linkedEntityLabel} onClick={() => setShowLinkedEvent(true)} />
            </div>
          )}

          {/* Last updated by */}
          {selectedInStore.updatedByMemberId && (() => {
            const m = members.find((x) => x.memberId === selectedInStore.updatedByMemberId);
            return m ? (
              <div className="li-inspector__scope-row">
                <span className="li-inspector__scope-label">{t("lastUpdatedBy")}</span>
                <span className="li-inspector__scope-value">{m.preferredName ?? m.name}</span>
              </div>
            ) : null;
          })()}
        </div>

        {/* Details section */}
        <div className="li-inspector__section">
          <div className="li-inspector__section-label">{t("detailsSection")}</div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-qty">{t("quantityLabel")}</label>
            <input
              id="li-qty"
              className="li-inspector__field-input"
              value={iQtyDraft}
              onChange={(e) => setIQtyDraft(e.target.value)}
              onBlur={commitBaseFields}
              placeholder={t("quantityPlaceholder")}
            />
          </div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-note">{t("noteLabel")}</label>
            <textarea
              id="li-note"
              className="li-inspector__field-textarea"
              value={iNoteDraft}
              onChange={(e) => setINoteDraft(e.target.value)}
              onBlur={commitBaseFields}
              placeholder={t("notePlaceholder")}
              rows={2}
            />
          </div>
        </div>

        {iSaving && <span className="li-inspector__saving">…</span>}

      </div>{/* /li-inspector__scroll */}

      {/* Bottom bar — sticky, outside scroll */}
      <div className="li-inspector__bottom-bar">
        <button
          type="button"
          className="li-inspector__bottom-close"
          onClick={() => setSelectedItem(null)}
          aria-label={t("cancel")}
          title={t("cancel")}
        >
          <IconChevronDown />
        </button>
        <span className="li-inspector__bottom-meta">
          {new Date(selectedInStore.updatedAtUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" })}
        </span>
        <button
          type="button"
          className="li-inspector__bottom-delete"
          onClick={handleInspectorRemove}
          aria-label={t("removeItem")}
          title={t("removeItem")}
        >
          <IconTrash />
        </button>
      </div>

    </div>
  ) : (
    <div className="li-inspector-hint">
      {/* Linked context at list level */}
      {renderLinkedContext("li-inspector-hint__context")}

      {detail && detail.listId === activeListId ? (
        <>
          <span className="li-inspector-hint__stat">
            {unchecked.length} {unchecked.length === 1 ? t("itemSingular") : t("itemPlural")} {t("remaining")}
          </span>
          {checked.length > 0 && (
            <span className="li-inspector-hint__stat">{checked.length} {t("done")}</span>
          )}
          {detail.kind !== "General" && (
            <span className="li-inspector-hint__stat">{detail.kind}</span>
          )}
          <span className="li-inspector-hint__text">{t("selectItemHint")}</span>
        </>
      ) : (
        <span className="li-inspector-hint__stat">{t("noListSelected")}</span>
      )}
    </div>
  );

  return (
    <div className="lists-surface l-surface">

      {/* Mobile header */}
      {isMobile && (
        <header className="lists-mobile-header">
          <button
            type="button"
            className="lists-mobile-header__trigger"
            onClick={(e) => { e.stopPropagation(); setShowSwitcherSheet(true); }}
          >
            <span className="lists-mobile-header__name">
              {activeListSummary ? activeListSummary.name : t("title")}
            </span>
            {activeListSummary && activeListSummary.uncheckedCount > 0 && (
              <span className="li-count">{activeListSummary.uncheckedCount}</span>
            )}
            <span aria-hidden="true">▾</span>
          </button>

          {/* Mobile view toggle */}
          {activeListId && (
            <button
              type="button"
              className={`lists-icon-btn${viewMode === "grid" ? " is-active" : ""}`}
              onClick={() => setViewMode((m) => m === "list" ? "grid" : "list")}
              title={viewMode === "grid" ? t("viewAsList") : t("viewAsGrid")}
            >
              {viewMode === "grid" ? <IconList /> : <IconGrid />}
            </button>
          )}
        </header>
      )}

      {/* Surface body */}
      <div className="lists-surface-body l-surface-body">

        {/* Left switcher — desktop only */}
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
            <div className="lists-empty-state">
              <p className="lists-empty-state__headline">{t("emptyHeadline")}</p>
              <p className="lists-empty-state__hint">{t("emptyHint")}</p>
              <button className="btn" onClick={() => setShowCreate(true)}>
                {t("newList")}
              </button>
            </div>
          )}

          {activeListId && (
            <>
              {detailStatus === "loading" && !detail && (
                <div className="loading-wrap">{t("loadingDetail")}</div>
              )}

              {detail && detail.listId === activeListId && (
                <>
                  {/* Desktop list header */}
                  {!isMobile && (
                    <div className="lists-list-header">
                      <div className="lists-list-header__main">
                        {renameDraft !== null ? (
                          <input
                            className="lists-list-header__rename"
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
                            className="lists-list-header__name-btn"
                            onClick={() => setRenameDraft(detail.name)}
                            title={t("renameList")}
                          >
                            {detail.name}
                            {activeListSummary && activeListSummary.uncheckedCount > 0 && (
                              <span className="li-count">{activeListSummary.uncheckedCount}</span>
                            )}
                          </button>
                        )}
                        {renameError && (
                          <p className="error-msg" style={{ margin: 0 }}>{renameError}</p>
                        )}
                      </div>

                      <div className="lists-list-header__actions">
                        {/* Linked event shortcut */}
                        {detail.linkedEntityDisplayName && (
                          <button
                            type="button"
                            className="btn btn-ghost btn-sm"
                            onClick={() => setShowLinkedEvent(true)}
                            style={{ fontSize: "0.78rem" }}
                          >
                            {t("goToLinkedEvent")}
                          </button>
                        )}

                        {/* View mode toggle (icon) */}
                        <button
                          type="button"
                          className={`lists-icon-btn${viewMode === "grid" ? " is-active" : ""}`}
                          onClick={() => setViewMode((m) => m === "list" ? "grid" : "list")}
                          title={viewMode === "grid" ? t("viewAsList") : t("viewAsGrid")}
                        >
                          {viewMode === "grid" ? <IconList /> : <IconGrid />}
                        </button>

                        {/* Options menu */}
                        <div className="lists-options-menu" ref={optionsMenuRef}>
                          <button
                            type="button"
                            className={`lists-icon-btn${showOptions ? " is-active" : ""}`}
                            onClick={() => setShowOptions((o) => !o)}
                            aria-label={t("listOptions")}
                            title={t("listOptions")}
                          >
                            <IconOptions />
                          </button>
                          {showOptions && (
                            <div className="lists-options-dropdown">
                              <button
                                type="button"
                                className="lists-options-item"
                                onClick={() => { setRenameDraft(detail.name); setShowOptions(false); }}
                              >
                                {t("renameList")}
                              </button>
                              <div className="lists-options-divider" />
                              <button
                                type="button"
                                className="lists-options-item lists-options-item--danger"
                                onClick={() => { setShowOptions(false); handleDelete(); }}
                                disabled={deleting}
                              >
                                {deleting ? t("deletingList") : t("deleteList")}
                              </button>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Quick add — TOP of list, expands on focus */}
                  <div
                    className={`lists-quick-add${addExpanded ? " lists-quick-add--expanded" : ""}`}
                    onFocusCapture={() => setAddExpanded(true)}
                    onBlur={handleAddFocusLeave}
                  >
                    <span className="lists-quick-add__check" aria-hidden="true" />
                    <div className="lists-quick-add__col">
                      <input
                        ref={!isMobile ? desktopAddRef : undefined}
                        type="text"
                        className="lists-quick-add__input"
                        placeholder={t("addItemPlaceholder")}
                        onKeyDown={handleDesktopAddKey}
                        aria-label={t("addItem")}
                      />
                      {addExpanded && (
                        <>
                          {/* Icon action row — one toggle per temporal field */}
                          <div className="lists-quick-add__actions">
                            <button
                              type="button"
                              className={`lists-quick-add__action-btn${addOpenPanel === "dueDate" || addDueDateDraft ? " is-active" : ""}`}
                              onClick={() => setAddOpenPanel((p) => p === "dueDate" ? null : "dueDate")}
                              aria-label={t("dueDateLabel")}
                              title={t("dueDateLabel")}
                            >
                              <IconCalendar />
                            </button>
                            <button
                              type="button"
                              className={`lists-quick-add__action-btn${addOpenPanel === "reminder" || addReminderDraft ? " is-active" : ""}`}
                              onClick={() => setAddOpenPanel((p) => p === "reminder" ? null : "reminder")}
                              aria-label={t("reminderLabel")}
                              title={t("reminderLabel")}
                            >
                              <IconBell />
                            </button>
                            <button
                              type="button"
                              className={`lists-quick-add__action-btn${addOpenPanel === "repeat" || addRepeatFreqDraft ? " is-active" : ""}`}
                              onClick={() => setAddOpenPanel((p) => p === "repeat" ? null : "repeat")}
                              aria-label={t("repeatLabel")}
                              title={t("repeatLabel")}
                            >
                              <IconRepeat />
                            </button>
                          </div>

                          {/* Contextual panel — one at a time */}
                          {addOpenPanel === "dueDate" && (
                            <div className="lists-quick-add__panel">
                              <label className="lists-quick-add__panel-label" htmlFor="qa-due">{t("dueDateLabel")}</label>
                              <input
                                id="qa-due"
                                type="date"
                                className="lists-quick-add__panel-input"
                                value={addDueDateDraft}
                                onChange={(e) => setAddDueDateDraft(e.target.value)}
                                aria-label={t("dueDateLabel")}
                              />
                            </div>
                          )}
                          {addOpenPanel === "reminder" && (
                            <div className="lists-quick-add__panel">
                              <label className="lists-quick-add__panel-label" htmlFor="qa-reminder">{t("reminderLabel")}</label>
                              <input
                                id="qa-reminder"
                                type="datetime-local"
                                className="lists-quick-add__panel-input"
                                value={addReminderDraft}
                                onChange={(e) => setAddReminderDraft(e.target.value)}
                                aria-label={t("reminderLabel")}
                              />
                            </div>
                          )}
                          {addOpenPanel === "repeat" && (
                            <div className="lists-quick-add__panel">
                              <label className="lists-quick-add__panel-label" htmlFor="qa-repeat">{t("repeatLabel")}</label>
                              <select
                                id="qa-repeat"
                                className="lists-quick-add__panel-input"
                                value={addRepeatFreqDraft}
                                onChange={(e) => {
                                  setAddRepeatFreqDraft(e.target.value);
                                  if (e.target.value !== "Weekly") setAddRepeatDaysDraft([]);
                                }}
                                aria-label={t("repeatLabel")}
                              >
                                <option value="">{t("repeatNone")}</option>
                                <option value="Daily">{t("repeatDaily")}</option>
                                <option value="Weekly">{t("repeatWeekly")}</option>
                                <option value="Monthly">{t("repeatMonthly")}</option>
                                <option value="Yearly">{t("repeatYearly")}</option>
                              </select>
                              {addRepeatFreqDraft === "Weekly" && (
                                <div className="li-inspector__repeat-days" style={{ marginTop: "0.5rem" }}>
                                  {WEEK_DAYS.map((label, idx) => (
                                    <button
                                      key={idx}
                                      type="button"
                                      className={`li-inspector__day-btn${addRepeatDaysDraft.includes(idx) ? " li-inspector__day-btn--on" : ""}`}
                                      onClick={() => setAddRepeatDaysDraft((prev) =>
                                        prev.includes(idx) ? prev.filter((d) => d !== idx) : [...prev, idx]
                                      )}
                                      aria-pressed={addRepeatDaysDraft.includes(idx)}
                                      aria-label={label}
                                    >
                                      {label}
                                    </button>
                                  ))}
                                </div>
                              )}
                            </div>
                          )}
                          <p className="lists-quick-add__hint">{t("addRowHint")}</p>
                        </>
                      )}
                    </div>
                  </div>

                  {addError && (
                    <p className="error-msg" style={{ padding: "0.25rem 1rem" }}>
                      {addError}
                    </p>
                  )}

                  {/* Item list / grid */}
                  <div
                    className={[
                      "lists-items-body",
                      viewMode === "grid" ? "lists-items-body--grid" : "",
                    ].filter(Boolean).join(" ")}
                  >
                    {viewMode === "list" ? (
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
                              selectedItemId={selectedItem?.item.itemId ?? null}
                              onSelect={handleSelectItem}
                              onToggle={handleItemToggle}
                            />
                          ))}
                        </SortableContext>
                      </DndContext>
                    ) : (
                      <>
                        {/* Grid column header */}
                        <div className="lists-grid-header">
                          <span></span>
                          <span>{t("itemName")}</span>
                          <span>{t("dueDateLabel")}</span>
                          <span>★</span>
                        </div>
                        {unchecked.map((item) => (
                          <ItemRow
                            key={item.itemId}
                            item={item}
                            listId={detail.listId}
                            selectedItemId={selectedItem?.item.itemId ?? null}
                            onSelect={handleSelectItem}
                            onToggle={handleItemToggle}
                            gridMode
                          />
                        ))}
                      </>
                    )}

                    {sorted.length === 0 && (
                      <p className="lists-items-empty">{t("noItems")}</p>
                    )}

                    {checked.length > 0 && (
                      <>
                        <button
                          type="button"
                          className="lists-section-toggle"
                          onClick={() => setCheckedCollapsed((c) => !c)}
                        >
                          {checkedCollapsed
                            ? `▶ ${t("expandChecked", { count: checked.length })}`
                            : `▼ ${t("collapseChecked")}`}
                        </button>
                        {!checkedCollapsed && checked.map((item) => (
                          <ItemRow
                            key={item.itemId}
                            item={item}
                            listId={detail.listId}
                            selectedItemId={selectedItem?.item.itemId ?? null}
                            onSelect={handleSelectItem}
                            onToggle={handleItemToggle}
                            gridMode={viewMode === "grid"}
                          />
                        ))}
                      </>
                    )}
                  </div>
                </>
              )}
            </>
          )}
        </div>

        {/* Desktop inspector */}
        <InspectorPanel
          title={selectedInStore?.name ?? (detail?.name)}
          onClose={selectedInStore ? () => setSelectedItem(null) : undefined}
        >
          {inspectorBody}
        </InspectorPanel>
      </div>

      {/* Mobile: FAB */}
      {isMobile && activeListId && !showMobileAdd && (
        <button
          type="button"
          className="lists-fab"
          aria-label={t("addItem")}
          onClick={() => setShowMobileAdd(true)}
        >
          +
        </button>
      )}

      {/* Mobile: add composer */}
      {isMobile && showMobileAdd && (
        <div className="lists-add-composer">
          <input
            ref={addInputRef}
            type="text"
            className="lists-add-composer__input"
            placeholder={t("addItemPlaceholder")}
            onKeyDown={async (e) => {
              if (e.key === "Escape") { setShowMobileAdd(false); setAddError(null); return; }
              if (e.key !== "Enter") return;
              const name = (e.target as HTMLInputElement).value.trim();
              if (!name) return;
              (e.target as HTMLInputElement).value = "";
              await handleMobileAddSubmit(name);
              addInputRef.current?.focus();
            }}
            autoFocus
            aria-label={t("addItem")}
          />
          <button
            type="button"
            className="lists-add-composer__close"
            onClick={() => { setShowMobileAdd(false); setAddError(null); }}
            aria-label={t("cancel")}
          >
            ✕
          </button>
        </div>
      )}

      {/* Mobile: item detail sheet */}
      {isMobile && selectedItem && (
        <BottomSheetDetail
          open={!!selectedItem}
          onClose={() => setSelectedItem(null)}
          title={selectedInStore?.name}
        >
          {inspectorBody}
        </BottomSheetDetail>
      )}

      {/* Mobile: list switcher sheet */}
      {isMobile && (
        <BottomSheetDetail
          open={showSwitcherSheet}
          onClose={() => setShowSwitcherSheet(false)}
          title={t("title")}
        >
          <div className="lists-switcher-sheet">
            {listsStatus === "loading" && lists.length === 0 ? (
              <p style={{ padding: "1rem", color: "var(--muted)", textAlign: "center" }}>{t("loading")}</p>
            ) : (
              lists.map((list) => (
                <button
                  key={list.id}
                  type="button"
                  className={`lists-switcher-sheet__row${list.id === activeListId ? " is-active" : ""}`}
                  onClick={() => handleSelectList(list.id)}
                >
                  <span className="lists-switcher-sheet__name">{list.name}</span>
                  {list.uncheckedCount > 0 && (
                    <span className="li-count">{list.uncheckedCount}</span>
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
      {showCreate && <CreateListModal onClose={() => setShowCreate(false)} />}

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

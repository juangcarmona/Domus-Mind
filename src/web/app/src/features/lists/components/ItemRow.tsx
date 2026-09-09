import { useTranslation } from "react-i18next";
import { useAppDispatch } from "../../../store/hooks";
import {
  optimisticSetImportance,
  setItemImportance,
} from "../../../store/listsSlice";
import type { SharedListItemDetail } from "../../../api/types/listTypes";

interface ItemRowProps {
  item: SharedListItemDetail;
  listId: string;
  dragHandleProps?: Record<string, unknown>;
  selectedItemId?: string | null;
  gridMode?: boolean;
  onSelect?: (item: SharedListItemDetail, listId: string) => void;
  onToggle?: (item: SharedListItemDetail) => void;
}

/** Format a YYYY-MM-DD string as a short "Apr 15" label */
function formatDueDate(iso: string, labels: { today: string; tomorrow: string; yesterday: string }): string {
  try {
    const d = new Date(iso + "T00:00:00");
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const diffDays = Math.round((d.getTime() - today.getTime()) / 86400000);
    if (diffDays === 0) return labels.today;
    if (diffDays === 1) return labels.tomorrow;
    if (diffDays === -1) return labels.yesterday;
    return d.toLocaleDateString(undefined, { month: "short", day: "numeric" });
  } catch {
    return iso;
  }
}

/** Format an ISO reminder DateTimeOffset as a short time label */
function formatReminder(iso: string): string {
  try {
    const d = new Date(iso);
    return d.toLocaleTimeString(undefined, { hour: "numeric", minute: "2-digit" });
  } catch {
    return "";
  }
}

export function ItemRow({
  item,
  listId,
  dragHandleProps,
  selectedItemId,
  gridMode = false,
  onSelect,
  onToggle,
}: ItemRowProps) {
  const { t } = useTranslation("lists");
  const dispatch = useAppDispatch();
  const isSelected = selectedItemId === item.itemId;

  const isOverdue = Boolean(
    item.dueDate &&
    !item.checked &&
    new Date(item.dueDate + "T00:00:00") < new Date(new Date().toDateString()),
  );

  function handleRowClick() {
    onSelect?.(item, listId);
  }

  function handleCheckboxClick(e: React.MouseEvent) {
    e.stopPropagation();
    onToggle?.(item);
  }

  function handleStarClick(e: React.MouseEvent) {
    e.stopPropagation();
    const next = !item.importance;
    dispatch(optimisticSetImportance({ itemId: item.itemId, importance: next }));
    dispatch(setItemImportance({ listId, itemId: item.itemId, importance: next }));
  }

  const dueDateLabels = {
    today: t("dueToday"),
    tomorrow: t("dueTomorrow"),
    yesterday: t("dueYesterday"),
  };

  return (
    <div
      className={[
        "li-row",
        gridMode ? "li-row--grid" : "",
        item.checked ? "li-row--checked" : "",
        isSelected ? "li-row--selected" : "",
        item.importance && !item.checked ? "li-row--important" : "",
        isOverdue ? "li-row--overdue" : "",
      ].filter(Boolean).join(" ")}
    >
      <div className="li-row__main" onClick={handleRowClick}>
        {/* Drag handle — visible only in reorder mode */}
        {dragHandleProps && (
          <button
            type="button"
            className="li-row__drag"
            aria-label={t("dragToReorder")}
            tabIndex={-1}
            onClick={(e) => e.stopPropagation()}
            {...dragHandleProps}
          >
            <span aria-hidden="true">⠿</span>
          </button>
        )}

        {/* Circular checkbox */}
        <button
          type="button"
          className="li-row__check"
          aria-label={item.checked ? t("uncheckItem") : t("checkItem")}
          onClick={handleCheckboxClick}
          tabIndex={-1}
        >
          <span className={`li-row__check-circle${item.checked ? " li-row__check-circle--done" : ""}`} aria-hidden="true" />
        </button>

        {/* Body: name + meta row */}
        <div className="li-row__body">
          <span className="li-row__name">{item.name}</span>
          {(item.quantity || item.note || item.dueDate || item.reminder || item.repeat) && !gridMode && (
            <span className="li-row__meta">
              {item.quantity && (
                <span className="li-row__tag">{item.quantity}</span>
              )}
              {item.dueDate && (
                <span className={`li-row__date${isOverdue ? " li-row__date--overdue" : ""}`}>
                  {formatDueDate(item.dueDate, dueDateLabels)}
                </span>
              )}
              {item.reminder && !item.dueDate && (
                <span className="li-row__reminder">⏰ {formatReminder(item.reminder)}</span>
              )}
              {item.repeat && (
                <span className="li-row__repeat" title={item.repeat}>↻</span>
              )}
              {item.note && (
                <span className="li-row__note">{item.note}</span>
              )}
            </span>
          )}
        </div>

        {/* Grid mode: date column (aligned with grid header) */}
        {gridMode && (
          <span className={`li-row__grid-date${isOverdue ? " li-row__date--overdue" : ""}`}>
            {item.dueDate ? formatDueDate(item.dueDate, dueDateLabels) : ""}
          </span>
        )}

        {/* Star / importance */}
        <button
          type="button"
          className={`li-row__star${item.importance ? " li-row__star--on" : ""}`}
          aria-label={item.importance ? t("removeImportance") : t("setImportance")}
          onClick={handleStarClick}
          tabIndex={-1}
        >
          {item.importance ? "★" : "☆"}
        </button>

      </div>
    </div>
  );
}

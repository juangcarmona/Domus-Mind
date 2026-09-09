// Inspector body rendered when no item is selected — shows list metadata,
// stats, color picker, linked plan context, and list-level actions.

import { useTranslation } from "react-i18next";
import { ContextChip } from "../../../components/ContextChip";
import { IconTrash, IconChevronDown } from "./ListsIcons";
import type { GetSharedListDetailResponse } from "../../../api/types/listTypes";

interface ListMetaInspectorContentProps {
  detail: GetSharedListDetailResponse | null;
  activeListId: string | null;
  uncheckedCount: number;
  checkedCount: number;
  temporalCount: number;
  saving: boolean;
  deleting: boolean;
  error: string | null;
  onColorChange: (color: string) => void;
  onClearColor: () => void;
  onLinkedEventClick: () => void;
  onUnlinkPlan: () => void;
  onClose: () => void;
  onDeleteList: () => void;
}

export function ListMetaInspectorContent({
  detail,
  activeListId,
  uncheckedCount,
  checkedCount,
  temporalCount,
  saving,
  deleting,
  error,
  onColorChange,
  onClearColor,
  onLinkedEventClick,
  onUnlinkPlan,
  onClose,
  onDeleteList,
}: ListMetaInspectorContentProps) {
  const { t } = useTranslation("lists");

  if (!detail || detail.listId !== activeListId) {
    return <span className="li-inspector-hint__stat">{t("noListSelected")}</span>;
  }

  return (
    <>
      <div className="li-list-inspector__identity">
        <span
          className={`li-list-inspector__kind li-list-inspector__kind--${detail.kind.toLowerCase()}`}
          style={detail.color ? { background: detail.color } : undefined}
        />
        <div className="li-list-inspector__identity-text">
          <strong>{detail.name}</strong>
          <span>{detail.kind}</span>
        </div>
      </div>

      <div className="li-list-inspector__section">
        <div className="li-list-inspector__section-label">{t("listColorLabel")}</div>
        <div className="li-inspector__scope-row">
          <label className="li-inspector__scope-label" htmlFor="list-color-picker">
            {t("listColorLabel")}
          </label>
          <input
            id="list-color-picker"
            type="color"
            value={detail.color ?? "#000000"}
            disabled={saving}
            onChange={(e) => onColorChange(e.target.value)}
            className="li-list-inspector__color-picker"
          />
          {detail.color && (
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              disabled={saving}
              onClick={onClearColor}
            >
              {t("clearColor")}
            </button>
          )}
        </div>
      </div>

      <div className="li-list-inspector__section">
        <div className="li-list-inspector__section-label">{t("contextSection")}</div>
        <div className="li-inspector__scope-row">
          <span className="li-inspector__scope-label">{t("scopeLabel")}</span>
          <span className="li-inspector__scope-value">{t("scopeHousehold")}</span>
        </div>
        {detail.linkedEntityDisplayName && (
          <div className="li-inspector__scope-row">
            <span className="li-inspector__scope-label">{t("planLabel")}</span>
            <ContextChip label={detail.linkedEntityDisplayName} onClick={onLinkedEventClick} />
            <button type="button" className="btn btn-ghost btn-sm" onClick={onUnlinkPlan}>
              {t("unlinkPlan")}
            </button>
          </div>
        )}
      </div>

      <div className="li-list-inspector__section">
        <div className="li-list-inspector__section-label">{t("listStats")}</div>
        <span className="li-inspector-hint__stat">
          {uncheckedCount} {uncheckedCount === 1 ? t("itemSingular") : t("itemPlural")} {t("remaining")}
        </span>
        <span className="li-inspector-hint__stat">{checkedCount} {t("done")}</span>
        <span className="li-inspector-hint__stat">{temporalCount} {t("timeEnabled")}</span>
      </div>

      <div className="li-list-inspector__section">
        <div className="li-list-inspector__section-label">{t("listActions")}</div>
        {detail.linkedEntityDisplayName && (
          <button type="button" className="btn btn-ghost btn-sm" onClick={onLinkedEventClick}>
            {t("goToLinkedEvent")}
          </button>
        )}
      </div>

      {error && <p className="error-msg">{error}</p>}
      <span className="li-inspector-hint__text">{t("selectItemHint")}</span>

      <div className="li-inspector__bottom-bar">
        <button
          type="button"
          className="li-inspector__bottom-close"
          onClick={onClose}
          aria-label={t("cancel")}
          title={t("cancel")}
        >
          <IconChevronDown />
        </button>
        <span className="li-inspector__bottom-meta">
          {uncheckedCount} {uncheckedCount === 1 ? t("itemSingular") : t("itemPlural")} {t("remaining")}
        </span>
        <button
          type="button"
          className="li-inspector__bottom-delete"
          onClick={onDeleteList}
          aria-label={t("deleteList")}
          title={t("deleteList")}
          disabled={deleting}
        >
          <IconTrash />
        </button>
      </div>
    </>
  );
}

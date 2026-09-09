// Inspector body rendered when a list item is selected.
// Receives all draft state and handlers from ListsPage to keep orchestration
// at the page level while keeping the JSX focused and readable here.

import { useTranslation } from "react-i18next";
import { ContextChip } from "../../../components/ContextChip";
import { WEEK_DAYS } from "../utils";
import { IconTrash, IconChevronDown } from "./ListsIcons";
import type { SharedListItemDetail, GetSharedListDetailResponse } from "../../../api/types/listTypes";
import type { HouseholdAreaItem, FamilyMemberResponse } from "../../../api/domusmindApi";

interface ListItemInspectorContentProps {
  item: SharedListItemDetail;
  detail: GetSharedListDetailResponse | null;
  areas: HouseholdAreaItem[];
  members: FamilyMemberResponse[];
  linkedArea: HouseholdAreaItem | null;
  linkedEntityLabel: string | null;
  nameDraft: string;
  qtyDraft: string;
  noteDraft: string;
  dueDateDraft: string;
  reminderDraft: string;
  repeatFreq: string;
  repeatDays: number[];
  itemAreaDraft: string;
  targetMemberDraft: string;
  saving: boolean;
  hasTemporalDraft: boolean;
  itemHasTemporal: boolean;
  onClose: () => void;
  onRemove: () => void;
  onToggle: () => void;
  onImportanceToggle: () => void;
  onNameChange: (v: string) => void;
  onQtyChange: (v: string) => void;
  onNoteChange: (v: string) => void;
  onBaseFieldsBlur: () => void;
  onDueDateChange: (v: string) => void;
  onReminderChange: (v: string) => void;
  onRepeatFreqChange: (freq: string) => void;
  onRepeatDayToggle: (day: number) => void;
  onTemporalBlur: () => void;
  onClearTemporal: () => void;
  onLinkedEventClick: () => void;
  onItemAreaChange: (v: string) => void;
  onTargetMemberChange: (v: string) => void;
}

export function ListItemInspectorContent({
  item,
  detail,
  areas,
  members,
  linkedArea,
  linkedEntityLabel,
  nameDraft,
  qtyDraft,
  noteDraft,
  dueDateDraft,
  reminderDraft,
  repeatFreq,
  repeatDays,
  itemAreaDraft,
  targetMemberDraft,
  saving,
  hasTemporalDraft,
  itemHasTemporal,
  onClose,
  onRemove,
  onToggle,
  onImportanceToggle,
  onNameChange,
  onQtyChange,
  onNoteChange,
  onBaseFieldsBlur,
  onDueDateChange,
  onReminderChange,
  onRepeatFreqChange,
  onRepeatDayToggle,
  onTemporalBlur,
  onClearTemporal,
  onLinkedEventClick,
  onItemAreaChange,
  onTargetMemberChange,
}: ListItemInspectorContentProps) {
  const { t } = useTranslation("lists");
  const updatedByMember = item.updatedByMemberId
    ? members.find((x) => x.memberId === item.updatedByMemberId)
    : null;

  return (
    <div className="li-inspector">
      <div className="li-inspector__scroll">

        {/* Status toggle */}
        <button
          type="button"
          className={`li-inspector__status${item.checked ? " li-inspector__status--done" : ""}`}
          onClick={onToggle}
        >
          <span className="li-inspector__status-circle" aria-hidden="true" />
          <span>{item.checked ? t("markUnchecked") : t("markChecked")}</span>
        </button>

        {/* Title */}
        <input
          className="li-inspector__title"
          value={nameDraft}
          onChange={(e) => onNameChange(e.target.value)}
          onBlur={onBaseFieldsBlur}
          onKeyDown={(e) => {
            if (e.key === "Enter") {
              e.preventDefault();
              onBaseFieldsBlur();
            }
          }}
          aria-label={t("itemName")}
        />

        {/* Importance */}
        <button
          type="button"
          className={`li-inspector__importance${item.importance ? " li-inspector__importance--on" : ""}`}
          onClick={onImportanceToggle}
        >
          <span className="li-inspector__importance-star" aria-hidden="true">
            {item.importance ? "★" : "☆"}
          </span>
          <span>{item.importance ? t("removeImportance") : t("setImportance")}</span>
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
              value={dueDateDraft}
              onChange={(e) => onDueDateChange(e.target.value)}
              onBlur={onTemporalBlur}
            />
          </div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-reminder">{t("reminderLabel")}</label>
            <input
              id="li-reminder"
              type="datetime-local"
              className="li-inspector__field-input"
              value={reminderDraft}
              onChange={(e) => onReminderChange(e.target.value)}
              onBlur={onTemporalBlur}
            />
          </div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-repeat">{t("repeatLabel")}</label>
            <select
              id="li-repeat"
              className="li-inspector__repeat-select"
              value={repeatFreq}
              onChange={(e) => onRepeatFreqChange(e.target.value)}
              onBlur={onTemporalBlur}
              aria-label={t("repeatLabel")}
            >
              <option value="">{t("repeatNone")}</option>
              <option value="Daily">{t("repeatDaily")}</option>
              <option value="Weekly">{t("repeatWeekly")}</option>
              <option value="Monthly">{t("repeatMonthly")}</option>
              <option value="Yearly">{t("repeatYearly")}</option>
            </select>

            {repeatFreq === "Weekly" && (
              <div className="li-inspector__repeat-days">
                {WEEK_DAYS.map((label, idx) => (
                  <button
                    key={idx}
                    type="button"
                    className={`li-inspector__day-btn${repeatDays.includes(idx) ? " li-inspector__day-btn--on" : ""}`}
                    onClick={() => onRepeatDayToggle(idx)}
                    onBlur={onTemporalBlur}
                    aria-pressed={repeatDays.includes(idx)}
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
              onClick={onClearTemporal}
            >
              {t("clearTemporal")}
            </button>
          )}

          {itemHasTemporal && (
            <p className="li-inspector__agenda-hint">{t("agendaProjectionHint")}</p>
          )}
        </div>

        {/* Context section */}
        <div className="li-inspector__section">
          <div className="li-inspector__section-label">{t("contextSection")}</div>

          {detail && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("listLabel")}</span>
              <span className="li-inspector__scope-value">{detail.name}</span>
            </div>
          )}

          <div className="li-inspector__scope-row">
            <span className="li-inspector__scope-label">{t("scopeLabel")}</span>
            <span className="li-inspector__scope-value">{t("scopeHousehold")}</span>
          </div>
          <p className="li-inspector__scope-hint">{t("householdScoped")}</p>

          <div className="li-inspector__scope-row">
            <label className="li-inspector__scope-label" htmlFor="item-area-select">{t("itemAreaLabel")}</label>
            <select
              id="item-area-select"
              className="li-list-inspector__select"
              value={itemAreaDraft}
              onChange={(e) => onItemAreaChange(e.target.value)}
            >
              <option value="">{t("areaNone")}</option>
              {areas.map((area) => (
                <option key={area.areaId} value={area.areaId}>{area.name}</option>
              ))}
            </select>
          </div>

          <div className="li-inspector__scope-row">
            <label className="li-inspector__scope-label" htmlFor="item-member-select">{t("targetMemberLabel")}</label>
            <select
              id="item-member-select"
              className="li-list-inspector__select"
              value={targetMemberDraft}
              onChange={(e) => onTargetMemberChange(e.target.value)}
            >
              <option value="">{t("memberNone")}</option>
              {members.map((m) => (
                <option key={m.memberId} value={m.memberId}>{m.preferredName ?? m.name}</option>
              ))}
            </select>
          </div>

          {linkedArea && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("areaLabel")}</span>
              <ContextChip label={linkedArea.name} />
            </div>
          )}

          {linkedEntityLabel && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("planLabel")}</span>
              <ContextChip label={linkedEntityLabel} onClick={onLinkedEventClick} />
            </div>
          )}

          {updatedByMember && (
            <div className="li-inspector__scope-row">
              <span className="li-inspector__scope-label">{t("lastUpdatedBy")}</span>
              <span className="li-inspector__scope-value">
                {updatedByMember.preferredName ?? updatedByMember.name}
              </span>
            </div>
          )}
        </div>

        {/* Details section */}
        <div className="li-inspector__section">
          <div className="li-inspector__section-label">{t("detailsSection")}</div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-qty">{t("quantityLabel")}</label>
            <input
              id="li-qty"
              className="li-inspector__field-input"
              value={qtyDraft}
              onChange={(e) => onQtyChange(e.target.value)}
              onBlur={onBaseFieldsBlur}
              placeholder={t("quantityPlaceholder")}
            />
          </div>

          <div className="li-inspector__field">
            <label className="li-inspector__field-label" htmlFor="li-note">{t("noteLabel")}</label>
            <textarea
              id="li-note"
              className="li-inspector__field-textarea"
              value={noteDraft}
              onChange={(e) => onNoteChange(e.target.value)}
              onBlur={onBaseFieldsBlur}
              placeholder={t("notePlaceholder")}
              rows={2}
            />
          </div>
        </div>

        {saving && <span className="li-inspector__saving">…</span>}

      </div>{/* /li-inspector__scroll */}

      {/* Bottom bar — sticky, outside the scroll area */}
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
          {t("updatedOn", {
            date: new Date(item.updatedAtUtc).toLocaleDateString(undefined, {
              month: "short",
              day: "numeric",
            }),
          })}
        </span>
        <button
          type="button"
          className="li-inspector__bottom-delete"
          onClick={onRemove}
          aria-label={t("removeItem")}
          title={t("removeItem")}
        >
          <IconTrash />
        </button>
      </div>
    </div>
  );
}

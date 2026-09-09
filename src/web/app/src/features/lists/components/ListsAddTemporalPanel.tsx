// Temporal field panels for the quick-add flow (due date / reminder / repeat).
// Shared between the desktop add row and the mobile add composer to avoid duplication.

import { useTranslation } from "react-i18next";
import { WEEK_DAYS } from "../utils";

interface ListsAddTemporalPanelProps {
  openPanel: "dueDate" | "reminder" | "repeat" | null;
  dueDateValue: string;
  reminderValue: string;
  repeatFreq: string;
  repeatDays: number[];
  dueDateInputId: string;
  reminderInputId: string;
  repeatInputId: string;
  onDueDateChange: (v: string) => void;
  onReminderChange: (v: string) => void;
  onRepeatFreqChange: (v: string) => void;
  onRepeatDayToggle: (day: number) => void;
}

export function ListsAddTemporalPanel({
  openPanel,
  dueDateValue,
  reminderValue,
  repeatFreq,
  repeatDays,
  dueDateInputId,
  reminderInputId,
  repeatInputId,
  onDueDateChange,
  onReminderChange,
  onRepeatFreqChange,
  onRepeatDayToggle,
}: ListsAddTemporalPanelProps) {
  const { t } = useTranslation("lists");

  return (
    <>
      {openPanel === "dueDate" && (
        <div className="lists-quick-add__panel">
          <label className="lists-quick-add__panel-label" htmlFor={dueDateInputId}>
            {t("dueDateLabel")}
          </label>
          <input
            id={dueDateInputId}
            type="date"
            className="lists-quick-add__panel-input"
            value={dueDateValue}
            onChange={(e) => onDueDateChange(e.target.value)}
            aria-label={t("dueDateLabel")}
          />
        </div>
      )}

      {openPanel === "reminder" && (
        <div className="lists-quick-add__panel">
          <label className="lists-quick-add__panel-label" htmlFor={reminderInputId}>
            {t("reminderLabel")}
          </label>
          <input
            id={reminderInputId}
            type="datetime-local"
            className="lists-quick-add__panel-input"
            value={reminderValue}
            onChange={(e) => onReminderChange(e.target.value)}
            aria-label={t("reminderLabel")}
          />
        </div>
      )}

      {openPanel === "repeat" && (
        <div className="lists-quick-add__panel">
          <label className="lists-quick-add__panel-label" htmlFor={repeatInputId}>
            {t("repeatLabel")}
          </label>
          <select
            id={repeatInputId}
            className="lists-quick-add__panel-input"
            value={repeatFreq}
            onChange={(e) => onRepeatFreqChange(e.target.value)}
            aria-label={t("repeatLabel")}
          >
            <option value="">{t("repeatNone")}</option>
            <option value="Daily">{t("repeatDaily")}</option>
            <option value="Weekly">{t("repeatWeekly")}</option>
            <option value="Monthly">{t("repeatMonthly")}</option>
            <option value="Yearly">{t("repeatYearly")}</option>
          </select>

          {repeatFreq === "Weekly" && (
            <div className="li-inspector__repeat-days" style={{ marginTop: "0.5rem" }}>
              {WEEK_DAYS.map((label, idx) => (
                <button
                  key={idx}
                  type="button"
                  className={`li-inspector__day-btn${repeatDays.includes(idx) ? " li-inspector__day-btn--on" : ""}`}
                  onClick={() => onRepeatDayToggle(idx)}
                  aria-pressed={repeatDays.includes(idx)}
                  aria-label={label}
                >
                  {label}
                </button>
              ))}
            </div>
          )}
        </div>
      )}
    </>
  );
}

import { useTranslation } from "react-i18next";
import { toIsoDate } from "../../today/utils/dateUtils";
import { AgendaMiniCalendar } from "../../agenda/components/AgendaMiniCalendar";
import type { PlanningView } from "./PlanningHeader";

const SELECT_PROMPT = "Select a plan to inspect";

export interface SelectedPlanItem {
  type: "event" | "task" | "routine";
  id: string;
  title: string;
  date?: string | null;
  time?: string | null;
  endTime?: string | null;
  status?: string;
  subtitle?: string | null;
  color?: string | null;
}

interface PlanningInspectorContentProps {
  selectedDate: string; // ISO YYYY-MM-DD
  view: PlanningView;
  firstDayOfWeek: string | null;
  selectedItem: SelectedPlanItem | null;
  onSelectDate: (date: string) => void;
  onEditItem?: (type: string, id: string) => void;
  onClearSelection: () => void;
}

/**
 * Content rendered inside the Planning InspectorPanel.
 *
 * Always shows AgendaMiniCalendar for date navigation.
 * Below the calendar: selected item summary, or a quiet prompt when nothing is selected.
 */
export function PlanningInspectorContent({
  selectedDate,
  view,
  firstDayOfWeek,
  selectedItem,
  onSelectDate,
  onEditItem,
  onClearSelection,
}: PlanningInspectorContentProps) {
  const { t: tCommon } = useTranslation("common");
  const { i18n } = useTranslation();

  const todayIso = toIsoDate(new Date());

  function formatItemDate(date: string | null | undefined, time: string | null | undefined): string | null {
    if (!date) return null;
    const locale = i18n.language;
    const datePart = new Date(date + "T00:00:00").toLocaleDateString(locale, {
      weekday: "short",
      day: "numeric",
      month: "short",
    });
    if (!time) return datePart;
    const timePart = time.slice(0, 5);
    return `${datePart} · ${timePart}`;
  }

  return (
    <div className="planning-inspector-content">
      {/* Mini calendar always present for date navigation */}
      <div className="planning-inspector-mini-cal">
        <AgendaMiniCalendar
          selectedDate={selectedDate}
          today={todayIso}
          view={view}
          firstDayOfWeek={firstDayOfWeek}
          onSelectDate={onSelectDate}
        />
      </div>

      <div className="planning-inspector-detail">
        {selectedItem ? (
          <div className="planning-item-detail">
            {selectedItem.color && (
              <span
                className="planning-item-color-dot"
                style={{ background: selectedItem.color }}
                aria-hidden="true"
              />
            )}
            <p className="planning-item-title">{selectedItem.title}</p>
            {(selectedItem.date || selectedItem.time) && (
              <p className="planning-item-meta">
                {formatItemDate(selectedItem.date, selectedItem.time)}
              </p>
            )}
            {selectedItem.subtitle && (
              <p className="planning-item-subtitle">{selectedItem.subtitle}</p>
            )}
            {selectedItem.status && (
              <p className="planning-item-status">{selectedItem.status}</p>
            )}
            {onEditItem && (
              <div className="planning-item-actions">
                <button
                  type="button"
                  className="btn btn-ghost btn-sm"
                  onClick={() => onEditItem(selectedItem.type, selectedItem.id)}
                >
                  Edit
                </button>
                <button
                  type="button"
                  className="btn btn-ghost btn-sm"
                  onClick={onClearSelection}
                  aria-label={tCommon("close")}
                >
                  ✕
                </button>
              </div>
            )}
          </div>
        ) : (
          <p className="planning-inspector-empty">{SELECT_PROMPT}</p>
        )}
      </div>
    </div>
  );
}

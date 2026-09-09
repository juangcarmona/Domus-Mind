import { useTranslation } from "react-i18next";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { HourTimeline } from "./HourTimeline";
import { toIsoDate } from "../../agenda-today/utils/dateUtils";
import { useIsMobile } from "../../../hooks/useIsMobile";

interface MemberSelectedDayTimedSectionProps {
  timedEntries: CalendarEntry[];
  selectedDate: string; // ISO YYYY-MM-DD
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
  onSlotClick?: (time: string) => void;
  /**
   * When true the timeline panel has no own overflow/max-height — it flows inline
   * inside a parent scrollable container (e.g. the week-view detail panel).
   * When false (default) the panel scrolls internally, suitable for day mode.
   */
  embedded?: boolean;
}

/**
 * Renders the timed part of a selected day via HourTimeline.
 *
 * If there are no timed entries the section renders nothing — callers should
 * never see an empty full-height timeline grid for a sparse day.
 */
export function MemberSelectedDayTimedSection({
  timedEntries,
  selectedDate,
  onItemClick,
  onSlotClick,
  embedded = false,
}: MemberSelectedDayTimedSectionProps) {
  const { t } = useTranslation("agenda");
  const isMobile = useIsMobile();

  if (timedEntries.length === 0) return null;

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;
  const now = new Date();
  const nowMinutes = isToday ? now.getHours() * 60 + now.getMinutes() : undefined;

  const panelClass = [
    "mday-timeline-panel",
    embedded ? "mday-timeline-panel--embedded" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className="mday-timed-section">
      <div className="mday-section-title">{t("day.timed", "Scheduled")}</div>
      <div className={panelClass}>
        <HourTimeline
          timedEntries={timedEntries}
          isToday={isToday}
          nowMinutes={nowMinutes}
          onItemClick={onItemClick}
          onSlotClick={onSlotClick}
          compact={isMobile}
        />
      </div>
    </div>
  );
}

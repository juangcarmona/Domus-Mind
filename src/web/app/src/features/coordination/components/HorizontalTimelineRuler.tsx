import { useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { EnrichedTimelineResponse } from "../../../api/domusmindApi";

interface HorizontalTimelineRulerProps {
  selectedDate: string; // ISO YYYY-MM-DD
  today: string;        // ISO YYYY-MM-DD
  timelineData: EnrichedTimelineResponse | null;
  onSelectDay: (date: string) => void;
}

function isoLocal(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

function addDaysToIso(iso: string, n: number): string {
  const d = new Date(iso + "T00:00:00");
  d.setDate(d.getDate() + n);
  return isoLocal(d);
}

function generateDayRange(startIso: string, endIso: string): string[] {
  const days: string[] = [];
  const end = new Date(endIso + "T00:00:00");
  const cur = new Date(startIso + "T00:00:00");
  while (cur <= end) {
    days.push(isoLocal(cur));
    cur.setDate(cur.getDate() + 1);
  }
  return days;
}

type EventDot = { type: "event" | "task"; title: string };

export function HorizontalTimelineRuler({
  selectedDate,
  today,
  timelineData,
  onSelectDay,
}: HorizontalTimelineRulerProps) {
  const { i18n, t } = useTranslation("coordination");
  const trackRef = useRef<HTMLDivElement>(null);
  const selectedRef = useRef<HTMLButtonElement>(null);

  const startIso = addDaysToIso(today, -90);
  const endIso = addDaysToIso(today, 365);
  const days = generateDayRange(startIso, endIso);

  // Build event map from timeline data (routines excluded — they are ongoing)
  const eventMap = new Map<string, EventDot[]>();
  if (timelineData) {
    for (const group of timelineData.groups) {
      for (const entry of group.entries) {
        if (!entry.effectiveDate || entry.entryType === "Routine") continue;
        const key = entry.effectiveDate.slice(0, 10);
        if (!eventMap.has(key)) eventMap.set(key, []);
        eventMap.get(key)!.push({
          type: entry.entryType === "CalendarEvent" ? "event" : "task",
          title: entry.title,
        });
      }
    }
  }

  // Auto-scroll to center the selected date
  useEffect(() => {
    if (selectedRef.current && trackRef.current) {
      const track = trackRef.current;
      const el = selectedRef.current;
      const offset = el.offsetLeft - track.clientWidth / 2 + el.offsetWidth / 2;
      track.scrollLeft = Math.max(0, offset);
    }
  }, [selectedDate]);

  return (
    <div className="coord-ruler-wrap">
      <div className="coord-ruler-hint">{t("timeline.scrollHint")}</div>
      <div className="coord-ruler-track" ref={trackRef}>
        {days.map((iso) => {
          const d = new Date(iso + "T00:00:00");
          const isToday = iso === today;
          const isSelected = iso === selectedDate;
          const isMonthStart = d.getDate() === 1;
          const dots = eventMap.get(iso) ?? [];
          const dayNum = d.getDate();
          const dayName = d.toLocaleDateString(i18n.language, { weekday: "short" });
          const monthLabel = isMonthStart
            ? d.toLocaleDateString(i18n.language, { month: "short", year: "numeric" })
            : null;

          const cls = [
            "coord-ruler-day",
            isToday ? "coord-ruler-day--today" : "",
            isSelected ? "coord-ruler-day--selected" : "",
            isMonthStart ? "coord-ruler-day--month-start" : "",
          ]
            .filter(Boolean)
            .join(" ");

          return (
            <button
              key={iso}
              ref={isSelected ? selectedRef : undefined}
              className={cls}
              onClick={() => onSelectDay(iso)}
              type="button"
              aria-label={iso}
              aria-pressed={isSelected}
            >
              <span className="coord-ruler-month-label">
                {monthLabel ?? "\u00a0"}
              </span>
              <span className="coord-ruler-day-name">{dayName}</span>
              <span className="coord-ruler-day-num">{dayNum}</span>
              <span className="coord-ruler-tick" />
              <span className="coord-ruler-dots">
                {dots.slice(0, 5).map((dot, idx) => (
                  <span
                    key={idx}
                    className={`coord-ruler-dot coord-ruler-dot--${dot.type}`}
                    title={dot.title}
                  />
                ))}
                {dots.length > 5 && (
                  <span className="coord-ruler-dot-more">+{dots.length - 5}</span>
                )}
              </span>
            </button>
          );
        })}
      </div>
    </div>
  );
}

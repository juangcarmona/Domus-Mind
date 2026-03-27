import { useRef, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import type { EnrichedTimelineResponse } from "../../../../api/domusmindApi";

interface TimelineRulerProps {
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

type EventDot = { type: "event" | "task"; title: string; color: string };

export function TimelineRuler({
  selectedDate,
  today,
  timelineData,
  onSelectDay,
}: TimelineRulerProps) {
  const { i18n, t } = useTranslation("today");
  const trackRef = useRef<HTMLDivElement>(null);
  const selectedRef = useRef<HTMLButtonElement>(null);
  const dragState = useRef<{ startX: number; scrollLeft: number; dragged: boolean } | null>(null);

  // Drag-to-scroll on the ruler track
  const onPointerDown = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    if (e.button !== 0) return;
    const track = trackRef.current;
    if (!track) return;
    dragState.current = { startX: e.clientX, scrollLeft: track.scrollLeft, dragged: false };
    track.setPointerCapture(e.pointerId);
    track.style.cursor = "grabbing";
    track.style.userSelect = "none";
  }, []);

  const onPointerMove = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    if (!dragState.current || !trackRef.current) return;
    const dx = e.clientX - dragState.current.startX;
    if (Math.abs(dx) > 4) dragState.current.dragged = true;
    trackRef.current.scrollLeft = dragState.current.scrollLeft - dx;
  }, []);

  const onPointerUpOrCancel = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    if (!dragState.current || !trackRef.current) return;
    trackRef.current.style.cursor = "";
    trackRef.current.style.userSelect = "";
    trackRef.current.releasePointerCapture(e.pointerId);
    dragState.current = null;
  }, []);

  // Suppress day-button click when the pointer was used to drag
  const onClickCapture = useCallback((e: React.MouseEvent) => {
    if (dragState.current?.dragged) {
      e.stopPropagation();
      e.preventDefault();
    }
  }, []);

  const startIso = addDaysToIso(today, -90);
  const endIso = addDaysToIso(today, 365);
  const days = generateDayRange(startIso, endIso);

  // Build event map from timeline data (routines excluded - they are ongoing)
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
          color: entry.color,
        });
      }
    }
  }

  // Auto-scroll to center the selected date in the viewport
  useEffect(() => {
    selectedRef.current?.scrollIntoView({ inline: "center", block: "nearest", behavior: "smooth" });
  }, [selectedDate]);

  return (
    <div className="coord-ruler-wrap">
      <div className="coord-ruler-hint">{t("timeline.scrollHint")}</div>
      <div className="coord-ruler-track" ref={trackRef}
        onPointerDown={onPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={onPointerUpOrCancel}
        onPointerCancel={onPointerUpOrCancel}
        onClickCapture={onClickCapture}
        style={{ cursor: "grab" }}
      >
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
                    className="coord-ruler-dot"
                    style={{ background: dot.color || (dot.type === "event" ? "var(--primary)" : "var(--accent)") }}
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

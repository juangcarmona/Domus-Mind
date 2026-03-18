import { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { setSelectedDate } from "../../../store/coordinationSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { weekApi } from "../../week/api/weekApi";
import type { WeeklyGridResponse } from "../../week/types";
import type { ApiError } from "../../../api/domusmindApi";
import { DayView } from "../components/DayView";
import { MonthView } from "../components/MonthView";
import { CoordinationWeekView } from "../components/CoordinationWeekView";
import { HorizontalTimelineRuler } from "../components/HorizontalTimelineRuler";

// ---- Date helpers ----

const DAY_ORDER = [
  "sunday",
  "monday",
  "tuesday",
  "wednesday",
  "thursday",
  "friday",
  "saturday",
];

function startOfWeek(d: Date, firstDayOfWeek?: string | null): Date {
  const targetDay = DAY_ORDER.indexOf(
    (firstDayOfWeek ?? "monday").toLowerCase(),
  );
  const safeTarget = targetDay < 0 ? 1 : targetDay;
  const day = d.getDay();
  let diff = day - safeTarget;
  if (diff < 0) diff += 7;
  return new Date(d.getFullYear(), d.getMonth(), d.getDate() - diff);
}

function toIsoDate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

function addDays(iso: string, n: number): string {
  const d = new Date(iso + "T00:00:00");
  d.setDate(d.getDate() + n);
  return toIsoDate(d);
}

function addMonths(iso: string, n: number): string {
  const d = new Date(iso + "T00:00:00");
  d.setMonth(d.getMonth() + n);
  return toIsoDate(d);
}

// ---- Member color palette (deterministic by index) ----
const MEMBER_COLORS = [
  "#b79ad9", // primary purple
  "#f0c872", // accent yellow
  "#6ec6a0", // teal-green
  "#e07b84", // salmon
  "#7bbde0", // sky blue
  "#e09d6e", // orange
  "#a0c46e", // lime
];

function getMemberColor(index: number): string {
  return MEMBER_COLORS[index % MEMBER_COLORS.length];
}

// ---- Component ----

export function CoordinationPage() {
  const dispatch = useAppDispatch();
  const { t, i18n } = useTranslation("coordination");

  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;
  const members = useAppSelector((s) => s.household.members);

  const selectedDate = useAppSelector((s) => s.coordination.selectedDate);
  const { data: timelineData, status: timelineStatus, error: timelineError } =
    useAppSelector((s) => s.timeline);

  // Mid-term section: local tab state
  const [midTermView, setMidTermView] = useState<"week" | "month">("week");

  // Month view anchor — navigated independently of selectedDate
  const [monthAnchor, setMonthAnchor] = useState<string>(selectedDate);
  useEffect(() => {
    setMonthAnchor(selectedDate);
  }, [selectedDate]);

  // Grid data (shared by Day View + Week View)
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);

  // Compute week start for the selected date
  const weekStartForSelected = toIsoDate(
    startOfWeek(new Date(selectedDate + "T00:00:00"), firstDayOfWeek),
  );

  const fetchGrid = useCallback(
    async (weekStart: string) => {
      if (!familyId) return;
      setGridLoading(true);
      setGridError(null);
      try {
        const data = await weekApi.getWeeklyGrid(familyId, weekStart);
        setGrid(data);
      } catch (err) {
        const apiErr = err as Partial<ApiError>;
        setGridError(apiErr.message ?? t("error"));
      } finally {
        setGridLoading(false);
      }
    },
    [familyId, t],
  );

  // Reload grid whenever the selected week changes
  useEffect(() => {
    if (familyId) {
      fetchGrid(weekStartForSelected);
    }
  }, [weekStartForSelected, fetchGrid, familyId]);

  // Load timeline data once (for the ruler)
  useEffect(() => {
    if (familyId && timelineStatus === "idle") {
      dispatch(fetchTimeline({ familyId }));
    }
  }, [familyId, timelineStatus, dispatch]);

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  // ---- Interaction handlers ----

  function handleDaySelect(date: string) {
    dispatch(setSelectedDate(date));
  }

  function handlePrevDay() {
    dispatch(setSelectedDate(addDays(selectedDate, -1)));
  }

  function handleNextDay() {
    dispatch(setSelectedDate(addDays(selectedDate, 1)));
  }

  function handleToday() {
    dispatch(setSelectedDate(todayIso));
  }

  function handlePrevWeek() {
    dispatch(setSelectedDate(addDays(selectedDate, -7)));
  }

  function handleNextWeek() {
    dispatch(setSelectedDate(addDays(selectedDate, 7)));
  }

  // ---- Labels ----

  const weekEnd = addDays(weekStartForSelected, 6);
  const weekNavLabel = `${new Date(weekStartForSelected + "T00:00:00").toLocaleDateString(
    i18n.language,
    { day: "numeric", month: "short" },
  )} – ${new Date(weekEnd + "T00:00:00").toLocaleDateString(i18n.language, {
    day: "numeric",
    month: "short",
    year: "numeric",
  })}`;

  // ---- Month calendar dots: per-day member colors from timeline data ----
  const dayDots: Record<string, string[]> = {};
  if (timelineStatus === "success" && timelineData) {
    // Build memberId → index map for consistent colors
    const memberIndexMap = new Map<string, number>();
    members.forEach((m, idx) => memberIndexMap.set(m.memberId, idx));

    for (const group of timelineData.groups) {
      for (const entry of group.entries) {
        if (!entry.effectiveDate || entry.entryType === "Routine") continue;
        const dayKey = entry.effectiveDate.slice(0, 10);
        if (!dayDots[dayKey]) dayDots[dayKey] = [];

        // Collect member colors for this entry (assignee or participants)
        const memberIds = new Set<string>();
        if (entry.assigneeId) memberIds.add(entry.assigneeId);
        if (entry.participants) {
          for (const p of entry.participants) memberIds.add(p.memberId);
        }

        for (const memberId of memberIds) {
          const idx = memberIndexMap.get(memberId);
          const color = idx !== undefined ? getMemberColor(idx) : getMemberColor(0);
          if (!dayDots[dayKey].includes(color)) {
            dayDots[dayKey].push(color);
          }
        }
      }
    }
  }

  return (
    <div className="page-content coord-page">
      {/* ── Day navigation bar (prev/next/today only — date shown in Day View) ── */}
      <div className="coord-date-nav">
        <button
          className="btn btn-ghost btn-sm coord-nav-btn"
          onClick={handlePrevDay}
          type="button"
          aria-label={t("nav.prevDay")}
        >
          ‹
        </button>
        {!isToday && (
          <button
            className="btn btn-ghost btn-sm coord-today-btn"
            onClick={handleToday}
            type="button"
          >
            {t("nav.today")}
          </button>
        )}
        <button
          className="btn btn-ghost btn-sm coord-nav-btn"
          onClick={handleNextDay}
          type="button"
          aria-label={t("nav.nextDay")}
        >
          ›
        </button>
      </div>

      {/* ── Section 1: Day View (always visible) ── */}
      <DayView
        grid={grid}
        selectedDate={selectedDate}
        loading={gridLoading}
        error={gridError}
      />

      {/* ── Section 2: Mid-term navigation (Week / Month) ── */}
      <div className="coord-midterm-section">
        {/* Centered tab switcher */}
        <div className="coord-midterm-tabbar">
          <button
            className={`coord-midterm-tab${midTermView === "week" ? " coord-midterm-tab--active" : ""}`}
            onClick={() => setMidTermView("week")}
            type="button"
          >
            <span className="coord-midterm-tab-icon">▦</span>
            {t("tabs.week")}
          </button>
          <button
            className={`coord-midterm-tab${midTermView === "month" ? " coord-midterm-tab--active" : ""}`}
            onClick={() => setMidTermView("month")}
            type="button"
          >
            <span className="coord-midterm-tab-icon">🗓</span>
            {t("tabs.month")}
          </button>
        </div>

        {/* Week nav — same style as month nav */}
        {midTermView === "week" && (
          <div className="coord-month-nav coord-midterm-week-nav">
            <button
              className="btn btn-ghost btn-sm"
              onClick={handlePrevWeek}
              type="button"
            >
              {t("nav.prevMonth")}
            </button>
            <span className="coord-month-label">{weekNavLabel}</span>
            <button
              className="btn btn-ghost btn-sm"
              onClick={handleNextWeek}
              type="button"
            >
              {t("nav.nextMonth")}
            </button>
          </div>
        )}

        {midTermView === "week" && (
          <CoordinationWeekView
            grid={grid}
            loading={gridLoading}
            error={gridError}
            selectedDate={selectedDate}
            onDayClick={handleDaySelect}
          />
        )}

        {midTermView === "month" && (
          <div className="coord-month-wrap">
            <MonthView
              selectedDate={selectedDate}
              today={todayIso}
              firstDayOfWeek={firstDayOfWeek}
              displayAnchor={monthAnchor}
              dayDots={dayDots}
              onSelectDay={handleDaySelect}
              onPrevMonth={() => setMonthAnchor(addMonths(monthAnchor, -1))}
              onNextMonth={() => setMonthAnchor(addMonths(monthAnchor, 1))}
            />
          </div>
        )}
      </div>

      {/* ── Section 3: Horizontal Timeline Ruler ── */}
      <div className="coord-ruler-section">
        <HorizontalTimelineRuler
          selectedDate={selectedDate}
          today={todayIso}
          timelineData={timelineStatus === "success" ? timelineData : null}
          onSelectDay={handleDaySelect}
        />
        {timelineError && (
          <p className="error-msg" style={{ padding: "0.5rem 0.75rem", margin: 0 }}>
            {timelineError}
          </p>
        )}
      </div>
    </div>
  );
}


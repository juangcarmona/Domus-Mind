import { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  setSelectedDate,
  setViewMode,
  selectDayAndSwitchToDay,
  type ViewMode,
} from "../../../store/coordinationSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { weekApi } from "../../week/api/weekApi";
import type { WeeklyGridResponse } from "../../week/types";
import type { ApiError } from "../../../api/domusmindApi";
import { CoordinationViewTabs } from "../components/CoordinationViewTabs";
import { DayView } from "../components/DayView";
import { MonthView } from "../components/MonthView";
import { CoordinationTimeline } from "../components/CoordinationTimeline";
import { CoordinationWeekView } from "../components/CoordinationWeekView";

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
  return d.toISOString().slice(0, 10);
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

// ---- Component ----

export function CoordinationPage() {
  const dispatch = useAppDispatch();
  const { t, i18n } = useTranslation("coordination");

  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;

  const { selectedDate, viewMode } = useAppSelector((s) => s.coordination);
  const { data: timelineData, status: timelineStatus, error: timelineError } =
    useAppSelector((s) => s.timeline);

  // Local grid state for Day/Week views
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);
  const [currentWeekStart, setCurrentWeekStart] = useState<string>(() =>
    toIsoDate(startOfWeek(new Date(selectedDate + "T00:00:00"), firstDayOfWeek)),
  );

  // ---- Sync weekStart when selectedDate or firstDayOfWeek changes ----
  useEffect(() => {
    const newWeekStart = toIsoDate(
      startOfWeek(new Date(selectedDate + "T00:00:00"), firstDayOfWeek),
    );
    setCurrentWeekStart(newWeekStart);
  }, [selectedDate, firstDayOfWeek]);

  // ---- Load weekly grid for Day and Week views ----
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

  useEffect(() => {
    if (viewMode === "day" || viewMode === "week") {
      fetchGrid(currentWeekStart);
    }
  }, [viewMode, currentWeekStart, fetchGrid]);

  // ---- Load timeline data for Timeline view ----
  useEffect(() => {
    if (viewMode === "timeline" && familyId) {
      if (timelineStatus === "idle") {
        dispatch(fetchTimeline({ familyId }));
      }
    }
  }, [viewMode, familyId, timelineStatus, dispatch]);

  // ---- Navigation ----

  function handlePrev() {
    if (viewMode === "day") {
      dispatch(setSelectedDate(addDays(selectedDate, -1)));
    } else if (viewMode === "week") {
      const prevWeekStart = addDays(currentWeekStart, -7);
      setCurrentWeekStart(prevWeekStart);
      dispatch(setSelectedDate(addDays(selectedDate, -7)));
    } else if (viewMode === "month") {
      dispatch(setSelectedDate(addMonths(selectedDate, -1)));
    }
  }

  function handleNext() {
    if (viewMode === "day") {
      dispatch(setSelectedDate(addDays(selectedDate, 1)));
    } else if (viewMode === "week") {
      const nextWeekStart = addDays(currentWeekStart, 7);
      setCurrentWeekStart(nextWeekStart);
      dispatch(setSelectedDate(addDays(selectedDate, 7)));
    } else if (viewMode === "month") {
      dispatch(setSelectedDate(addMonths(selectedDate, 1)));
    }
  }

  function handleToday() {
    const todayIso = toIsoDate(new Date());
    dispatch(setSelectedDate(todayIso));
  }

  function handleViewMode(mode: ViewMode) {
    dispatch(setViewMode(mode));
  }

  function handleDaySelect(date: string) {
    dispatch(selectDayAndSwitchToDay(date));
  }

  // ---- Date label for navigation bar ----
  function getDateNavLabel(): string {
    const d = new Date(selectedDate + "T00:00:00");
    if (viewMode === "day") {
      return d.toLocaleDateString(i18n.language, {
        weekday: "long",
        day: "numeric",
        month: "long",
        year: "numeric",
      });
    }
    if (viewMode === "week") {
      const weekEnd = addDays(currentWeekStart, 6);
      const start = new Date(currentWeekStart + "T00:00:00");
      const end = new Date(weekEnd + "T00:00:00");
      return `${start.toLocaleDateString(i18n.language, { day: "numeric", month: "short" })} – ${end.toLocaleDateString(i18n.language, { day: "numeric", month: "short", year: "numeric" })}`;
    }
    if (viewMode === "month") {
      return d.toLocaleDateString(i18n.language, {
        month: "long",
        year: "numeric",
      });
    }
    return "";
  }

  function getPrevLabel(): string {
    if (viewMode === "day") return t("nav.prevDay");
    if (viewMode === "week") return t("nav.prevWeek");
    if (viewMode === "month") return t("nav.prevMonth");
    return "";
  }

  function getNextLabel(): string {
    if (viewMode === "day") return t("nav.nextDay");
    if (viewMode === "week") return t("nav.nextWeek");
    if (viewMode === "month") return t("nav.nextMonth");
    return "";
  }

  const showDateNav = viewMode !== "timeline";
  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  return (
    <div className="page-content coord-page">
      {/* View mode tabs */}
      <div className="coord-controls">
        <CoordinationViewTabs viewMode={viewMode} onSelect={handleViewMode} />

        {showDateNav && (
          <div className="coord-date-nav">
            <button
              className="btn btn-ghost btn-sm"
              onClick={handlePrev}
              type="button"
            >
              {getPrevLabel()}
            </button>
            <span className="coord-date-label">{getDateNavLabel()}</span>
            <button
              className="btn btn-ghost btn-sm"
              onClick={handleNext}
              type="button"
            >
              {getNextLabel()}
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
          </div>
        )}
      </div>

      {/* View content */}
      <div className="coord-content">
        {viewMode === "timeline" && (
          <CoordinationTimeline
            data={timelineData}
            loading={timelineStatus === "loading"}
            error={timelineError}
            selectedDate={selectedDate}
            onSelectDay={handleDaySelect}
          />
        )}

        {viewMode === "day" && (
          <DayView
            grid={grid}
            selectedDate={selectedDate}
            loading={gridLoading}
            error={gridError}
          />
        )}

        {viewMode === "week" && (
          <CoordinationWeekView
            grid={grid}
            loading={gridLoading}
            error={gridError}
            selectedDate={selectedDate}
            onDayClick={handleDaySelect}
          />
        )}

        {viewMode === "month" && (
          <MonthView
            selectedDate={selectedDate}
            today={todayIso}
            firstDayOfWeek={firstDayOfWeek}
            onSelectDay={handleDaySelect}
            onPrevMonth={() => dispatch(setSelectedDate(addMonths(selectedDate, -1)))}
            onNextMonth={() => dispatch(setSelectedDate(addMonths(selectedDate, 1)))}
          />
        )}
      </div>
    </div>
  );
}

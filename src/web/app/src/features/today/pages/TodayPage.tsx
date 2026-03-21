import { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { setSelectedDate } from "../../../store/todaySlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { weekApi } from "../api/weekApi";
import type { WeeklyGridResponse, DayTypeSummary } from "../types";
import type { ApiError } from "../../../api/domusmindApi";
import { EditEntityModal, type EditableEntityType } from "../../editors/components/EditEntityModal";
import { PlanningAddModal } from "../../planning/components/modals/PlanningAddModal";
import { TodayBoard } from "../components/board/TodayBoard";
import { MonthView } from "../components/MonthView";
import { WeeklyHouseholdGrid } from "../components/grid/WeeklyHouseholdGrid";
import { TimelineRuler } from "../components/timeline/TimelineRuler";

// ---- Date helpers ----

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

// ---- Shared constants ----

const DAY_ORDER = [
  "sunday",
  "monday",
  "tuesday",
  "wednesday",
  "thursday",
  "friday",
  "saturday",
];


export function TodayPage() {
  const dispatch = useAppDispatch();
  const { t, i18n } = useTranslation("today");

  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;
  const members = useAppSelector((s) => s.household.members);

  const selectedDate = useAppSelector((s) => s.today.selectedDate);
  const { data: timelineData, status: timelineStatus, error: timelineError } =
    useAppSelector((s) => s.timeline);

  // Reset to today every time the user enters this page.
  // selectedDate is shared Redux state and would otherwise persist whatever
  // date the user had scrolled to in a previous visit.
  useEffect(() => {
    dispatch(setSelectedDate(toIsoDate(new Date())));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

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
  const [editTarget, setEditTarget] = useState<{ type: EditableEntityType; id: string } | null>(
    null,
  );
  const [addModal, setAddModal] = useState(false);

  // Month grid cache: weekStart → WeeklyGridResponse (for month calendar dots)
  const [monthGridCache, setMonthGridCache] = useState<Record<string, WeeklyGridResponse>>({});
  // Track which week starts have already been requested so we never double-fetch.
  // A ref is used intentionally — it doesn't need to be part of any effect's dep array.
  const requestedMonthWeeks = useRef<Set<string>>(new Set());

  // Reset the month grid cache and request tracker when the active household changes
  useEffect(() => {
    setMonthGridCache({});
    requestedMonthWeeks.current.clear();
  }, [familyId]);

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

  // Fetch weekly grids for all weeks visible in the month calendar.
  // Uses requestedMonthWeeks ref to track already-requested weeks, so
  // monthGridCache itself does not need to be in the dependency array.
  useEffect(() => {
    if (midTermView !== "month" || !familyId) return;

    // Reset the request tracker when familyId changes so fresh data is loaded
    // (handled implicitly: when familyId changes the cache is already empty via state reset)

    // Compute first visible date for the month grid
    const anchor = new Date(monthAnchor + "T00:00:00");
    const year = anchor.getFullYear();
    const month = anchor.getMonth();
    const firstDayIdx = Math.max(
      0,
      DAY_ORDER.indexOf((firstDayOfWeek ?? "monday").toLowerCase()),
    );
    const firstOfMonth = new Date(year, month, 1);
    let startPad = firstOfMonth.getDay() - firstDayIdx;
    if (startPad < 0) startPad += 7;
    const firstVisible = new Date(year, month, 1 - startPad);
    const lastOfMonth = new Date(year, month + 1, 0);

    // Collect week starts for all visible weeks
    const weekStarts: string[] = [];
    let cursor = new Date(firstVisible);
    while (cursor <= lastOfMonth) {
      weekStarts.push(toIsoDate(cursor));
      cursor = new Date(cursor.getFullYear(), cursor.getMonth(), cursor.getDate() + 7);
    }

    // Only request weeks that haven't been fetched/requested yet
    const missing = weekStarts.filter(
      (ws) => !requestedMonthWeeks.current.has(ws),
    );
    if (missing.length === 0) return;

    // Mark them as requested immediately so concurrent effect runs don't double-fetch
    missing.forEach((ws) => requestedMonthWeeks.current.add(ws));

    Promise.all(missing.map((ws) => weekApi.getWeeklyGrid(familyId, ws)))
      .then((grids) => {
        setMonthGridCache((prev) => {
          const next = { ...prev };
          grids.forEach((g, i) => {
            next[missing[i]] = g;
          });
          return next;
        });
      })
      .catch(() => {
        // silently ignore — dots are best-effort; remove from requested so retry is possible
        missing.forEach((ws) => requestedMonthWeeks.current.delete(ws));
      });
  }, [midTermView, monthAnchor, familyId, firstDayOfWeek]);

  // Load timeline data (for the ruler only — month dots now use weekly grids).
  // Re-fetches whenever familyId changes or if a previous attempt failed.
  useEffect(() => {
    if (!familyId) return;
    if (timelineStatus === "idle" || timelineStatus === "error") {
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

  function handleItemClick(type: "event" | "task" | "routine", id: string) {
    setEditTarget({ type, id });
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

  // ---- Month calendar summary: per-day type counts from weekly grid cache ----
  const monthDaySummary = useMemo(() => {
    const summary: Record<string, DayTypeSummary> = {};
    if (Object.keys(monthGridCache).length === 0) return summary;

    for (const weekGrid of Object.values(monthGridCache)) {
      const allCells = [
        ...(weekGrid.sharedCells ?? []),
        ...((weekGrid.members ?? []).flatMap((m) => m.cells)),
      ];
      for (const cell of allCells) {
        const dayKey = cell.date.slice(0, 10);
        if (!summary[dayKey]) summary[dayKey] = { events: 0, tasks: 0, routines: 0 };
        summary[dayKey].events += cell.events?.length ?? 0;
        summary[dayKey].tasks += cell.tasks?.length ?? 0;
        summary[dayKey].routines += cell.routines?.length ?? 0;
      }
    }
    return summary;
  }, [monthGridCache]);

  return (
    <div className="page-content coord-page">
      {/* ── Section 1: Day View (always visible, nav integrated inside) ── */}
      <TodayBoard
        grid={grid}
        selectedDate={selectedDate}
        loading={gridLoading}
        error={gridError}
        isToday={isToday}
        onPrevDay={handlePrevDay}
        onNextDay={handleNextDay}
        onToday={handleToday}
        onItemClick={handleItemClick}
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
              className="btn btn-ghost btn-sm coord-nav-btn"
              onClick={handlePrevWeek}
              type="button"
              aria-label={t("nav.prevWeek")}
            >
              ‹
            </button>
            <span className="coord-month-label">{weekNavLabel}</span>
            <button
              className="btn btn-ghost btn-sm coord-nav-btn"
              onClick={handleNextWeek}
              type="button"
              aria-label={t("nav.nextWeek")}
            >
              ›
            </button>
          </div>
        )}

        {midTermView === "week" && (
          <WeeklyHouseholdGrid
            grid={grid}
            loading={gridLoading}
            error={gridError}
            selectedDate={selectedDate}
            onDayClick={handleDaySelect}
            onItemClick={handleItemClick}
          />
        )}

        {midTermView === "month" && (
          <div className="coord-month-wrap">
            <MonthView
              selectedDate={selectedDate}
              today={todayIso}
              firstDayOfWeek={firstDayOfWeek}
              displayAnchor={monthAnchor}
              daySummary={monthDaySummary}
              onSelectDay={handleDaySelect}
              onPrevMonth={() => setMonthAnchor(addMonths(monthAnchor, -1))}
              onNextMonth={() => setMonthAnchor(addMonths(monthAnchor, 1))}
            />
          </div>
        )}
      </div>

      {/* ── Section 3: Horizontal Timeline Ruler ── */}
      <div className="coord-ruler-section">
        <TimelineRuler
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
      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={async () => {
            setEditTarget(null);
            if (familyId) {
              await Promise.all([
                fetchGrid(weekStartForSelected),
                dispatch(fetchTimeline({ familyId })),
              ]);
            }
          }}
        />
      )}
      <button
        className="fab-add"
        type="button"
        aria-label={t("addItem")}
        onClick={() => setAddModal(true)}
      >
        +
      </button>
      {addModal && (
        <PlanningAddModal
          familyId={familyId}
          members={members}
          onClose={() => setAddModal(false)}
          onSuccess={() => {
            setAddModal(false);
            fetchGrid(weekStartForSelected);
          }}
        />
      )}
    </div>
  );
}

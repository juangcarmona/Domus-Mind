import { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { setSelectedDate } from "../../../store/coordinationSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { weekApi } from "../../week/api/weekApi";
import type { WeeklyGridResponse } from "../../week/types";
import type { ApiError } from "../../../api/domusmindApi";
import { EditEntityModal, type EditableEntityType } from "../../../components/EditEntityModal";
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

/** Dot color used for unassigned entries (no assignee, no participants). */
const UNASSIGNED_DOT_COLOR = "var(--muted)";

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
  const [editTarget, setEditTarget] = useState<{ type: EditableEntityType; id: string } | null>(
    null,
  );

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

  // ---- Month calendar dots: per-day member colors from weekly grid cache ----
  const monthDayDots = useMemo(() => {
    const dots: Record<string, string[]> = {};
    if (Object.keys(monthGridCache).length === 0) return dots;

    const memberIndexMap = new Map<string, number>();
    members.forEach((m, idx) => memberIndexMap.set(m.memberId, idx));

    for (const weekGrid of Object.values(monthGridCache)) {
      // Household-level shared cells (plans / tasks with no individual assignee)
      for (const cell of weekGrid.sharedCells ?? []) {
        const dayKey = cell.date.slice(0, 10);
        const hasItems =
          (cell.events?.length ?? 0) > 0 || (cell.tasks?.length ?? 0) > 0;
        if (hasItems) {
          if (!dots[dayKey]) dots[dayKey] = [];
          if (!dots[dayKey].includes(UNASSIGNED_DOT_COLOR)) {
            dots[dayKey].push(UNASSIGNED_DOT_COLOR);
          }
        }
      }
      // Per-member cells
      for (const member of weekGrid.members ?? []) {
        const idx = memberIndexMap.get(member.memberId) ?? 0;
        const color = getMemberColor(idx);
        for (const cell of member.cells) {
          const dayKey = cell.date.slice(0, 10);
          const hasItems =
            (cell.events?.length ?? 0) > 0 || (cell.tasks?.length ?? 0) > 0;
          if (hasItems) {
            if (!dots[dayKey]) dots[dayKey] = [];
            if (!dots[dayKey].includes(color)) {
              dots[dayKey].push(color);
            }
          }
        }
      }
    }
    return dots;
  }, [monthGridCache, members]);

  return (
    <div className="page-content coord-page">
      {/* ── Section 1: Day View (always visible, nav integrated inside) ── */}
      <DayView
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
          <CoordinationWeekView
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
              dayDots={monthDayDots}
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
    </div>
  );
}

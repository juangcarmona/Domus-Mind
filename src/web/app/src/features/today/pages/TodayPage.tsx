import { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { setSelectedDate } from "../../../store/todaySlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { weekApi } from "../api/weekApi";
import type { WeeklyGridResponse } from "../types";
import type { ApiError } from "../../../api/domusmindApi";
import { EditEntityModal, type EditableEntityType } from "../../editors/components/EditEntityModal";
import { PlanningAddModal } from "../../planning/components/modals/PlanningAddModal";
import { TodayBoard } from "../components/board/TodayBoard";
import { MonthView } from "../components/MonthView";
import { WeeklyHouseholdGrid } from "../components/grid/WeeklyHouseholdGrid";
import { TimelineRuler } from "../components/timeline/TimelineRuler";
import { startOfWeek, toIsoDate, addDays, addMonths } from "../utils/dateUtils";
import { useMonthGridCache } from "../hooks/useMonthGridCache";


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

  // Month view anchor - navigated independently of selectedDate
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

  const { monthDaySummary } = useMonthGridCache(
    familyId,
    monthAnchor,
    firstDayOfWeek,
    midTermView === "month",
  );

  // Load timeline data (for the ruler only - month dots now use weekly grids).
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

        {/* Week nav - same style as month nav */}
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

// Legacy imports removed — this file has been replaced by the temporal
// workbench implementation below. The old list-manager shell (tabs,
// PlanningOverview, PlansTab, RoutinesTab, TasksTab) is no longer used here.
// Those components remain in the repo temporarily until Phase 3+.

import { useState, useCallback, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAppSelector } from "../../../store/hooks";
import { weekApi } from "../../today/api/weekApi";
import type { WeeklyGridResponse } from "../../today/types";
import type { ApiError } from "../../../api/domusmindApi";
import { toIsoDate, addDays, addMonths, startOfWeek } from "../../today/utils/dateUtils";
import { useMonthGridCache } from "../../today/hooks/useMonthGridCache";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { EditEntityModal, type EditableEntityType } from "../../editors/components/EditEntityModal";
import {
  PlanningAddModal,
  type PlanningAddModalDefaults,
} from "../components/modals/PlanningAddModal";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { WeeklyHouseholdGrid } from "../../today/components/grid/WeeklyHouseholdGrid";
import { MonthView } from "../../today/components/MonthView";
import { PlanningHeader, type PlanningView } from "../components/PlanningHeader";
import { PlanningDayCanvas } from "../components/PlanningDayCanvas";
import { PlanningInspectorContent, type SelectedPlanItem } from "../components/PlanningInspectorContent";
import "../planning.css";

/**
 * Locate an item in the weekly grid response by type and id.
 * Returns a SelectedPlanItem for the inspector / bottom sheet.
 */
function findGridItem(
  grid: WeeklyGridResponse | null,
  type: "event" | "task" | "routine",
  id: string,
): SelectedPlanItem | null {
  if (!grid) return null;

  const allCells = [
    ...grid.sharedCells,
    ...grid.members.flatMap((m) => m.cells),
  ];

  for (const cell of allCells) {
    if (type === "event") {
      const e = (cell.events ?? []).find((ev) => ev.eventId === id);
      if (e) {
        return {
          type: "event",
          id: e.eventId,
          title: e.title,
          date: e.date,
          time: e.time,
          endTime: e.endTime,
          status: e.status,
          subtitle: e.participants?.map((p) => p.displayName).join(", ") || null,
          color: e.color,
        };
      }
    }
    if (type === "task") {
      const tk = (cell.tasks ?? []).find((t) => t.taskId === id);
      if (tk) {
        return {
          type: "task",
          id: tk.taskId,
          title: tk.title,
          date: tk.dueDate,
          time: null,
          status: tk.status,
          subtitle: null,
          color: tk.color,
        };
      }
    }
    if (type === "routine") {
      const r = (cell.routines ?? []).find((rt) => rt.routineId === id);
      if (r) {
        return {
          type: "routine",
          id: r.routineId,
          title: r.name,
          date: null,
          time: r.time,
          endTime: r.endTime,
          status: undefined,
          subtitle: r.frequency,
          color: r.color,
        };
      }
    }
  }

  // Fallback: item not in current grid window
  return { type, id, title: id, date: null, time: null };
}

export function PlanningPage() {
  const { t } = useTranslation("agenda");

  const isMobile = useIsMobile();
  const family = useAppSelector((s) => s.household.family);
  const members = useAppSelector((s) => s.household.members);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;

  // ---- View and date state ----
  const [view, setView] = useState<PlanningView>("week"); // Week is the default
  const [selectedDate, setSelectedDate] = useState<string>(toIsoDate(new Date()));

  // ---- Week grid data ----
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);

  // ---- Selected item (for inspector / bottom sheet) ----
  const [selectedItem, setSelectedItem] = useState<SelectedPlanItem | null>(null);

  // ---- Modals ----
  const [showAddModal, setShowAddModal] = useState(false);
  const [addModalDefaults, setAddModalDefaults] = useState<PlanningAddModalDefaults>({});
  const [editTarget, setEditTarget] = useState<{ type: EditableEntityType; id: string } | null>(
    null,
  );

  // ---- Month anchor (can drift independently from selected date) ----
  const [monthAnchor, setMonthAnchor] = useState<string>(selectedDate);
  useEffect(() => {
    setMonthAnchor(selectedDate);
  }, [selectedDate]);

  // ---- Month grid cache ----
  const { monthDaySummary } = useMonthGridCache(
    familyId,
    monthAnchor,
    firstDayOfWeek,
    view === "month",
  );

  // ---- Week start for selected date ----
  const weekStartForSelected = toIsoDate(
    startOfWeek(new Date(selectedDate + "T00:00:00"), firstDayOfWeek),
  );

  // ---- Grid fetching ----
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
    if (familyId) {
      fetchGrid(weekStartForSelected);
    }
  }, [weekStartForSelected, fetchGrid, familyId]);

  // ---- Navigation ----
  function handlePrev() {
    if (view === "week") setSelectedDate(addDays(selectedDate, -7));
    else if (view === "day") setSelectedDate(addDays(selectedDate, -1));
    else setSelectedDate(addMonths(selectedDate, -1));
  }

  function handleNext() {
    if (view === "week") setSelectedDate(addDays(selectedDate, 7));
    else if (view === "day") setSelectedDate(addDays(selectedDate, 1));
    else setSelectedDate(addMonths(selectedDate, 1));
  }

  function handleToday() {
    setSelectedDate(toIsoDate(new Date()));
  }

  function handleDayClick(date: string) {
    setSelectedDate(date);
    if (view !== "day") setView("day");
  }

  // ---- Item interaction ----
  function handleItemClick(type: "event" | "task" | "routine", id: string) {
    setSelectedItem(findGridItem(grid, type, id));
  }

  function handleSlotClick(time: string) {
    setAddModalDefaults({ initialStartDate: selectedDate, initialStartClock: time });
    setShowAddModal(true);
  }

  function handleAddPlan(defaults?: PlanningAddModalDefaults) {
    setAddModalDefaults(defaults ?? {});
    setShowAddModal(true);
  }

  function handleEditItem(type: string, id: string) {
    setEditTarget({ type: type as EditableEntityType, id });
    setSelectedItem(null);
  }

  function handleModalSuccess() {
    fetchGrid(weekStartForSelected);
  }

  const inspectorContent = (
    <PlanningInspectorContent
      selectedDate={selectedDate}
      view={view}
      firstDayOfWeek={firstDayOfWeek}
      selectedItem={selectedItem}
      onSelectDate={setSelectedDate}
      onEditItem={handleEditItem}
      onClearSelection={() => setSelectedItem(null)}
    />
  );

  return (
    <div className="planning-surface l-surface">
      <PlanningHeader
        selectedDate={selectedDate}
        view={view}
        firstDayOfWeek={firstDayOfWeek}
        onViewChange={setView}
        onPrev={handlePrev}
        onNext={handleNext}
        onToday={handleToday}
        onAddPlan={() => handleAddPlan({ initialStartDate: selectedDate })}
      />

      <div className="planning-body l-surface-body">
        {/* Calendar canvas — fills all available space */}
        <div className="planning-canvas l-surface-content">
          {view === "week" && (
            <WeeklyHouseholdGrid
              grid={grid}
              loading={gridLoading}
              error={gridError}
              selectedDate={selectedDate}
              onDayClick={handleDayClick}
              onItemClick={handleItemClick}
            />
          )}
          {view === "day" && (
            <PlanningDayCanvas
              grid={grid}
              selectedDate={selectedDate}
              loading={gridLoading}
              error={gridError}
              onItemClick={handleItemClick}
              onSlotClick={handleSlotClick}
            />
          )}
          {view === "month" && (
            <MonthView
              selectedDate={selectedDate}
              today={toIsoDate(new Date())}
              firstDayOfWeek={firstDayOfWeek}
              displayAnchor={monthAnchor}
              daySummary={monthDaySummary}
              onSelectDay={handleDayClick}
              onPrevMonth={() => setMonthAnchor(addMonths(monthAnchor, -1))}
              onNextMonth={() => setMonthAnchor(addMonths(monthAnchor, 1))}
            />
          )}
        </div>

        {/* Desktop inspector — hidden on mobile via InspectorPanel.css */}
        <InspectorPanel title="Planning">
          {inspectorContent}
        </InspectorPanel>
      </div>

      {/* Mobile bottom sheet for selected item detail */}
      {isMobile && (
        <BottomSheetDetail
          open={!!selectedItem}
          onClose={() => setSelectedItem(null)}
          title={selectedItem?.title ?? ""}
        >
          {inspectorContent}
        </BottomSheetDetail>
      )}

      {/* Create modal */}
      {showAddModal && familyId && (
        <PlanningAddModal
          familyId={familyId}
          members={members}
          initialStep="plan"
          defaults={addModalDefaults}
          onClose={() => setShowAddModal(false)}
          onSuccess={() => {
            setShowAddModal(false);
            handleModalSuccess();
          }}
        />
      )}

      {/* Edit modal */}
      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={() => {
            setEditTarget(null);
            handleModalSuccess();
          }}
        />
      )}
    </div>
  );
}

// The following functions/components are no longer part of this file.
// They are defined in their own modules and not referenced here.
// The old list-manager exports below were removed in Phase 2 of the
// surface-system reboot. See:
//   features/planning/components/PlansTab.tsx
//   features/planning/components/RoutinesTab.tsx
//   features/planning/components/TasksTab.tsx

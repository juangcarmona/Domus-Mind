import { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { setSelectedDate } from "../../../store/todaySlice";
import { weekApi } from "../api/weekApi";
import type { WeeklyGridResponse, WeeklyGridCell } from "../types";
import type { ApiError } from "../../../api/domusmindApi";
import { EditEntityModal, type EditableEntityType } from "../../editors/components/EditEntityModal";
import { PlanningAddModal } from "../../planning/components/modals/PlanningAddModal";
import { TodayBoard } from "../components/board/TodayBoard";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { startOfWeek, toIsoDate, addDays } from "../utils/dateUtils";
import { ENTRY_GLYPH } from "../utils/calendarEntry";
import { useIsMobile } from "../../../hooks/useIsMobile";


// ----------------------------------------------------------------
// Selected item model — used by inspector and bottom sheet
// ----------------------------------------------------------------

interface TodaySelectedItem {
  type: "event" | "task" | "routine";
  id: string;
  title: string;
  date: string | null;
  time: string | null;
  endTime?: string | null;
  status?: string;
  subtitle: string | null;
  color?: string | null;
}

function findGridItem(
  grid: WeeklyGridResponse | null,
  type: "event" | "task" | "routine",
  id: string,
): TodaySelectedItem | null {
  if (!grid) return null;

  const allCells: WeeklyGridCell[] = [
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

  return { type, id, title: id, date: null, time: null, subtitle: null };
}

// ----------------------------------------------------------------
// Item detail — shared by InspectorPanel and BottomSheetDetail
// ----------------------------------------------------------------

function TodayItemDetail({
  item,
  onEdit,
}: {
  item: TodaySelectedItem;
  onEdit: (type: string, id: string) => void;
}) {
  const { t } = useTranslation("today");
  const glyph = ENTRY_GLYPH[
    item.type === "event" ? "event" : item.type === "task" ? "task" : "routine"
  ];

  return (
    <div className="today-item-detail">
      <p className="today-item-detail-type">
        <span className="today-item-detail-glyph">{glyph}</span>
        {item.type}
      </p>
      <p className="today-item-detail-title">{item.title}</p>
      {item.date && <p className="today-item-detail-meta">{item.date}</p>}
      {item.time && (
        <p className="today-item-detail-meta">
          {item.time}
          {item.endTime ? ` – ${item.endTime}` : ""}
        </p>
      )}
      {item.status && (
        <p className="today-item-detail-status">{item.status}</p>
      )}
      {item.subtitle && (
        <p className="today-item-detail-meta">{item.subtitle}</p>
      )}
      <button
        className="btn btn-secondary btn-sm today-item-detail-edit"
        type="button"
        onClick={() => onEdit(item.type, item.id)}
      >
        {t("item.edit")}
      </button>
    </div>
  );
}

// ----------------------------------------------------------------
// Page
// ----------------------------------------------------------------

export function TodayPage() {
  const dispatch = useAppDispatch();
  const { t } = useTranslation("today");
  const isMobile = useIsMobile();

  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;
  const members = useAppSelector((s) => s.household.members);

  const selectedDate = useAppSelector((s) => s.today.selectedDate);

  // Reset to today every time the user enters this page.
  // selectedDate is shared Redux state and would otherwise persist whatever
  // date the user had scrolled to in a previous visit.
  useEffect(() => {
    dispatch(setSelectedDate(toIsoDate(new Date())));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Grid data for the day board
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);

  // Inspector / bottom-sheet: selected item
  const [selectedItem, setSelectedItem] = useState<TodaySelectedItem | null>(null);

  // Edit modal: opened from inspector / sheet
  const [editTarget, setEditTarget] = useState<{ type: EditableEntityType; id: string } | null>(
    null,
  );

  // Add modal
  const [addModal, setAddModal] = useState(false);

  // Compute week start for grid fetching
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

  const todayIso = toIsoDate(new Date());
  const isToday = selectedDate === todayIso;

  // ---- Interaction handlers ----

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
    setSelectedItem(findGridItem(grid, type, id));
  }

  function handleEditItem(type: string, id: string) {
    setEditTarget({ type: type as EditableEntityType, id });
    setSelectedItem(null);
  }

  const navigate = useNavigate();

  function handleMemberClick(memberId: string) {
    navigate(`/agenda/members/${memberId}?date=${selectedDate}`);
  }

  function handleSharedClick() {
    navigate(`/agenda/shared?date=${selectedDate}`);
  }

  return (
    <div className="today-surface l-surface">
      <div className="today-surface-body l-surface-body">
        {/* Main day board — fills all available space */}
        <div className="l-surface-content">
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
            onMemberClick={handleMemberClick}
            onSharedClick={handleSharedClick}
          />
        </div>

        {/* Desktop inspector — shows selected item detail */}
        {!isMobile && selectedItem && (
          <InspectorPanel
            title={selectedItem.title}
            onClose={() => setSelectedItem(null)}
          >
            <TodayItemDetail item={selectedItem} onEdit={handleEditItem} />
          </InspectorPanel>
        )}
      </div>

      {/* Mobile bottom sheet — contextual item detail */}
      {isMobile && (
        <BottomSheetDetail
          open={!!selectedItem}
          onClose={() => setSelectedItem(null)}
          title={selectedItem?.title ?? ""}
        >
          {selectedItem && (
            <TodayItemDetail item={selectedItem} onEdit={handleEditItem} />
          )}
        </BottomSheetDetail>
      )}

      {/* Edit modal — opened from inspector / sheet */}
      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={async () => {
            setEditTarget(null);
            setSelectedItem(null);
            await fetchGrid(weekStartForSelected);
          }}
        />
      )}

      {/* FAB — quick add */}
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

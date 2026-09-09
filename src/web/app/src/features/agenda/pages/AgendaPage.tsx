import { useState, useCallback, useEffect } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { weekApi } from "../../agenda-today/api/weekApi";
import type { WeeklyGridResponse } from "../../agenda-today/types";
import type { ApiError } from "../../../api/domusmindApi";
import { externalCalendarApi } from "../../../api/externalCalendarApi";
import { fetchPlans } from "../../../store/plansSlice";
import { fetchRoutines } from "../../../store/routinesSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { PlanningAddModal } from "../../agenda-planning/components/modals/PlanningAddModal";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { AgendaHeader, type AgendaView } from "../components/AgendaHeader";
import { MemberDayView } from "../components/MemberDayView";
import { MemberWeekView } from "../components/MemberWeekView";
import { MemberMonthView } from "../components/MemberMonthView";
import { AgendaSelectedDayDetail } from "../components/AgendaSelectedDayDetail";
import { TodayBoard } from "../../agenda-today/components/board/TodayBoard";
import { WeeklyHouseholdGrid } from "../../agenda-today/components/grid/WeeklyHouseholdGrid";
import { PlanningMobileWeekStrip } from "../../agenda-planning/components/PlanningMobileWeekStrip";
import { MonthView } from "../../agenda-today/components/MonthView";
import { useMonthGridCache } from "../../agenda-today/hooks/useMonthGridCache";
import type { CalendarEntry } from "../../agenda-today/utils/calendarEntry";
import { normalizeCellItems } from "../../agenda-today/utils/calendarEntry";
import {
  AgendaInlineEntityEditor,
  AgendaProjectedListItemBridge,
  AgendaReadOnlyEntryDetail,
} from "../../../components/agenda/AgendaInspectorContent";
import {
  toIsoDate,
  addDays,
  addMonths,
  startOfWeek,
} from "../../agenda-today/utils/dateUtils";
import "../agenda.css";
import "../../editors/editors.css";

const VALID_MODES: AgendaView[] = ["day", "week", "month"];

export function AgendaPage() {
  const dispatch = useAppDispatch();
  const { memberId } = useParams<{ memberId?: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const { t } = useTranslation("agenda");
  const isMobile = useIsMobile();

  const isHousehold = memberId === undefined;

  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;
  const members = useAppSelector((s) => s.household.members);

  const householdMember = isHousehold
    ? undefined
    : members.find((m) => m.memberId === memberId);

  const todayIso = toIsoDate(new Date());
  const initialDate = searchParams.get("date") ?? todayIso;
  const [selectedDate, setSelectedDate] = useState<string>(initialDate);

  const modeParam = searchParams.get("mode");
  const view: AgendaView = VALID_MODES.includes(modeParam as AgendaView)
    ? (modeParam as AgendaView)
    : isHousehold ? "day" : "week";

  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);

  const [selectedEntry, setSelectedEntry] = useState<CalendarEntry | null>(null);
  const [selectionHydrationError, setSelectionHydrationError] = useState<string | null>(null);
  const [externalEntryLoading, setExternalEntryLoading] = useState(false);

  const [showAddModal, setShowAddModal] = useState(false);
  const [addModalTime, setAddModalTime] = useState<string | undefined>();

  const [monthAnchor, setMonthAnchor] = useState<string>(selectedDate);
  useEffect(() => {
    if (selectedDate.slice(0, 7) !== monthAnchor.slice(0, 7)) {
      setMonthAnchor(selectedDate);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDate]);

  const { monthDaySummary } = useMonthGridCache(
    familyId,
    monthAnchor,
    firstDayOfWeek,
    isHousehold && view === "month",
  );

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

  useEffect(() => {
    if (familyId) {
      fetchGrid(weekStartForSelected);
    }
  }, [weekStartForSelected, fetchGrid, familyId]);

  useEffect(() => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        next.set("date", selectedDate);
        return next;
      },
      { replace: true },
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDate]);

  useEffect(() => {
    setSelectedEntry(null);
    setSelectionHydrationError(null);
    setExternalEntryLoading(false);
  }, [selectedDate, view]);

  function handleViewChange(newView: AgendaView) {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (newView === "day") next.delete("mode");
        else next.set("mode", newView);
        return next;
      },
      { replace: false },
    );
  }

  function handleScopeChange(newScope: "household" | string) {
    const targetView = newScope !== "household" ? "week" : view;
    const params = new URLSearchParams();
    if (targetView !== "day") params.set("mode", targetView);
    params.set("date", selectedDate);
    const qs = params.toString() ? `?${params.toString()}` : "";
    if (newScope === "household") {
      navigate(`/agenda${qs}`);
    } else {
      navigate(`/agenda/members/${newScope}${qs}`);
    }
  }

  function handlePrev() {
    if (view === "day") setSelectedDate(addDays(selectedDate, -1));
    else if (view === "week") setSelectedDate(addDays(selectedDate, -7));
    else setSelectedDate(addMonths(selectedDate, -1));
  }

  function handleNext() {
    if (view === "day") setSelectedDate(addDays(selectedDate, 1));
    else if (view === "week") setSelectedDate(addDays(selectedDate, 7));
    else setSelectedDate(addMonths(selectedDate, 1));
  }

  function handleToday() {
    setSelectedDate(todayIso);
  }

  function findEntryAcrossGrid(
    type: "event" | "task" | "routine" | "list-item",
    id: string,
  ): CalendarEntry | null {
    if (!grid) return null;
    const memberCells = isHousehold
      ? grid.members.flatMap((m) => m.cells)
      : (grid.members.find((m) => m.memberId === memberId)?.cells ?? []);
    const cells = [...(grid.sharedCells ?? []), ...memberCells];

    for (const cell of cells) {
      const entries = normalizeCellItems(cell);
      const found = entries.find((e) => e.id === id && e.sourceType === type);
      if (found) return found;
    }
    return null;
  }

  function handleItemClick(type: "event" | "task" | "routine" | "list-item", id: string) {
    setSelectedEntry(null);
    setSelectionHydrationError(null);

    const found = findEntryAcrossGrid(type, id);
    if (found) {
      setSelectedEntry(found);
      return;
    }

    if (type === "list-item") {
      setSelectionHydrationError(t("item.listItemNotFound"));
      return;
    }

    if (type === "event") {
      if (!familyId || !memberId) {
        setSelectionHydrationError(t("item.entryNotFound"));
        return;
      }

      setExternalEntryLoading(true);
      externalCalendarApi.getExternalEntry(familyId, memberId, id)
        .then((res) => {
          const entry: CalendarEntry = {
            id: res.entryId,
            sourceType: "event",
            displayType: "event",
            title: res.title,
            time: res.time ?? null,
            endTime: res.endTime ?? null,
            date: res.date,
            endDate: res.endDate ?? null,
            isAllDay: res.isAllDay,
            subtitle: null,
            status: res.status,
            color: null,
            isCompleted: res.status === "Cancelled",
            isOverdue: false,
            isReadOnly: true,
            sourceLabel: res.providerLabel ?? undefined,
            openInProviderUrl: res.openInProviderUrl ?? undefined,
            calendarName: res.calendarName ?? undefined,
            location: res.location ?? undefined,
          };
          setSelectedEntry(entry);
        })
        .catch((err: { status?: number }) => {
          if (err?.status === 404) {
            setSelectionHydrationError(t("item.entryNotFound"));
          } else {
            setSelectionHydrationError(t("item.entryLoadFailed"));
          }
        })
        .finally(() => setExternalEntryLoading(false));
      return;
    }

    setSelectionHydrationError(t("item.entryLoadFailed"));
  }

  function handleOpenListItemInLists(listId: string, itemId: string) {
    navigate(`/lists/${listId}?itemId=${encodeURIComponent(itemId)}`);
  }

  function handleDayDrill(date: string) {
    setSelectedDate(date);
    handleViewChange("day");
  }

  function handleMonthSelectDate(date: string) {
    setSelectedDate(date);
  }

  function handleMemberClick(clickedMemberId: string) {
    handleScopeChange(clickedMemberId);
  }

  function handleAddEntry(time?: string) {
    setAddModalTime(time);
    setShowAddModal(true);
  }

  async function handleModalSuccess() {
    await fetchGrid(weekStartForSelected);
  }

  async function handleInlineEditorSaved() {
    if (!familyId || !selectedEntry) {
      return;
    }

    if (selectedEntry.sourceType === "task") {
      await dispatch(fetchTimeline({ familyId }));
    } else if (selectedEntry.sourceType === "routine") {
      await dispatch(fetchRoutines(familyId));
    } else if (selectedEntry.sourceType === "event") {
      await dispatch(fetchPlans({ familyId }));
    }

    await fetchGrid(weekStartForSelected);

    const refreshed = findEntryAcrossGrid(selectedEntry.sourceType, selectedEntry.id);
    if (refreshed) {
      setSelectedEntry(refreshed);
    }
  }

  const memberRow = isHousehold
    ? null
    : (grid?.members.find((m) => m.memberId === memberId) ?? null);

  function renderInspectorBody() {
    if (!selectedEntry) return null;

    if (selectedEntry.sourceType === "list-item") {
      return (
        <AgendaProjectedListItemBridge
          entry={selectedEntry}
          onOpenInLists={handleOpenListItemInLists}
        />
      );
    }

    if (!selectedEntry.isReadOnly) {
      return (
        <AgendaInlineEntityEditor
          entry={selectedEntry}
          familyId={familyId}
          members={members}
          onCancel={() => setSelectedEntry(null)}
          onSaved={handleInlineEditorSaved}
        />
      );
    }

    return <AgendaReadOnlyEntryDetail entry={selectedEntry} />;
  }

  function renderInspectorContent() {
    if (externalEntryLoading) {
      return <p className="agenda-inspector-warning">{t("item.externalEntryLoading")}</p>;
    }
    if (selectionHydrationError) {
      return <p className="agenda-inspector-warning">{selectionHydrationError}</p>;
    }

    const body = renderInspectorBody();
    if (!body) {
      return <p className="agenda-inspector-warning">{t("item.entryLoadFailed")}</p>;
    }
    return body;
  }

  const scope = isHousehold ? "household" : (memberId ?? "");
  const inspectorTitle = selectedEntry?.title ?? "";

  const headerMembers = members
    .filter((m) => ["Adult", "Child", "Caregiver"].includes(m.role ?? ""))
    .map((m) => ({
      memberId: m.memberId,
      name: m.name,
      avatarInitial: m.avatarInitial,
      avatarIconId: m.avatarIconId,
      avatarColorId: m.avatarColorId,
    }));

  const householdLabel = family?.name ?? t("household", "Household");

  if (!familyId) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }

  if (!isHousehold && memberId && !householdMember) {
    return <p className="error-msg">{t("memberNotFound")}</p>;
  }

  const emptyMemberRow = {
    memberId: memberId ?? "",
    name: householdMember?.name ?? "",
    role: "",
    cells: [],
  };

  return (
    <div className={`agenda-surface l-surface${selectedEntry && !isMobile ? " agenda-surface--inspector" : ""}`}>
      <AgendaHeader
        scope={scope}
        members={headerMembers}
        householdLabel={householdLabel}
        selectedDate={selectedDate}
        view={view}
        firstDayOfWeek={firstDayOfWeek}
        onScopeChange={handleScopeChange}
        onViewChange={handleViewChange}
        onPrev={handlePrev}
        onNext={handleNext}
        onToday={handleToday}
      />

      <div className="agenda-body l-surface-body">
        <div className="agenda-canvas l-surface-content">
          {gridLoading && <div className="loading-wrap">{t("loading")}</div>}
          {gridError && <p className="error-msg">{gridError}</p>}

          {!gridLoading && !gridError && (
            <>
              {isHousehold && view === "day" && (
                <TodayBoard
                  grid={grid}
                  selectedDate={selectedDate}
                  loading={false}
                  error={null}
                  isToday={selectedDate === todayIso}
                  onPrevDay={handlePrev}
                  onNextDay={handleNext}
                  onToday={handleToday}
                  onItemClick={handleItemClick}
                  onMemberClick={handleMemberClick}
                />
              )}
              {isHousehold && view === "week" && isMobile && (
                <PlanningMobileWeekStrip
                  grid={grid}
                  loading={false}
                  error={null}
                  selectedDate={selectedDate}
                  onDaySelect={setSelectedDate}
                  onItemClick={handleItemClick}
                />
              )}
              {isHousehold && view === "week" && !isMobile && (
                <WeeklyHouseholdGrid
                  grid={grid}
                  loading={false}
                  error={null}
                  selectedDate={selectedDate}
                  onDayClick={handleDayDrill}
                  onItemClick={handleItemClick}
                />
              )}
              {isHousehold && view === "month" && (
                <>
                  <MonthView
                    selectedDate={selectedDate}
                    today={todayIso}
                    firstDayOfWeek={firstDayOfWeek}
                    displayAnchor={monthAnchor}
                    daySummary={monthDaySummary}
                    onSelectDay={handleMonthSelectDate}
                    onPrevMonth={() => setMonthAnchor(addMonths(monthAnchor, -1))}
                    onNextMonth={() => setMonthAnchor(addMonths(monthAnchor, 1))}
                  />
                  <AgendaSelectedDayDetail
                    grid={grid}
                    selectedDate={selectedDate}
                    loading={gridLoading}
                    onItemClick={handleItemClick}
                  />
                </>
              )}

              {!isHousehold && view === "day" && (
                <MemberDayView
                  member={memberRow ?? emptyMemberRow}
                  selectedDate={selectedDate}
                  sharedCell={grid?.sharedCells.find((c) => c.date.slice(0, 10) === selectedDate) ?? null}
                  onItemClick={handleItemClick}
                  onSlotClick={(time) => handleAddEntry(time)}
                />
              )}
              {!isHousehold && view === "week" && (
                <MemberWeekView
                  member={memberRow ?? emptyMemberRow}
                  selectedDate={selectedDate}
                  sharedCells={grid?.sharedCells ?? []}
                  onItemClick={handleItemClick}
                  onDaySelect={setSelectedDate}
                  onDayClick={handleDayDrill}
                  onSlotClick={(time) => handleAddEntry(time)}
                />
              )}
              {!isHousehold && view === "month" && (
                <MemberMonthView
                  memberId={memberId ?? null}
                  selectedDate={selectedDate}
                  firstDayOfWeek={firstDayOfWeek}
                  memberRow={memberRow}
                  sharedCells={grid?.sharedCells ?? []}
                  gridLoading={gridLoading}
                  onSelectDay={handleMonthSelectDate}
                  onItemClick={handleItemClick}
                />
              )}
            </>
          )}
        </div>

        {!isMobile && (selectedEntry || selectionHydrationError || externalEntryLoading) && (
          <InspectorPanel
            title={selectedEntry ? inspectorTitle : t("inspector.loadIssue")}
            onClose={() => {
              setSelectedEntry(null);
              setSelectionHydrationError(null);
              setExternalEntryLoading(false);
            }}
          >
            {renderInspectorContent()}
          </InspectorPanel>
        )}
      </div>

      <button
        type="button"
        className="agenda-fab"
        aria-label={t("addEntry")}
        onClick={() => handleAddEntry()}
      >
        +
      </button>

      {isMobile && (selectedEntry || selectionHydrationError || externalEntryLoading) && (
        <BottomSheetDetail
          open
          onClose={() => {
            setSelectedEntry(null);
            setSelectionHydrationError(null);
            setExternalEntryLoading(false);
          }}
          title={selectedEntry?.title ?? t("inspector.loadIssue")}
        >
          {renderInspectorContent()}
        </BottomSheetDetail>
      )}

      {showAddModal && (
        <PlanningAddModal
          familyId={familyId}
          members={members}
          onClose={() => setShowAddModal(false)}
          onSuccess={async () => {
            setShowAddModal(false);
            await handleModalSuccess();
          }}
          defaults={{
            participantMemberIds: isHousehold || !memberId ? [] : [memberId],
            initialStartDate: selectedDate,
            initialStartClock: addModalTime,
          }}
        />
      )}
    </div>
  );
}

export { AgendaPage as MemberAgendaPage };

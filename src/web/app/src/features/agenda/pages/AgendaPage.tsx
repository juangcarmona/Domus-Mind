import { useState, useCallback, useEffect } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppSelector } from "../../../store/hooks";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { weekApi } from "../../today/api/weekApi";
import type { WeeklyGridResponse } from "../../today/types";
import type { ApiError } from "../../../api/domusmindApi";
import { EditEntityModal, type EditableEntityType } from "../../editors/components/EditEntityModal";
import { PlanningAddModal } from "../../planning/components/modals/PlanningAddModal";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { AgendaHeader, type AgendaView } from "../components/AgendaHeader";
import { MemberDayView } from "../components/MemberDayView";
import { MemberWeekView } from "../components/MemberWeekView";
import { MemberMonthView } from "../components/MemberMonthView";
import { AgendaSelectedDayDetail } from "../components/AgendaSelectedDayDetail";
import { TodayBoard } from "../../today/components/board/TodayBoard";
import { WeeklyHouseholdGrid } from "../../today/components/grid/WeeklyHouseholdGrid";
import { PlanningMobileWeekStrip } from "../../planning/components/PlanningMobileWeekStrip";
import { MonthView } from "../../today/components/MonthView";
import { useMonthGridCache } from "../../today/hooks/useMonthGridCache";
import { normalizeCellItems, type CalendarEntry } from "../../today/utils/calendarEntry";
import {
  toIsoDate,
  addDays,
  addMonths,
  startOfWeek,
} from "../../today/utils/dateUtils";
import "../agenda.css";

const VALID_MODES: AgendaView[] = ["day", "week", "month"];

// ----------------------------------------------------------------
// Inspector item display — read-only imported event
// ----------------------------------------------------------------

function ImportedEventDetail({
  entry,
  onClose,
}: {
  entry: CalendarEntry;
  onClose: () => void;
}) {
  const { t } = useTranslation("agenda");

  return (
    <div className="agenda-inspector-item agenda-inspector-item--imported">
      <div className="agenda-inspector-item-provider-badge">
        {entry.sourceLabel ?? t("item.externalCalendar", "External Calendar")}
      </div>
      <p className="agenda-inspector-item-title">{entry.title}</p>
      {entry.time && (
        <p className="agenda-inspector-item-meta">
          {entry.time}
          {entry.endTime ? ` – ${entry.endTime}` : ""}
        </p>
      )}
      {!entry.time && (
        <p className="agenda-inspector-item-meta">
          {t("day.allDay", "All day")}
        </p>
      )}
      {entry.location && (
        <p className="agenda-inspector-item-meta agenda-inspector-item-location">
          {entry.location}
        </p>
      )}
      {entry.subtitle && (
        <p className="agenda-inspector-item-meta">{entry.subtitle}</p>
      )}
      {entry.status && entry.status !== "confirmed" && (
        <p className="agenda-inspector-item-status">{entry.status}</p>
      )}
      <p className="agenda-inspector-item-readonly-note">
        {t("item.importedReadOnly", "Imported · read only")}
      </p>
      <div className="agenda-inspector-item-actions">
        {entry.openInProviderUrl && (
          <a
            className="btn btn-ghost btn-sm"
            href={entry.openInProviderUrl}
            target="_blank"
            rel="noreferrer"
          >
            {t("item.openInProvider", "Open in Outlook")}
          </a>
        )}
        <button type="button" className="btn btn-ghost btn-sm" onClick={onClose}>
          ✕
        </button>
      </div>
    </div>
  );
}

// ----------------------------------------------------------------
// Inspector item display — editable / local entry
// ----------------------------------------------------------------

function AgendaItemDetail({
  entry,
  onEdit,
  onClose,
}: {
  entry: CalendarEntry;
  onEdit: (type: EditableEntityType, id: string) => void;
  onClose: () => void;
}) {
  const { t } = useTranslation("agenda");

  if (entry.isReadOnly) {
    return <ImportedEventDetail entry={entry} onClose={onClose} />;
  }

  return (
    <div className="agenda-inspector-item">
      <p className="agenda-inspector-item-title">{entry.title}</p>
      {entry.time && (
        <p className="agenda-inspector-item-meta">
          {entry.time}
          {entry.endTime ? ` – ${entry.endTime}` : ""}
        </p>
      )}
      {entry.subtitle && (
        <p className="agenda-inspector-item-meta">{entry.subtitle}</p>
      )}
      {entry.status && (
        <p className="agenda-inspector-item-status">{entry.status}</p>
      )}
      <div className="agenda-inspector-item-actions">
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={() => {
            onEdit(entry.sourceType, entry.id);
            onClose();
          }}
        >
          {t("item.edit", "Edit")}
        </button>
        <button type="button" className="btn btn-ghost btn-sm" onClick={onClose}>
          ✕
        </button>
      </div>
    </div>
  );
}

// ----------------------------------------------------------------
// Page
// ----------------------------------------------------------------

/**
 * Unified Agenda surface.
 *
 * Handles both Household scope (/agenda) and Member scope (/agenda/members/:id).
 * Mode (day/week/month) is driven by the ?mode= query param.
 *
 * Replaces TodayPage, PlanningPage, and the old MemberAgendaPage.
 */
export function AgendaPage() {
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

  // ---- Date state (from URL param or today) ----
  const todayIso = toIsoDate(new Date());
  const initialDate = searchParams.get("date") ?? todayIso;
  const [selectedDate, setSelectedDate] = useState<string>(initialDate);

  // ---- View mode (from URL param) ----
  // Member scope defaults to "week" (week-first model).
  // Household scope keeps "day" as the legacy default.
  const modeParam = searchParams.get("mode");
  const view: AgendaView = VALID_MODES.includes(modeParam as AgendaView)
    ? (modeParam as AgendaView)
    : isHousehold ? "day" : "week";

  // ---- Grid state ----
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);

  // ---- Selected calendar entry (inspector / bottom sheet) ----
  const [selectedEntry, setSelectedEntry] = useState<CalendarEntry | null>(null);

  // ---- Modal state ----
  const [editTarget, setEditTarget] = useState<{ type: EditableEntityType; id: string } | null>(
    null,
  );
  const [showAddModal, setShowAddModal] = useState(false);
  const [addModalTime, setAddModalTime] = useState<string | undefined>();

  // ---- Month data (household scope month view) ----
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

  // ---- Grid fetching ----
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

  // Sync ?date= into URL.
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

  // Clear selected entry on date/view change.
  useEffect(() => {
    setSelectedEntry(null);
  }, [selectedDate, view]);

  // ---- Navigation handlers ----

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
    // When switching to a member scope, default to week — week-first model.
    // When switching back to household, keep the current view.
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

  // ---- Item interaction ----

  function handleItemClick(type: "event" | "task" | "routine", id: string) {
    if (!grid) return;

    // Build a flat search corpus over ALL cells in the loaded grid —
    // shared cells AND every member cell across all days.
    //
    // Why this matters for household scope: the TodayBoard renders member rows
    // (which include imported external events). Those events live in member cells,
    // NOT shared cells. The previous implementation only searched shared cells
    // for household scope, so imported events were never found and fell through
    // to setEditTarget, opening the edit modal on a read-only entry.
    //
    // Why this matters for member scope: week-view clicks may be on a day other
    // than selectedDate; we must search all seven cells, not just the current day.
    const allCells = [
      ...(grid.sharedCells ?? []),
      ...(grid.members ?? []).flatMap((m) => m.cells),
    ];

    for (const cell of allCells) {
      const found = normalizeCellItems(cell).find(
        (e) => e.id === id && e.sourceType === type,
      );
      if (found) {
        setSelectedEntry(found);
        return;
      }
    }

    // Fallthrough: entry is not in the currently loaded grid window
    // (e.g. month-view click on a day outside the fetched week, or a stale ID).
    //
    // Guard: never open the edit path for a read-only (imported) event.
    // Check the raw event items by ID before opening any write surface.
    if (type === "event") {
      for (const cell of allCells) {
        if (cell.events?.some((e) => e.eventId === id && e.isReadOnly)) return;
      }
    }

    setEditTarget({ type, id });
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

  // ---- Inspector / sheet content ----

  const memberRow = isHousehold
    ? null
    : (grid?.members.find((m) => m.memberId === memberId) ?? null);

  function renderInspectorBody() {
    if (!selectedEntry) return null;
    return (
      <AgendaItemDetail
        entry={selectedEntry}
        onEdit={(type, id) => setEditTarget({ type, id })}
        onClose={() => setSelectedEntry(null)}
      />
    );
  }

  // Inspector title: selected item title.
  const scope = isHousehold ? "household" : (memberId ?? "");
  const inspectorTitle = selectedEntry?.title ?? "";

  // ---- Header props ----

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

  // ---- Guards ----

  if (!familyId) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }

  if (!isHousehold && memberId && !householdMember) {
    return <p className="error-msg">{t("memberNotFound")}</p>;
  }

  // ---- Empty member fallback for member views ----
  const emptyMemberRow = {
    memberId: memberId ?? "",
    name: householdMember?.name ?? "",
    role: "",
    cells: [],
  };

  return (
    <div className={`agenda-surface l-surface${selectedEntry && !isMobile ? " agenda-surface--inspector" : ""}`}>
      {/* ── Header ── */}
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

      {/* ── Surface body: canvas | inspector ── */}
      <div className="agenda-body l-surface-body">
        {/* Main canvas */}
        <div className="agenda-canvas l-surface-content">
          {gridLoading && <div className="loading-wrap">{t("loading")}</div>}
          {gridError && <p className="error-msg">{gridError}</p>}

          {!gridLoading && !gridError && (
            <>
              {/* ── Household views ── */}
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

              {/* ── Member views ── */}
              {!isHousehold && view === "day" && (
                <MemberDayView
                  member={memberRow ?? emptyMemberRow}
                  selectedDate={selectedDate}
                  onItemClick={handleItemClick}
                  onSlotClick={(time) => handleAddEntry(time)}
                />
              )}
              {!isHousehold && view === "week" && (
                <MemberWeekView
                  member={memberRow ?? emptyMemberRow}
                  selectedDate={selectedDate}
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
                  gridLoading={gridLoading}
                  onSelectDay={handleMonthSelectDate}
                  onItemClick={handleItemClick}
                />
              )}
            </>
          )}
        </div>

        {/* Desktop inspector — visible only when an item is selected */}
        {!isMobile && selectedEntry && (
          <InspectorPanel title={inspectorTitle} onClose={() => setSelectedEntry(null)}>
            {renderInspectorBody()}
          </InspectorPanel>
        )}
      </div>

      {/* FAB */}
      <button
        type="button"
        className="agenda-fab"
        aria-label={t("addEntry")}
        onClick={() => handleAddEntry()}
      >
        +
      </button>

      {/* Mobile bottom sheet */}
      {isMobile && selectedEntry && (
        <BottomSheetDetail
          open
          onClose={() => setSelectedEntry(null)}
          title={selectedEntry.title}
        >
          {renderInspectorBody()}
        </BottomSheetDetail>
      )}

      {/* Edit modal */}
      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={async () => {
            setEditTarget(null);
            setSelectedEntry(null);
            await handleModalSuccess();
          }}
        />
      )}

      {/* Add modal */}
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

/**
 * Named alias kept for backward compatibility with existing imports.
 * @deprecated Import AgendaPage directly.
 */
export { AgendaPage as MemberAgendaPage };

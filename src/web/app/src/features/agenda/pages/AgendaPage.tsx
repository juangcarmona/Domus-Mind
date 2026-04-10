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
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { normalizeCellItems } from "../../today/utils/calendarEntry";
import {
  toIsoDate,
  addDays,
  addMonths,
  startOfWeek,
} from "../../today/utils/dateUtils";
import "../agenda.css";

const VALID_MODES: AgendaView[] = ["day", "week", "month"];

// ----------------------------------------------------------------
// Inspector item display
// ----------------------------------------------------------------

/**
 * Enriched inspector body for a selected calendar entry.
 *
 * Structured in consistent sections:
 *   1. Type / source cue
 *   2. Time range (if timed)
 *   3. Type-specific metadata (participants, status, due date, recurrence, scope)
 *   4. External action (read-only imported entries)
 *   5. Edit action (native entries only)
 *
 * No close button — InspectorPanel header owns the close affordance.
 */
function AgendaItemDetail({
  entry,
  onEdit,
}: {
  entry: CalendarEntry;
  onEdit: (type: EditableEntityType, id: string) => void;
}) {
  const { t } = useTranslation("agenda");
  const canEdit = !entry.isReadOnly;

  const typeLabel =
    entry.isReadOnly && entry.sourceLabel
      ? entry.sourceLabel
      : entry.sourceType === "routine"
      ? t("item.typeRoutine")
      : entry.sourceType === "task"
      ? t("item.typeTask")
      : t("item.typePlan");

  return (
    <div className="agenda-inspector-item">
      {/* Type / source cue */}
      <p className="agenda-inspector-item-type-cue">
        {typeLabel}
        {entry.isReadOnly && (
          <span className="agenda-inspector-item-readonly-badge">
            {" · "}{t("item.readOnly")}
          </span>
        )}
      </p>

      {/* Time range */}
      {entry.time && (
        <p className="agenda-inspector-item-meta">
          {entry.time}
          {entry.endTime ? ` – ${entry.endTime}` : ""}
        </p>
      )}

      {/* Participants (events / plans) */}
      {entry.subtitle && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.participants")}</span>
          {" "}{entry.subtitle}
        </p>
      )}

      {/* Status (tasks) */}
      {entry.sourceType === "task" && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.status")}</span>
          {" "}{entry.status}
        </p>
      )}

      {/* Due date (tasks) */}
      {entry.sourceType === "task" && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.dueDate")}</span>
          {" "}{entry.dueDate ?? t("item.noDueDate")}
        </p>
      )}

      {/* Recurrence summary (routines) */}
      {entry.sourceType === "routine" && entry.recurrenceSummary && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.recurrence")}</span>
          {" "}{entry.recurrenceSummary}
        </p>
      )}

      {/* Scope (routines) */}
      {entry.sourceType === "routine" && entry.scope && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.scope")}</span>
          {" "}{entry.scope}
        </p>
      )}

      {/* External open link (imported read-only entries) */}
      {entry.isReadOnly && entry.openInProviderUrl && (
        <a
          href={entry.openInProviderUrl}
          target="_blank"
          rel="noreferrer"
          className="agenda-inspector-external-link"
        >
          {t("item.openInProvider")}
        </a>
      )}

      {/* Edit action (native entries only) */}
      {canEdit && (
        <div className="agenda-inspector-item-actions">
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => onEdit(entry.sourceType, entry.id)}
          >
            {t("item.edit")}
          </button>
        </div>
      )}
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

  /**
   * Find a calendar entry by type + id across all cells of the currently loaded grid.
   *
   * Searches the raw cell data so it works for any day in the loaded week,
   * not only the currently selected date. Used by handleItemClick.
   */
  function findEntryAcrossGrid(
    type: "event" | "task" | "routine",
    id: string,
  ): CalendarEntry | null {
    if (!grid) return null;
    const cells = isHousehold
      ? grid.sharedCells
      : (grid.members.find((m) => m.memberId === memberId)?.cells ?? []);
    for (const cell of cells) {
      const entries = normalizeCellItems(cell);
      const found = entries.find((e) => e.id === id && e.sourceType === type);
      if (found) return found;
    }
    return null;
  }

  function handleItemClick(type: "event" | "task" | "routine", id: string) {
    const found = findEntryAcrossGrid(type, id);
    if (found) {
      setSelectedEntry(found);
    } else {
      // Entry not in the currently loaded grid (e.g., stale data) — fall back to edit.
      setEditTarget({ type, id });
    }
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

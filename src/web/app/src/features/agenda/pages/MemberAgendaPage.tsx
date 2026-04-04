import { useState, useCallback, useEffect } from "react";
import { useParams, useSearchParams } from "react-router-dom";
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
import { AgendaMiniCalendar } from "../components/AgendaMiniCalendar";
import { SelectedDateCard } from "../components/SelectedDateCard";
import { MemberDayView } from "../components/MemberDayView";
import { MemberWeekView } from "../components/MemberWeekView";
import { MemberMonthView } from "../components/MemberMonthView";
import { SharedDayView } from "../components/SharedDayView";
import { SharedWeekView } from "../components/SharedWeekView";
import type { CalendarEntry } from "../../today/utils/calendarEntry";
import { buildMemberEntries, buildSharedEntries } from "../../today/utils/todayPanelHelpers";
import {
  toIsoDate,
  addDays,
  addMonths,
  startOfWeek,
} from "../../today/utils/dateUtils";

/**
 * Unified agenda surface that supports two subject types:
 *  - member  — routed as /agenda/members/:memberId
 *  - shared  — routed as /agenda/shared
 *
 * Phase 5: aligned with the shared surface shell (l-surface / InspectorPanel / BottomSheetDetail / FAB).
 */
export function AgendaPage() {
  const { memberId } = useParams<{ memberId?: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const { t, i18n } = useTranslation("agenda");
  const isMobile = useIsMobile();

  const isShared = memberId === undefined;

  const family = useAppSelector((s) => s.household.family);
  const familyId = family?.familyId ?? "";
  const firstDayOfWeek = family?.firstDayOfWeek ?? null;
  const members = useAppSelector((s) => s.household.members);

  // Resolve member identity (member subject only).
  const householdMember = isShared
    ? undefined
    : members.find((m) => m.memberId === memberId);

  // Selected date comes from ?date= query param, defaulting to today.
  const todayIso = toIsoDate(new Date());
  const initialDate = searchParams.get("date") ?? todayIso;
  const [selectedDate, setSelectedDate] = useState<string>(initialDate);
  const [view, setView] = useState<AgendaView>("day");

  // Grid state (reused weekly grid API).
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [gridLoading, setGridLoading] = useState(false);
  const [gridError, setGridError] = useState<string | null>(null);

  // Selected calendar entry (drives inspector on desktop, bottom sheet on mobile).
  const [selectedEntry, setSelectedEntry] = useState<CalendarEntry | null>(null);

  // Entity edit modal state.
  const [editTarget, setEditTarget] = useState<{ type: EditableEntityType; id: string } | null>(null);

  // Create-entry modal state: open PlanningAddModal with optional time context.
  const [showAddModal, setShowAddModal] = useState(false);
  const [addModalTime, setAddModalTime] = useState<string | undefined>();

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

  // Keep ?date= in sync with selectedDate so deep links work.
  useEffect(() => {
    setSearchParams({ date: selectedDate }, { replace: true });
  }, [selectedDate, setSearchParams]);

  // Clear selected entry when date or view changes.
  useEffect(() => {
    setSelectedEntry(null);
  }, [selectedDate, view]);

  // Derived grid data for the active subject.
  const memberGrid = isShared
    ? null
    : (grid?.members.find((m) => m.memberId === memberId) ?? null);
  const sharedCells = grid?.sharedCells ?? [];

  // Empty member fallback (no data for the loaded week — member subject only).
  const emptyMember = {
    memberId: memberId ?? "",
    name: householdMember?.name ?? "",
    role: "",
    cells: [],
  };

  // ---- Navigation handlers ----

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

  function handleItemClick(type: "event" | "task" | "routine", id: string) {
    // Look up the entry so we can display it in the inspector first.
    const allEntries: CalendarEntry[] = isShared
      ? buildSharedEntries(sharedCells, selectedDate)
      : memberGrid
      ? buildMemberEntries(memberGrid, selectedDate)
      : [];
    const found = allEntries.find((e) => e.id === id && e.sourceType === type) ?? null;
    if (found) {
      setSelectedEntry(found);
    } else {
      // Entry not in current date slice (e.g. week view, other day) — open edit directly.
      setEditTarget({ type, id });
    }
  }

  /** Called from Week view: select the date and switch to Day view. */
  function handleDayDrill(date: string) {
    setSelectedDate(date);
    setView("day");
  }

  /** Called from Month view: update selected date only, stay in Month view. */
  function handleMonthSelectDate(date: string) {
    setSelectedDate(date);
  }

  /** Open the create-entry modal for the selected date, optional time. */
  function handleAddEntry(time?: string) {
    setAddModalTime(time);
    setShowAddModal(true);
  }

  /** Called from Day timeline slot click: open create for that specific time. */
  function handleSlotClick(time: string) {
    setAddModalTime(time);
    setShowAddModal(true);
  }

  async function handleModalSuccess() {
    await fetchGrid(weekStartForSelected);
  }

  // Untimed entries for the selected date — feeds the SelectedDateCard in the inspector.
  const backlogEntries = isShared
    ? buildSharedEntries(sharedCells, selectedDate).filter((e) => e.time === null)
    : memberGrid
    ? buildMemberEntries(memberGrid, selectedDate).filter((e) => e.time === null)
    : [];

  const backlogDateLabel =
    selectedDate === todayIso
      ? t("dateCard.today")
      : new Date(selectedDate + "T00:00:00").toLocaleDateString(i18n.language, {
          weekday: "long",
          month: "long",
          day: "numeric",
        });

  // ---- Subject label ----

  const subjectLabel = isShared
    ? t("shared.label")
    : (householdMember?.name ?? memberId ?? "");

  // ---- Inspector content ----

  function renderInspectorContent() {
    if (selectedEntry) {
      return (
        <div className="agenda-inspector-item">
          <p className="agenda-inspector-item-title">{selectedEntry.title}</p>
          {selectedEntry.time && (
            <p className="agenda-inspector-item-meta">
              {selectedEntry.time}
              {selectedEntry.endTime ? ` – ${selectedEntry.endTime}` : ""}
            </p>
          )}
          {selectedEntry.subtitle && (
            <p className="agenda-inspector-item-meta">{selectedEntry.subtitle}</p>
          )}
          {selectedEntry.status && (
            <p className="agenda-inspector-item-status">{selectedEntry.status}</p>
          )}
          <div className="agenda-inspector-item-actions">
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              onClick={() => {
                setEditTarget({ type: selectedEntry.sourceType, id: selectedEntry.id });
                setSelectedEntry(null);
              }}
            >
              {t("editEntry", "Edit")}
            </button>
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              onClick={() => setSelectedEntry(null)}
            >
              ✕
            </button>
          </div>
        </div>
      );
    }

    return (
      <>
        <AgendaMiniCalendar
          selectedDate={selectedDate}
          today={todayIso}
          view={view}
          firstDayOfWeek={firstDayOfWeek}
          onSelectDate={setSelectedDate}
        />
        {backlogEntries.length > 0 && (
          <SelectedDateCard
            entries={backlogEntries}
            dateLabel={backlogDateLabel}
            onItemClick={(type, id) => handleItemClick(type, id)}
          />
        )}
      </>
    );
  }

  // ---- Early-exit guards ----

  if (!familyId) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }

  if (!isShared && memberId && !householdMember) {
    return <p className="error-msg">{t("memberNotFound")}</p>;
  }

  return (
    <div className="agenda-surface l-surface">
      {/* ── Header ── */}
      <AgendaHeader
        subjectLabel={subjectLabel}
        selectedDate={selectedDate}
        view={view}
        onViewChange={setView}
        onPrev={handlePrev}
        onNext={handleNext}
        onToday={handleToday}
      />

      {/* ── Surface body: canvas | inspector ── */}
      <div className="agenda-body l-surface-body">
        {/* Main time canvas */}
        <div className="agenda-canvas l-surface-content">
          {gridLoading && <div className="loading-wrap">{t("loading")}</div>}
          {gridError && <p className="error-msg">{gridError}</p>}

          {!gridLoading && !gridError && (
            <>
              {/* Member subject views */}
              {!isShared && view === "day" && (
                <MemberDayView
                  member={memberGrid ?? emptyMember}
                  selectedDate={selectedDate}
                  onItemClick={handleItemClick}
                  onSlotClick={handleSlotClick}
                />
              )}
              {!isShared && view === "week" && (
                <MemberWeekView
                  member={memberGrid ?? emptyMember}
                  selectedDate={selectedDate}
                  onItemClick={handleItemClick}
                  onDayClick={handleDayDrill}
                />
              )}
              {!isShared && view === "month" && (
                <MemberMonthView
                  memberId={memberId ?? null}
                  selectedDate={selectedDate}
                  firstDayOfWeek={firstDayOfWeek}
                  onSelectDay={handleMonthSelectDate}
                />
              )}

              {/* Shared subject views */}
              {isShared && view === "day" && (
                <SharedDayView
                  sharedCells={sharedCells}
                  selectedDate={selectedDate}
                  onItemClick={handleItemClick}
                  onSlotClick={handleSlotClick}
                />
              )}
              {isShared && view === "week" && (
                <SharedWeekView
                  sharedCells={sharedCells}
                  selectedDate={selectedDate}
                  onItemClick={handleItemClick}
                  onDayClick={handleDayDrill}
                />
              )}
              {isShared && view === "month" && (
                <MemberMonthView
                  memberId={null}
                  selectedDate={selectedDate}
                  firstDayOfWeek={firstDayOfWeek}
                  onSelectDay={handleMonthSelectDate}
                />
              )}
            </>
          )}
        </div>

        {/* Desktop inspector — always present; hidden on mobile via CSS */}
        <InspectorPanel title={selectedEntry ? selectedEntry.title : subjectLabel}>
          {renderInspectorContent()}
        </InspectorPanel>
      </div>

      {/* FAB — add entry */}
      <button
        type="button"
        className="agenda-fab"
        aria-label={t("addEntry")}
        onClick={() => handleAddEntry()}
      >
        +
      </button>

      {/* Mobile: item detail bottom sheet */}
      {isMobile && selectedEntry && (
        <BottomSheetDetail
          open
          onClose={() => setSelectedEntry(null)}
          title={selectedEntry.title}
        >
          {renderInspectorContent()}
        </BottomSheetDetail>
      )}

      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={async () => {
            setEditTarget(null);
            await handleModalSuccess();
          }}
        />
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
            participantMemberIds: isShared || !memberId ? [] : [memberId],
            initialStartDate: selectedDate,
            initialStartClock: addModalTime,
          }}
        />
      )}
    </div>
  );
}

/**
 * Named alias kept for backward compatibility with existing route imports.
 * @deprecated Import AgendaPage directly.
 */
export { AgendaPage as MemberAgendaPage };



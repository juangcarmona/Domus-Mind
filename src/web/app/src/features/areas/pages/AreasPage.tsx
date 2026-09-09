// Areas surface — ownership-first dense list with contextual inspector.
import { useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchAreas,
  assignPrimaryOwner,
  assignSecondaryOwner,
  removeSecondaryOwner,
  transferArea,
  renameArea,
  updateAreaColor,
} from "../../../store/areasSlice";
import { fetchPlans } from "../../../store/plansSlice";
import { fetchRoutines, pauseRoutine, resumeRoutine } from "../../../store/routinesSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { completeTask, cancelTask } from "../../../store/tasksSlice";
import type { HouseholdAreaItem, FamilyMemberResponse } from "../../../api/domusmindApi";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { EditEntityModal } from "../../editors/components/EditEntityModal";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { CreateAreaModal } from "../components/CreateAreaModal";
import { PlanningAddModal } from "../../agenda-planning/components/modals/PlanningAddModal";
import { AreaRelatedWorkSection } from "../components/AreaRelatedWorkSection";
import { AREA_PALETTE } from "../utils/areaColors";

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

// ── Dense area list row ───────────────────────────────────────────────────────

function AreaListRow({
  area,
  members,
  selected,
  onClick,
  taskCount = 0,
  planCount = 0,
  routineCount = 0,
}: {
  area: HouseholdAreaItem;
  members: FamilyMemberResponse[];
  selected: boolean;
  onClick: () => void;
  taskCount?: number;
  planCount?: number;
  routineCount?: number;
}) {
  const { t } = useTranslation("areas");
  const hasOwner = !!area.primaryOwnerId;

  const supporterNames = area.secondaryOwnerIds
    .map((id) => members.find((m) => m.memberId === id))
    .filter(Boolean)
    .map((m) => m!.preferredName || m!.name);

  const hasCounts = taskCount > 0 || planCount > 0 || routineCount > 0;

  return (
    <button
      type="button"
      className={`area-list-row${selected ? " area-list-row--selected" : ""}${!hasOwner ? " area-list-row--unowned" : ""}`}
      onClick={onClick}
      aria-pressed={selected}
    >
      <span className="area-row-dot" style={{ background: area.color }} aria-hidden="true" />
      <span className="area-row-body">
        <span className="area-row-name">{area.name}</span>
        <span className="area-row-meta">
          {hasOwner ? (
            <span className="area-row-owner">{area.primaryOwnerName}</span>
          ) : (
            <span className="area-row-gap-cue">{t("needsOwner")}</span>
          )}
          {supporterNames.length > 0 && (
            <span className="area-row-supporters">
              {" · "}
              {supporterNames.length === 1
                ? supporterNames[0]
                : `${supporterNames[0]} +${supporterNames.length - 1}`}
            </span>
          )}
        </span>
      </span>
      {hasCounts && (
        <span className="area-row-counts" aria-hidden="true">
          {taskCount > 0 && (
            <span className="area-row-count-badge area-row-count-badge--task" title={`${taskCount} open tasks`}>
              {taskCount}T
            </span>
          )}
          {planCount > 0 && (
            <span className="area-row-count-badge area-row-count-badge--plan" title={`${planCount} plans`}>
              {planCount}P
            </span>
          )}
          {routineCount > 0 && (
            <span className="area-row-count-badge area-row-count-badge--routine" title={`${routineCount} routines`}>
              {routineCount}R
            </span>
          )}
        </span>
      )}
    </button>
  );
}

// ── Inspector content ─────────────────────────────────────────────────────────

/**
 * Area inspector — primary desktop detail.
 *
 * - Read-first: owner and supporters are navigable identities.
 * - Inline rename and inline color change via compact swatch row.
 * - Related work (tasks, plans, routines) shown in scrollable section.
 * - Explicit create buttons (Task / Routine / Plan) replace the generic chooser.
 */
function AreaInspectorContent({
  area,
  members,
  saving,
  ownerError,
  supporterError,
  isEditingName,
  nameInput,
  renaming,
  renameError,
  isEditingOwner,
  showPalette,
  tasksLoading,
  linkedTasks,
  linkedPlans,
  linkedRoutines,
  memberMap,
  onOwnerChange,
  onStartEditOwner,
  onAddSupporter,
  onRemoveSupporter,
  onStartRename,
  onNameInputChange,
  onNameSave,
  onNameKeyDown,
  onColorChange,
  onTogglePalette,
  onCreateTask,
  onCreateRoutine,
  onCreatePlan,
  onEdit,
  onCompleteTask,
  onCancelTask,
  onPauseRoutine,
  onResumeRoutine,
}: {
  area: HouseholdAreaItem | null;
  members: FamilyMemberResponse[];
  saving: boolean;
  ownerError: string | null;
  supporterError: string | null;
  isEditingName: boolean;
  nameInput: string;
  renaming: boolean;
  renameError: string | null;
  isEditingOwner: boolean;
  showPalette: boolean;
  tasksLoading: boolean;
  linkedTasks: import("../../../api/domusmindApi").EnrichedTimelineEntry[];
  linkedPlans: import("../../../api/domusmindApi").FamilyTimelineEventItem[];
  linkedRoutines: import("../../../api/domusmindApi").RoutineListItem[];
  memberMap: Record<string, string>;
  onOwnerChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  onStartEditOwner: () => void;
  onAddSupporter: (id: string) => void;
  onRemoveSupporter: (id: string) => void;
  onStartRename: () => void;
  onNameInputChange: (v: string) => void;
  onNameSave: () => void;
  onNameKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  onColorChange: (color: string) => void;
  onTogglePalette: () => void;
  onCreateTask: () => void;
  onCreateRoutine: () => void;
  onCreatePlan: () => void;
  onEdit: (target: { type: "task" | "routine" | "event"; id: string }) => void;
  onCompleteTask: (id: string) => void;
  onCancelTask: (id: string) => void;
  onPauseRoutine: (id: string) => void;
  onResumeRoutine: (id: string) => void;
}) {
  const { t } = useTranslation("areas");
  const nameRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isEditingName) nameRef.current?.focus();
  }, [isEditingName]);

  if (!area) {
    return (
      <div className="area-inspector-idle">
        <p className="area-inspector-idle-hint">{t("selectAreaHint")}</p>
      </div>
    );
  }

  const hasOwner = !!area.primaryOwnerId;
  const existingSupporters = members.filter((m) =>
    area.secondaryOwnerIds.includes(m.memberId),
  );
  const availableForSupport = members.filter(
    (m) =>
      m.memberId !== area.primaryOwnerId &&
      !area.secondaryOwnerIds.includes(m.memberId),
  );

  return (
    <div className="area-inspector-content">
      {/* Identity: color dot (click to change) + name (click to rename) */}
      <div className="area-inspector-name-row">
        <button
          type="button"
          className="area-inspector-dot area-inspector-dot--btn"
          style={{ background: area.color }}
          onClick={onTogglePalette}
          title={t("colorHint")}
          aria-label={t("colorHint")}
        />
        {isEditingName ? (
          <input
            ref={nameRef}
            className="area-inspector-name-input"
            value={nameInput}
            disabled={renaming}
            onChange={(e) => onNameInputChange(e.target.value)}
            onBlur={onNameSave}
            onKeyDown={onNameKeyDown}
            aria-label={t("renameHint")}
          />
        ) : (
          <button
            type="button"
            className="area-inspector-name"
            title={t("renameHint")}
            onClick={onStartRename}
          >
            {area.name}
          </button>
        )}
      </div>

      {/* Compact inline color palette — visible when toggled */}
      {showPalette && (
        <div className="area-inspector-palette">
          {AREA_PALETTE.map((c) => (
            <button
              key={c}
              type="button"
              className={`area-color-swatch${area.color === c ? " area-color-swatch--active" : ""}`}
              style={{ background: c }}
              onClick={() => onColorChange(c)}
              aria-label={c}
            />
          ))}
        </div>
      )}

      {renameError && <p className="error-msg area-inspector-error">{renameError}</p>}

      {/* Owner — read-first with edit affordance */}
      <div className="area-inspector-section">
        <p className="area-inspector-section-label">{t("ownerLabel")}</p>
        {isEditingOwner ? (
          <select
            className="form-control area-row-select"
            value={area.primaryOwnerId ?? ""}
            disabled={saving}
            onChange={onOwnerChange}
            aria-label={t("ownerLabel")}
            // eslint-disable-next-line jsx-a11y/no-autofocus
            autoFocus
          >
            {!hasOwner && <option value="">{t("noOwner")}</option>}
            {hasOwner &&
              !members.some((m) => m.memberId === area.primaryOwnerId) && (
                <option value={area.primaryOwnerId!}>
                  {area.primaryOwnerName ?? area.primaryOwnerId}
                </option>
              )}
            {members.map((m) => (
              <option key={m.memberId} value={m.memberId}>
                {m.preferredName || m.name}
              </option>
            ))}
          </select>
        ) : (
          <div className="area-inspector-owner-display">
            {hasOwner ? (
              <Link
                to={`/agenda/members/${area.primaryOwnerId}`}
                className="area-inspector-member-link"
                title={t("ownerAgenda")}
              >
                {area.primaryOwnerName}
              </Link>
            ) : (
              <span className="area-inspector-gap-cue">{t("needsOwner")}</span>
            )}
            <button
              type="button"
              className="btn btn-ghost btn-xs area-inspector-edit-btn"
              disabled={saving}
              onClick={onStartEditOwner}
            >
              {hasOwner ? t("changeOwner") : t("setOwner")}
            </button>
          </div>
        )}
        {!hasOwner && !isEditingOwner && (
          <p className="area-inspector-hint">{t("noOwnerInstruction")}</p>
        )}
        {ownerError && (
          <p className="error-msg area-inspector-error">{ownerError}</p>
        )}
      </div>

      {/* Support members — navigable identities + add/remove */}
      <div className="area-inspector-section">
        <p className="area-inspector-section-label">{t("supportersLabel")}</p>
        {existingSupporters.length > 0 && (
          <ul className="area-supporters-list">
            {existingSupporters.map((m) => (
              <li key={m.memberId} className="area-supporter-tag">
                <Link
                  to={`/agenda/members/${m.memberId}`}
                  className="area-inspector-member-link"
                  title={t("ownerAgenda")}
                >
                  {m.preferredName || m.name}
                </Link>
                <button
                  type="button"
                  className="area-supporter-remove"
                  disabled={saving}
                  onClick={() => onRemoveSupporter(m.memberId)}
                  aria-label={t("removeSupporter")}
                >
                  ×
                </button>
              </li>
            ))}
          </ul>
        )}
        {availableForSupport.length > 0 && (
          <select
            className="form-control area-row-select"
            value=""
            disabled={saving}
            onChange={(e) => {
              if (e.target.value) onAddSupporter(e.target.value);
            }}
            aria-label={t("addSupporter")}
          >
            <option value="">{t("addSupporter")}</option>
            {availableForSupport.map((m) => (
              <option key={m.memberId} value={m.memberId}>
                {m.preferredName || m.name}
              </option>
            ))}
          </select>
        )}
        {supporterError && (
          <p className="error-msg area-inspector-error">{supporterError}</p>
        )}
      </div>

      {/* Related work — linked tasks, plans, routines */}
      <AreaRelatedWorkSection
        tasksLoading={tasksLoading}
        linkedTasks={linkedTasks}
        linkedPlans={linkedPlans}
        linkedRoutines={linkedRoutines}
        memberMap={memberMap}
        onEdit={onEdit}
        onCompleteTask={onCompleteTask}
        onCancelTask={onCancelTask}
        onPauseRoutine={onPauseRoutine}
        onResumeRoutine={onResumeRoutine}
      />

      {/* Explicit area-context creation actions */}
      <div className="area-inspector-actions">
        <p className="area-inspector-section-label" style={{ marginBottom: "0.5rem" }}>
          {t("createFromArea")}
        </p>
        <div className="area-inspector-create-row">
          <button type="button" className="btn btn-sm" onClick={onCreateTask}>
            + {t("createTask")}
          </button>
          <button type="button" className="btn btn-sm" onClick={onCreateRoutine}>
            + {t("createRoutine")}
          </button>
          <button type="button" className="btn btn-sm" onClick={onCreatePlan}>
            + {t("createPlan")}
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export function AreasPage() {
  const { t } = useTranslation("areas");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const location = useLocation();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: areas, status, error } = useAppSelector((s) => s.areas);
  const { items: planItems, status: plansStatus } = useAppSelector((s) => s.plans);
  const { items: routineItems, status: routinesStatus } = useAppSelector((s) => s.routines);
  const timeline = useAppSelector((s) => s.timeline);
  const familyId = family?.familyId;
  const isMobile = useIsMobile();
  const currentMemberId = members.find((m) => m.isCurrentUser)?.memberId;

  // List state
  const [selectedAreaId, setSelectedAreaId] = useState<string | null>(null);
  const [filter, setFilter] = useState<"all" | "unowned" | "mine">("all");
  const [showCreate, setShowCreate] = useState(false);

  // Inspector editing state — reset when selected area changes
  const [saving, setSaving] = useState(false);
  const [ownerError, setOwnerError] = useState<string | null>(null);
  const [supporterError, setSupporterError] = useState<string | null>(null);
  const [isEditingName, setIsEditingName] = useState(false);
  const [nameInput, setNameInput] = useState("");
  const [renaming, setRenaming] = useState(false);
  const [renameError, setRenameError] = useState<string | null>(null);
  const [isEditingOwner, setIsEditingOwner] = useState(false);
  const [showPalette, setShowPalette] = useState(false);

  // Create-from-area: explicit entry points per concept
  const [showCreateTask, setShowCreateTask] = useState(false);
  const [showCreateRoutine, setShowCreateRoutine] = useState(false);
  const [showCreatePlan, setShowCreatePlan] = useState(false);

  // Edit target for inline item editing
  const [editTarget, setEditTarget] = useState<{ type: "task" | "routine" | "event"; id: string } | null>(null);

  // Restore deep-link selection from location state (set by AreaDetailPage redirect)
  useEffect(() => {
    const state = location.state as { selectAreaId?: string } | null;
    if (state?.selectAreaId) {
      setSelectedAreaId(state.selectAreaId);
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (familyId) dispatch(fetchAreas(familyId));
  }, [familyId, dispatch]);

  // Load linked work for count cues and inspector detail
  useEffect(() => {
    if (familyId && plansStatus === "idle") dispatch(fetchPlans({ familyId, from: todayIso() }));
    if (familyId && routinesStatus === "idle") dispatch(fetchRoutines(familyId));
  }, [familyId, plansStatus, routinesStatus, dispatch]);

  useEffect(() => {
    if (familyId) dispatch(fetchTimeline({ familyId, types: "Task" }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId]);

  useEffect(() => {
    setIsEditingName(false);
    setNameInput("");
    setOwnerError(null);
    setSupporterError(null);
    setRenameError(null);
    setSaving(false);
    setIsEditingOwner(false);
    setShowPalette(false);
  }, [selectedAreaId]);

  if (!familyId) return null;

  const selectedArea = areas.find((a) => a.areaId === selectedAreaId) ?? null;
  const loading = status === "loading";
  const hasAreas = areas.length > 0;
  const tasksLoading = timeline.status === "loading";

  // Per-area work count cues derived from already-loaded slices
  const taskCountByArea = useMemo<Record<string, number>>(() => {
    const counts: Record<string, number> = {};
    for (const entry of timeline.data?.groups.flatMap((g) => g.entries) ?? []) {
      if (entry.entryType === "Task" && entry.areaId && entry.status === "Pending") {
        counts[entry.areaId] = (counts[entry.areaId] ?? 0) + 1;
      }
    }
    return counts;
  }, [timeline.data]);

  const planCountByArea = useMemo<Record<string, number>>(() => {
    const counts: Record<string, number> = {};
    for (const plan of planItems) {
      if (plan.areaId) counts[plan.areaId] = (counts[plan.areaId] ?? 0) + 1;
    }
    return counts;
  }, [planItems]);

  const routineCountByArea = useMemo<Record<string, number>>(() => {
    const counts: Record<string, number> = {};
    for (const routine of routineItems) {
      if (routine.areaId) counts[routine.areaId] = (counts[routine.areaId] ?? 0) + 1;
    }
    return counts;
  }, [routineItems]);

  // Linked work for selected area (used by inspector)
  const linkedTasks = useMemo(
    () =>
      (timeline.data?.groups.flatMap((g) => g.entries) ?? []).filter(
        (e) => e.entryType === "Task" && e.areaId === selectedAreaId && e.status === "Pending",
      ),
    [timeline.data, selectedAreaId],
  );
  const linkedPlans = planItems.filter((p) => p.areaId === selectedAreaId);
  const linkedRoutines = routineItems.filter((r) => r.areaId === selectedAreaId);
  const memberMap = useMemo<Record<string, string>>(
    () => Object.fromEntries(members.map((m) => [m.memberId, m.preferredName || m.name])),
    [members],
  );

  // Creation defaults: pre-fill current area + area owner
  const creationDefaults = useMemo(() => ({
    areaId: selectedAreaId ?? undefined,
    assigneeId: selectedArea?.primaryOwnerId ?? undefined,
    participantMemberIds: selectedArea?.primaryOwnerId ? [selectedArea.primaryOwnerId] : undefined,
  }), [selectedAreaId, selectedArea?.primaryOwnerId]);

  const membersForModal = useMemo(
    () => members.map((m) => ({ memberId: m.memberId, name: m.preferredName || m.name })),
    [members],
  );

  function refreshLinkedWork() {
    if (!familyId) return;
    dispatch(fetchTimeline({ familyId, types: "Task" }));
    dispatch(fetchPlans({ familyId, from: todayIso() }));
    dispatch(fetchRoutines(familyId));
  }

  const filteredAreas = areas.filter((a) => {
    if (filter === "unowned") return !a.primaryOwnerId;
    if (filter === "mine")
      return (
        a.primaryOwnerId === currentMemberId ||
        a.secondaryOwnerIds.includes(currentMemberId ?? "")
      );
    return true;
  });

  const unowned = filteredAreas.filter((a) => !a.primaryOwnerId);
  const owned = filteredAreas.filter((a) => !!a.primaryOwnerId);

  function handleSelectArea(areaId: string) {
    setSelectedAreaId((prev) => (prev === areaId ? null : areaId));
  }

  async function handleOwnerChange(e: React.ChangeEvent<HTMLSelectElement>) {
    if (!selectedArea || !familyId) return;
    const newId = e.target.value;
    if (!newId) return;
    setSaving(true);
    setOwnerError(null);
    if (selectedArea.primaryOwnerId) {
      const result = await dispatch(
        transferArea({ areaId: selectedArea.areaId, newPrimaryOwnerId: newId, familyId }),
      );
      if (!transferArea.fulfilled.match(result))
        setOwnerError((result.payload as string) ?? tCommon("failed"));
    } else {
      const result = await dispatch(
        assignPrimaryOwner({ areaId: selectedArea.areaId, memberId: newId, familyId }),
      );
      if (!assignPrimaryOwner.fulfilled.match(result))
        setOwnerError((result.payload as string) ?? tCommon("failed"));
    }
    setSaving(false);
    setIsEditingOwner(false);
  }

  async function handleAddSupporter(memberId: string) {
    if (!selectedArea || !familyId) return;
    setSupporterError(null);
    setSaving(true);
    const result = await dispatch(
      assignSecondaryOwner({ areaId: selectedArea.areaId, memberId, familyId }),
    );
    if (!assignSecondaryOwner.fulfilled.match(result))
      setSupporterError((result.payload as string) ?? tCommon("failed"));
    setSaving(false);
  }

  async function handleRemoveSupporter(memberId: string) {
    if (!selectedArea || !familyId) return;
    setSupporterError(null);
    setSaving(true);
    const result = await dispatch(
      removeSecondaryOwner({ areaId: selectedArea.areaId, memberId, familyId }),
    );
    if (!removeSecondaryOwner.fulfilled.match(result))
      setSupporterError((result.payload as string) ?? tCommon("failed"));
    setSaving(false);
  }

  async function handleRename() {
    const trimmed = nameInput.trim();
    if (!selectedArea || !trimmed || trimmed === selectedArea.name) {
      setIsEditingName(false);
      return;
    }
    setRenaming(true);
    setRenameError(null);
    const result = await dispatch(
      renameArea({ areaId: selectedArea.areaId, name: trimmed }),
    );
    setRenaming(false);
    if (renameArea.fulfilled.match(result)) {
      setIsEditingName(false);
      return;
    }
    setRenameError((result.payload as string) ?? tCommon("failed"));
  }

  function handleColorChange(color: string) {
    if (!selectedAreaId) return;
    void dispatch(updateAreaColor({ areaId: selectedAreaId, color }));
    setShowPalette(false);
  }

  async function handleCompleteTask(taskId: string) {
    await dispatch(completeTask(taskId));
    refreshLinkedWork();
  }

  async function handleCancelTask(taskId: string) {
    await dispatch(cancelTask(taskId));
    refreshLinkedWork();
  }

  async function handlePauseRoutine(routineId: string) {
    if (!familyId) return;
    await dispatch(pauseRoutine({ routineId, familyId }));
  }

  async function handleResumeRoutine(routineId: string) {
    if (!familyId) return;
    await dispatch(resumeRoutine({ routineId, familyId }));
  }

  const inspectorProps = {
    area: selectedArea,
    members,
    saving,
    ownerError,
    supporterError,
    isEditingName,
    nameInput,
    renaming,
    renameError,
    isEditingOwner,
    showPalette,
    tasksLoading,
    linkedTasks,
    linkedPlans,
    linkedRoutines,
    memberMap,
    onOwnerChange: (e: React.ChangeEvent<HTMLSelectElement>) => { void handleOwnerChange(e); },
    onStartEditOwner: () => setIsEditingOwner(true),
    onAddSupporter: (id: string) => { void handleAddSupporter(id); },
    onRemoveSupporter: (id: string) => { void handleRemoveSupporter(id); },
    onStartRename: () => {
      setNameInput(selectedArea?.name ?? "");
      setIsEditingName(true);
    },
    onNameInputChange: setNameInput,
    onNameSave: () => { void handleRename(); },
    onNameKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === "Enter") void handleRename();
      if (e.key === "Escape") {
        setIsEditingName(false);
        setRenameError(null);
      }
    },
    onColorChange: handleColorChange,
    onTogglePalette: () => setShowPalette((p) => !p),
    onCreateTask: () => setShowCreateTask(true),
    onCreateRoutine: () => setShowCreateRoutine(true),
    onCreatePlan: () => setShowCreatePlan(true),
    onEdit: setEditTarget,
    onCompleteTask: (id: string) => { void handleCompleteTask(id); },
    onCancelTask: (id: string) => { void handleCancelTask(id); },
    onPauseRoutine: (id: string) => { void handlePauseRoutine(id); },
    onResumeRoutine: (id: string) => { void handleResumeRoutine(id); },
  };

  return (
    <div className="areas-surface l-surface">
      {/* ── Compact header ── */}
      <div className="areas-header">
        <div className="areas-header-row">
          <h1 className="areas-header-title">{t("title")}</h1>
          {hasAreas && (
            <button
              type="button"
              className="btn btn-sm"
              onClick={() => setShowCreate(true)}
            >
              + {t("add")}
            </button>
          )}
        </div>
        <div className="areas-filter-tabs" role="tablist">
          {(["all", "unowned", "mine"] as const).map((f) => (
            <button
              key={f}
              type="button"
              role="tab"
              aria-selected={filter === f}
              className={`areas-filter-tab${filter === f ? " areas-filter-tab--active" : ""}`}
              onClick={() => setFilter(f)}
            >
              {t(
                f === "all"
                  ? "filterAll"
                  : f === "unowned"
                  ? "filterUnowned"
                  : "filterMine",
              )}
            </button>
          ))}
        </div>
      </div>

      {/* ── Surface body: list | inspector ── */}
      <div className="l-surface-body">
        <div className="l-surface-content areas-list-pane">
          {loading && <div className="loading-wrap">{t("loading")}</div>}
          {status === "error" && <p className="error-msg">{error}</p>}

          {!loading && !hasAreas && status !== "error" && (
            <div className="empty-state">
              <p className="empty-state-headline">{t("emptyHeadline")}</p>
              <p className="empty-state-hint">{t("emptyHint")}</p>
              <button
                type="button"
                className="btn"
                onClick={() => setShowCreate(true)}
              >
                {t("add")}
              </button>
            </div>
          )}

          {!loading && hasAreas && filteredAreas.length === 0 && (
            <p className="areas-filter-empty">{t("filterEmpty")}</p>
          )}

          {unowned.length > 0 && (
            <div className="areas-group">
              <div className="areas-group-label areas-group-label--gap">
                {t("sectionNeedsOwner")}
                <span className="areas-group-count">{unowned.length}</span>
              </div>
              {unowned.map((area) => (
                <AreaListRow
                  key={area.areaId}
                  area={area}
                  members={members}
                  selected={selectedAreaId === area.areaId}
                  onClick={() => handleSelectArea(area.areaId)}
                  taskCount={taskCountByArea[area.areaId] ?? 0}
                  planCount={planCountByArea[area.areaId] ?? 0}
                  routineCount={routineCountByArea[area.areaId] ?? 0}
                />
              ))}
            </div>
          )}

          {owned.length > 0 && (
            <div className="areas-group">
              {unowned.length > 0 && (
                <div className="areas-group-label">{t("sectionOwned")}</div>
              )}
              {owned.map((area) => (
                <AreaListRow
                  key={area.areaId}
                  area={area}
                  members={members}
                  selected={selectedAreaId === area.areaId}
                  onClick={() => handleSelectArea(area.areaId)}
                  taskCount={taskCountByArea[area.areaId] ?? 0}
                  planCount={planCountByArea[area.areaId] ?? 0}
                  routineCount={routineCountByArea[area.areaId] ?? 0}
                />
              ))}
            </div>
          )}
        </div>

        {/* Desktop inspector — always present; hidden on mobile via CSS */}
        <InspectorPanel
          title={selectedArea ? selectedArea.name : t("title")}
          onClose={selectedArea ? () => setSelectedAreaId(null) : undefined}
        >
          <AreaInspectorContent {...inspectorProps} />
        </InspectorPanel>
      </div>

      {/* Mobile: area detail bottom sheet */}
      <BottomSheetDetail
        open={!!selectedArea && isMobile}
        onClose={() => setSelectedAreaId(null)}
        title={selectedArea?.name}
      >
        <AreaInspectorContent {...inspectorProps} />
      </BottomSheetDetail>

      {showCreate && familyId && (
        <CreateAreaModal
          familyId={familyId}
          onClose={() => setShowCreate(false)}
        />
      )}

      {/* Explicit area-context create modals — skip the generic chooser step */}
      {showCreateTask && familyId && selectedArea && (
        <PlanningAddModal
          familyId={familyId}
          members={membersForModal}
          initialStep="task"
          defaults={creationDefaults}
          onClose={() => setShowCreateTask(false)}
          onSuccess={() => { setShowCreateTask(false); refreshLinkedWork(); }}
        />
      )}

      {showCreateRoutine && familyId && selectedArea && (
        <PlanningAddModal
          familyId={familyId}
          members={membersForModal}
          initialStep="routine"
          defaults={creationDefaults}
          onClose={() => setShowCreateRoutine(false)}
          onSuccess={() => { setShowCreateRoutine(false); refreshLinkedWork(); }}
        />
      )}

      {showCreatePlan && familyId && selectedArea && (
        <PlanningAddModal
          familyId={familyId}
          members={membersForModal}
          initialStep="plan"
          defaults={creationDefaults}
          onClose={() => setShowCreatePlan(false)}
          onSuccess={() => { setShowCreatePlan(false); refreshLinkedWork(); }}
        />
      )}

      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={() => { setEditTarget(null); refreshLinkedWork(); }}
        />
      )}
    </div>
  );
}


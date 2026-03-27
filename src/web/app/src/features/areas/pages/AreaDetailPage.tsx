import { useEffect, useMemo, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
  import { fetchAreas, assignPrimaryOwner, assignSecondaryOwner, removeSecondaryOwner, transferArea, renameArea, updateAreaColor } from "../../../store/areasSlice";
import { fetchPlans } from "../../../store/plansSlice";
import { fetchRoutines, pauseRoutine, resumeRoutine } from "../../../store/routinesSlice";
import { fetchTimeline } from "../../../store/timelineSlice";
import { completeTask, cancelTask } from "../../../store/tasksSlice";
import { EditEntityModal } from "../../editors/components/EditEntityModal";
import { PlanningAddModal } from "../../planning/components/modals/PlanningAddModal";
import type { PlanningAddModalDefaults } from "../../planning/components/modals/PlanningAddModal";
import { AREA_PALETTE } from "../utils/areaColors";
import { AreaDetailHeader } from "../components/AreaDetailHeader";
import { AreaOwnerSection } from "../components/AreaOwnerSection";
import { AreaRelatedWorkSection } from "../components/AreaRelatedWorkSection";

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

// ── Area Detail Page ──────────────────────────────────────────────────────────
//
export function AreaDetailPage() {
  const { areaId } = useParams<{ areaId: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation("areas");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: areas, status } = useAppSelector((s) => s.areas);
  const { items: planItems, status: plansStatus } = useAppSelector((s) => s.plans);
  const { items: routineItems, status: routinesStatus } = useAppSelector((s) => s.routines);
  const timeline = useAppSelector((s) => s.timeline);
  const familyId = family?.familyId;

  const area = areas.find((a) => a.areaId === areaId);

  const [color, setColor] = useState<string>(area?.color ?? AREA_PALETTE[0]);
  const [saving, setSaving] = useState(false);
  const [isEditingName, setIsEditingName] = useState(false);
  const [nameInput, setNameInput] = useState("");
  const [renaming, setRenaming] = useState(false);
  const [ownerError, setOwnerError] = useState<string | null>(null);
  const [renameError, setRenameError] = useState<string | null>(null);
  const [supporterError, setSupporterError] = useState<string | null>(null);
  const [editTarget, setEditTarget] = useState<{ type: "task" | "routine" | "event"; id: string } | null>(null);
  const [addModal, setAddModal] = useState(false);

  // Area-context creation defaults: pre-select this area + area owner as assignee.
  const creationDefaults = useMemo<PlanningAddModalDefaults>(() => ({
    areaId: areaId,
    assigneeId: area?.primaryOwnerId ?? undefined,
    participantMemberIds: area?.primaryOwnerId ? [area.primaryOwnerId] : undefined,
  }), [areaId, area?.primaryOwnerId]);

  // Tasks: use timelineSlice (same source as EditEntityModal) so edit lookups always succeed.
  const linkedTasks = useMemo(
    () =>
      (timeline.data?.groups.flatMap((g) => g.entries) ?? [])
        .filter((e) => e.entryType === "Task" && e.areaId === areaId && e.status === "Pending"),
    [timeline.data, areaId],
  );
  const tasksLoading = timeline.status === "loading";

  const linkedPlans = planItems.filter((p) => p.areaId === areaId);
  const linkedRoutines = routineItems.filter((r) => r.areaId === areaId);

  useEffect(() => {
    if (familyId && status === "idle") dispatch(fetchAreas(familyId));
  }, [familyId, status, dispatch]);

  useEffect(() => {
    if (familyId && plansStatus === "idle") dispatch(fetchPlans({ familyId, from: todayIso() }));
    if (familyId && routinesStatus === "idle") dispatch(fetchRoutines(familyId));
  }, [familyId, plansStatus, routinesStatus, dispatch]);

  // Force a fresh task timeline fetch on every mount so newly created tasks appear.
  // EditEntityModal uses timelineSlice to look up tasks - we must keep this in sync.
  useEffect(() => {
    if (familyId) dispatch(fetchTimeline({ familyId, types: "Task" }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId]);

  useEffect(() => {
    setColor(area?.color ?? AREA_PALETTE[0]);
    setSaving(false);
    setIsEditingName(false);
    setNameInput("");
    setRenaming(false);
    setOwnerError(null);
    setRenameError(null);
    setSupporterError(null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [areaId]);

  // Sync color when server data loads or is updated externally
  useEffect(() => {
    if (area?.color) setColor(area.color);
  }, [area?.color]);

  // Member display map (id → display name), used by task + routine cards.
  const memberMap = useMemo<Record<string, string>>(
    () => Object.fromEntries(members.map((m) => [m.memberId, m.preferredName || m.name])),
    [members],
  );

  function refreshLinkedWork() {
    if (!familyId) return;
    dispatch(fetchTimeline({ familyId, types: "Task" }));
    dispatch(fetchPlans({ familyId, from: todayIso() }));
    dispatch(fetchRoutines(familyId));
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

  function handleColorChange(e: React.ChangeEvent<HTMLInputElement>) {
    setColor(e.target.value);
  }

  function handleColorBlur() {
    if (areaId) void dispatch(updateAreaColor({ areaId, color }));
  }

  function handleSwatchClick(c: string) {
    setColor(c);
    if (areaId) void dispatch(updateAreaColor({ areaId, color: c }));
  }

  function handleNameClick() {
    setRenameError(null);
    setNameInput(area?.name ?? "");
    setIsEditingName(true);
  }

  async function handleNameSave() {
    const trimmedName = nameInput.trim();
    if (!area || !trimmedName || trimmedName === area.name) {
      setRenameError(null);
      setIsEditingName(false);
      return;
    }

    setRenaming(true);
    setRenameError(null);
    const result = await dispatch(renameArea({ areaId: area.areaId, name: trimmedName }));
    setRenaming(false);

    if (renameArea.fulfilled.match(result)) {
      setIsEditingName(false);
      return;
    }

    setRenameError((result.payload as string) ?? tCommon("failed"));
  }

  function handleNameKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter") { void handleNameSave(); }
    if (e.key === "Escape") {
      setRenameError(null);
      setIsEditingName(false);
    }
  }

  async function handleAddSupporter(memberId: string) {
    if (!area || !familyId) return;
    setSupporterError(null);
    setSaving(true);
    const result = await dispatch(assignSecondaryOwner({ areaId: area.areaId, memberId, familyId }));
    setSaving(false);
    if (!assignSecondaryOwner.fulfilled.match(result)) {
      setSupporterError((result.payload as string) ?? tCommon("failed"));
    }
  }

  async function handleRemoveSupporter(memberId: string) {
    if (!area || !familyId) return;
    setSupporterError(null);
    setSaving(true);
    const result = await dispatch(removeSecondaryOwner({ areaId: area.areaId, memberId, familyId }));
    setSaving(false);
    if (!removeSecondaryOwner.fulfilled.match(result)) {
      setSupporterError((result.payload as string) ?? tCommon("failed"));
    }
  }

  async function handleOwnerChange(e: React.ChangeEvent<HTMLSelectElement>) {
    if (!area || !familyId) return;
    const newId = e.target.value;
    if (!newId) return;

    setOwnerError(null);
    setSaving(true);

    if (area.primaryOwnerId) {
      const result = await dispatch(transferArea({ areaId: area.areaId, newPrimaryOwnerId: newId, familyId }));
      if (!transferArea.fulfilled.match(result)) {
        setOwnerError((result.payload as string) ?? tCommon("failed"));
      }
    } else {
      const result = await dispatch(assignPrimaryOwner({ areaId: area.areaId, memberId: newId, familyId }));
      if (!assignPrimaryOwner.fulfilled.match(result)) {
        setOwnerError((result.payload as string) ?? tCommon("failed"));
      }
    }

    setSaving(false);
  }

  if (!familyId) return null;

  if (status === "loading" && !area) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }

  if (status === "success" && !area) {
    return (
      <div>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={() => navigate("/areas")}
          style={{ marginBottom: "1rem" }}
        >
          ← {t("backToAreas")}
        </button>
        <p style={{ color: "var(--muted)" }}>{t("notFound")}</p>
      </div>
    );
  }

  // Areas list not loaded yet and we haven't resolved the area - wait.
  if (!area) return null;

  return (
    <div className="area-detail-page">
      <button
        type="button"
        className="btn btn-ghost btn-sm"
        onClick={() => navigate("/areas")}
        style={{ marginBottom: "1.25rem" }}
      >
        ← {t("backToAreas")}
      </button>

      <AreaDetailHeader
        areaName={area.name}
        color={color}
        isEditingName={isEditingName}
        nameInput={nameInput}
        renaming={renaming}
        renameError={renameError}
        onSwatchClick={handleSwatchClick}
        onColorChange={handleColorChange}
        onColorBlur={handleColorBlur}
        onNameClick={handleNameClick}
        onNameInputChange={setNameInput}
        onNameSave={() => { void handleNameSave(); }}
        onNameKeyDown={handleNameKeyDown}
      />

      <AreaOwnerSection
        area={area}
        members={members}
        saving={saving}
        ownerError={ownerError}
        onOwnerChange={handleOwnerChange}
        supporterError={supporterError}
        onAddSupporter={(id) => { void handleAddSupporter(id); }}
        onRemoveSupporter={(id) => { void handleRemoveSupporter(id); }}
      />

      <AreaRelatedWorkSection
        tasksLoading={tasksLoading}
        linkedTasks={linkedTasks}
        linkedPlans={linkedPlans}
        linkedRoutines={linkedRoutines}
        memberMap={memberMap}
        onAddClick={() => setAddModal(true)}
        onEdit={setEditTarget}
        onCompleteTask={(taskId) => { void handleCompleteTask(taskId); }}
        onCancelTask={(taskId) => { void handleCancelTask(taskId); }}
        onPauseRoutine={(routineId) => { void handlePauseRoutine(routineId); }}
        onResumeRoutine={(routineId) => { void handleResumeRoutine(routineId); }}
      />

      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={() => { setEditTarget(null); refreshLinkedWork(); }}
        />
      )}

      {addModal && area && familyId && (
        <PlanningAddModal
          familyId={familyId}
          members={members.map((m) => ({ memberId: m.memberId, name: m.preferredName || m.name }))}
          initialStep="choose"
          defaults={creationDefaults}
          onClose={() => setAddModal(false)}
          onSuccess={() => { setAddModal(false); refreshLinkedWork(); }}
        />
      )}
    </div>
  );
}

// Phase 6: Areas surface — ownership-first dense list with contextual inspector.
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchAreas,
  assignPrimaryOwner,
  assignSecondaryOwner,
  removeSecondaryOwner,
  transferArea,
  renameArea,
} from "../../../store/areasSlice";
import type { HouseholdAreaItem, FamilyMemberResponse } from "../../../api/domusmindApi";
import { InspectorPanel } from "../../../components/InspectorPanel";
import { BottomSheetDetail } from "../../../components/BottomSheetDetail";
import { useIsMobile } from "../../../hooks/useIsMobile";
import { CreateAreaModal } from "../components/CreateAreaModal";

// ── Dense area list row ───────────────────────────────────────────────────────

function AreaListRow({
  area,
  members,
  selected,
  onClick,
}: {
  area: HouseholdAreaItem;
  members: FamilyMemberResponse[];
  selected: boolean;
  onClick: () => void;
}) {
  const { t } = useTranslation("areas");
  const hasOwner = !!area.primaryOwnerId;

  const supporterNames = area.secondaryOwnerIds
    .map((id) => members.find((m) => m.memberId === id))
    .filter(Boolean)
    .map((m) => m!.preferredName || m!.name);

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
    </button>
  );
}

// ── Inspector content ─────────────────────────────────────────────────────────

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
  onOwnerChange,
  onAddSupporter,
  onRemoveSupporter,
  onStartRename,
  onNameInputChange,
  onNameSave,
  onNameKeyDown,
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
  onOwnerChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  onAddSupporter: (id: string) => void;
  onRemoveSupporter: (id: string) => void;
  onStartRename: () => void;
  onNameInputChange: (v: string) => void;
  onNameSave: () => void;
  onNameKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
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
      {/* Name — click to rename */}
      <div className="area-inspector-name-row">
        <span
          className="area-inspector-dot"
          style={{ background: area.color }}
          aria-hidden="true"
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
      {renameError && <p className="error-msg area-inspector-error">{renameError}</p>}

      {/* Owner */}
      <div className="area-inspector-section">
        <p className="area-inspector-section-label">{t("ownerLabel")}</p>
        <select
          className="form-control area-row-select"
          value={area.primaryOwnerId ?? ""}
          disabled={saving}
          onChange={onOwnerChange}
          aria-label={t("ownerLabel")}
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
        {!hasOwner && (
          <p className="area-inspector-hint">{t("noOwnerInstruction")}</p>
        )}
        {ownerError && (
          <p className="error-msg area-inspector-error">{ownerError}</p>
        )}
      </div>

      {/* Support */}
      <div className="area-inspector-section">
        <p className="area-inspector-section-label">{t("supportersLabel")}</p>
        {existingSupporters.length > 0 && (
          <ul className="area-supporters-list">
            {existingSupporters.map((m) => (
              <li key={m.memberId} className="area-supporter-tag">
                <span>{m.preferredName || m.name}</span>
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

      {/* Full detail link */}
      <div className="area-inspector-actions">
        <Link to={`/areas/${area.areaId}`} className="area-inspector-detail-link">
          {t("viewDetail")} →
        </Link>
      </div>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export function AreasPage() {
  const { t } = useTranslation("areas");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: areas, status, error } = useAppSelector((s) => s.areas);
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

  useEffect(() => {
    if (familyId) dispatch(fetchAreas(familyId));
  }, [familyId, dispatch]);

  useEffect(() => {
    setIsEditingName(false);
    setNameInput("");
    setOwnerError(null);
    setSupporterError(null);
    setRenameError(null);
    setSaving(false);
  }, [selectedAreaId]);

  if (!familyId) return null;

  const selectedArea = areas.find((a) => a.areaId === selectedAreaId) ?? null;
  const loading = status === "loading";
  const hasAreas = areas.length > 0;

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
    onOwnerChange: handleOwnerChange,
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
    </div>
  );
}

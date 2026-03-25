import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchAreas, assignPrimaryOwner, transferArea } from "../../../store/areasSlice";
import type { HouseholdAreaItem, FamilyMemberResponse } from "../../../api/domusmindApi";
import { getAreaColor } from "../utils/areaColors";
import { CreateAreaModal } from "../components/CreateAreaModal";

// ── Area row ──────────────────────────────────────────────────────────────────

function AreaRow({
  area,
  members,
  familyId,
}: {
  area: HouseholdAreaItem;
  members: FamilyMemberResponse[];
  familyId: string;
}) {
  const dispatch = useAppDispatch();
  const { t } = useTranslation("areas");
  const { t: tCommon } = useTranslation("common");
  const navigate = useNavigate();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const hasOwner = !!area.primaryOwnerId;

  async function handleOwnerChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const newId = e.target.value;
    if (!newId) return;

    setError(null);
    setSaving(true);

    if (hasOwner) {
      const result = await dispatch(transferArea({ areaId: area.areaId, newPrimaryOwnerId: newId, familyId }));
      if (!transferArea.fulfilled.match(result)) {
        setError((result.payload as string) ?? tCommon("failed"));
      }
    } else {
      const result = await dispatch(assignPrimaryOwner({ areaId: area.areaId, memberId: newId, familyId }));
      if (!assignPrimaryOwner.fulfilled.match(result)) {
        setError((result.payload as string) ?? tCommon("failed"));
      }
    }

    setSaving(false);
  }

  return (
    <div
      className={`item-card area-row${hasOwner ? "" : " area-row--unowned"}`}
      style={{ borderLeft: `4px solid ${getAreaColor(area.areaId)}`, cursor: "pointer" }}
      role="button"
      tabIndex={0}
      onClick={() => navigate(`/areas/${area.areaId}`)}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          navigate(`/areas/${area.areaId}`);
        }
      }}
    >
      <div className="item-card-body">
        <div className="item-card-title">{area.name}</div>
        {area.primaryOwnerName && (
          <div className="item-card-subtitle">{area.primaryOwnerName}</div>
        )}
      </div>
      <div className="item-card-actions" onClick={(e) => e.stopPropagation()}>
        <select
          className="form-control area-row-select"
          value={area.primaryOwnerId ?? ""}
          disabled={saving}
          onChange={handleOwnerChange}
          aria-label={t("ownerLabel")}
        >
          {!hasOwner && <option value="">{t("noOwner")}</option>}
          {/* Guard: current owner may no longer be in the members list */}
          {hasOwner && !members.some((m) => m.memberId === area.primaryOwnerId) && (
            <option value={area.primaryOwnerId!}>{area.primaryOwnerName ?? area.primaryOwnerId}</option>
          )}
          {members.map((m) => (
            <option key={m.memberId} value={m.memberId}>
              {m.preferredName || m.name}
            </option>
          ))}
        </select>
        {error && <p className="error-msg" style={{ marginTop: "0.35rem" }}>{error}</p>}
      </div>
    </div>
  );
}

// ── Empty state ───────────────────────────────────────────────────────────────

function AreasEmptyState({ onAdd }: { onAdd: () => void }) {
  const { t } = useTranslation("areas");
  return (
    <div className="empty-state">
      <p style={{ fontWeight: 600, fontSize: "1rem", marginBottom: "0.4rem" }}>
        {t("emptyHeadline")}
      </p>
      <p style={{ marginBottom: "1.25rem" }}>{t("emptyHint")}</p>
      <button type="button" className="btn" onClick={onAdd}>
        {t("add")}
      </button>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export function AreasPage() {
  const { t } = useTranslation("areas");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: areas, status, error } = useAppSelector((s) => s.areas);
  const familyId = family?.familyId;

  const [showCreate, setShowCreate] = useState(false);

  useEffect(() => {
    if (familyId) dispatch(fetchAreas(familyId));
  }, [familyId, dispatch]);

  if (!familyId) return null;

  const loading = status === "loading";
  const hasAreas = areas.length > 0;
  const unowned = areas.filter((a) => !a.primaryOwnerId);
  const owned = areas.filter((a) => !!a.primaryOwnerId);

  return (
    <div>
      <div className="page-header">
        <h1>{t("title")}</h1>
        {hasAreas && (
          <button type="button" className="btn" onClick={() => setShowCreate(true)}>
            {t("add")}
          </button>
        )}
      </div>

      {loading && <div className="loading-wrap">{t("loading")}</div>}
      {status === "error" && <p className="error-msg">{error}</p>}

      {!loading && !hasAreas && status !== "error" && (
        <AreasEmptyState onAdd={() => setShowCreate(true)} />
      )}

      {unowned.length > 0 && (
        <div className="roster-group">
          <div className="roster-group-title area-group-title--unowned">
            {t("sectionNeedsOwner")} · {unowned.length}
          </div>
          <div className="item-list">
            {unowned.map((area) => (
              <AreaRow key={area.areaId} area={area} members={members} familyId={familyId} />
            ))}
          </div>
        </div>
      )}

      {owned.length > 0 && (
        <div className="roster-group">
          {unowned.length > 0 && (
            <div className="roster-group-title">{t("sectionOwned")}</div>
          )}
          <div className="item-list">
            {owned.map((area) => (
              <AreaRow key={area.areaId} area={area} members={members} familyId={familyId} />
            ))}
          </div>
        </div>
      )}

      {showCreate && familyId && (
        <CreateAreaModal familyId={familyId} onClose={() => setShowCreate(false)} />
      )}
    </div>
  );
}

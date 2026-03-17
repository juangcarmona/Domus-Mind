import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchAreas,
  createArea,
  assignPrimaryOwner,
  transferArea,
} from "../../../store/areasSlice";
import type { HouseholdAreaItem } from "../../../api/domusmindApi";

function AssignOwnerModal({
  area,
  familyId,
  members,
  onClose,
}: {
  area: HouseholdAreaItem;
  familyId: string;
  members: { memberId: string; name: string }[];
  onClose: () => void;
}) {
  const { t } = useTranslation("areas");
  const dispatch = useAppDispatch();
  const [memberId, setMemberId] = useState(area.primaryOwnerId ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { t: tCommon } = useTranslation("common");

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!memberId) return;
    setSubmitting(true);
    setError(null);

    const isTransfer = !!area.primaryOwnerId && area.primaryOwnerId !== memberId;
    if (isTransfer) {
      const result = await dispatch(transferArea({ areaId: area.areaId, newPrimaryOwnerId: memberId, familyId }));
      setSubmitting(false);
      if (transferArea.fulfilled.match(result)) { onClose(); }
      else { setError((result as { payload?: unknown }).payload as string ?? tCommon("failed")); }
    } else {
      const result = await dispatch(assignPrimaryOwner({ areaId: area.areaId, memberId, familyId }));
      setSubmitting(false);
      if (assignPrimaryOwner.fulfilled.match(result)) { onClose(); }
      else { setError((result as { payload?: unknown }).payload as string ?? tCommon("failed")); }
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>
          {area.primaryOwnerId ? t("transfer") : t("assign")} — {area.name}
        </h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="owner-select">{t("responsible")}</label>
            <select
              id="owner-select"
              className="form-control"
              value={memberId}
              onChange={(e) => setMemberId(e.target.value)}
              required
              autoFocus
            >
              <option value="">{tCommon("selectPerson")}</option>
              {members.map((m) => (
                <option key={m.memberId} value={m.memberId}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {tCommon("cancel")}
            </button>
            <button type="submit" className="btn" disabled={submitting || !memberId}>
              {submitting ? tCommon("saving") : tCommon("save")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export function AreasPage() {
  const { t } = useTranslation("areas");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: areas, status, error } = useAppSelector((s) => s.areas);
  const familyId = family?.familyId;

  const [showForm, setShowForm] = useState(false);
  const [areaName, setAreaName] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [assignTarget, setAssignTarget] = useState<HouseholdAreaItem | null>(null);

  useEffect(() => {
    if (familyId) dispatch(fetchAreas(familyId));
  }, [familyId, dispatch]);

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!familyId || !areaName.trim()) return;
    setSubmitting(true);
    setFormError(null);
    const result = await dispatch(createArea({ familyId, name: areaName.trim() }));
    setSubmitting(false);
    if (createArea.fulfilled.match(result)) {
      setAreaName("");
      setShowForm(false);
    } else {
      setFormError(result.payload as string ?? t("createError"));
    }
  }

  if (!familyId) return null;

  return (
    <div>
      <div className="page-header">
        <h1>{t("title")}</h1>
        <button className="btn" onClick={() => { setShowForm(true); setFormError(null); }}>
          {t("add")}
        </button>
      </div>

      {showForm && (
        <div className="card">
          <h2>{t("createHeading")}</h2>
          <form onSubmit={handleCreate}>
            <div className="form-group">
              <label htmlFor="area-name">{t("nameLabel")}</label>
              <input
                id="area-name"
                className="form-control"
                type="text"
                value={areaName}
                onChange={(e) => setAreaName(e.target.value)}
                required
                autoFocus
                placeholder={t("namePlaceholder")}
              />
            </div>
            {formError && <p className="error-msg">{formError}</p>}
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button type="submit" className="btn" disabled={submitting}>
                {submitting ? tCommon("creating") : tCommon("create")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => setShowForm(false)}
              >
                {tCommon("cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {status === "loading" && (
        <div className="loading-wrap">{t("loading")}</div>
      )}

      {status === "error" && <p className="error-msg">{error}</p>}

      {status === "success" && areas.length === 0 && (
        <div className="empty-state">
          <p>{t("empty")}</p>
          <p>{t("emptyHint")}</p>
        </div>
      )}

      {areas.length > 0 && (
        <div className="item-list">
          {areas.map((area) => (
            <div key={area.areaId} className="item-card">
              <div className="item-card-body">
                <div className="item-card-title">{area.name}</div>
                <div className="item-card-subtitle">
                  {area.primaryOwnerName ? (
                    <>{t("owner")}: {area.primaryOwnerName}</>
                  ) : (
                    <span style={{ color: "var(--accent)" }}>{t("noOwner")}</span>
                  )}
                  {area.secondaryOwnerIds.length > 0 && (
                    <span>
                      {" "}
                      · {area.secondaryOwnerIds.length} {t("secondary")}
                    </span>
                  )}
                </div>
              </div>
              <div className="item-card-actions">
                <button
                  className="btn btn-ghost btn-sm"
                  onClick={() => setAssignTarget(area)}
                >
                  {area.primaryOwnerId ? t("transfer") : t("assign")}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {assignTarget && (
        <AssignOwnerModal
          area={assignTarget}
          familyId={familyId}
          members={members}
          onClose={() => setAssignTarget(null)}
        />
      )}
    </div>
  );
}

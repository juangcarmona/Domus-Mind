import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch } from "../../../store/hooks";
import { assignPrimaryOwner, transferArea } from "../../../store/areasSlice";
import type { FamilyMemberResponse, HouseholdAreaItem } from "../../../api/domusmindApi";

interface AssignOwnerModalProps {
  area: HouseholdAreaItem;
  familyId: string;
  members: FamilyMemberResponse[];
  onClose: () => void;
}

export function AssignOwnerModal({
  area,
  familyId,
  members,
  onClose,
}: AssignOwnerModalProps) {
  const { t } = useTranslation("areas");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();

  const [memberId, setMemberId] = useState(area.primaryOwnerId ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!memberId) return;
    setSubmitting(true);
    setError(null);

    const isTransfer = !!area.primaryOwnerId && area.primaryOwnerId !== memberId;
    let succeeded = false;

    if (isTransfer) {
      const result = await dispatch(
        transferArea({ areaId: area.areaId, newPrimaryOwnerId: memberId, familyId }),
      );
      succeeded = transferArea.fulfilled.match(result);
      if (!succeeded) {
        setError(
          (result as { payload?: unknown }).payload as string ?? tCommon("failed"),
        );
      }
    } else {
      const result = await dispatch(
        assignPrimaryOwner({ areaId: area.areaId, memberId, familyId }),
      );
      succeeded = assignPrimaryOwner.fulfilled.match(result);
      if (!succeeded) {
        setError(
          (result as { payload?: unknown }).payload as string ?? tCommon("failed"),
        );
      }
    }

    setSubmitting(false);
    if (succeeded) onClose();
  }

  const title = area.primaryOwnerId
    ? `${t("changeOwner")} - ${area.name}`
    : `${t("setOwner")} - ${area.name}`;

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>{title}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="owner-select">{t("ownerLabel")}</label>
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
                  {m.preferredName || m.name}
                </option>
              ))}
            </select>
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {tCommon("cancel")}
            </button>
            <button
              type="submit"
              className="btn"
              disabled={submitting || !memberId}
            >
              {submitting ? tCommon("saving") : tCommon("save")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

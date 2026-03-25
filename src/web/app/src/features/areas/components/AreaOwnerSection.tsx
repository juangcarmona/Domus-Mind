import { useTranslation } from "react-i18next";
import type { FamilyMemberResponse, HouseholdAreaItem } from "../../../api/domusmindApi";

interface AreaOwnerSectionProps {
  area: HouseholdAreaItem;
  members: FamilyMemberResponse[];
  saving: boolean;
  ownerError: string | null;
  onOwnerChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
}

export function AreaOwnerSection({
  area,
  members,
  saving,
  ownerError,
  onOwnerChange,
}: AreaOwnerSectionProps) {
  const { t } = useTranslation("areas");
  const hasOwner = !!area.primaryOwnerId;

  return (
    <div className="area-detail-section">
      <div className="area-detail-section-header">
        <span className="area-detail-section-title">{t("ownerLabel")}</span>
      </div>
      <select
        className="form-control area-row-select"
        value={area.primaryOwnerId ?? ""}
        disabled={saving}
        onChange={onOwnerChange}
        aria-label={t("ownerLabel")}
      >
        {!hasOwner && <option value="">{t("noOwner")}</option>}
        {hasOwner && !members.some((m) => m.memberId === area.primaryOwnerId) && (
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
      {!hasOwner && <p className="area-detail-hint">{t("noOwnerInstruction")}</p>}
      {ownerError && <p className="error-msg" style={{ marginTop: "0.5rem" }}>{ownerError}</p>}
    </div>
  );
}

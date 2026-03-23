import { useTranslation } from "react-i18next";
import type { MemberAccessStatus } from "../../../api/domusmindApi";

interface MemberAccessActionsProps {
  isCurrentUserManager: boolean;
  hasAccount: boolean;
  accessStatus: MemberAccessStatus;
  canGrantAccess: boolean;
  regenSaving: boolean;
  regenResult: string | null;
  regenError: string | null;
  disableSaving: boolean;
  disableError: string | null;
  enableSaving: boolean;
  enableError: string | null;
  onGrantAccess: () => void;
  onRegenPassword: () => void;
  onDisable: () => void;
  onEnable: () => void;
}

export function MemberAccessActions({
  isCurrentUserManager,
  hasAccount,
  accessStatus,
  canGrantAccess,
  regenSaving,
  regenResult,
  regenError,
  disableSaving,
  disableError,
  enableSaving,
  enableError,
  onGrantAccess,
  onRegenPassword,
  onDisable,
  onEnable,
}: MemberAccessActionsProps) {
  const { t } = useTranslation("members");

  return (
    <>
      <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap" }}>
        {canGrantAccess && (
          <button type="button" className="btn btn-ghost btn-sm" onClick={onGrantAccess}>
            {t("actions.grantAccess")}
          </button>
        )}
        {isCurrentUserManager && hasAccount && accessStatus !== "Disabled" && (
          <>
            <button type="button" className="btn btn-ghost btn-sm" disabled={regenSaving} onClick={onRegenPassword}>
              {regenSaving ? t("actions.saving") : t("actions.resetPassword")}
            </button>
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              style={{ color: "#ef4444", borderColor: "#ef4444" }}
              disabled={disableSaving}
              onClick={onDisable}
            >
              {disableSaving ? t("actions.saving") : t("actions.disableAccess")}
            </button>
          </>
        )}
        {isCurrentUserManager && hasAccount && accessStatus === "Disabled" && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            style={{ color: "#22c55e", borderColor: "#22c55e" }}
            disabled={enableSaving}
            onClick={onEnable}
          >
            {enableSaving ? t("actions.saving") : t("actions.enableAccess")}
          </button>
        )}
      </div>

      {regenResult && (
        <div className="credential-reveal">
          <div className="credential-reveal-body">
            <span className="credential-reveal-label">{t("form.newTemporaryPassword")}:</span>
            <strong className="credential-reveal-value">{regenResult}</strong>
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => navigator.clipboard?.writeText(regenResult!)}>
              {t("actions.copy")}
            </button>
          </div>
          <div className="credential-reveal-warning">{t("form.credentialsSaveWarning")}</div>
        </div>
      )}
      {regenError && <p className="error-msg" style={{ marginTop: "0.5rem" }}>{regenError}</p>}
      {disableError && <p className="error-msg" style={{ marginTop: "0.5rem" }}>{disableError}</p>}
      {enableError && <p className="error-msg" style={{ marginTop: "0.5rem" }}>{enableError}</p>}
    </>
  );
}

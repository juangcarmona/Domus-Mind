import { useTranslation } from "react-i18next";
import type { FamilyMemberResponse } from "../../../api/domusmindApi";
import { Avatar } from "./Avatar";
import { AccessStatusBadge } from "./AccessStatusBadge";

export interface MemberCardProps {
  m: FamilyMemberResponse;
  isCurrentUserManager: boolean;
  onSelect: (m: FamilyMemberResponse) => void;
  onEdit: (id: string) => void;
  onGrantAccess: (id: string) => void;
  onRegenPassword: (id: string) => void;
  onDisable: (id: string) => void;
  onEnable: (id: string) => void;
  regenMemberId: string | null;
  regenResult: string | null;
  regenSaving: boolean;
  regenError: string | null;
  disableSaving: string | null;
  disableError: { memberId: string; message: string } | null;
  enableSaving: string | null;
  enableError: { memberId: string; message: string } | null;
}

export function MemberCard({
  m,
  isCurrentUserManager,
  onSelect,
  onEdit,
  onGrantAccess,
  onRegenPassword,
  onDisable,
  onEnable,
  regenMemberId,
  regenResult,
  regenSaving,
  regenError,
  disableSaving,
  disableError,
  enableSaving,
  enableError,
}: MemberCardProps) {
  const { t } = useTranslation("settings");
  const tM = (key: string) => t(`household.members.${key}` as never);
  const displayName = m.preferredName || m.name;

  return (
    <div className="item-card" style={{ flexWrap: "wrap" }}>
      <button
        type="button"
        style={{
          display: "flex",
          alignItems: "center",
          gap: "0.75rem",
          flex: 1,
          minWidth: 0,
          background: "none",
          border: "none",
          cursor: "pointer",
          textAlign: "left",
          padding: 0,
          color: "inherit",
        }}
        onClick={() => onSelect(m)}
        aria-label={displayName + " details"}
      >
        <Avatar initial={m.avatarInitial} />
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ fontWeight: 600, display: "flex", alignItems: "center", gap: "0.4rem", flexWrap: "wrap" }}>
            <span>{displayName}</span>
            {m.isManager && (
              <span style={{ fontSize: "0.68rem", padding: "0.1rem 0.4rem", borderRadius: 4, background: "color-mix(in srgb, var(--primary) 20%, transparent)", color: "var(--primary)" }}>
                {tM("managerBadge")}
              </span>
            )}
            <AccessStatusBadge status={m.accessStatus} />
          </div>
          <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
            {t(`household.members.roles.${m.role}` as never, m.role)}
            {m.linkedEmail && <span style={{ marginLeft: 8 }}>{m.linkedEmail}</span>}
          </div>
        </div>
      </button>

      <div style={{ display: "flex", gap: "0.35rem", flexShrink: 0, flexWrap: "wrap", justifyContent: "flex-end" }}>
        {m.canEdit && (
          <button type="button" className="btn btn-ghost" style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem" }} onClick={() => onEdit(m.memberId)}>
            {tM("edit")}
          </button>
        )}
        {m.canGrantAccess && (
          <button type="button" className="btn btn-ghost" style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem" }} onClick={() => onGrantAccess(m.memberId)}>
            {tM("provisionAccess")}
          </button>
        )}
        {isCurrentUserManager && m.hasAccount && m.accessStatus !== "Disabled" && (
          <>
            <button
              type="button"
              className="btn btn-ghost"
              style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem" }}
              disabled={regenSaving && regenMemberId === m.memberId}
              onClick={() => onRegenPassword(m.memberId)}
            >
              {regenSaving && regenMemberId === m.memberId ? tM("saving") : tM("regeneratePassword")}
            </button>
            <button
              type="button"
              className="btn btn-ghost"
              style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem", color: "#ef4444" }}
              disabled={disableSaving === m.memberId}
              onClick={() => onDisable(m.memberId)}
            >
              {disableSaving === m.memberId ? tM("saving") : tM("disableAccess")}
            </button>
          </>
        )}
        {isCurrentUserManager && m.hasAccount && m.accessStatus === "Disabled" && (
          <button
            type="button"
            className="btn btn-ghost"
            style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem", color: "#22c55e" }}
            disabled={enableSaving === m.memberId}
            onClick={() => onEnable(m.memberId)}
          >
            {enableSaving === m.memberId ? tM("saving") : tM("enableAccess")}
          </button>
        )}
      </div>

      {regenMemberId === m.memberId && regenResult && (
        <div style={{ width: "100%", marginTop: "0.5rem", background: "color-mix(in srgb, var(--primary) 8%, transparent)", borderRadius: 6, padding: "0.5rem 0.75rem", fontFamily: "monospace", fontSize: "0.85rem" }}>
          <span style={{ color: "var(--muted)", marginRight: 8 }}>{tM("newTemporaryPassword")}:</span>
          <strong>{regenResult}</strong>
          <button
            type="button"
            className="btn btn-ghost"
            style={{ fontSize: "0.7rem", padding: "0.1rem 0.5rem", marginLeft: 8 }}
            onClick={() => navigator.clipboard?.writeText(regenResult!)}
          >
            {tM("copy")}
          </button>
          <div style={{ fontSize: "0.75rem", color: "#f5a623", marginTop: "0.25rem" }}>{tM("credentialsSaveWarning")}</div>
        </div>
      )}
      {regenMemberId === m.memberId && regenError && (
        <p className="error-msg" style={{ marginTop: "0.25rem", width: "100%" }}>{regenError}</p>
      )}
      {disableError?.memberId === m.memberId && (
        <p className="error-msg" style={{ marginTop: "0.25rem", width: "100%" }}>{disableError.message}</p>
      )}
      {enableError?.memberId === m.memberId && (
        <p className="error-msg" style={{ marginTop: "0.25rem", width: "100%" }}>{enableError.message}</p>
      )}
    </div>
  );
}

import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  addMember,
  disableMemberAccess,
  provisionMemberAccess,
  regeneratePassword,
  updateMember,
} from "../../../store/householdSlice";
import {
  AddMemberModal,
  EditMemberModal,
  GrantAccessModal,
  type MemberFormValues,
} from "./MemberModals";

function AccessStatusBadge({
  status,
  tM,
}: {
  status: string;
  tM: (key: string) => string;
}) {
  const map: Record<string, { label: string; color: string }> = {
    None: { label: tM("noAccount"), color: "var(--muted)" },
    PasswordChangeRequired: { label: tM("passwordChangeRequired"), color: "#f5a623" },
    Active: { label: tM("accountActive"), color: "#22c55e" },
    Disabled: { label: tM("accountDisabled"), color: "#ef4444" },
  };
  const badge = map[status] ?? map["None"];
  return (
    <span
      style={{
        fontSize: "0.7rem",
        padding: "0.1rem 0.4rem",
        borderRadius: 4,
        background: `color-mix(in srgb, ${badge.color} 18%, transparent)`,
        color: badge.color,
      }}
    >
      {badge.label}
    </span>
  );
}

export function MembersManagementSection() {
  const { t } = useTranslation("settings");
  const { user } = useAuth();
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const isCurrentUserManager = members.some(
    (m) =>
      (m.authUserId === user?.userId || (user?.memberId != null && m.memberId === user?.memberId)) &&
      m.isManager,
  );

  const tM = (key: string) => t(`household.members.${key}` as never);

  // ── Modal state ─────────────────────────────────────────────────────────────
  const [showAddMember, setShowAddMember] = useState(false);
  const [addSaving, setAddSaving] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  const [grantingAccessId, setGrantingAccessId] = useState<string | null>(null);
  const [provisionSaving, setProvisionSaving] = useState(false);
  const [provisionError, setProvisionError] = useState<string | null>(null);
  const [provisioned, setProvisioned] = useState<{
    email: string;
    temporaryPassword: string;
  } | null>(null);

  // Regenerate password state (per-member, shown inline)
  const [regenMemberId, setRegenMemberId] = useState<string | null>(null);
  const [regenResult, setRegenResult] = useState<string | null>(null);
  const [regenSaving, setRegenSaving] = useState(false);
  const [regenError, setRegenError] = useState<string | null>(null);

  // Disable access state
  const [disableSaving, setDisableSaving] = useState<string | null>(null);
  const [disableError, setDisableError] = useState<{ memberId: string; message: string } | null>(null);

  if (!family) return null;

  const myMemberId = members.find(
    (m) => m.authUserId === user?.userId || (user?.memberId != null && m.memberId === user?.memberId),
  )?.memberId;

  const roleOrder = (role: string) => {
    switch (role) {
      case "Adult": return 0;
      case "Child": return 1;
      case "Pet": return 2;
      default: return 3;
    }
  };

  const visibleMembers = members
    .filter((m) => m.memberId !== myMemberId)
    .slice()
    .sort((a, b) => {
      const rd = roleOrder(a.role) - roleOrder(b.role);
      if (rd !== 0) return rd;
      const md = (b.isManager ? 1 : 0) - (a.isManager ? 1 : 0);
      if (md !== 0) return md;
      return a.name.localeCompare(b.name);
    });

  // ── Handlers ────────────────────────────────────────────────────────────────
  async function handleProfileSave(values: MemberFormValues) {
    if (!editingId) return;
    setEditSaving(true);
    setEditError(null);

    const result = await dispatch(
      updateMember({
        familyId: family!.familyId,
        memberId: editingId,
        name: values.name,
        role: values.role,
        birthDate: values.birthDate || null,
        isManager: values.isManager,
      }),
    );

    setEditSaving(false);
    if (updateMember.fulfilled.match(result)) {
      setEditingId(null);
    } else {
      setEditError((result.payload as string) ?? tM("updateError"));
    }
  }

  async function handleProvisionAccess(email: string, displayName: string | null) {
    if (!grantingAccessId) return;
    setProvisionSaving(true);
    setProvisionError(null);

    const result = await dispatch(
      provisionMemberAccess({
        familyId: family!.familyId,
        memberId: grantingAccessId,
        email,
        displayName,
      }),
    );

    setProvisionSaving(false);
    if (provisionMemberAccess.fulfilled.match(result)) {
      setProvisioned({
        email: result.payload.email,
        temporaryPassword: result.payload.temporaryPassword,
      });
    } else {
      setProvisionError((result.payload as string) ?? tM("provisionError"));
    }
  }

  async function handleRegenPassword(memberId: string) {
    setRegenMemberId(memberId);
    setRegenResult(null);
    setRegenError(null);
    setRegenSaving(true);

    const result = await dispatch(
      regeneratePassword({ familyId: family!.familyId, memberId }),
    );

    setRegenSaving(false);
    if (regeneratePassword.fulfilled.match(result)) {
      setRegenResult(result.payload.temporaryPassword);
    } else {
      setRegenError((result.payload as string) ?? tM("regenError"));
    }
  }

  async function handleDisableAccess(memberId: string) {
    setDisableSaving(memberId);
    setDisableError(null);

    const result = await dispatch(
      disableMemberAccess({ familyId: family!.familyId, memberId }),
    );

    setDisableSaving(null);
    if (!disableMemberAccess.fulfilled.match(result)) {
      setDisableError({
        memberId,
        message: (result.payload as string) ?? tM("disableError"),
      });
    }
  }

  async function handleAddMember({ name, role, birthDate, isManager }: { name: string; role: string; birthDate: string; isManager: boolean }) {
    setAddSaving(true);
    setAddError(null);
    const result = await dispatch(
      addMember({
        familyId: family!.familyId,
        name,
        role,
        birthDate: birthDate || null,
        isManager,
      }),
    );
    setAddSaving(false);
    if (addMember.fulfilled.match(result)) {
      setShowAddMember(false);
    } else {
      setAddError((result.payload as string) ?? tM("addError"));
    }
  }

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">{tM("title")}</h2>
      {isCurrentUserManager && (
        <div style={{ marginBottom: "0.85rem" }}>
          <button
            type="button"
            className="btn"
            onClick={() => { setShowAddMember(true); setAddError(null); }}
          >
            + {tM("addMember")}
          </button>
        </div>
      )}

      {visibleMembers.length === 0 ? (
        <p style={{ color: "var(--muted)", fontSize: "0.9rem" }}>{tM("noMembers")}</p>
      ) : (
        <div className="item-list">
          {visibleMembers.map((m) => (
            <div key={m.memberId} className="item-card">
              <div
                style={{
                  width: 36,
                  height: 36,
                  borderRadius: "50%",
                  background: "color-mix(in srgb, var(--primary) 15%, transparent)",
                  color: "var(--primary)",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  fontWeight: 700,
                  fontSize: "0.9rem",
                  flexShrink: 0,
                }}
              >
                {m.name[0]?.toUpperCase()}
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontWeight: 600, display: "flex", alignItems: "center", gap: "0.4rem", flexWrap: "wrap" }}>
                  <span>{m.name}</span>
                  {m.isManager && (
                    <span
                      style={{
                        fontSize: "0.7rem",
                        padding: "0.1rem 0.4rem",
                        borderRadius: 4,
                        background: "color-mix(in srgb, var(--primary) 20%, transparent)",
                        color: "var(--primary)",
                      }}
                    >
                      {tM("managerBadge")}
                    </span>
                  )}
                  {m.authUserId === user?.userId && (
                    <span
                      style={{
                        fontSize: "0.7rem",
                        padding: "0.1rem 0.4rem",
                        borderRadius: 4,
                        background: "color-mix(in srgb, var(--primary) 12%, transparent)",
                        color: "var(--primary)",
                        fontStyle: "italic",
                      }}
                    >
                      {tM("youBadge")}
                    </span>
                  )}
                  <AccessStatusBadge status={m.accessStatus} tM={tM} />
                </div>
                <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                  {t(`household.members.roles.${m.role}` as never, m.role)}
                  {m.linkedEmail && (
                    <span style={{ marginLeft: 8 }}>{m.linkedEmail}</span>
                  )}
                </div>
                {/* Inline regen results */}
                {regenMemberId === m.memberId && regenResult && (
                  <div
                    style={{
                      marginTop: "0.5rem",
                      background: "color-mix(in srgb, var(--primary) 8%, transparent)",
                      borderRadius: 6,
                      padding: "0.5rem 0.75rem",
                      fontFamily: "monospace",
                      fontSize: "0.85rem",
                    }}
                  >
                    <span style={{ color: "var(--muted)", marginRight: 8 }}>
                      {tM("newTemporaryPassword")}:
                    </span>
                    <strong>{regenResult}</strong>
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ fontSize: "0.7rem", padding: "0.1rem 0.5rem", marginLeft: 8 }}
                      onClick={() => navigator.clipboard?.writeText(regenResult!)}
                    >
                      {tM("copy")}
                    </button>
                    <div style={{ fontSize: "0.75rem", color: "var(--warning, #f5a623)", marginTop: "0.25rem" }}>
                      {tM("credentialsSaveWarning")}
                    </div>
                  </div>
                )}
                {regenMemberId === m.memberId && regenError && (
                  <p className="error-msg" style={{ marginTop: "0.25rem" }}>{regenError}</p>
                )}
                {disableError?.memberId === m.memberId && (
                  <p className="error-msg" style={{ marginTop: "0.25rem" }}>{disableError.message}</p>
                )}
              </div>
              <div style={{ display: "flex", gap: "0.4rem", flexShrink: 0, flexWrap: "wrap", justifyContent: "flex-end" }}>
                {(isCurrentUserManager || m.authUserId === user?.userId) && (
                  <button
                    type="button"
                    className="btn btn-ghost"
                    style={{ fontSize: "0.8rem", padding: "0.25rem 0.6rem" }}
                    onClick={() => { setEditingId(m.memberId); setEditError(null); }}
                  >
                    {tM("edit")}
                  </button>
                )}
                {isCurrentUserManager && !m.authUserId && (
                  <button
                    type="button"
                    className="btn btn-ghost"
                    style={{ fontSize: "0.8rem", padding: "0.25rem 0.6rem" }}
                    onClick={() => { setGrantingAccessId(m.memberId); setProvisionError(null); setProvisioned(null); }}
                  >
                    {tM("provisionAccess")}
                  </button>
                )}
                {isCurrentUserManager &&
                  m.authUserId &&
                  m.accessStatus !== "Disabled" &&
                  m.authUserId !== user?.userId && (
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ fontSize: "0.8rem", padding: "0.25rem 0.6rem" }}
                      disabled={regenSaving && regenMemberId === m.memberId}
                      onClick={() => handleRegenPassword(m.memberId)}
                    >
                      {regenSaving && regenMemberId === m.memberId
                        ? tM("saving")
                        : tM("regeneratePassword")}
                    </button>
                  )}
                {isCurrentUserManager &&
                  m.authUserId &&
                  m.accessStatus !== "Disabled" &&
                  m.authUserId !== user?.userId && (
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ fontSize: "0.8rem", padding: "0.25rem 0.6rem", color: "#ef4444" }}
                      disabled={disableSaving === m.memberId}
                      onClick={() => handleDisableAccess(m.memberId)}
                    >
                      {disableSaving === m.memberId ? tM("saving") : tM("disableAccess")}
                    </button>
                  )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* ── Modals ─────────────────────────────────────────────────────────── */}
      {showAddMember && (
        <AddMemberModal
          saving={addSaving}
          error={addError}
          onSave={handleAddMember}
          onClose={() => { setShowAddMember(false); setAddError(null); }}
        />
      )}
      {editingId !== null && (() => {
        const editingMember = members.find((m) => m.memberId === editingId);
        return editingMember ? (
          <EditMemberModal
            member={editingMember}
            saving={editSaving}
            error={editError}
            onSave={handleProfileSave}
            onClose={() => { setEditingId(null); setEditError(null); }}
          />
        ) : null;
      })()}
      {grantingAccessId !== null && (() => {
        const grantingMember = members.find((m) => m.memberId === grantingAccessId);
        return grantingMember ? (
          <GrantAccessModal
            memberName={grantingMember.name}
            provisioned={provisioned}
            saving={provisionSaving}
            error={provisionError}
            onSave={handleProvisionAccess}
            onClose={() => { setGrantingAccessId(null); setProvisionError(null); setProvisioned(null); }}
          />
        ) : null;
      })()}
    </section>
  );
}

import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  addMember,
  disableMemberAccess,
  enableMemberAccess,
  provisionMemberAccess,
  regeneratePassword,
  updateMember,
  updateMemberProfile,
} from "../../../store/householdSlice";
import {
  AddMemberModal,
  EditMemberModal,
  EditProfileModal,
  GrantAccessModal,
  type MemberFormValues,
  type ProfileFormValues,
} from "./MemberModals";
import type { FamilyMemberResponse, MemberAccessStatus } from "../../../api/domusmindApi";
import { domusmindApi, type MemberDetailResponse } from "../../../api/domusmindApi";

// -- Access status badge -------------------------------------------------------

function AccessStatusBadge({
  status,
  tM,
}: {
  status: MemberAccessStatus;
  tM: (key: string) => string;
}) {
  const map: Record<MemberAccessStatus, { label: string; color: string }> = {
    NoAccess: { label: tM("noAccessBadge"), color: "var(--muted)" },
    InvitedOrProvisioned: { label: tM("invitedOrProvisioned"), color: "#3b82f6" },
    PasswordResetRequired: { label: tM("passwordChangeRequired"), color: "#f5a623" },
    Active: { label: tM("accountActive"), color: "#22c55e" },
    Disabled: { label: tM("accountDisabled"), color: "#ef4444" },
  };
  const badge = map[status] ?? map["NoAccess"];
  return (
    <span
      style={{
        fontSize: "0.68rem",
        padding: "0.1rem 0.4rem",
        borderRadius: 4,
        background: `color-mix(in srgb, ${badge.color} 18%, transparent)`,
        color: badge.color,
        whiteSpace: "nowrap",
      }}
    >
      {badge.label}
    </span>
  );
}

// -- Avatar circle ---------------------------------------------------------------

function Avatar({ initial, size = 36 }: { initial: string; size?: number }) {
  return (
    <div
      style={{
        width: size,
        height: size,
        borderRadius: "50%",
        background: "color-mix(in srgb, var(--primary) 15%, transparent)",
        color: "var(--primary)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        fontWeight: 700,
        fontSize: size >= 40 ? "1rem" : "0.85rem",
        flexShrink: 0,
      }}
    >
      {initial}
    </div>
  );
}

// -- Member card ----------------------------------------------------------------

interface MemberCardProps {
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
  tM: (key: string) => string;
}

function MemberCard({
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
  tM,
}: MemberCardProps) {
  const { t } = useTranslation("settings");
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
            <AccessStatusBadge status={m.accessStatus} tM={tM} />
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
            <button type="button" className="btn btn-ghost" style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem" }} disabled={regenSaving && regenMemberId === m.memberId} onClick={() => onRegenPassword(m.memberId)}>
              {regenSaving && regenMemberId === m.memberId ? tM("saving") : tM("regeneratePassword")}
            </button>
            <button type="button" className="btn btn-ghost" style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem", color: "#ef4444" }} disabled={disableSaving === m.memberId} onClick={() => onDisable(m.memberId)}>
              {disableSaving === m.memberId ? tM("saving") : tM("disableAccess")}
            </button>
          </>
        )}
        {isCurrentUserManager && m.hasAccount && m.accessStatus === "Disabled" && (
          <button type="button" className="btn btn-ghost" style={{ fontSize: "0.78rem", padding: "0.2rem 0.55rem", color: "#22c55e" }} disabled={enableSaving === m.memberId} onClick={() => onEnable(m.memberId)}>
            {enableSaving === m.memberId ? tM("saving") : tM("enableAccess")}
          </button>
        )}
      </div>

      {regenMemberId === m.memberId && regenResult && (
        <div style={{ width: "100%", marginTop: "0.5rem", background: "color-mix(in srgb, var(--primary) 8%, transparent)", borderRadius: 6, padding: "0.5rem 0.75rem", fontFamily: "monospace", fontSize: "0.85rem" }}>
          <span style={{ color: "var(--muted)", marginRight: 8 }}>{tM("newTemporaryPassword")}:</span>
          <strong>{regenResult}</strong>
          <button type="button" className="btn btn-ghost" style={{ fontSize: "0.7rem", padding: "0.1rem 0.5rem", marginLeft: 8 }} onClick={() => navigator.clipboard?.writeText(regenResult!)}>
            {tM("copy")}
          </button>
          <div style={{ fontSize: "0.75rem", color: "#f5a623", marginTop: "0.25rem" }}>{tM("credentialsSaveWarning")}</div>
        </div>
      )}
      {regenMemberId === m.memberId && regenError && <p className="error-msg" style={{ marginTop: "0.25rem", width: "100%" }}>{regenError}</p>}
      {disableError?.memberId === m.memberId && <p className="error-msg" style={{ marginTop: "0.25rem", width: "100%" }}>{disableError.message}</p>}
      {enableError?.memberId === m.memberId && <p className="error-msg" style={{ marginTop: "0.25rem", width: "100%" }}>{enableError.message}</p>}
    </div>
  );
}

// -- Member detail side panel --------------------------------------------------

interface DetailPanelProps {
  member: FamilyMemberResponse;
  detail: MemberDetailResponse | null;
  loadingDetail: boolean;
  isCurrentUserManager: boolean;
  familyId: string;
  onClose: () => void;
  onEditCore: (id: string) => void;
  onGrantAccess: (id: string) => void;
  onRegenPassword: (id: string) => void;
  onDisable: (id: string) => void;
  onEnable: (id: string) => void;
  onProfileSave: (values: ProfileFormValues) => void;
  profileSaving: boolean;
  profileError: string | null;
  tM: (key: string) => string;
}

function DetailPanel({
  member, detail, loadingDetail, isCurrentUserManager,
  onClose, onEditCore, onGrantAccess, onRegenPassword, onDisable, onEnable,
  onProfileSave, profileSaving, profileError, tM,
}: DetailPanelProps) {
  const { t } = useTranslation("settings");
  const [activeTab, setActiveTab] = useState<"core" | "access" | "contacts" | "notes">("core");
  const [isEditingProfile, setIsEditingProfile] = useState(false);
  const displayName = member.preferredName || member.name;

  function TabBtn({ id, label }: { id: typeof activeTab; label: string }) {
    return (
      <button type="button" onClick={() => setActiveTab(id)}
        style={{ padding: "0.4rem 0.75rem", fontSize: "0.82rem", border: "none", borderBottom: activeTab === id ? "2px solid var(--primary)" : "2px solid transparent", background: "none", cursor: "pointer", color: activeTab === id ? "var(--primary)" : "var(--muted)", fontWeight: activeTab === id ? 600 : 400 }}>
        {label}
      </button>
    );
  }

  return (
    <div style={{ position: "fixed", inset: 0, zIndex: 200, display: "flex", justifyContent: "flex-end" }}>
      <div style={{ position: "absolute", inset: 0, background: "rgba(0,0,0,0.35)" }} onClick={onClose} />
      <div style={{ position: "relative", width: "min(420px, 95vw)", height: "100%", background: "var(--surface, #fff)", boxShadow: "-4px 0 24px rgba(0,0,0,0.18)", display: "flex", flexDirection: "column", overflow: "hidden" }} onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "0.75rem", padding: "1rem 1rem 0.75rem", borderBottom: "1px solid var(--border, #eee)" }}>
          <Avatar initial={member.avatarInitial} size={44} />
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ fontWeight: 700, fontSize: "1rem", display: "flex", alignItems: "center", gap: "0.4rem", flexWrap: "wrap" }}>
              <span>{displayName}</span>
              {member.isManager && (
                <span style={{ fontSize: "0.68rem", padding: "0.1rem 0.4rem", borderRadius: 4, background: "color-mix(in srgb, var(--primary) 20%, transparent)", color: "var(--primary)" }}>
                  {tM("managerBadge")}
                </span>
              )}
            </div>
            <div style={{ fontSize: "0.82rem", color: "var(--muted)" }}>
              {t(`household.members.roles.${member.role}` as never, member.role)}
            </div>
          </div>
          <button type="button" className="btn btn-ghost btn-sm" onClick={onClose} aria-label={tM("closePanel")}>✕</button>
        </div>

        {/* Tabs */}
        <div style={{ display: "flex", borderBottom: "1px solid var(--border, #eee)", padding: "0 0.5rem" }}>
          <TabBtn id="core" label={tM("profile")} />
          <TabBtn id="access" label={tM("access")} />
          <TabBtn id="contacts" label={tM("contacts")} />
          <TabBtn id="notes" label={tM("notes")} />
        </div>

        {/* Body */}
        <div style={{ flex: 1, overflowY: "auto", padding: "1rem" }}>
          {loadingDetail && <p style={{ color: "var(--muted)", fontSize: "0.9rem" }}>Loading…</p>}

          {!loadingDetail && activeTab === "core" && (
            <div>
              <div className="settings-field-group" style={{ marginBottom: "1rem" }}>
                <div className="settings-field">
                  <span className="settings-field-label">{tM("name")}</span>
                  <span className="settings-field-value">{member.name}</span>
                </div>
                {detail?.preferredName && (
                  <div className="settings-field">
                    <span className="settings-field-label">{tM("preferredName")}</span>
                    <span className="settings-field-value">{detail.preferredName}</span>
                  </div>
                )}
                <div className="settings-field">
                  <span className="settings-field-label">{tM("role")}</span>
                  <span className="settings-field-value">{t(`household.members.roles.${member.role}` as never, member.role)}</span>
                </div>
                {member.birthDate && (
                  <div className="settings-field">
                    <span className="settings-field-label">{tM("birthDate")}</span>
                    <span className="settings-field-value">{new Date(member.birthDate).toLocaleDateString()}</span>
                  </div>
                )}
              </div>
              {member.canEdit && (
                <>
                  <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem", marginRight: "0.4rem" }} onClick={() => onEditCore(member.memberId)}>
                    {tM("edit")}
                  </button>
                  <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem" }} onClick={() => setIsEditingProfile(true)}>
                    {tM("editProfile")}
                  </button>
                </>
              )}
            </div>
          )}

          {!loadingDetail && activeTab === "access" && (
            <div>
              <div className="settings-field-group" style={{ marginBottom: "1rem" }}>
                <div className="settings-field">
                  <span className="settings-field-label">{tM("access")}</span>
                  <AccessStatusBadge status={member.accessStatus} tM={tM} />
                </div>
                {member.linkedEmail && (
                  <div className="settings-field">
                    <span className="settings-field-label">{tM("email")}</span>
                    <span className="settings-field-value">{member.linkedEmail}</span>
                  </div>
                )}
                {detail?.lastLoginAtUtc && (
                  <div className="settings-field">
                    <span className="settings-field-label">{tM("lastLogin")}</span>
                    <span className="settings-field-value">{new Date(detail.lastLoginAtUtc).toLocaleString()}</span>
                  </div>
                )}
                {member.hasAccount && !detail?.lastLoginAtUtc && (
                  <div className="settings-field">
                    <span className="settings-field-label">{tM("lastLogin")}</span>
                    <span className="settings-field-value" style={{ color: "var(--muted)" }}>{tM("neverLoggedIn")}</span>
                  </div>
                )}
              </div>
              <div style={{ display: "flex", gap: "0.4rem", flexWrap: "wrap" }}>
                {member.canGrantAccess && (
                  <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem" }} onClick={() => onGrantAccess(member.memberId)}>
                    {tM("provisionAccess")}
                  </button>
                )}
                {isCurrentUserManager && member.hasAccount && member.accessStatus !== "Disabled" && (
                  <>
                    <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem" }} onClick={() => onRegenPassword(member.memberId)}>
                      {tM("regeneratePassword")}
                    </button>
                    <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem", color: "#ef4444" }} onClick={() => onDisable(member.memberId)}>
                      {tM("disableAccess")}
                    </button>
                  </>
                )}
                {isCurrentUserManager && member.hasAccount && member.accessStatus === "Disabled" && (
                  <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem", color: "#22c55e" }} onClick={() => onEnable(member.memberId)}>
                    {tM("enableAccess")}
                  </button>
                )}
              </div>
            </div>
          )}

          {!loadingDetail && activeTab === "contacts" && (
            <div>
              <div className="settings-field-group" style={{ marginBottom: "1rem" }}>
                <div className="settings-field">
                  <span className="settings-field-label">{tM("primaryPhone")}</span>
                  <span className="settings-field-value">{detail?.primaryPhone || <span style={{ color: "var(--muted)" }}>—</span>}</span>
                </div>
                <div className="settings-field">
                  <span className="settings-field-label">{tM("primaryEmail")}</span>
                  <span className="settings-field-value">{detail?.primaryEmail || <span style={{ color: "var(--muted)" }}>—</span>}</span>
                </div>
              </div>
              {member.canEdit && (
                <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem" }} onClick={() => setIsEditingProfile(true)}>
                  {tM("editProfile")}
                </button>
              )}
            </div>
          )}

          {!loadingDetail && activeTab === "notes" && (
            <div>
              <div style={{ background: "color-mix(in srgb, var(--primary) 5%, transparent)", borderRadius: 8, padding: "0.75rem", fontSize: "0.88rem", minHeight: 72, marginBottom: "0.75rem", color: detail?.householdNote ? "inherit" : "var(--muted)" }}>
                {detail?.householdNote || tM("householdNotePlaceholder")}
              </div>
              {(member.canEdit || isCurrentUserManager) && (
                <button type="button" className="btn btn-ghost" style={{ fontSize: "0.82rem", padding: "0.3rem 0.75rem" }} onClick={() => setIsEditingProfile(true)}>
                  {tM("editProfile")}
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {isEditingProfile && (
        <EditProfileModal
          member={detail ?? member}
          saving={profileSaving}
          error={profileError}
          onSave={(values) => { onProfileSave(values); setIsEditingProfile(false); }}
          onClose={() => setIsEditingProfile(false)}
        />
      )}
    </div>
  );
}

// -- Member group label --------------------------------------------------------

function MemberGroup({
  title, members, ...rest
}: {
  title: string;
  members: FamilyMemberResponse[];
} & Omit<MemberCardProps, "m">) {
  if (members.length === 0) return null;
  return (
    <div style={{ marginBottom: "1.25rem" }}>
      <div style={{ fontSize: "0.75rem", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.06em", color: "var(--muted)", marginBottom: "0.4rem", paddingLeft: "0.25rem" }}>
        {title}
      </div>
      <div className="item-list">
        {members.map((m) => <MemberCard key={m.memberId} m={m} {...rest} />)}
      </div>
    </div>
  );
}

// -- Main component ------------------------------------------------------------

export function MembersManagementSection() {
  const { t } = useTranslation("settings");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const isCurrentUserManager = members.some((m) => m.isCurrentUser && m.isManager);
  const tM = (key: string) => t(`household.members.${key}` as never);

  const [showAddMember, setShowAddMember] = useState(false);
  const [addSaving, setAddSaving] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  const [grantingAccessId, setGrantingAccessId] = useState<string | null>(null);
  const [provisionSaving, setProvisionSaving] = useState(false);
  const [provisionError, setProvisionError] = useState<string | null>(null);
  const [provisioned, setProvisioned] = useState<{ email: string; temporaryPassword: string } | null>(null);

  const [regenMemberId, setRegenMemberId] = useState<string | null>(null);
  const [regenResult, setRegenResult] = useState<string | null>(null);
  const [regenSaving, setRegenSaving] = useState(false);
  const [regenError, setRegenError] = useState<string | null>(null);

  const [disableSaving, setDisableSaving] = useState<string | null>(null);
  const [disableError, setDisableError] = useState<{ memberId: string; message: string } | null>(null);

  const [enableSaving, setEnableSaving] = useState<string | null>(null);
  const [enableError, setEnableError] = useState<{ memberId: string; message: string } | null>(null);

  const [selectedMember, setSelectedMember] = useState<FamilyMemberResponse | null>(null);
  const [memberDetail, setMemberDetail] = useState<MemberDetailResponse | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);

  if (!family) return null;

  const accessPriority = (m: FamilyMemberResponse) => {
    if (m.accessStatus === "Active") return 0;
    if (m.accessStatus === "InvitedOrProvisioned" || m.accessStatus === "PasswordResetRequired") return 1;
    if (m.accessStatus === "Disabled") return 2;
    return 3;
  };

  const sortedOthers = members
    .filter((m) => !m.isCurrentUser)
    .slice()
    .sort((a, b) => {
      const md = (b.isManager ? 1 : 0) - (a.isManager ? 1 : 0);
      if (md !== 0) return md;
      const ad = accessPriority(a) - accessPriority(b);
      if (ad !== 0) return ad;
      return (a.preferredName || a.name).localeCompare(b.preferredName || b.name);
    });

  const adults = sortedOthers.filter((m) => m.role === "Adult" || m.role === "Caregiver");
  const children = sortedOthers.filter((m) => m.role === "Child");
  const pets = sortedOthers.filter((m) => m.role === "Pet");

  async function handleSelectMember(m: FamilyMemberResponse) {
    setSelectedMember(m);
    setMemberDetail(null);
    setLoadingDetail(true);
    try {
      const detail = await domusmindApi.getMemberDetails(family!.familyId, m.memberId);
      setMemberDetail(detail);
    } catch { /* ignore */ } finally {
      setLoadingDetail(false);
    }
  }

  async function handleProfileSave(values: ProfileFormValues) {
    if (!selectedMember) return;
    setProfileSaving(true);
    setProfileError(null);
    const result = await dispatch(
      updateMemberProfile({
        familyId: family!.familyId,
        memberId: selectedMember.memberId,
        preferredName: values.preferredName || null,
        primaryPhone: values.primaryPhone || null,
        primaryEmail: values.primaryEmail || null,
        householdNote: values.householdNote || null,
      }),
    );
    setProfileSaving(false);
    if (updateMemberProfile.fulfilled.match(result)) {
      try {
        const detail = await domusmindApi.getMemberDetails(family!.familyId, selectedMember.memberId);
        setMemberDetail(detail);
      } catch { /* ignore */ }
    } else {
      setProfileError((result.payload as string) ?? tM("updateError"));
    }
  }

  async function handleEditCoreSave(values: MemberFormValues) {
    if (!editingId) return;
    setEditSaving(true);
    setEditError(null);
    const result = await dispatch(
      updateMember({ familyId: family!.familyId, memberId: editingId, name: values.name, role: values.role, birthDate: values.birthDate || null, isManager: values.isManager }),
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
    const result = await dispatch(provisionMemberAccess({ familyId: family!.familyId, memberId: grantingAccessId, email, displayName }));
    setProvisionSaving(false);
    if (provisionMemberAccess.fulfilled.match(result)) {
      setProvisioned({ email: result.payload.email, temporaryPassword: result.payload.temporaryPassword });
    } else {
      setProvisionError((result.payload as string) ?? tM("provisionError"));
    }
  }

  async function handleRegenPassword(memberId: string) {
    setRegenMemberId(memberId);
    setRegenResult(null);
    setRegenError(null);
    setRegenSaving(true);
    const result = await dispatch(regeneratePassword({ familyId: family!.familyId, memberId }));
    setRegenSaving(false);
    if (regeneratePassword.fulfilled.match(result)) { setRegenResult(result.payload.temporaryPassword); }
    else { setRegenError((result.payload as string) ?? tM("regenError")); }
  }

  async function handleDisable(memberId: string) {
    setDisableSaving(memberId);
    setDisableError(null);
    const result = await dispatch(disableMemberAccess({ familyId: family!.familyId, memberId }));
    setDisableSaving(null);
    if (!disableMemberAccess.fulfilled.match(result)) {
      setDisableError({ memberId, message: (result.payload as string) ?? tM("disableError") });
    }
    if (selectedMember?.memberId === memberId) {
      try { setMemberDetail(await domusmindApi.getMemberDetails(family!.familyId, memberId)); } catch { /* ignore */ }
    }
  }

  async function handleEnable(memberId: string) {
    setEnableSaving(memberId);
    setEnableError(null);
    const result = await dispatch(enableMemberAccess({ familyId: family!.familyId, memberId }));
    setEnableSaving(null);
    if (!enableMemberAccess.fulfilled.match(result)) {
      setEnableError({ memberId, message: (result.payload as string) ?? tM("enableError") });
    }
    if (selectedMember?.memberId === memberId) {
      try { setMemberDetail(await domusmindApi.getMemberDetails(family!.familyId, memberId)); } catch { /* ignore */ }
    }
  }

  async function handleAddMember({ name, role, birthDate, isManager }: { name: string; role: string; birthDate: string; isManager: boolean }) {
    setAddSaving(true);
    setAddError(null);
    const result = await dispatch(addMember({ familyId: family!.familyId, name, role, birthDate: birthDate || null, isManager }));
    setAddSaving(false);
    if (addMember.fulfilled.match(result)) { setShowAddMember(false); }
    else { setAddError((result.payload as string) ?? tM("addError")); }
  }

  const cardProps = {
    isCurrentUserManager,
    onSelect: handleSelectMember,
    onEdit: (id: string) => { setEditingId(id); setEditError(null); },
    onGrantAccess: (id: string) => { setGrantingAccessId(id); setProvisionError(null); setProvisioned(null); },
    onRegenPassword: handleRegenPassword,
    onDisable: handleDisable,
    onEnable: handleEnable,
    regenMemberId,
    regenResult,
    regenSaving,
    regenError,
    disableSaving,
    disableError,
    enableSaving,
    enableError,
    tM,
  };

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">{t("membersTab.householdMembers")}</h2>

      {isCurrentUserManager && (
        <div style={{ marginBottom: "0.85rem" }}>
          <button type="button" className="btn" onClick={() => { setShowAddMember(true); setAddError(null); }}>
            + {tM("addMember")}
          </button>
        </div>
      )}

      {adults.length === 0 && children.length === 0 && pets.length === 0 ? (
        <p style={{ color: "var(--muted)", fontSize: "0.9rem" }}>{tM("noMembers")}</p>
      ) : (
        <>
          <MemberGroup title={t("household.members.groups.adults" as never)} members={adults} {...cardProps} />
          <MemberGroup title={t("household.members.groups.children" as never)} members={children} {...cardProps} />
          <MemberGroup title={t("household.members.groups.pets" as never)} members={pets} {...cardProps} />
        </>
      )}

      {selectedMember && (
        <DetailPanel
          member={selectedMember}
          detail={memberDetail}
          loadingDetail={loadingDetail}
          isCurrentUserManager={isCurrentUserManager}
          familyId={family.familyId}
          onClose={() => { setSelectedMember(null); setMemberDetail(null); }}
          onEditCore={(id) => { setEditingId(id); setEditError(null); }}
          onGrantAccess={(id) => { setGrantingAccessId(id); setProvisionError(null); setProvisioned(null); }}
          onRegenPassword={handleRegenPassword}
          onDisable={handleDisable}
          onEnable={handleEnable}
          onProfileSave={handleProfileSave}
          profileSaving={profileSaving}
          profileError={profileError}
          tM={tM}
        />
      )}

      {showAddMember && (
        <AddMemberModal saving={addSaving} error={addError} onSave={handleAddMember} onClose={() => { setShowAddMember(false); setAddError(null); }} />
      )}
      {editingId !== null && (() => {
        const em = members.find((m) => m.memberId === editingId);
        return em ? (
          <EditMemberModal member={em} saving={editSaving} error={editError} onSave={handleEditCoreSave} onClose={() => { setEditingId(null); setEditError(null); }} />
        ) : null;
      })()}
      {grantingAccessId !== null && (() => {
        const gm = members.find((m) => m.memberId === grantingAccessId);
        return gm ? (
          <GrantAccessModal memberName={gm.preferredName || gm.name} provisioned={provisioned} saving={provisionSaving} error={provisionError} onSave={handleProvisionAccess} onClose={() => { setGrantingAccessId(null); setProvisionError(null); setProvisioned(null); }} />
        ) : null;
      })()}
    </section>
  );
}

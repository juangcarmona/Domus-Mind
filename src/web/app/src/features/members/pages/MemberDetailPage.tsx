import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  updateMember,
  updateMemberProfile,
  provisionMemberAccess,
  regeneratePassword,
  disableMemberAccess,
  enableMemberAccess,
  fetchMembers,
} from "../../../store/householdSlice";
import { domusmindApi, type MemberDetailResponse } from "../../../api/domusmindApi";
import {
  EditMemberModal,
  EditProfileModal,
  GrantAccessModal,
  type MemberFormValues,
  type ProfileFormValues,
} from "../components/MemberModals";
import { AccessStatusBadge } from "../components/AccessStatusBadge";
import { DetailSection } from "../components/DetailSection";
import { MemberAccessActions } from "../components/MemberAccessActions";

export function MemberDetailPage() {
  const { memberId } = useParams<{ memberId: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation("members");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const isCurrentUserManager = members.some((m) => m.isCurrentUser && m.isManager);
  const member = members.find((m) => m.memberId === memberId);

  const [detail, setDetail] = useState<MemberDetailResponse | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);

  const [editingCore, setEditingCore] = useState(false);
  const [coreSaving, setCoreSaving] = useState(false);
  const [coreError, setCoreError] = useState<string | null>(null);

  const [editingProfile, setEditingProfile] = useState(false);
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);

  const [grantingAccess, setGrantingAccess] = useState(false);
  const [provisionSaving, setProvisionSaving] = useState(false);
  const [provisionError, setProvisionError] = useState<string | null>(null);
  const [provisioned, setProvisioned] = useState<{ email: string; temporaryPassword: string } | null>(null);

  const [regenSaving, setRegenSaving] = useState(false);
  const [regenResult, setRegenResult] = useState<string | null>(null);
  const [regenError, setRegenError] = useState<string | null>(null);

  const [disableSaving, setDisableSaving] = useState(false);
  const [disableError, setDisableError] = useState<string | null>(null);

  const [enableSaving, setEnableSaving] = useState(false);
  const [enableError, setEnableError] = useState<string | null>(null);

  useEffect(() => {
    if (!memberId || !family) return;
    setLoadingDetail(true);
    setDetail(null);
    domusmindApi
      .getMemberDetails(family.familyId, memberId)
      .then(setDetail)
      .catch(() => { /* ignore */ })
      .finally(() => setLoadingDetail(false));
  }, [memberId, family]);

  if (!family) return null;

  if (!member) {
    return (
      <div className="member-detail-page">
        <button type="button" className="btn btn-ghost btn-sm" onClick={() => navigate("/members")} style={{ marginBottom: "1rem" }}>
          ← {t("detail.back")}
        </button>
        <p style={{ color: "var(--muted)" }}>{t("errors.notFound")}</p>
      </div>
    );
  }

  const displayName = member.preferredName || member.name;
  const effectiveDetail = detail ?? member;

  async function refreshDetail() {
    if (!family || !memberId) return;
    await dispatch(fetchMembers(family.familyId));
    try {
      const d = await domusmindApi.getMemberDetails(family.familyId, memberId);
      setDetail(d);
    } catch { /* ignore */ }
  }

  async function handleCoreSave(values: MemberFormValues) {
    setCoreSaving(true); setCoreError(null);
    const result = await dispatch(updateMember({ familyId: family!.familyId, memberId: memberId!, name: values.name, role: values.role, birthDate: values.birthDate || null, isManager: values.isManager }));
    setCoreSaving(false);
    if (updateMember.fulfilled.match(result)) { setEditingCore(false); await refreshDetail(); }
    else { setCoreError((result.payload as string) ?? t("errors.updateFailed")); }
  }

  async function handleProfileSave(values: ProfileFormValues) {
    setProfileSaving(true); setProfileError(null);
    const result = await dispatch(updateMemberProfile({ familyId: family!.familyId, memberId: memberId!, preferredName: values.preferredName || null, primaryPhone: values.primaryPhone || null, primaryEmail: values.primaryEmail || null, householdNote: values.householdNote || null }));
    setProfileSaving(false);
    if (updateMemberProfile.fulfilled.match(result)) { setEditingProfile(false); await refreshDetail(); }
    else { setProfileError((result.payload as string) ?? t("errors.updateFailed")); }
  }

  async function handleProvisionAccess(email: string, displayNameVal: string | null) {
    setProvisionSaving(true); setProvisionError(null);
    const result = await dispatch(provisionMemberAccess({ familyId: family!.familyId, memberId: memberId!, email, displayName: displayNameVal }));
    setProvisionSaving(false);
    if (provisionMemberAccess.fulfilled.match(result)) {
      setProvisioned({ email: result.payload.email, temporaryPassword: result.payload.temporaryPassword });
      await refreshDetail();
    } else {
      setProvisionError((result.payload as string) ?? t("errors.provisionFailed"));
    }
  }

  async function handleRegenPassword() {
    setRegenSaving(true); setRegenResult(null); setRegenError(null);
    const result = await dispatch(regeneratePassword({ familyId: family!.familyId, memberId: memberId! }));
    setRegenSaving(false);
    if (regeneratePassword.fulfilled.match(result)) { setRegenResult(result.payload.temporaryPassword); }
    else { setRegenError((result.payload as string) ?? t("errors.regenFailed")); }
  }

  async function handleDisable() {
    setDisableSaving(true); setDisableError(null);
    const result = await dispatch(disableMemberAccess({ familyId: family!.familyId, memberId: memberId! }));
    setDisableSaving(false);
    if (!disableMemberAccess.fulfilled.match(result)) { setDisableError((result.payload as string) ?? t("errors.disableFailed")); }
    await refreshDetail();
  }

  async function handleEnable() {
    setEnableSaving(true); setEnableError(null);
    const result = await dispatch(enableMemberAccess({ familyId: family!.familyId, memberId: memberId! }));
    setEnableSaving(false);
    if (!enableMemberAccess.fulfilled.match(result)) { setEnableError((result.payload as string) ?? t("errors.enableFailed")); }
    await refreshDetail();
  }

  const showContacts = member.canEdit || isCurrentUserManager;
  const showNotes = member.canEdit || isCurrentUserManager;

  return (
    <div className="member-detail-page">
      <button type="button" className="btn btn-ghost btn-sm" onClick={() => navigate("/members")} style={{ marginBottom: "1.25rem" }}>
        ← {t("detail.back")}
      </button>

      <div className="member-detail-header">
        <div className="member-detail-avatar" aria-hidden="true">{member.avatarInitial}</div>
        <div className="member-detail-identity">
          <h1 className="member-detail-name">
            <span>{displayName}</span>
            {member.isManager && <span className="member-badge member-badge--manager">{t("managerBadge")}</span>}
            {member.isCurrentUser && <span className="member-badge member-badge--you">{t("youBadge")}</span>}
          </h1>
          <div className="member-detail-subtitle">
            <span>{t(`roles.${member.role}` as never, member.role)}</span>
            {member.linkedEmail && <span style={{ marginLeft: "0.6rem", color: "var(--muted)" }}>· {member.linkedEmail}</span>}
          </div>
        </div>
        <div className="member-detail-status">
          <AccessStatusBadge status={member.accessStatus} />
        </div>
      </div>

      <DetailSection
        title={t("detail.coreDetails")}
        action={member.canEdit ? (
          <button type="button" className="btn btn-ghost btn-sm" onClick={() => { setEditingCore(true); setCoreError(null); }}>
            {t("actions.edit")}
          </button>
        ) : undefined}
      >
        <div className="settings-field-group">
          <div className="settings-field">
            <span className="settings-field-label">{t("detail.name")}</span>
            <span className="settings-field-value">{member.name}</span>
          </div>
          {(detail?.preferredName ?? member.preferredName) && (
            <div className="settings-field">
              <span className="settings-field-label">{t("detail.preferredName")}</span>
              <span className="settings-field-value">{detail?.preferredName ?? member.preferredName}</span>
            </div>
          )}
          <div className="settings-field">
            <span className="settings-field-label">{t("detail.role")}</span>
            <span className="settings-field-value">{t(`roles.${member.role}` as never, member.role)}</span>
          </div>
          {member.birthDate && (
            <div className="settings-field">
              <span className="settings-field-label">{t("detail.birthDate")}</span>
              <span className="settings-field-value">{new Date(member.birthDate).toLocaleDateString()}</span>
            </div>
          )}
        </div>
      </DetailSection>

      <DetailSection title={t("detail.access")}>
        <div className="settings-field-group" style={{ marginBottom: "0.85rem" }}>
          <div className="settings-field">
            <span className="settings-field-label">{t("detail.status")}</span>
            <AccessStatusBadge status={member.accessStatus} />
          </div>
          {member.linkedEmail && (
            <div className="settings-field">
              <span className="settings-field-label">{t("detail.email")}</span>
              <span className="settings-field-value">{member.linkedEmail}</span>
            </div>
          )}
          {!loadingDetail && member.hasAccount && (
            <div className="settings-field">
              <span className="settings-field-label">{t("detail.lastLogin")}</span>
              <span className="settings-field-value">
                {detail?.lastLoginAtUtc ? new Date(detail.lastLoginAtUtc).toLocaleString() : <span style={{ color: "var(--muted)" }}>{t("detail.neverLoggedIn")}</span>}
              </span>
            </div>
          )}
        </div>
        <MemberAccessActions
          isCurrentUserManager={isCurrentUserManager}
          hasAccount={member.hasAccount}
          accessStatus={member.accessStatus}
          canGrantAccess={member.canGrantAccess}
          regenSaving={regenSaving}
          regenResult={regenResult}
          regenError={regenError}
          disableSaving={disableSaving}
          disableError={disableError}
          enableSaving={enableSaving}
          enableError={enableError}
          onGrantAccess={() => { setGrantingAccess(true); setProvisionError(null); setProvisioned(null); }}
          onRegenPassword={handleRegenPassword}
          onDisable={handleDisable}
          onEnable={handleEnable}
        />
      </DetailSection>

      {showContacts && (
        <DetailSection
          title={t("detail.contacts")}
          action={member.canEdit ? (
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => { setEditingProfile(true); setProfileError(null); }}>
              {t("actions.editProfile")}
            </button>
          ) : undefined}
        >
          {loadingDetail ? <p style={{ color: "var(--muted)", fontSize: "0.875rem" }}>…</p> : (
            <div className="settings-field-group">
              <div className="settings-field">
                <span className="settings-field-label">{t("detail.primaryPhone")}</span>
                <span className="settings-field-value">{detail?.primaryPhone || <span style={{ color: "var(--muted)" }}>—</span>}</span>
              </div>
              <div className="settings-field">
                <span className="settings-field-label">{t("detail.primaryEmail")}</span>
                <span className="settings-field-value">{detail?.primaryEmail || <span style={{ color: "var(--muted)" }}>—</span>}</span>
              </div>
            </div>
          )}
        </DetailSection>
      )}

      {showNotes && (
        <DetailSection
          title={t("detail.notes")}
          action={(member.canEdit || isCurrentUserManager) ? (
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => { setEditingProfile(true); setProfileError(null); }}>
              {t("actions.editProfile")}
            </button>
          ) : undefined}
        >
          {loadingDetail ? <p style={{ color: "var(--muted)", fontSize: "0.875rem" }}>…</p> : (
            <div className="member-note-block">
              {detail?.householdNote || <span style={{ color: "var(--muted)" }}>{t("detail.householdNotePlaceholder")}</span>}
            </div>
          )}
        </DetailSection>
      )}

      {editingCore && (
        <EditMemberModal member={member} saving={coreSaving} error={coreError} onSave={handleCoreSave} onClose={() => { setEditingCore(false); setCoreError(null); }} />
      )}
      {editingProfile && (
        <EditProfileModal member={effectiveDetail} saving={profileSaving} error={profileError} onSave={handleProfileSave} onClose={() => { setEditingProfile(false); setProfileError(null); }} />
      )}
      {grantingAccess && (
        <GrantAccessModal memberName={displayName} provisioned={provisioned} saving={provisionSaving} error={provisionError} onSave={handleProvisionAccess} onClose={() => { setGrantingAccess(false); setProvisionError(null); setProvisioned(null); }} />
      )}
    </div>
  );
}

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
  GrantAccessModal,
  type MemberFormValues,
  type ProfileFormValues,
} from "./MemberModals";
import { MemberGroup } from "./MemberGroup";
import { MemberDetailPanel } from "./MemberDetailPanel";
import type { FamilyMemberResponse, MemberAccessStatus } from "../../../api/domusmindApi";
import { domusmindApi, type MemberDetailResponse } from "../../../api/domusmindApi";

function accessPriority(status: MemberAccessStatus): number {
  if (status === "Active") return 0;
  if (status === "InvitedOrProvisioned" || status === "PasswordResetRequired") return 1;
  if (status === "Disabled") return 2;
  return 3;
}

export function MembersManagementSection() {
  const { t } = useTranslation("settings");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const isCurrentUserManager = members.some((m) => m.isCurrentUser && m.isManager);

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

  const tM = (key: string) => t(`household.members.${key}` as never);

  const sortedOthers = members
    .filter((m) => !m.isCurrentUser)
    .slice()
    .sort((a, b) => {
      const md = (b.isManager ? 1 : 0) - (a.isManager ? 1 : 0);
      if (md !== 0) return md;
      const ad = accessPriority(a.accessStatus) - accessPriority(b.accessStatus);
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
    if (regeneratePassword.fulfilled.match(result)) {
      setRegenResult(result.payload.temporaryPassword);
    } else {
      setRegenError((result.payload as string) ?? tM("regenError"));
    }
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
    if (addMember.fulfilled.match(result)) {
      setShowAddMember(false);
    } else {
      setAddError((result.payload as string) ?? tM("addError"));
    }
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
        <MemberDetailPanel
          member={selectedMember}
          detail={memberDetail}
          loadingDetail={loadingDetail}
          isCurrentUserManager={isCurrentUserManager}
          onClose={() => { setSelectedMember(null); setMemberDetail(null); }}
          onEditCore={(id) => { setEditingId(id); setEditError(null); }}
          onGrantAccess={(id) => { setGrantingAccessId(id); setProvisionError(null); setProvisioned(null); }}
          onRegenPassword={handleRegenPassword}
          onDisable={handleDisable}
          onEnable={handleEnable}
          onProfileSave={handleProfileSave}
          profileSaving={profileSaving}
          profileError={profileError}
        />
      )}

      {showAddMember && (
        <AddMemberModal
          saving={addSaving}
          error={addError}
          onSave={handleAddMember}
          onClose={() => { setShowAddMember(false); setAddError(null); }}
        />
      )}
      {editingId !== null && (() => {
        const em = members.find((m) => m.memberId === editingId);
        return em ? (
          <EditMemberModal
            member={em}
            saving={editSaving}
            error={editError}
            onSave={handleEditCoreSave}
            onClose={() => { setEditingId(null); setEditError(null); }}
          />
        ) : null;
      })()}
      {grantingAccessId !== null && (() => {
        const gm = members.find((m) => m.memberId === grantingAccessId);
        return gm ? (
          <GrantAccessModal
            memberName={gm.preferredName || gm.name}
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

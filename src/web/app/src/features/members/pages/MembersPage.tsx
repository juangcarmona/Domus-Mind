import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { addMember } from "../../../store/householdSlice";
import { AddMemberModal } from "../components/MemberModals";
import { AccessStatusBadge } from "../components/AccessStatusBadge";
import type { FamilyMemberResponse, MemberAccessStatus } from "../../../api/domusmindApi";

// ── Sort helpers ───────────────────────────────────────────────────────────────

function accessPriority(m: FamilyMemberResponse): number {
  const priorities: Record<MemberAccessStatus, number> = {
    Active: 0,
    InvitedOrProvisioned: 1,
    PasswordResetRequired: 1,
    Disabled: 2,
    NoAccess: 3,
  };
  return priorities[m.accessStatus] ?? 3;
}

function sortMembers(list: FamilyMemberResponse[]): FamilyMemberResponse[] {
  return list.slice().sort((a, b) => {
    const managerDiff = (b.isManager ? 1 : 0) - (a.isManager ? 1 : 0);
    if (managerDiff !== 0) return managerDiff;
    const accessDiff = accessPriority(a) - accessPriority(b);
    if (accessDiff !== 0) return accessDiff;
    return (a.preferredName || a.name).localeCompare(b.preferredName || b.name);
  });
}

// ── Member row ─────────────────────────────────────────────────────────────────

function MemberRow({ m }: { m: FamilyMemberResponse }) {
  const { t } = useTranslation("members");
  const navigate = useNavigate();
  const displayName = m.preferredName || m.name;

  return (
    <button
      type="button"
      className={`member-row${m.isCurrentUser ? " member-row--you" : ""}`}
      onClick={() => navigate(`/members/${m.memberId}`)}
      aria-label={displayName}
    >
      <div className="member-row-avatar" aria-hidden="true">
        {m.avatarInitial}
      </div>
      <div className="member-row-body">
        <div className="member-row-name">
          <span className="member-row-display-name">{displayName}</span>
          {m.name !== displayName && (
            <span className="member-row-legal-name">({m.name})</span>
          )}
          {m.isManager && (
            <span className="member-badge member-badge--manager">
              {t("managerBadge")}
            </span>
          )}
          {m.isCurrentUser && (
            <span className="member-badge member-badge--you">
              {t("youBadge")}
            </span>
          )}
        </div>
        <div className="member-row-meta">
          <span>{t(`roles.${m.role}` as never, m.role)}</span>
          {m.linkedEmail && (
            <span className="member-row-meta-sep">· {m.linkedEmail}</span>
          )}
        </div>
      </div>
      <div className="member-row-status">
        <AccessStatusBadge status={m.accessStatus} hideNoAccess />
      </div>
      <span className="member-row-chevron" aria-hidden="true">›</span>
    </button>
  );
}

// ── Roster group ───────────────────────────────────────────────────────────────

function RosterGroup({ title, members }: { title: string; members: FamilyMemberResponse[] }) {
  if (members.length === 0) return null;
  return (
    <div className="roster-group">
      <div className="roster-group-title">{title}</div>
      <div className="item-list">
        {members.map((m) => (
          <MemberRow key={m.memberId} m={m} />
        ))}
      </div>
    </div>
  );
}

// ── Page ───────────────────────────────────────────────────────────────────────

export function MembersPage() {
  const { t } = useTranslation("members");
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const isManager = members.some((m) => m.isCurrentUser && m.isManager);

  const [showAdd, setShowAdd] = useState(false);
  const [addSaving, setAddSaving] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);

  if (!family) return null;

  const sorted = sortMembers(members);
  const adults = sorted.filter((m) => m.role === "Adult" || m.role === "Caregiver");
  const children = sorted.filter((m) => m.role === "Child");
  const pets = sorted.filter((m) => m.role === "Pet");
  const others = sorted.filter(
    (m) => !["Adult", "Caregiver", "Child", "Pet"].includes(m.role),
  );

  async function handleAddMember(values: {
    name: string;
    role: string;
    birthDate: string;
    isManager: boolean;
  }) {
    setAddSaving(true);
    setAddError(null);
    const result = await dispatch(
      addMember({
        familyId: family!.familyId,
        name: values.name,
        role: values.role,
        birthDate: values.birthDate || null,
        isManager: values.isManager,
      }),
    );
    setAddSaving(false);
    if (addMember.fulfilled.match(result)) {
      setShowAdd(false);
    } else {
      setAddError((result.payload as string) ?? t("errors.addFailed"));
    }
  }

  return (
    <div className="members-page">
      <div className="page-header">
        <h1 className="page-title">{t("pageTitle")}</h1>
        {isManager && (
          <button
            type="button"
            className="btn"
            onClick={() => {
              setShowAdd(true);
              setAddError(null);
            }}
          >
            + {t("addMember")}
          </button>
        )}
      </div>

      {members.length === 0 ? (
        <div className="empty-state">
          <p>{t("noMembers")}</p>
        </div>
      ) : (
        <>
          <RosterGroup title={t("groups.adults")} members={adults} />
          <RosterGroup title={t("groups.children")} members={children} />
          <RosterGroup title={t("groups.pets")} members={pets} />
          {others.length > 0 && (
            <RosterGroup title={t("groups.others")} members={others} />
          )}
        </>
      )}

      {showAdd && (
        <AddMemberModal
          saving={addSaving}
          error={addError}
          onSave={handleAddMember}
          onClose={() => {
            setShowAdd(false);
            setAddError(null);
          }}
        />
      )}
    </div>
  );
}

import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { updateMember } from "../../../store/householdSlice";
import { EditMemberModal, type MemberFormValues } from "./MemberModals";
import { MembersManagementSection } from "./MembersManagementSection";

export function MembersSettingsSection() {
  const { t } = useTranslation("settings");
  const { user } = useAuth();
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const me = members.find((m) => m.isCurrentUser);
  const tM = (key: string) => t(`household.members.${key}` as never);

  const [isEditing, setIsEditing] = useState(false);
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  async function handleProfileSave(values: MemberFormValues) {
    if (!me || !family) return;
    setEditSaving(true);
    setEditError(null);
    const result = await dispatch(
      updateMember({
        familyId: family.familyId,
        memberId: me.memberId,
        name: values.name,
        role: values.role,
        birthDate: values.birthDate || null,
        isManager: values.isManager,
      }),
    );
    setEditSaving(false);
    if (updateMember.fulfilled.match(result)) {
      setIsEditing(false);
    } else {
      setEditError((result.payload as string) ?? tM("updateError"));
    }
  }

  // Effective display name: preferredName if set, else name
  const displayName = me?.preferredName || me?.name;
  const avatarInitial = displayName?.[0]?.toUpperCase() ?? "?";

  return (
    <>
      {/* ── Section A: My Profile ─────────────────────────────────────────── */}
      {me && (
        <section className="settings-section">
          <h2 className="settings-section-title">{t("membersTab.myProfile")}</h2>
          <div
            className="card"
            style={{
              display: "flex",
              alignItems: "center",
              gap: "1rem",
              padding: "1rem",
              border: "2px solid var(--primary)",
            }}
          >
            <div
              style={{
                width: 48,
                height: 48,
                borderRadius: "50%",
                background: "color-mix(in srgb, var(--primary) 25%, transparent)",
                color: "var(--primary)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontWeight: 700,
                fontSize: "1.2rem",
                flexShrink: 0,
              }}
            >
              {avatarInitial}
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontWeight: 700, fontSize: "1.05rem", display: "flex", alignItems: "center", gap: "0.5rem", flexWrap: "wrap" }}>
                <span>{displayName}</span>
                {me.name !== displayName && (
                  <span style={{ fontSize: "0.8rem", color: "var(--muted)", fontStyle: "italic" }}>
                    ({me.name})
                  </span>
                )}
                {me.isManager && (
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
                  {t("membersTab.youBadge")}
                </span>
              </div>
              <div style={{ fontSize: "0.85rem", color: "var(--muted)", marginTop: "0.2rem" }}>
                {t(`household.members.roles.${me.role}` as never, me.role)}
                {me.birthDate && (
                  <span style={{ marginLeft: "0.75rem" }}>
                    · {new Date(me.birthDate).toLocaleDateString()}
                  </span>
                )}
              </div>
              <div style={{ fontSize: "0.8rem", color: "var(--muted)", marginTop: "0.15rem" }}>
                {me.linkedEmail ?? user?.email ?? ""}
              </div>
            </div>
            <div style={{ display: "flex", flexDirection: "column", gap: "0.4rem", flexShrink: 0, alignItems: "flex-end" }}>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                onClick={() => { setIsEditing(true); setEditError(null); }}
              >
                {t("membersTab.editProfile")}
              </button>
            </div>
          </div>

          {isEditing && (
            <EditMemberModal
              member={me}
              saving={editSaving}
              error={editError}
              onSave={handleProfileSave}
              onClose={() => { setIsEditing(false); setEditError(null); }}
            />
          )}
        </section>
      )}

      {/* ── Section B: Household Members ─────────────────────────────────── */}
      <MembersManagementSection />
    </>
  );
}

import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { inviteMember, updateMember } from "../../../store/householdSlice";

const MEMBER_ROLES = ["Adult", "Child", "Pet", "Caregiver"] as const;

export function MembersManagementSection() {
  const { t } = useTranslation("settings");
  const { user } = useAuth();
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);

  const isCurrentUserManager = members.some(
    (m) => m.authUserId === user?.userId && m.isManager,
  );

  // ── Edit state ──────────────────────────────────────────────────────────────
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editRole, setEditRole] = useState("Adult");
  const [editBirthDate, setEditBirthDate] = useState("");
  const [editIsManager, setEditIsManager] = useState(false);
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  // ── Invite state ────────────────────────────────────────────────────────────
  const [showInvite, setShowInvite] = useState(false);
  const [inviteName, setInviteName] = useState("");
  const [inviteRole, setInviteRole] = useState("Adult");
  const [inviteBirthDate, setInviteBirthDate] = useState("");
  const [inviteIsManager, setInviteIsManager] = useState(false);
  const [inviteUsername, setInviteUsername] = useState("");
  const [invitePassword, setInvitePassword] = useState("");
  const [inviteSaving, setInviteSaving] = useState(false);
  const [inviteError, setInviteError] = useState<string | null>(null);
  const [inviteCredentials, setInviteCredentials] = useState<{
    username: string;
    password: string;
  } | null>(null);

  if (!family) return null;

  // ── Edit handlers ───────────────────────────────────────────────────────────
  function openEdit(memberId: string) {
    const m = members.find((x) => x.memberId === memberId);
    if (!m) return;
    setEditingId(memberId);
    setEditName(m.name);
    setEditRole(m.role);
    setEditBirthDate(m.birthDate ?? "");
    setEditIsManager(m.isManager);
    setEditError(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setEditError(null);
  }

  async function handleEdit(e: FormEvent) {
    e.preventDefault();
    if (!editingId) return;
    setEditSaving(true);
    setEditError(null);

    const result = await dispatch(
      updateMember({
        familyId: family!.familyId,
        memberId: editingId,
        name: editName.trim(),
        role: editRole,
        birthDate: editBirthDate || null,
        isManager: editIsManager && editRole === "Adult",
      }),
    );

    setEditSaving(false);
    if (updateMember.fulfilled.match(result)) {
      setEditingId(null);
    } else {
      setEditError((result.payload as string) ?? t("household.members.updateError"));
    }
  }

  // ── Invite handlers ─────────────────────────────────────────────────────────
  function openInvite() {
    setShowInvite(true);
    setInviteName("");
    setInviteRole("Adult");
    setInviteBirthDate("");
    setInviteIsManager(false);
    setInviteUsername("");
    setInvitePassword("");
    setInviteError(null);
    setInviteCredentials(null);
  }

  function cancelInvite() {
    setShowInvite(false);
    setInviteCredentials(null);
  }

  async function handleInvite(e: FormEvent) {
    e.preventDefault();
    setInviteSaving(true);
    setInviteError(null);

    const result = await dispatch(
      inviteMember({
        familyId: family!.familyId,
        name: inviteName.trim(),
        role: inviteRole,
        birthDate: inviteBirthDate || null,
        isManager: inviteIsManager && inviteRole === "Adult",
        username: inviteUsername.trim(),
        temporaryPassword: invitePassword,
      }),
    );

    setInviteSaving(false);
    if (inviteMember.fulfilled.match(result)) {
      setInviteCredentials({
        username: inviteUsername.trim(),
        password: invitePassword,
      });
    } else {
      setInviteError((result.payload as string) ?? t("household.members.inviteError"));
    }
  }

  const tMembers = (key: string) =>
    t(`household.members.${key}` as never);

  return (
    <section className="settings-section">
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          marginBottom: "1rem",
        }}
      >
        <h2 className="settings-section-title" style={{ margin: 0 }}>
          {tMembers("title")}
        </h2>
        {isCurrentUserManager && !showInvite && (
          <button className="btn" type="button" onClick={openInvite}>
            + {tMembers("invite")}
          </button>
        )}
      </div>

      {/* Invite form */}
      {showInvite && !inviteCredentials && (
        <div className="card" style={{ marginBottom: "1rem" }}>
          <h3 style={{ marginBottom: "1rem" }}>{tMembers("inviteTitle")}</h3>
          <form onSubmit={handleInvite}>
            <div className="form-group">
              <label>{tMembers("name")}</label>
              <input
                className="form-control"
                type="text"
                value={inviteName}
                onChange={(e) => setInviteName(e.target.value)}
                required
                autoFocus
              />
            </div>
            <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tMembers("role")}</label>
                <select
                  className="form-control"
                  value={inviteRole}
                  onChange={(e) => {
                    setInviteRole(e.target.value);
                    if (e.target.value !== "Adult") setInviteIsManager(false);
                  }}
                >
                  {MEMBER_ROLES.map((r) => (
                    <option key={r} value={r}>
                      {t(`household.members.roles.${r}` as never)}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tMembers("birthDate")}</label>
                <input
                  className="form-control"
                  type="date"
                  value={inviteBirthDate}
                  onChange={(e) => setInviteBirthDate(e.target.value)}
                />
              </div>
            </div>
            {inviteRole === "Adult" && (
              <div className="form-group">
                <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
                  <input
                    type="checkbox"
                    checked={inviteIsManager}
                    onChange={(e) => setInviteIsManager(e.target.checked)}
                  />
                  {tMembers("isManager")}
                </label>
              </div>
            )}
            <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tMembers("username")}</label>
                <input
                  className="form-control"
                  type="text"
                  value={inviteUsername}
                  onChange={(e) => setInviteUsername(e.target.value)}
                  required
                  autoComplete="off"
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label>{tMembers("temporaryPassword")}</label>
                <input
                  className="form-control"
                  type="text"
                  value={invitePassword}
                  onChange={(e) => setInvitePassword(e.target.value)}
                  required
                  minLength={8}
                  autoComplete="off"
                />
              </div>
            </div>
            {inviteError && <p className="error-msg">{inviteError}</p>}
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button type="submit" className="btn" disabled={inviteSaving}>
                {inviteSaving ? tMembers("saving") : tMembers("invite")}
              </button>
              <button type="button" className="btn btn-ghost" onClick={cancelInvite}>
                {tMembers("cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Credential display after successful invite */}
      {inviteCredentials && (
        <div
          className="card"
          style={{ marginBottom: "1rem", borderColor: "var(--primary)", borderWidth: 2 }}
        >
          <p style={{ fontWeight: 600, marginBottom: "0.5rem" }}>
            {tMembers("credentialsTitle")}
          </p>
          <p style={{ fontSize: "0.8rem", color: "var(--muted)", marginBottom: "0.75rem" }}>
            {tMembers("credentialsNote")}
          </p>
          <div
            style={{
              background: "color-mix(in srgb, var(--primary) 8%, transparent)",
              borderRadius: 8,
              padding: "0.75rem",
              fontFamily: "monospace",
              marginBottom: "1rem",
            }}
          >
            <div>
              <span style={{ color: "var(--muted)", marginRight: 8 }}>{tMembers("username")}:</span>
              <strong>{inviteCredentials.username}</strong>
            </div>
            <div>
              <span style={{ color: "var(--muted)", marginRight: 8 }}>{tMembers("temporaryPassword")}:</span>
              <strong>{inviteCredentials.password}</strong>
            </div>
          </div>
          <button type="button" className="btn" onClick={cancelInvite}>
            {tMembers("done")}
          </button>
        </div>
      )}

      {/* Member list */}
      {members.length === 0 ? (
        <p style={{ color: "var(--muted)", fontSize: "0.9rem" }}>{tMembers("noMembers")}</p>
      ) : (
        <div className="item-list">
          {members.map((m) => (
            <div key={m.memberId}>
              {editingId === m.memberId ? (
                <div className="card" style={{ padding: "1rem" }}>
                  <h3 style={{ marginBottom: "1rem" }}>{tMembers("editTitle")}</h3>
                  <form onSubmit={handleEdit}>
                    <div className="form-group">
                      <label>{tMembers("name")}</label>
                      <input
                        className="form-control"
                        type="text"
                        value={editName}
                        onChange={(e) => setEditName(e.target.value)}
                        required
                        autoFocus
                      />
                    </div>
                    <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
                      <div className="form-group" style={{ flex: 1 }}>
                        <label>{tMembers("role")}</label>
                        <select
                          className="form-control"
                          value={editRole}
                          onChange={(e) => {
                            setEditRole(e.target.value);
                            if (e.target.value !== "Adult") setEditIsManager(false);
                          }}
                        >
                          {MEMBER_ROLES.map((r) => (
                            <option key={r} value={r}>
                              {t(`household.members.roles.${r}` as never)}
                            </option>
                          ))}
                        </select>
                      </div>
                      <div className="form-group" style={{ flex: 1 }}>
                        <label>{tMembers("birthDate")}</label>
                        <input
                          className="form-control"
                          type="date"
                          value={editBirthDate}
                          onChange={(e) => setEditBirthDate(e.target.value)}
                        />
                      </div>
                    </div>
                    {editRole === "Adult" && (
                      <div className="form-group">
                        <label
                          style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}
                        >
                          <input
                            type="checkbox"
                            checked={editIsManager}
                            onChange={(e) => setEditIsManager(e.target.checked)}
                          />
                          {tMembers("isManager")}
                        </label>
                      </div>
                    )}
                    {editError && <p className="error-msg">{editError}</p>}
                    <div style={{ display: "flex", gap: "0.5rem" }}>
                      <button type="submit" className="btn" disabled={editSaving}>
                        {editSaving ? tMembers("saving") : tMembers("save")}
                      </button>
                      <button type="button" className="btn btn-ghost" onClick={cancelEdit}>
                        {tMembers("cancel")}
                      </button>
                    </div>
                  </form>
                </div>
              ) : (
                <div className="item-card">
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
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: 600 }}>
                      {m.name}
                      {m.isManager && (
                        <span
                          style={{
                            marginLeft: "0.5rem",
                            fontSize: "0.7rem",
                            padding: "0.1rem 0.4rem",
                            borderRadius: 4,
                            background: "color-mix(in srgb, var(--primary) 20%, transparent)",
                            color: "var(--primary)",
                            verticalAlign: "middle",
                          }}
                        >
                          {tMembers("managerBadge")}
                        </span>
                      )}
                    </div>
                    <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                      {t(`household.members.roles.${m.role}` as never, m.role)}
                    </div>
                  </div>
                  {isCurrentUserManager && (
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ fontSize: "0.8rem", padding: "0.25rem 0.6rem" }}
                      onClick={() => openEdit(m.memberId)}
                    >
                      {tMembers("edit")}
                    </button>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </section>
  );
}

import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { addMember, linkMemberAccount, updateMember } from "../../../store/householdSlice";

const MEMBER_ROLES = ["Adult", "Child", "Pet", "Caregiver"] as const;
const ADD_MEMBER_ROLES = MEMBER_ROLES.filter((r) => r !== "Caregiver");

/** Generate a random temporary password: 8 chars, mixed upper/lower/digits.
 *  Uses crypto.getRandomValues() for unpredictable output.
 *  Ambiguous characters (0, O, I, l, 1) are excluded to improve readability. */
function generatePassword(): string {
  const upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
  const lower = "abcdefghjkmnpqrstuvwxyz";
  const digits = "23456789";
  const all = upper + lower + digits;

  function pick(charset: string): string {
    const arr = new Uint32Array(1);
    do {
      crypto.getRandomValues(arr);
    } while (arr[0] >= Math.floor(0x100000000 / charset.length) * charset.length);
    return charset[arr[0] % charset.length];
  }

  // Guarantee at least 2 of each category
  const chars = [pick(upper), pick(upper), pick(lower), pick(lower), pick(digits), pick(digits)];
  for (let i = 6; i < 8; i++) chars.push(pick(all));
  // Fisher-Yates shuffle using crypto.getRandomValues
  for (let i = chars.length - 1; i > 0; i--) {
    const arr = new Uint32Array(1);
    crypto.getRandomValues(arr);
    const j = arr[0] % (i + 1);
    [chars[i], chars[j]] = [chars[j], chars[i]];
  }
  return chars.join("");
}

type EditMode = "profile" | "linkAccount";

export function MembersManagementSection() {
  const { t } = useTranslation("settings");
  const { user } = useAuth();
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const isCurrentUserManager = members.some(
    (m) => m.authUserId === user?.userId && m.isManager,
  );

  const tM = (key: string) => t(`household.members.${key}` as never);

  // ── Edit state ──────────────────────────────────────────────────────────────
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editMode, setEditMode] = useState<EditMode>("profile");

  // Profile form
  const [editName, setEditName] = useState("");
  const [editRole, setEditRole] = useState("Adult");
  const [editBirthDate, setEditBirthDate] = useState("");
  const [editIsManager, setEditIsManager] = useState(false);
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  // Link account form
  const [linkEmail, setLinkEmail] = useState("");
  const [linkPassword, setLinkPassword] = useState("");
  const [linkSaving, setLinkSaving] = useState(false);
  const [linkError, setLinkError] = useState<string | null>(null);
  const [linkedCredentials, setLinkedCredentials] = useState<{
    username: string;
    password: string;
  } | null>(null);
  const [showAddMember, setShowAddMember] = useState(false);
  const [addName, setAddName] = useState("");
  const [addRole, setAddRole] = useState("Adult");
  const [addSaving, setAddSaving] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);

  if (!family) return null;

  // ── Edit helpers ────────────────────────────────────────────────────────────
  function openEdit(memberId: string) {
    const m = members.find((x) => x.memberId === memberId);
    if (!m) return;
    setEditingId(memberId);
    setEditMode("profile");
    setEditName(m.name);
    setEditRole(m.role);
    setEditBirthDate(m.birthDate ?? "");
    setEditIsManager(m.isManager);
    setEditError(null);
    setLinkEmail("");
    setLinkPassword("");
    setLinkError(null);
    setLinkedCredentials(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setLinkedCredentials(null);
    setEditError(null);
    setLinkError(null);
  }

  async function handleProfileSave(e: FormEvent) {
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
      setEditError((result.payload as string) ?? tM("updateError"));
    }
  }

  async function handleLinkAccount(e: FormEvent) {
    e.preventDefault();
    if (!editingId) return;
    setLinkSaving(true);
    setLinkError(null);

    const result = await dispatch(
      linkMemberAccount({
        familyId: family!.familyId,
        memberId: editingId,
        username: linkEmail.trim().toLowerCase(),
        temporaryPassword: linkPassword,
      }),
    );

    setLinkSaving(false);
    if (linkMemberAccount.fulfilled.match(result)) {
      setLinkedCredentials({
        username: linkEmail.trim().toLowerCase(),
        password: linkPassword,
      });
    } else {
      setLinkError((result.payload as string) ?? tM("linkAccountError"));
    }
  }

  async function handleAddMember(e: FormEvent) {
    e.preventDefault();
    if (!addName.trim()) return;
    setAddSaving(true);
    setAddError(null);
    const result = await dispatch(
      addMember({
        familyId: family!.familyId,
        name: addName.trim(),
        role: addRole,
      }),
    );
    setAddSaving(false);
    if (addMember.fulfilled.match(result)) {
      setAddName("");
      setAddRole("Adult");
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
          <button type="button" className="btn" onClick={() => setShowAddMember(true)}>
            + {tM("addMember")}
          </button>
        </div>
      )}

      {showAddMember && (
        <div className="card" style={{ padding: "1rem", marginBottom: "0.75rem" }}>
          <h3 style={{ marginBottom: "0.75rem" }}>{tM("addMember")}</h3>
          <form onSubmit={handleAddMember}>
            <div className="form-group">
              <label>{tM("name")}</label>
              <input
                className="form-control"
                type="text"
                value={addName}
                onChange={(e) => setAddName(e.target.value)}
                required
                autoFocus
              />
            </div>
            <div className="form-group">
              <label>{tM("role")}</label>
              <select
                className="form-control"
                value={addRole}
                onChange={(e) => setAddRole(e.target.value)}
              >
                {ADD_MEMBER_ROLES.map((r) => (
                  <option key={r} value={r}>
                    {t(`household.members.roles.${r}` as never)}
                  </option>
                ))}
              </select>
            </div>
            {addError && <p className="error-msg">{addError}</p>}
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button type="submit" className="btn" disabled={addSaving}>
                {addSaving ? tM("saving") : tM("addMember")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => {
                  setShowAddMember(false);
                  setAddError(null);
                  setAddName("");
                  setAddRole("Adult");
                }}
              >
                {tM("cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {members.length === 0 ? (
        <p style={{ color: "var(--muted)", fontSize: "0.9rem" }}>{tM("noMembers")}</p>
      ) : (
        <div className="item-list">
          {members.map((m) => (
            <div key={m.memberId}>
              {editingId === m.memberId ? (
                <div className="card" style={{ padding: "1rem" }}>
                  {/* Credentials display after successful link */}
                  {linkedCredentials ? (
                    <div>
                      <p style={{ fontWeight: 600, marginBottom: "0.5rem" }}>{tM("credentialsTitle")}</p>
                      <div
                        style={{
                          background: "color-mix(in srgb, var(--warning, #f5a623) 12%, transparent)",
                          border: "1px solid color-mix(in srgb, var(--warning, #f5a623) 40%, transparent)",
                          borderRadius: 8,
                          padding: "0.75rem",
                          marginBottom: "0.75rem",
                          fontSize: "0.85rem",
                        }}
                      >
                        {tM("credentialsSaveWarning")}
                      </div>
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
                          <span style={{ color: "var(--muted)", marginRight: 8 }}>{tM("email")}:</span>
                          <strong>{linkedCredentials.username}</strong>
                        </div>
                        <div>
                          <span style={{ color: "var(--muted)", marginRight: 8 }}>{tM("temporaryPassword")}:</span>
                          <strong>{linkedCredentials.password}</strong>
                        </div>
                      </div>
                      <button type="button" className="btn" onClick={cancelEdit}>
                        {tM("done")}
                      </button>
                    </div>
                  ) : (
                    <>
                      {/* Tab bar */}
                      <div style={{ display: "flex", gap: "0.5rem", marginBottom: "1rem", borderBottom: "1px solid var(--border, rgba(255,255,255,0.1))", paddingBottom: "0.5rem" }}>
                        <button
                          type="button"
                          className={`btn btn-ghost${editMode === "profile" ? " active" : ""}`}
                          style={{ fontSize: "0.85rem", padding: "0.25rem 0.6rem", fontWeight: editMode === "profile" ? 700 : undefined }}
                          onClick={() => setEditMode("profile")}
                        >
                          {tM("editTitle")}
                        </button>
                        {isCurrentUserManager && !m.authUserId && (
                          <button
                            type="button"
                            className={`btn btn-ghost${editMode === "linkAccount" ? " active" : ""}`}
                            style={{ fontSize: "0.85rem", padding: "0.25rem 0.6rem", fontWeight: editMode === "linkAccount" ? 700 : undefined }}
                            onClick={() => setEditMode("linkAccount")}
                          >
                            {tM("linkAccount")}
                          </button>
                        )}
                      </div>

                      {editMode === "profile" && (
                        <form onSubmit={handleProfileSave}>
                          <div className="form-group">
                            <label>{tM("name")}</label>
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
                              <label>{tM("role")}</label>
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
                              <label>{tM("birthDate")}</label>
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
                              <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
                                <input
                                  type="checkbox"
                                  checked={editIsManager}
                                  onChange={(e) => setEditIsManager(e.target.checked)}
                                />
                                {tM("isManager")}
                              </label>
                            </div>
                          )}
                          {editError && <p className="error-msg">{editError}</p>}
                          <div style={{ display: "flex", gap: "0.5rem" }}>
                            <button type="submit" className="btn" disabled={editSaving}>
                              {editSaving ? tM("saving") : tM("save")}
                            </button>
                            <button type="button" className="btn btn-ghost" onClick={cancelEdit}>
                              {tM("cancel")}
                            </button>
                          </div>
                        </form>
                      )}

                      {editMode === "linkAccount" && (
                        <form onSubmit={handleLinkAccount}>
                          <p style={{ fontSize: "0.85rem", color: "var(--muted)", marginBottom: "0.75rem" }}>
                            {tM("linkAccountSubtitle")}
                          </p>
                          <div className="form-group">
                            <label>{tM("email")}</label>
                            <input
                              className="form-control"
                              type="email"
                              value={linkEmail}
                              onChange={(e) => setLinkEmail(e.target.value)}
                              required
                              autoFocus
                              autoComplete="off"
                            />
                          </div>
                          <div className="form-group">
                            <label style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                              <span>{tM("temporaryPassword")}</span>
                              <button
                                type="button"
                                className="btn btn-ghost"
                                style={{ fontSize: "0.75rem", padding: "0.1rem 0.5rem" }}
                                onClick={() => setLinkPassword(generatePassword())}
                              >
                                {tM("generatePassword")}
                              </button>
                            </label>
                            <input
                              className="form-control"
                              type="text"
                              value={linkPassword}
                              onChange={(e) => setLinkPassword(e.target.value)}
                              required
                              minLength={6}
                              autoComplete="off"
                            />
                          </div>
                          {linkError && <p className="error-msg">{linkError}</p>}
                          <div style={{ display: "flex", gap: "0.5rem" }}>
                            <button type="submit" className="btn" disabled={linkSaving}>
                              {linkSaving ? tM("saving") : tM("linkAccount")}
                            </button>
                            <button type="button" className="btn btn-ghost" onClick={cancelEdit}>
                              {tM("cancel")}
                            </button>
                          </div>
                        </form>
                      )}
                    </>
                  )}
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
                      <span
                        style={{
                          fontSize: "0.7rem",
                          padding: "0.1rem 0.4rem",
                          borderRadius: 4,
                          background: m.authUserId
                            ? "color-mix(in srgb, #22c55e 18%, transparent)"
                            : "color-mix(in srgb, var(--muted) 20%, transparent)",
                          color: m.authUserId ? "#22c55e" : "var(--muted)",
                        }}
                      >
                        {m.authUserId ? `🔗 ${tM("accountLinked")}` : tM("noAccount")}
                      </span>
                    </div>
                    <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                      {t(`household.members.roles.${m.role}` as never, m.role)}
                    </div>
                  </div>
                  {(isCurrentUserManager || m.authUserId === user?.userId) && (
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ fontSize: "0.8rem", padding: "0.25rem 0.6rem", flexShrink: 0 }}
                      onClick={() => openEdit(m.memberId)}
                    >
                      {tM("edit")}
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

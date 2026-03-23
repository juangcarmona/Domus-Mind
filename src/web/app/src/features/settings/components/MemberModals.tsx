import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { FamilyMemberResponse } from "../../../api/domusmindApi";

const MEMBER_ROLES = ["Adult", "Child", "Pet", "Caregiver"] as const;
const ADD_MEMBER_ROLES = MEMBER_ROLES.filter((r) => r !== "Caregiver");

// ── Shared field types ───────────────────────────────────────────────────────

export interface MemberFormValues {
  name: string;
  role: string;
  birthDate: string;
  isManager: boolean;
}

// ── Edit member modal ────────────────────────────────────────────────────────

interface EditMemberModalProps {
  member: Pick<FamilyMemberResponse, "memberId" | "name" | "role" | "birthDate" | "isManager">;
  saving: boolean;
  error: string | null;
  onSave: (values: MemberFormValues) => void;
  onClose: () => void;
}

export function EditMemberModal({ member, saving, error, onSave, onClose }: EditMemberModalProps) {
  const { t } = useTranslation("settings");
  const tM = (key: string) => t(`household.members.${key}` as never);

  const [name, setName] = useState(member.name);
  const [role, setRole] = useState(member.role);
  const [birthDate, setBirthDate] = useState(member.birthDate ?? "");
  const [isManager, setIsManager] = useState(member.isManager);

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    onSave({ name: name.trim(), role, birthDate: birthDate || "", isManager: isManager && role === "Adult" });
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "1rem" }}>{tM("editTitle")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{tM("name")}</label>
            <input
              className="form-control"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{tM("role")}</label>
              <select
                className="form-control"
                value={role}
                onChange={(e) => {
                  setRole(e.target.value);
                  if (e.target.value !== "Adult") setIsManager(false);
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
                value={birthDate}
                onChange={(e) => setBirthDate(e.target.value)}
              />
            </div>
          </div>
          {role === "Adult" && (
            <div className="form-group">
              <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
                <input
                  type="checkbox"
                  checked={isManager}
                  onChange={(e) => setIsManager(e.target.checked)}
                />
                {tM("isManager")}
              </label>
            </div>
          )}
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>{tM("cancel")}</button>
            <button type="submit" className="btn" disabled={saving}>
              {saving ? tM("saving") : tM("save")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ── Add member modal ─────────────────────────────────────────────────────────

interface AddMemberModalProps {
  saving: boolean;
  error: string | null;
  onSave: (values: { name: string; role: string; birthDate: string; isManager: boolean }) => void;
  onClose: () => void;
}

export function AddMemberModal({ saving, error, onSave, onClose }: AddMemberModalProps) {
  const { t } = useTranslation("settings");
  const tM = (key: string) => t(`household.members.${key}` as never);

  const [name, setName] = useState("");
  const [role, setRole] = useState("Adult");
  const [birthDate, setBirthDate] = useState("");
  const [isManager, setIsManager] = useState(false);

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    onSave({ name: name.trim(), role, birthDate: birthDate || "", isManager: isManager && role === "Adult" });
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "1rem" }}>{tM("addMember")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{tM("name")}</label>
            <input
              className="form-control"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{tM("role")}</label>
              <select
                className="form-control"
                value={role}
                onChange={(e) => {
                  setRole(e.target.value);
                  if (e.target.value !== "Adult") setIsManager(false);
                }}
              >
                {ADD_MEMBER_ROLES.map((r) => (
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
                value={birthDate}
                onChange={(e) => setBirthDate(e.target.value)}
              />
            </div>
          </div>
          {role === "Adult" && (
            <div className="form-group">
              <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
                <input
                  type="checkbox"
                  checked={isManager}
                  onChange={(e) => setIsManager(e.target.checked)}
                />
                {tM("isManager")}
              </label>
            </div>
          )}
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>{tM("cancel")}</button>
            <button type="submit" className="btn" disabled={saving || !name.trim()}>
              {saving ? tM("saving") : tM("addMember")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ── Grant access modal ───────────────────────────────────────────────────────

interface GrantAccessModalProps {
  memberName: string;
  provisioned: { email: string; temporaryPassword: string } | null;
  saving: boolean;
  error: string | null;
  onSave: (email: string, displayName: string | null) => void;
  onClose: () => void;
}

export function GrantAccessModal({
  memberName,
  provisioned,
  saving,
  error,
  onSave,
  onClose,
}: GrantAccessModalProps) {
  const { t } = useTranslation("settings");
  const tM = (key: string) => t(`household.members.${key}` as never);

  const [email, setEmail] = useState("");
  const [displayName, setDisplayName] = useState("");

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    onSave(email.trim().toLowerCase(), displayName.trim() || null);
  }

  if (provisioned) {
    return (
      <div className="modal-backdrop" onClick={onClose}>
        <div className="modal" onClick={(e) => e.stopPropagation()}>
          <h2 style={{ marginBottom: "0.5rem" }}>{tM("credentialsTitle")}</h2>
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
              <strong>{provisioned.email}</strong>
            </div>
            <div style={{ marginTop: "0.35rem" }}>
              <span style={{ color: "var(--muted)", marginRight: 8 }}>{tM("temporaryPassword")}:</span>
              <strong>{provisioned.temporaryPassword}</strong>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                style={{ marginLeft: 8 }}
                onClick={() => navigator.clipboard?.writeText(provisioned.temporaryPassword)}
              >
                {tM("copy")}
              </button>
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn" onClick={onClose}>{tM("done")}</button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "0.25rem" }}>{tM("provisionAccess")}</h2>
        <p style={{ fontSize: "0.85rem", color: "var(--muted)", marginBottom: "0.85rem" }}>
          {memberName} — {tM("provisionAccessSubtitle")}
        </p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{tM("email")}</label>
            <input
              className="form-control"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoFocus
              autoComplete="off"
            />
          </div>
          <div className="form-group">
            <label>{tM("displayName")}</label>
            <input
              className="form-control"
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder={tM("displayNamePlaceholder")}
              autoComplete="off"
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>{tM("cancel")}</button>
            <button type="submit" className="btn" disabled={saving}>
              {saving ? tM("saving") : tM("provisionAccess")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

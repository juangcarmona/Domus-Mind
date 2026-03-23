import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { FamilyMemberResponse } from "../../../api/domusmindApi";

const MEMBER_ROLES = ["Adult", "Child", "Pet", "Caregiver"] as const;
const ADD_MEMBER_ROLES = MEMBER_ROLES.filter((r) => r !== "Caregiver");

export interface MemberFormValues {
  name: string;
  role: string;
  birthDate: string;
  isManager: boolean;
}

export interface ProfileFormValues {
  preferredName: string;
  primaryPhone: string;
  primaryEmail: string;
  householdNote: string;
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
  const { t } = useTranslation("members");

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
        <h2 style={{ marginBottom: "1rem" }}>{t("form.editTitle")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t("form.name")}</label>
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
              <label>{t("form.role")}</label>
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
                    {t(`roles.${r}` as never)}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{t("form.birthDate")}</label>
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
                {t("form.isManager")}
              </label>
              <span className="form-hint">{t("form.managerNote")}</span>
            </div>
          )}
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose} disabled={saving}>
              {t("actions.cancel")}
            </button>
            <button type="submit" className="btn" disabled={saving || !name.trim()}>
              {saving ? t("actions.saving") : t("actions.save")}
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
  const { t } = useTranslation("members");

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
        <h2 style={{ marginBottom: "1rem" }}>{t("form.addTitle")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t("form.name")}</label>
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
              <label>{t("form.role")}</label>
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
                    {t(`roles.${r}` as never)}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{t("form.birthDate")}</label>
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
                {t("form.isManager")}
              </label>
            </div>
          )}
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {t("actions.cancel")}
            </button>
            <button type="submit" className="btn" disabled={saving || !name.trim()}>
              {saving ? t("actions.saving") : t("addMember")}
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
  const { t } = useTranslation("members");

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
          <h2 style={{ marginBottom: "0.5rem" }}>{t("form.credentialsTitle")}</h2>
          <div
            style={{
              background: "color-mix(in srgb, #f5a623 12%, transparent)",
              border: "1px solid color-mix(in srgb, #f5a623 40%, transparent)",
              borderRadius: 8,
              padding: "0.75rem",
              marginBottom: "0.75rem",
              fontSize: "0.85rem",
            }}
          >
            {t("form.credentialsSaveWarning")}
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
              <span style={{ color: "var(--muted)", marginRight: 8 }}>{t("form.email")}:</span>
              <strong>{provisioned.email}</strong>
            </div>
            <div style={{ marginTop: "0.35rem" }}>
              <span style={{ color: "var(--muted)", marginRight: 8 }}>{t("form.temporaryPassword")}:</span>
              <strong>{provisioned.temporaryPassword}</strong>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                style={{ marginLeft: 8 }}
                onClick={() => navigator.clipboard?.writeText(provisioned.temporaryPassword)}
              >
                {t("actions.copy")}
              </button>
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn" onClick={onClose}>
              {t("form.done")}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "0.25rem" }}>{t("form.grantAccessTitle")}</h2>
        <p style={{ fontSize: "0.85rem", color: "var(--muted)", marginBottom: "0.85rem" }}>
          {memberName} — {t("form.grantAccessSubtitle")}
        </p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t("form.email")}</label>
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
            <label>{t("form.displayName")}</label>
            <input
              className="form-control"
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder={t("form.displayNamePlaceholder")}
              autoComplete="off"
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {t("actions.cancel")}
            </button>
            <button type="submit" className="btn" disabled={saving}>
              {saving ? t("actions.saving") : t("actions.grantAccess")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ── Edit profile modal ───────────────────────────────────────────────────────

interface EditProfileModalProps {
  member: {
    preferredName?: string | null;
    primaryPhone?: string | null;
    primaryEmail?: string | null;
    householdNote?: string | null;
  };
  saving: boolean;
  error: string | null;
  onSave: (values: ProfileFormValues) => void;
  onClose: () => void;
}

export function EditProfileModal({ member, saving, error, onSave, onClose }: EditProfileModalProps) {
  const { t } = useTranslation("members");

  const [preferredName, setPreferredName] = useState(member.preferredName ?? "");
  const [primaryPhone, setPrimaryPhone] = useState(member.primaryPhone ?? "");
  const [primaryEmail, setPrimaryEmail] = useState(member.primaryEmail ?? "");
  const [householdNote, setHouseholdNote] = useState(member.householdNote ?? "");

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    onSave({
      preferredName: preferredName.trim(),
      primaryPhone: primaryPhone.trim(),
      primaryEmail: primaryEmail.trim(),
      householdNote: householdNote.trim(),
    });
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "1rem" }}>{t("form.editProfileTitle")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t("form.preferredName")}</label>
            <input
              className="form-control"
              type="text"
              value={preferredName}
              onChange={(e) => setPreferredName(e.target.value)}
              placeholder={t("form.preferredNamePlaceholder")}
              autoFocus
            />
          </div>
          <div className="form-group">
            <label>{t("form.primaryPhone")}</label>
            <input
              className="form-control"
              type="tel"
              value={primaryPhone}
              onChange={(e) => setPrimaryPhone(e.target.value)}
              autoComplete="off"
            />
          </div>
          <div className="form-group">
            <label>{t("form.primaryEmail")}</label>
            <input
              className="form-control"
              type="email"
              value={primaryEmail}
              onChange={(e) => setPrimaryEmail(e.target.value)}
              autoComplete="off"
            />
          </div>
          <div className="form-group">
            <label>{t("form.householdNote")}</label>
            <textarea
              className="form-control"
              value={householdNote}
              onChange={(e) => setHouseholdNote(e.target.value)}
              placeholder={t("form.householdNotePlaceholder")}
              rows={3}
              style={{ resize: "vertical" }}
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {t("actions.cancel")}
            </button>
            <button type="submit" className="btn" disabled={saving}>
              {saving ? t("actions.saving") : t("actions.save")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

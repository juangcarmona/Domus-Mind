import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { ADD_MEMBER_ROLES, type MemberFormValues } from "./memberModalTypes";

interface AddMemberModalProps {
  saving: boolean;
  error: string | null;
  onSave: (values: MemberFormValues) => void;
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
            <input className="form-control" type="text" value={name} onChange={(e) => setName(e.target.value)} required autoFocus />
          </div>
          <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{tM("role")}</label>
              <select
                className="form-control"
                value={role}
                onChange={(e) => { setRole(e.target.value); if (e.target.value !== "Adult") setIsManager(false); }}
              >
                {ADD_MEMBER_ROLES.map((r) => (
                  <option key={r} value={r}>{t(`household.members.roles.${r}` as never)}</option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{tM("birthDate")}</label>
              <input className="form-control" type="date" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
            </div>
          </div>
          {role === "Adult" && (
            <div className="form-group">
              <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
                <input type="checkbox" checked={isManager} onChange={(e) => setIsManager(e.target.checked)} />
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

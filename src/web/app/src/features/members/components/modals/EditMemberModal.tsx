import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { FamilyMemberResponse } from "../../../../api/domusmindApi";
import { MEMBER_ROLES, type MemberFormValues } from "./memberModalTypes";

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
            <input className="form-control" type="text" value={name} onChange={(e) => setName(e.target.value)} required autoFocus />
          </div>
          <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{t("form.role")}</label>
              <select className="form-control" value={role} onChange={(e) => { setRole(e.target.value); if (e.target.value !== "Adult") setIsManager(false); }}>
                {MEMBER_ROLES.map((r) => <option key={r} value={r}>{t(`roles.${r}` as never)}</option>)}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label>{t("form.birthDate")}</label>
              <input className="form-control" type="date" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
            </div>
          </div>
          {role === "Adult" && (
            <div className="form-group">
              <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
                <input type="checkbox" checked={isManager} onChange={(e) => setIsManager(e.target.checked)} />
                {t("form.isManager")}
              </label>
              <span className="form-hint">{t("form.managerNote")}</span>
            </div>
          )}
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose} disabled={saving}>{t("actions.cancel")}</button>
            <button type="submit" className="btn" disabled={saving || !name.trim()}>{saving ? t("actions.saving") : t("actions.save")}</button>
          </div>
        </form>
      </div>
    </div>
  );
}

import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { ProfileFormValues } from "./memberModalTypes";

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
    onSave({ preferredName: preferredName.trim(), primaryPhone: primaryPhone.trim(), primaryEmail: primaryEmail.trim(), householdNote: householdNote.trim() });
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "1rem" }}>{t("form.editProfileTitle")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t("form.preferredName")}</label>
            <input className="form-control" type="text" value={preferredName} onChange={(e) => setPreferredName(e.target.value)} placeholder={t("form.preferredNamePlaceholder")} autoFocus />
          </div>
          <div className="form-group">
            <label>{t("form.primaryPhone")}</label>
            <input className="form-control" type="tel" value={primaryPhone} onChange={(e) => setPrimaryPhone(e.target.value)} autoComplete="off" />
          </div>
          <div className="form-group">
            <label>{t("form.primaryEmail")}</label>
            <input className="form-control" type="email" value={primaryEmail} onChange={(e) => setPrimaryEmail(e.target.value)} autoComplete="off" />
          </div>
          <div className="form-group">
            <label>{t("form.householdNote")}</label>
            <textarea className="form-control" value={householdNote} onChange={(e) => setHouseholdNote(e.target.value)} placeholder={t("form.householdNotePlaceholder")} rows={3} style={{ resize: "vertical" }} />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>{t("actions.cancel")}</button>
            <button type="submit" className="btn" disabled={saving}>{saving ? t("actions.saving") : t("actions.save")}</button>
          </div>
        </form>
      </div>
    </div>
  );
}

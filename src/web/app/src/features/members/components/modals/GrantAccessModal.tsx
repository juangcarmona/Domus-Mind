import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";

interface GrantAccessModalProps {
  memberName: string;
  provisioned: { email: string; temporaryPassword: string } | null;
  saving: boolean;
  error: string | null;
  onSave: (email: string, displayName: string | null) => void;
  onClose: () => void;
}

export function GrantAccessModal({ memberName, provisioned, saving, error, onSave, onClose }: GrantAccessModalProps) {
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
          <div style={{ background: "color-mix(in srgb, #f5a623 12%, transparent)", border: "1px solid color-mix(in srgb, #f5a623 40%, transparent)", borderRadius: 8, padding: "0.75rem", marginBottom: "0.75rem", fontSize: "0.85rem" }}>
            {t("form.credentialsSaveWarning")}
          </div>
          <div style={{ background: "color-mix(in srgb, var(--primary) 8%, transparent)", borderRadius: 8, padding: "0.75rem", fontFamily: "monospace", marginBottom: "1rem" }}>
            <div><span style={{ color: "var(--muted)", marginRight: 8 }}>{t("form.email")}:</span><strong>{provisioned.email}</strong></div>
            <div style={{ marginTop: "0.35rem" }}>
              <span style={{ color: "var(--muted)", marginRight: 8 }}>{t("form.temporaryPassword")}:</span>
              <strong>{provisioned.temporaryPassword}</strong>
              <button type="button" className="btn btn-ghost btn-sm" style={{ marginLeft: 8 }} onClick={() => navigator.clipboard?.writeText(provisioned.temporaryPassword)}>{t("actions.copy")}</button>
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn" onClick={onClose}>{t("form.done")}</button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2 style={{ marginBottom: "0.25rem" }}>{t("form.grantAccessTitle")}</h2>
        <p style={{ fontSize: "0.85rem", color: "var(--muted)", marginBottom: "0.85rem" }}>{memberName} — {t("form.grantAccessSubtitle")}</p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t("form.email")}</label>
            <input className="form-control" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus autoComplete="off" />
          </div>
          <div className="form-group">
            <label>{t("form.displayName")}</label>
            <input className="form-control" type="text" value={displayName} onChange={(e) => setDisplayName(e.target.value)} placeholder={t("form.displayNamePlaceholder")} autoComplete="off" />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>{t("actions.cancel")}</button>
            <button type="submit" className="btn" disabled={saving}>{saving ? t("actions.saving") : t("actions.grantAccess")}</button>
          </div>
        </form>
      </div>
    </div>
  );
}

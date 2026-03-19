import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";

export function AccountSettingsSection() {
  const { t } = useTranslation("settings");
  const { t: tCommon } = useTranslation("common");
  const { user, changePassword, logout } = useAuth();

  const [currentPw, setCurrentPw] = useState("");
  const [newPw, setNewPw] = useState("");
  const [confirmPw, setConfirmPw] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  async function handleChangePassword(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    if (newPw !== confirmPw) {
      setError(t("account.passwordMismatch"));
      return;
    }

    setSaving(true);
    try {
      await changePassword(currentPw, newPw);
      setSuccess(t("account.passwordSuccess"));
      setCurrentPw("");
      setNewPw("");
      setConfirmPw("");
      await logout();
    } catch (err) {
      setError((err as ApiError).message ?? tCommon("failed"));
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">{t("account.title")}</h2>

      <div className="settings-field-group">
        <div className="settings-field">
          <span className="settings-field-label">{t("account.email")}</span>
          <span className="settings-field-value">{user?.email ?? "—"}</span>
        </div>
        <div className="settings-field">
          <span className="settings-field-label">{t("account.userId")}</span>
          <span className="settings-field-value settings-field-mono">{user?.userId ?? "—"}</span>
        </div>
      </div>

      <div className="settings-subsection">
        <h3 className="settings-subsection-title">{t("account.changePassword")}</h3>
        <form onSubmit={handleChangePassword} className="settings-form">
          <div className="form-group">
            <label htmlFor="current-pw">{t("account.currentPassword")}</label>
            <input
              id="current-pw"
              type="password"
              className="form-control"
              value={currentPw}
              onChange={(e) => setCurrentPw(e.target.value)}
              required
              autoComplete="current-password"
            />
          </div>
          <div className="form-group">
            <label htmlFor="new-pw">{t("account.newPassword")}</label>
            <input
              id="new-pw"
              type="password"
              className="form-control"
              value={newPw}
              onChange={(e) => setNewPw(e.target.value)}
              required
              minLength={8}
              autoComplete="new-password"
            />
          </div>
          <div className="form-group">
            <label htmlFor="confirm-pw">{t("account.confirmPassword")}</label>
            <input
              id="confirm-pw"
              type="password"
              className="form-control"
              value={confirmPw}
              onChange={(e) => setConfirmPw(e.target.value)}
              required
              minLength={8}
              autoComplete="new-password"
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          {success && <p className="success-msg">{success}</p>}
          <button
            type="submit"
            className="btn"
            disabled={saving || !currentPw || !newPw || !confirmPw}
          >
            {saving ? t("account.saving") : t("account.save")}
          </button>
        </form>
      </div>
    </section>
  );
}

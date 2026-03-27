import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

export function ForceChangePasswordPage() {
  const { changePassword } = useAuth();
  const { t } = useTranslation("auth");
  const [currentPw, setCurrentPw] = useState("");
  const [newPw, setNewPw] = useState("");
  const [confirmPw, setConfirmPw] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    if (newPw !== confirmPw) {
      setError(t("passwordMismatch"));
      return;
    }

    setSaving(true);
    try {
      await changePassword(currentPw, newPw);
      // changePassword calls logout() - the app will redirect to login automatically.
    } catch (err) {
      setError((err as ApiError).message ?? t("changePasswordFailed"));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div style={{ textAlign: "center", marginBottom: "1.25rem", color: "var(--primary)" }}>
          <HouseholdLogo size={40} />
        </div>
        <h1 style={{ textAlign: "center", marginBottom: "0.5rem" }}>{t("forceChangeTitle")}</h1>
        <p style={{ textAlign: "center", color: "var(--muted)", fontSize: "0.9rem", marginBottom: "1.25rem" }}>
          {t("forceChangeSubtitle")}
        </p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="fcp-current">{t("temporaryPassword")}</label>
            <input
              id="fcp-current"
              className="form-control"
              type="password"
              value={currentPw}
              onChange={(e) => setCurrentPw(e.target.value)}
              required
              autoFocus
              autoComplete="current-password"
            />
          </div>
          <div className="form-group">
            <label htmlFor="fcp-new">{t("newPassword")}</label>
            <input
              id="fcp-new"
              className="form-control"
              type="password"
              value={newPw}
              onChange={(e) => setNewPw(e.target.value)}
              required
              minLength={8}
              autoComplete="new-password"
            />
          </div>
          <div className="form-group">
            <label htmlFor="fcp-confirm">{t("confirmPassword")}</label>
            <input
              id="fcp-confirm"
              className="form-control"
              type="password"
              value={confirmPw}
              onChange={(e) => setConfirmPw(e.target.value)}
              required
              minLength={8}
              autoComplete="new-password"
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button
            type="submit"
            className="btn"
            disabled={saving || !currentPw || !newPw || !confirmPw}
            style={{ width: "100%", justifyContent: "center", marginTop: "0.5rem" }}
          >
            {saving ? t("saving") : t("setNewPassword")}
          </button>
        </form>
      </div>
    </div>
  );
}

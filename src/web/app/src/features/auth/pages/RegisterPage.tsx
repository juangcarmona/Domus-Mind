import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Props { onSuccess?: () => void; onGoToLogin?: () => void; }

export function RegisterPage({ onSuccess, onGoToLogin }: Props) {
  const { register } = useAuth();
  const { t } = useTranslation("auth");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [done, setDone] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try { await register(email, password); setDone(true); onSuccess?.(); }
    catch (err) { setError((err as ApiError).message ?? t("registrationFailed")); }
    finally { setLoading(false); }
  }

  if (done) {
    return (
      <div className="auth-wrap">
        <div className="auth-card" style={{ textAlign: "center" }}>
          <p>{t("accountCreated")}</p>
          <button className="btn" style={{ width: "100%", justifyContent: "center" }} onClick={onGoToLogin}>
            {t("signIn")}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div className="auth-logo">
          <HouseholdLogo size={40} />
        </div>
        <h1>{t("register")}</h1>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="reg-email">{t("email")}</label>
            <input id="reg-email" className="form-control" type="email" value={email}
              onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </div>
          <div className="form-group">
            <label htmlFor="reg-password">{t("password")}</label>
            <input id="reg-password" className="form-control" type="password" value={password}
              onChange={(e) => setPassword(e.target.value)} required minLength={8} />
            <span className="form-hint">{t("passwordHint")}</span>
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn auth-submit" disabled={loading}>
            {loading ? t("loading") : t("register")}
          </button>
        </form>
        {onGoToLogin && (
          <p className="auth-footer">
            {t("haveAccount")}{" "}
            <button onClick={onGoToLogin}>{t("signIn")}</button>
          </p>
        )}
      </div>
    </div>
  );
}

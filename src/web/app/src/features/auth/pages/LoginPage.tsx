import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Props { onSuccess?: () => void; onGoToRegister?: () => void; }

export function LoginPage({ onSuccess, onGoToRegister }: Props) {
  const { login } = useAuth();
  const { t } = useTranslation("auth");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try { await login(email, password); onSuccess?.(); }
    catch (err) { setError((err as ApiError).message ?? t("signInFailed")); }
    finally { setLoading(false); }
  }

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div className="auth-logo">
          <HouseholdLogo size={40} />
        </div>
        <h1>{t("signIn")}</h1>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">{t("email")}</label>
            <input id="email" className="form-control" type="email" value={email}
              onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </div>
          <div className="form-group">
            <label htmlFor="password">{t("password")}</label>
            <input id="password" className="form-control" type="password" value={password}
              onChange={(e) => setPassword(e.target.value)} required />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn auth-submit" disabled={loading}>
            {loading ? t("signingIn") : t("signIn")}
          </button>
        </form>
        {onGoToRegister && (
          <p className="auth-footer">
            {t("noAccount")}{" "}
            <button onClick={onGoToRegister}>{t("createOne")}</button>
          </p>
        )}
      </div>
    </div>
  );
}

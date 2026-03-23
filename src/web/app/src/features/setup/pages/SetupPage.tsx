import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { setupApi } from "../../../api/setupApi";
import { useAuth } from "../../../auth/AuthProvider";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Props {
  onInitialized: () => void;
}

export function SetupPage({ onInitialized }: Props) {
  const { t } = useTranslation("setup");
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await setupApi.initialize({ email, password });
      // Auto-login so the user lands in the app without a second sign-in step.
      await login(email, password);
      onInitialized();
    } catch (err) {
      const apiErr = err as { status?: number; message?: string };
      setError(apiErr.message ?? t("failed"));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div style={{ textAlign: "center", marginBottom: "1.25rem", color: "var(--primary)" }}>
          <HouseholdLogo size={40} />
        </div>
        <h1 style={{ textAlign: "center", marginBottom: "0.5rem" }}>{t("title")}</h1>
        <p style={{ textAlign: "center", color: "var(--muted, #888)", marginBottom: "1.5rem", fontSize: "0.9rem" }}>
          {t("subtitle")}
        </p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="setup-email">{t("email")}</label>
            <input
              id="setup-email"
              className="form-control"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div className="form-group">
            <label htmlFor="setup-password">{t("password")}</label>
            <input
              id="setup-password"
              className="form-control"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={8}
            />
            <span className="form-hint">{t("passwordHint")}</span>
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button
            type="submit"
            className="btn"
            disabled={loading}
            style={{ width: "100%", justifyContent: "center", marginTop: "0.5rem" }}
          >
            {loading ? t("submitting") : t("submit")}
          </button>
        </form>
      </div>
    </div>
  );
}

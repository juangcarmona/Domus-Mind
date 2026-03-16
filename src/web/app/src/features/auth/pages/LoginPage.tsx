import { useState, type FormEvent } from "react";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Props { onSuccess?: () => void; onGoToRegister?: () => void; }

export function LoginPage({ onSuccess, onGoToRegister }: Props) {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try { await login(email, password); onSuccess?.(); }
    catch (err) { setError((err as ApiError).message ?? "Sign in failed."); }
    finally { setLoading(false); }
  }

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div style={{ textAlign: "center", marginBottom: "1.25rem", color: "var(--primary)" }}>
          <HouseholdLogo size={40} />
        </div>
        <h1 style={{ textAlign: "center", marginBottom: "1.25rem" }}>Sign in</h1>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input id="email" className="form-control" type="email" value={email}
              onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </div>
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input id="password" className="form-control" type="password" value={password}
              onChange={(e) => setPassword(e.target.value)} required />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn" disabled={loading}
            style={{ width: "100%", justifyContent: "center", marginTop: "0.5rem" }}>
            {loading ? "Signing in\u2026" : "Sign in"}
          </button>
        </form>
        {onGoToRegister && (
          <p className="auth-footer">
            No account?{" "}
            <button onClick={onGoToRegister}>Create one</button>
          </p>
        )}
      </div>
    </div>
  );
}

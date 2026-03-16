import { useState, type FormEvent } from "react";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Props { onSuccess?: () => void; onGoToLogin?: () => void; }

export function RegisterPage({ onSuccess, onGoToLogin }: Props) {
  const { register } = useAuth();
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
    catch (err) { setError((err as ApiError).message ?? "Registration failed."); }
    finally { setLoading(false); }
  }

  if (done) {
    return (
      <div className="auth-wrap">
        <div className="auth-card" style={{ textAlign: "center" }}>
          <p>Account created.</p>
          <button className="btn" style={{ width: "100%", justifyContent: "center" }} onClick={onGoToLogin}>
            Sign in
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div style={{ textAlign: "center", marginBottom: "1.25rem", color: "var(--primary)" }}>
          <HouseholdLogo size={40} />
        </div>
        <h1 style={{ textAlign: "center", marginBottom: "1.25rem" }}>Create account</h1>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="reg-email">Email</label>
            <input id="reg-email" className="form-control" type="email" value={email}
              onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </div>
          <div className="form-group">
            <label htmlFor="reg-password">Password</label>
            <input id="reg-password" className="form-control" type="password" value={password}
              onChange={(e) => setPassword(e.target.value)} required minLength={8} />
            <span className="form-hint">At least 8 characters</span>
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn" disabled={loading}
            style={{ width: "100%", justifyContent: "center", marginTop: "0.5rem" }}>
            {loading ? "Creating\u2026" : "Create account"}
          </button>
        </form>
        {onGoToLogin && (
          <p className="auth-footer">
            Have an account?{" "}
            <button onClick={onGoToLogin}>Sign in</button>
          </p>
        )}
      </div>
    </div>
  );
}

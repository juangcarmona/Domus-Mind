import { useState, type FormEvent } from "react";
import { useAuth } from "../../../auth/AuthProvider";
import type { ApiError } from "../../../api/authApi";

interface Props {
  onLogout?: () => void;
}

export function ProfilePage({ onLogout }: Props) {
  const { user, logout, refreshToken, changePassword } = useAuth();

  const [currentPw, setCurrentPw] = useState("");
  const [newPw, setNewPw] = useState("");
  const [pwMessage, setPwMessage] = useState<string | null>(null);
  const [pwError, setPwError] = useState<string | null>(null);
  const [pwLoading, setPwLoading] = useState(false);

  async function handleChangePassword(e: FormEvent) {
    e.preventDefault();
    setPwError(null);
    setPwMessage(null);
    setPwLoading(true);
    try {
      await changePassword(currentPw, newPw);
      setPwMessage("Password changed. You have been signed out.");
      onLogout?.();
    } catch (err) {
      setPwError((err as ApiError).message ?? "Failed to change password.");
    } finally {
      setPwLoading(false);
    }
  }

  async function handleRefresh() {
    const ok = await refreshToken();
    if (!ok) onLogout?.();
  }

  return (
    <div style={{ maxWidth: 480, margin: "60px auto", padding: 24 }}>
      <h2>Profile</h2>

      <section style={{ marginBottom: 24, padding: 16, border: "1px solid #ccc", borderRadius: 8 }}>
        <h3>Current user</h3>
        <p><strong>User ID:</strong> {user?.userId}</p>
        <p><strong>Email:</strong> {user?.email ?? "—"}</p>
        <div style={{ display: "flex", gap: 8 }}>
          <button onClick={handleRefresh} style={styles.button}>Refresh token</button>
          <button
            onClick={async () => { await logout(); onLogout?.(); }}
            style={{ ...styles.button, background: "#e55" }}
          >
            Log out
          </button>
        </div>
      </section>

      <section style={{ padding: 16, border: "1px solid #ccc", borderRadius: 8 }}>
        <h3>Change password</h3>
        <form onSubmit={handleChangePassword} style={styles.form}>
          <label>
            Current password
            <input
              type="password"
              value={currentPw}
              onChange={(e) => setCurrentPw(e.target.value)}
              required
              style={styles.input}
            />
          </label>
          <label>
            New password
            <input
              type="password"
              value={newPw}
              onChange={(e) => setNewPw(e.target.value)}
              required
              minLength={8}
              style={styles.input}
            />
          </label>
          {pwError && <p style={{ color: "crimson", margin: 0 }}>{pwError}</p>}
          {pwMessage && <p style={{ color: "green", margin: 0 }}>{pwMessage}</p>}
          <button type="submit" disabled={pwLoading} style={styles.button}>
            {pwLoading ? "Changing…" : "Change password"}
          </button>
        </form>
      </section>
    </div>
  );
}

const styles = {
  form: { display: "flex", flexDirection: "column" as const, gap: 12 },
  input: { display: "block", width: "100%", marginTop: 4, padding: "6px 8px", boxSizing: "border-box" as const },
  button: { padding: "8px 16px", cursor: "pointer" } as const,
};

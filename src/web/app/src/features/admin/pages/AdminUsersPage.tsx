import { useEffect, useState } from "react";
import { adminApi, type AdminUserSummary } from "../../../api/adminApi";

export function AdminUsersPage() {
  const [items, setItems] = useState<AdminUserSummary[] | null>(null);
  const [search, setSearch] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    adminApi.getUsers(undefined)
      .then((r) => { if (!cancelled) setItems(r.items); })
      .catch((e: { message?: string }) => { if (!cancelled) setError(e.message ?? "Failed to load"); });
    return () => { cancelled = true; };
  }, []);

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    const q = search.trim() || undefined;
    setError(null);
    adminApi.getUsers(q)
      .then((r) => setItems(r.items))
      .catch((e: { message?: string }) => setError(e.message ?? "Failed to load"));
  }

  function refresh() {
    const q = search.trim() || undefined;
    setError(null);
    adminApi.getUsers(q)
      .then((r) => setItems(r.items))
      .catch((e: { message?: string }) => setError(e.message ?? "Failed to load"));
  }

  async function toggleDisabled(user: AdminUserSummary) {
    setActionError(null);
    try {
      if (user.isDisabled) {
        await adminApi.enableUser(user.userId);
      } else {
        await adminApi.disableUser(user.userId);
      }
      refresh();
    } catch (e) {
      const err = e as { message?: string };
      setActionError(err.message ?? "Action failed");
    }
  }

  return (
    <div>
      <div className="admin-section-header">
        <h2 className="admin-section-title">Users</h2>
        <form onSubmit={handleSearch} style={{ display: "flex", gap: "0.5rem" }}>
          <input
            className="admin-search"
            placeholder="Search by email…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <button type="submit" className="btn" style={{ fontSize: "0.875rem" }}>Search</button>
        </form>
      </div>
      {error && <p className="error-msg">{error}</p>}
      {actionError && <p className="error-msg">{actionError}</p>}
      <div className="admin-card">
        {items === null ? (
          <p style={{ padding: "1rem" }}>Loading…</p>
        ) : items.length === 0 ? (
          <p className="admin-empty">No users found.</p>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th>Email</th>
                <th>Status</th>
                <th>Role</th>
                <th>Household</th>
                <th>Last login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((u) => (
                <tr key={u.userId}>
                  <td>
                    <div>{u.displayName ?? u.email}</div>
                    {u.displayName && <div style={{ fontSize: "0.75rem", color: "var(--muted)" }}>{u.email}</div>}
                  </td>
                  <td>
                    <span className={`admin-badge ${u.isDisabled ? "admin-badge--disabled" : "admin-badge--active"}`}>
                      {u.isDisabled ? "Disabled" : "Active"}
                    </span>
                  </td>
                  <td>
                    {u.isOperator && <span className="admin-badge admin-badge--operator">Operator</span>}
                  </td>
                  <td style={{ fontFamily: "monospace", fontSize: "0.7rem", color: "var(--muted)" }}>
                    {u.linkedFamilyId ? u.linkedFamilyId.slice(0, 8) + "…" : "—"}
                  </td>
                  <td style={{ fontSize: "0.75rem", color: "var(--muted)" }}>
                    {u.lastLoginAtUtc ? new Date(u.lastLoginAtUtc).toLocaleDateString() : "Never"}
                  </td>
                  <td>
                    {!u.isOperator && (
                      <button
                        type="button"
                        className="btn btn-secondary"
                        style={{ fontSize: "0.75rem", padding: "0.2rem 0.5rem" }}
                        onClick={() => void toggleDisabled(u)}
                      >
                        {u.isDisabled ? "Enable" : "Disable"}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

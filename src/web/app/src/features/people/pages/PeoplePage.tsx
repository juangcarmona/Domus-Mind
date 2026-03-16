import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchMembers, addMember } from "../../../store/householdSlice";

export function PeoplePage() {
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const familyId = family?.familyId;
  const { t } = useTranslation();

  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState("");
  const [role, setRole] = useState("Adult");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (familyId) dispatch(fetchMembers(familyId));
  }, [familyId, dispatch]);

  async function handleAdd(e: FormEvent) {
    e.preventDefault();
    if (!familyId || !name.trim()) return;
    setSubmitting(true);
    setError(null);
    const result = await dispatch(
      addMember({ familyId, name: name.trim(), role }),
    );
    setSubmitting(false);
    if (addMember.fulfilled.match(result)) {
      setName("");
      setRole("Adult");
      setShowForm(false);
    } else {
      setError(result.payload as string ?? t("common.error"));
    }
  }

  if (!familyId) return null;

  return (
    <div>
      <div className="page-header">
        <h1>{t("people.title")}</h1>
        <button
          className="btn"
          onClick={() => { setShowForm(true); setError(null); }}
        >
          + {t("people.add")}
        </button>
      </div>

      {showForm && (
        <div className="card">
          <h2>{t("people.add")}</h2>
          <form onSubmit={handleAdd}>
            <div className="inline-form" style={{ marginBottom: "0.75rem" }}>
              <div className="form-group" style={{ flex: 2 }}>
                <label htmlFor="person-name">{t("people.form.name")}</label>
                <input
                  id="person-name"
                  className="form-control"
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                  autoFocus
                />
              </div>
              <div className="form-group" style={{ flex: 1 }}>
                <label htmlFor="person-role">{t("people.form.role")}</label>
                <select
                  id="person-role"
                  className="form-control"
                  value={role}
                  onChange={(e) => setRole(e.target.value)}
                >
                  <option value="Adult">{t("onboarding.people.roles.Adult")}</option>
                  <option value="Child">{t("onboarding.people.roles.Child")}</option>
                  <option value="Teen">{t("onboarding.people.roles.Teen")}</option>
                </select>
              </div>
            </div>
            {error && <p className="error-msg">{error}</p>}
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button type="submit" className="btn" disabled={submitting}>
                {submitting ? t("common.saving") : t("people.form.save")}
              </button>
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => setShowForm(false)}
              >
                {t("people.form.cancel")}
              </button>
            </div>
          </form>
        </div>
      )}

      {members.length === 0 ? (
        <div className="empty-state">
          <p>{t("people.title")}</p>
        </div>
      ) : (
        <div className="item-list">
          {members.map((m) => (
            <div key={m.memberId} className="item-card">
              <div
                style={{
                  width: 36, height: 36, borderRadius: "50%",
                  background: "color-mix(in srgb, var(--primary) 15%, transparent)",
                  color: "var(--primary)",
                  display: "flex", alignItems: "center", justifyContent: "center",
                  fontWeight: 700, fontSize: "0.9rem", flexShrink: 0,
                }}
              >
                {m.name[0]?.toUpperCase()}
              </div>
              <div>
                <div style={{ fontWeight: 600 }}>{m.name}</div>
                <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                  {t(`onboarding.people.roles.${m.role}` as never, m.role)}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

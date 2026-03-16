import { NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthProvider";
import { HouseholdLogo } from "./HouseholdLogo";
import { useAppSelector } from "../store/hooks";

export function AppShell({ children }: { children: React.ReactNode }) {
  const { logout } = useAuth();
  const nav = useNavigate();
  const { t } = useTranslation();
  const family = useAppSelector((s) => s.household.family);

  async function handleLogout() {
    await logout();
    nav("/login");
  }

  return (
    <div className="app-layout">
      <header className="site-header">
        <NavLink to="/timeline" className="brand">
          <HouseholdLogo className="brand-mark" />
          {family?.name ?? "DomusMind"}
        </NavLink>

        <nav aria-label="Primary">
          <ul>
            <li>
              <NavLink to="/timeline" className={({ isActive }) => isActive ? "active" : undefined}>
                {t("nav.timeline")}
              </NavLink>
            </li>
            <li>
              <NavLink to="/people" className={({ isActive }) => isActive ? "active" : undefined}>
                {t("nav.people")}
              </NavLink>
            </li>
            <li>
              <NavLink to="/areas" className={({ isActive }) => isActive ? "active" : undefined}>
                {t("nav.areas")}
              </NavLink>
            </li>
            <li>
              <NavLink to="/plans" className={({ isActive }) => isActive ? "active" : undefined}>
                {t("nav.plans")}
              </NavLink>
            </li>
            <li>
              <NavLink to="/tasks" className={({ isActive }) => isActive ? "active" : undefined}>
                {t("nav.chores")}
              </NavLink>
            </li>
          </ul>
        </nav>

        <div className="header-controls">
          <button className="btn btn-ghost btn-sm" onClick={handleLogout}>
            {t("nav.signOut")}
          </button>
        </div>
      </header>

      <main className="app-main">{children}</main>
    </div>
  );
}

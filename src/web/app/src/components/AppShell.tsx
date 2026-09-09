import { useState, useRef, useEffect } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthProvider";
import { HouseholdLogo } from "./HouseholdLogo";
import { ThemeToggle } from "./ThemeToggle";
import { useAppSelector } from "../store/hooks";
import { MemberAvatar } from "../features/settings/components/avatar/MemberAvatar";
import { NavRail } from "./NavRail";
import type { NavRailItem } from "./NavRail";

const IconAgenda = (
  <svg width="18" height="18" viewBox="0 0 18 18" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <rect x="2" y="3" width="14" height="13" rx="2"/>
    <path d="M6 1v4M12 1v4M2 7h14"/>
  </svg>
);

const IconLists = (
  <svg width="18" height="18" viewBox="0 0 18 18" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" aria-hidden="true">
    <line x1="7" y1="5" x2="15" y2="5"/>
    <line x1="7" y1="9" x2="15" y2="9"/>
    <line x1="7" y1="13" x2="15" y2="13"/>
    <circle cx="3.5" cy="5" r="1.2" fill="currentColor" stroke="none"/>
    <circle cx="3.5" cy="9" r="1.2" fill="currentColor" stroke="none"/>
    <circle cx="3.5" cy="13" r="1.2" fill="currentColor" stroke="none"/>
  </svg>
);

const IconAreas = (
  <svg width="18" height="18" viewBox="0 0 18 18" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <rect x="2" y="2" width="6" height="6" rx="1"/>
    <rect x="10" y="2" width="6" height="6" rx="1"/>
    <rect x="2" y="10" width="6" height="6" rx="1"/>
    <rect x="10" y="10" width="6" height="6" rx="1"/>
  </svg>
);

const IconMeals = (
  <svg width="18" height="18" viewBox="0 0 18 18" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M3 4h12M3 8h8M3 12h5"/>
    <circle cx="14" cy="12" r="2.5"/>
    <path d="M14 6v3.5"/>
  </svg>
);

const IconRecipes = (
  <svg width="18" height="18" viewBox="0 0 18 18" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M4 3h10a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1z"/>
    <line x1="6" y1="7" x2="12" y2="7"/>
    <line x1="6" y1="10" x2="10" y2="10"/>
    <circle cx="13" cy="13" r="2.5" fill="none"/>
    <line x1="14.8" y1="14.8" x2="16.5" y2="16.5"/>
  </svg>
);

const NAV_ITEMS: readonly NavRailItem[] = [
  { to: "/agenda", labelKey: "agenda", icon: IconAgenda },
  { to: "/lists", labelKey: "lists", icon: IconLists },
  { to: "/meal-planning", labelKey: "mealPlanning", icon: IconMeals },
  { to: "/recipes", labelKey: "recipes", icon: IconRecipes },
  { to: "/areas", labelKey: "areas", icon: IconAreas },
] as const;

export function AppShell({ children }: { children: React.ReactNode }) {
  const { logout, user } = useAuth();
  const nav = useNavigate();
  const { t: tNav } = useTranslation("nav");
  const { t: tCommon } = useTranslation("common");
  const family = useAppSelector((s) => s.household.family);

  const [avatarMenuOpen, setAvatarMenuOpen] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const avatarMenuRef = useRef<HTMLDivElement>(null);

  // Close avatar dropdown on outside click
  useEffect(() => {
    function onOutside(e: MouseEvent) {
      if (avatarMenuRef.current && !avatarMenuRef.current.contains(e.target as Node)) {
        setAvatarMenuOpen(false);
      }
    }
    if (avatarMenuOpen) document.addEventListener("mousedown", onOutside);
    return () => document.removeEventListener("mousedown", onOutside);
  }, [avatarMenuOpen]);

  // Close drawer on Escape
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") setDrawerOpen(false);
    }
    if (drawerOpen) document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [drawerOpen]);

  async function handleLogout() {
    setAvatarMenuOpen(false);
    await logout();
    nav("/login");
  }

  const members = useAppSelector((s) => s.household.members);
  const currentMember = members.find(
    (m) => m.authUserId === user?.userId || (user?.memberId != null && m.memberId === user?.memberId),
  );
  const userName = user?.displayName ?? user?.memberName ?? currentMember?.name ?? user?.email ?? "";

  return (
    <div className="app-layout">
      {/* Desktop navigation rail — hidden on mobile via CSS */}
      <NavRail items={NAV_ITEMS} />

      {/* Mobile header — hidden on desktop via CSS */}
      <header className="site-header">
        <button
          className="hamburger"
          aria-label="Open navigation"
          aria-expanded={drawerOpen}
          onClick={() => setDrawerOpen(true)}
          type="button"
        >
          <span />
          <span />
          <span />
        </button>

        <NavLink to="/" className="brand">
          <HouseholdLogo className="brand-mark" />
          <span className="brand-name">{family?.name ?? "DomusMind"}</span>
        </NavLink>

        <div className="header-end" ref={avatarMenuRef}>
          <button
            className="user-avatar"
            style={{ background: "none", border: "none", padding: 0, cursor: "pointer", display: "flex" }}
            onClick={() => setAvatarMenuOpen((o) => !o)}
            aria-label="Open user menu"
            title={userName}
            type="button"
          >
            <MemberAvatar
              initial={currentMember?.avatarInitial ?? userName[0]?.toUpperCase() ?? "?"}
              avatarIconId={currentMember?.avatarIconId}
              avatarColorId={currentMember?.avatarColorId}
              size={32}
            />
          </button>
          <ThemeToggle />
          {avatarMenuOpen && (
            <div className="avatar-menu" role="menu">
              <NavLink
                to="/settings"
                className="avatar-menu-item"
                role="menuitem"
                onClick={() => setAvatarMenuOpen(false)}
              >
                {tNav("settings")}
              </NavLink>
              <div className="avatar-menu-divider" role="separator" />
              <button
                className="avatar-menu-item avatar-menu-item--danger"
                role="menuitem"
                onClick={handleLogout}
                type="button"
              >
                {tNav("signOut")}
              </button>
            </div>
          )}
        </div>
      </header>

      {/* Mobile navigation drawer */}
      {drawerOpen && (
        <>
          <div
            className="drawer-backdrop"
            aria-hidden="true"
            onClick={() => setDrawerOpen(false)}
          />
          <nav className="mobile-drawer" aria-label="Mobile navigation">
            <div className="mobile-drawer-header">
              <NavLink to="/" className="brand" onClick={() => setDrawerOpen(false)}>
                <HouseholdLogo className="brand-mark" />
                <span>{family?.name ?? "DomusMind"}</span>
              </NavLink>
              <button
                className="drawer-close"
                aria-label={tCommon("close")}
                onClick={() => setDrawerOpen(false)}
                type="button"
              >
                ✕
              </button>
            </div>
            <ul>
              {NAV_ITEMS.map(({ to, labelKey }) => (
                <li key={to}>
                  <NavLink
                    to={to}
                    end={to === "/"}
                    className={({ isActive }) => isActive ? "active" : undefined}
                    onClick={() => setDrawerOpen(false)}
                  >
                    {tNav(labelKey as never)}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>
        </>
      )}

      <main className="app-main">{children}</main>
    </div>
  );
}

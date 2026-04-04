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

const NAV_ITEMS: readonly NavRailItem[] = [
  { to: "/", labelKey: "today" },
  { to: "/planning", labelKey: "planning" },
  { to: "/lists", labelKey: "lists" },
  { to: "/areas", labelKey: "areas" },
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
              <li>
                <NavLink
                  to="/settings"
                  className={({ isActive }) => isActive ? "active" : undefined}
                  onClick={() => setDrawerOpen(false)}
                >
                  {tNav("settings")}
                </NavLink>
              </li>
            </ul>
          </nav>
        </>
      )}

      <main className="app-main">{children}</main>
    </div>
  );
}

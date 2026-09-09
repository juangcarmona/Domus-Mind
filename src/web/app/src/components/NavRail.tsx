import { useState, useRef, useEffect } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthProvider";
import { HouseholdLogo } from "./HouseholdLogo";
import { useAppSelector, useAppDispatch } from "../store/hooks";
import { setTheme, type Theme } from "../store/uiSlice";
import { MemberAvatar } from "../features/settings/components/avatar/MemberAvatar";

export type NavRailItem = {
  to: string;
  labelKey: string;
  icon?: React.ReactNode;
};

interface NavRailProps {
  items: readonly NavRailItem[];
}

// ---- Sidebar collapse/expand toggle icon ----
// Inline from /public/sidebar-collapse.svg (viewBox 0 0 64 64, fill-rule evenodd)
// When expanded: arrow points left → "Close sidebar"
// When collapsed: scaleX(-1) → arrow points right → "Open sidebar"
function SidebarCollapseIcon() {
  return (
    <svg
      viewBox="0 0 64 64"
      width="20"
      height="20"
      fill="currentColor"
      aria-hidden="true"
      style={{ fillRule: "evenodd", clipRule: "evenodd", flexShrink: 0 }}
    >
      <path d="M49.984,56l-35.989,0c-3.309,0 -5.995,-2.686 -5.995,-5.995l0,-36.011c0,-3.308 2.686,-5.995 5.995,-5.995l35.989,0c3.309,0 5.995,2.687 5.995,5.995l0,36.011c0,3.309 -2.686,5.995 -5.995,5.995Zm-25.984,-4.001l0,-39.999l-9.012,0c-1.65,0 -2.989,1.339 -2.989,2.989l0,34.021c0,1.65 1.339,2.989 2.989,2.989l9.012,0Zm24.991,-39.999l-20.991,0l0,39.999l20.991,0c1.65,0 2.989,-1.339 2.989,-2.989l0,-34.021c0,-1.65 -1.339,-2.989 -2.989,-2.989Z" />
      <path d="M19.999,38.774l-6.828,-6.828l6.828,-6.829l2.829,2.829l-4,4l4,4l-2.829,2.828Z" />
    </svg>
  );
}

const IconSettings = (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <circle cx="12" cy="12" r="3" />
    <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
  </svg>
);

const IconSun = (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <circle cx="12" cy="12" r="5" />
    <line x1="12" y1="1" x2="12" y2="3" />
    <line x1="12" y1="21" x2="12" y2="23" />
    <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
    <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
    <line x1="1" y1="12" x2="3" y2="12" />
    <line x1="21" y1="12" x2="23" y2="12" />
    <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
    <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
  </svg>
);

const IconMoon = (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
  </svg>
);

const IconLogOut = (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
    <polyline points="16 17 21 12 16 7" />
    <line x1="21" y1="12" x2="9" y2="12" />
  </svg>
);

export function NavRail({ items }: NavRailProps) {
  const { logout, user } = useAuth();
  const nav = useNavigate();
  const { t: tNav } = useTranslation("nav");
  const family = useAppSelector((s) => s.household.family);
  const members = useAppSelector((s) => s.household.members);
  const theme = useAppSelector((s) => s.ui.theme);
  const dispatch = useAppDispatch();

  const isDark = theme === "dark" || (theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches);

  const currentMember = members.find(
    (m) => m.authUserId === user?.userId || (user?.memberId != null && m.memberId === user?.memberId),
  );
  const userName = user?.displayName ?? user?.memberName ?? currentMember?.name ?? user?.email ?? "";

  const [avatarMenuOpen, setAvatarMenuOpen] = useState(false);
  const avatarMenuRef = useRef<HTMLDivElement>(null);

  const [collapsed, setCollapsed] = useState(() => {
    return localStorage.getItem("navrail-collapsed") === "true";
  });

  function toggleCollapsed() {
    setCollapsed((c) => {
      const next = !c;
      localStorage.setItem("navrail-collapsed", String(next));
      return next;
    });
  }

  function toggleTheme() {
    const next: Theme = isDark ? "light" : "dark";
    dispatch(setTheme(next));
  }

  useEffect(() => {
    function onOutside(e: MouseEvent) {
      if (avatarMenuRef.current && !avatarMenuRef.current.contains(e.target as Node)) {
        setAvatarMenuOpen(false);
      }
    }
    if (avatarMenuOpen) document.addEventListener("mousedown", onOutside);
    return () => document.removeEventListener("mousedown", onOutside);
  }, [avatarMenuOpen]);

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") setAvatarMenuOpen(false);
    }
    if (avatarMenuOpen) document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [avatarMenuOpen]);

  async function handleLogout() {
    setAvatarMenuOpen(false);
    await logout();
    nav("/login");
  }

  return (
    <aside
      className={`nav-rail${collapsed ? " nav-rail--collapsed" : ""}`}
      aria-label="Primary navigation"
    >
      {/* ---- Brand row ---- */}
      <div className="nav-rail-brand">
        <NavLink to="/agenda" className="nav-rail-brand-link" title={family?.name ?? "DomusMind"}>
          <HouseholdLogo className="nav-rail-brand-mark" />
          <span className="nav-rail-brand-name">{family?.name ?? "DomusMind"}</span>
        </NavLink>

        <button
          className="nav-rail-collapse-btn"
          onClick={toggleCollapsed}
          aria-label={collapsed ? "Open sidebar" : "Close sidebar"}
          title={collapsed ? "Open sidebar" : "Close sidebar"}
          type="button"
        >
          <span className={`nav-rail-collapse-icon${collapsed ? " nav-rail-collapse-icon--flipped" : ""}`}>
            <SidebarCollapseIcon />
          </span>
        </button>
      </div>

      {/* ---- Nav items ---- */}
      <nav className="nav-rail-nav">
        <ul>
          {items.map(({ to, labelKey, icon }) => (
            <li key={to}>
              <NavLink
                to={to}
                end={to === "/" || to === "/agenda"}
                className={({ isActive }) => `nav-rail-item${isActive ? " active" : ""}`}
                title={collapsed ? tNav(labelKey as never) : undefined}
              >
                {icon && <span className="nav-rail-item-icon" aria-hidden="true">{icon}</span>}
                <span className="nav-rail-item-label">{tNav(labelKey as never)}</span>
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      {/* ---- Footer: account trigger ---- */}
      <div className="nav-rail-footer" ref={avatarMenuRef}>
        <button
          className="nav-rail-avatar-btn"
          onClick={() => setAvatarMenuOpen((o) => !o)}
          aria-label="Open account menu"
          aria-expanded={avatarMenuOpen}
          title={collapsed ? userName : undefined}
          type="button"
        >
          <MemberAvatar
            initial={currentMember?.avatarInitial ?? userName[0]?.toUpperCase() ?? "?"}
            avatarIconId={currentMember?.avatarIconId}
            avatarColorId={currentMember?.avatarColorId}
            size={28}
          />
          <span className="nav-rail-user-name">{userName}</span>
        </button>

        {avatarMenuOpen && (
          <div className="nav-rail-account-menu" role="menu">
            {/* Header block */}
            <div className="account-menu-header" aria-hidden="true">
              <MemberAvatar
                initial={currentMember?.avatarInitial ?? userName[0]?.toUpperCase() ?? "?"}
                avatarIconId={currentMember?.avatarIconId}
                avatarColorId={currentMember?.avatarColorId}
                size={32}
              />
              <div className="account-menu-header-text">
                <span className="account-menu-name">{userName}</span>
                {family?.name && (
                  <span className="account-menu-household">{family.name}</span>
                )}
              </div>
            </div>

            <div className="account-menu-divider" role="separator" />

            {/* Settings */}
            <NavLink
              to="/settings"
              className="account-menu-item"
              role="menuitem"
              onClick={() => setAvatarMenuOpen(false)}
            >
              <span className="account-menu-item-icon">{IconSettings}</span>
              {tNav("settings")}
            </NavLink>

            <div className="account-menu-divider" role="separator" />

            {/* Theme toggle */}
            <button
              className="account-menu-item"
              role="menuitem"
              type="button"
              onClick={toggleTheme}
            >
              <span className="account-menu-item-icon">{isDark ? IconSun : IconMoon}</span>
              {isDark ? tNav("switchToLight") : tNav("switchToDark")}
            </button>

            <div className="account-menu-divider" role="separator" />

            {/* Log out */}
            <button
              className="account-menu-item account-menu-item--danger"
              role="menuitem"
              onClick={handleLogout}
              type="button"
            >
              <span className="account-menu-item-icon">{IconLogOut}</span>
              {tNav("signOut")}
            </button>
          </div>
        )}
      </div>
    </aside>
  );
}

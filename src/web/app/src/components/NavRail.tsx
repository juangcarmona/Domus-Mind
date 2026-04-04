import { useState, useRef, useEffect } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthProvider";
import { HouseholdLogo } from "./HouseholdLogo";
import { ThemeToggle } from "./ThemeToggle";
import { useAppSelector } from "../store/hooks";
import { MemberAvatar } from "../features/settings/components/avatar/MemberAvatar";

export type NavRailItem = {
  to: string;
  labelKey: string;
};

interface NavRailProps {
  items: readonly NavRailItem[];
}

export function NavRail({ items }: NavRailProps) {
  const { logout, user } = useAuth();
  const nav = useNavigate();
  const { t: tNav } = useTranslation("nav");
  const family = useAppSelector((s) => s.household.family);
  const members = useAppSelector((s) => s.household.members);

  const currentMember = members.find(
    (m) => m.authUserId === user?.userId || (user?.memberId != null && m.memberId === user?.memberId),
  );
  const userName = user?.displayName ?? user?.memberName ?? currentMember?.name ?? user?.email ?? "";

  const [avatarMenuOpen, setAvatarMenuOpen] = useState(false);
  const avatarMenuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function onOutside(e: MouseEvent) {
      if (avatarMenuRef.current && !avatarMenuRef.current.contains(e.target as Node)) {
        setAvatarMenuOpen(false);
      }
    }
    if (avatarMenuOpen) document.addEventListener("mousedown", onOutside);
    return () => document.removeEventListener("mousedown", onOutside);
  }, [avatarMenuOpen]);

  async function handleLogout() {
    setAvatarMenuOpen(false);
    await logout();
    nav("/login");
  }

  return (
    <aside className="nav-rail" aria-label="Primary navigation">
      <div className="nav-rail-brand">
        <NavLink to="/" className="nav-rail-brand-link">
          <HouseholdLogo className="nav-rail-brand-mark" />
          <span className="nav-rail-brand-name">{family?.name ?? "DomusMind"}</span>
        </NavLink>
      </div>

      <nav className="nav-rail-nav">
        <ul>
          {items.map(({ to, labelKey }) => (
            <li key={to}>
              <NavLink
                to={to}
                end={to === "/"}
                className={({ isActive }) => `nav-rail-item${isActive ? " active" : ""}`}
              >
                {tNav(labelKey as never)}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      <div className="nav-rail-footer" ref={avatarMenuRef}>
        <ThemeToggle />
        <button
          className="nav-rail-avatar-btn"
          onClick={() => setAvatarMenuOpen((o) => !o)}
          aria-label="Open user menu"
          title={userName}
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
          <div className="nav-rail-avatar-menu" role="menu">
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
    </aside>
  );
}

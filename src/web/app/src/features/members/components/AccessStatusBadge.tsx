import { useTranslation } from "react-i18next";
import type { MemberAccessStatus } from "../../../api/domusmindApi";

const STATUS_MAP: Record<MemberAccessStatus, { labelKey: string; color: string } | null> = {
  NoAccess: { labelKey: "access.noAccess", color: "var(--muted)" },
  InvitedOrProvisioned: { labelKey: "access.invited", color: "#3b82f6" },
  PasswordResetRequired: { labelKey: "access.passwordResetRequired", color: "#f5a623" },
  Active: { labelKey: "access.active", color: "#22c55e" },
  Disabled: { labelKey: "access.disabled", color: "#ef4444" },
};

interface AccessStatusBadgeProps {
  status: MemberAccessStatus;
  /** When true, returns null for NoAccess instead of showing a muted badge */
  hideNoAccess?: boolean;
}

export function AccessStatusBadge({ status, hideNoAccess }: AccessStatusBadgeProps) {
  const { t } = useTranslation("members");
  const badge = STATUS_MAP[status];

  if (!badge || (hideNoAccess && status === "NoAccess")) return null;

  return (
    <span
      style={{
        fontSize: "0.78rem",
        fontWeight: 600,
        padding: "0.15rem 0.55rem",
        borderRadius: 999,
        background: `color-mix(in srgb, ${badge.color} 14%, transparent)`,
        color: badge.color,
        border: `1px solid color-mix(in srgb, ${badge.color} 26%, transparent)`,
        whiteSpace: "nowrap",
      }}
    >
      {t(badge.labelKey as never)}
    </span>
  );
}

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
      className="c-access-badge"
      style={{
        ["--badge-color" as string]: badge.color,
        ["--badge-bg-strength" as string]: "14%",
        ["--badge-border-strength" as string]: "26%",
      }}
    >
      {t(badge.labelKey as never)}
    </span>
  );
}

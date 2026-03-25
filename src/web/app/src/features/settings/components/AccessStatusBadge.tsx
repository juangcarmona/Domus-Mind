import { useTranslation } from "react-i18next";
import type { MemberAccessStatus } from "../../../api/domusmindApi";

const STATUS_MAP: Record<MemberAccessStatus, { labelKey: string; color: string }> = {
  NoAccess: { labelKey: "noAccessBadge", color: "var(--muted)" },
  InvitedOrProvisioned: { labelKey: "invitedOrProvisioned", color: "#3b82f6" },
  PasswordResetRequired: { labelKey: "passwordChangeRequired", color: "#f5a623" },
  Active: { labelKey: "accountActive", color: "#22c55e" },
  Disabled: { labelKey: "accountDisabled", color: "#ef4444" },
};

export function AccessStatusBadge({ status }: { status: MemberAccessStatus }) {
  const { t } = useTranslation("settings");
  const { labelKey, color } = STATUS_MAP[status] ?? STATUS_MAP["NoAccess"];
  return (
    <span
      className="c-access-badge"
      style={{
        ["--badge-color" as string]: color,
        ["--badge-bg-strength" as string]: "18%",
        ["--badge-border-strength" as string]: "0%",
        fontSize: "0.68rem",
        padding: "0.1rem 0.4rem",
        borderRadius: 4,
        border: "none",
      }}
    >
      {t(`household.members.${labelKey}` as never)}
    </span>
  );
}

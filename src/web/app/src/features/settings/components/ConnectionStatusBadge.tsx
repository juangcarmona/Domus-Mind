interface Props {
  status: string;
}

const STATUS_LABELS: Record<string, string> = {
  idle: "Idle",
  success: "Connected",
  syncing: "Syncing…",
  partial_failure: "Partial failure",
  failed: "Failed",
  auth_expired: "Re-authorization needed",
  rehydrating: "Rehydrating…",
  disconnected: "Disconnected",
  pending_initial_sync: "Pending sync",
  healthy: "Connected",
  needs_attention: "Needs attention",
};

const STATUS_CLASS: Record<string, string> = {
  idle: "status-badge--pending",
  success: "status-badge--healthy",
  syncing: "status-badge--syncing",
  partial_failure: "status-badge--warning",
  failed: "status-badge--error",
  auth_expired: "status-badge--error",
  rehydrating: "status-badge--syncing",
  disconnected: "status-badge--muted",
  pending_initial_sync: "status-badge--pending",
  healthy: "status-badge--healthy",
  needs_attention: "status-badge--warning",
};

export function ConnectionStatusBadge({ status }: Props) {
  const label = STATUS_LABELS[status] ?? status;
  const cls = STATUS_CLASS[status] ?? "status-badge--muted";
  return <span className={`status-badge ${cls}`}>{label}</span>;
}

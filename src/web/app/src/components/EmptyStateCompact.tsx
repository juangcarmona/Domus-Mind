interface EmptyStateCompactProps {
  message: string;
  action?: { label: string; onClick: () => void };
}

/**
 * EmptyStateCompact — compact empty state.
 *
 * Replaces the full centred .empty-state blocks that consume excessive
 * vertical space on desktop. Stays tight to surface density expectations.
 *
 * Usage:
 *   <EmptyStateCompact
 *     message="No areas yet."
 *     action={{ label: "Add area", onClick: () => setShowCreate(true) }}
 *   />
 */
export function EmptyStateCompact({ message, action }: EmptyStateCompactProps) {
  return (
    <div className="empty-state-compact">
      <p className="empty-state-compact-msg">{message}</p>
      {action && (
        <button type="button" className="btn btn-sm btn-ghost" onClick={action.onClick}>
          {action.label}
        </button>
      )}
    </div>
  );
}

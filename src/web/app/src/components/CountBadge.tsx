interface CountBadgeProps {
  count: number;
  className?: string;
}

/**
 * CountBadge — compact count indicator.
 *
 * Shows unchecked counts, task counts, upcoming items, etc.
 * Returns null when count is 0 to avoid rendering empty badges.
 *
 * Usage:
 *   <CountBadge count={uncheckedItems} />
 */
export function CountBadge({ count, className }: CountBadgeProps) {
  if (count === 0) return null;
  return (
    <span className={`count-badge${className ? ` ${className}` : ""}`}>
      {count}
    </span>
  );
}

import type { KeyboardEvent, ReactNode } from "react";

interface EntityCardProps {
  title: string;
  /** When true, renders the title with a line-through. */
  titleStrike?: boolean;
  subtitle?: ReactNode;
  accentColor: string;
  /** Applies the .overdue CSS modifier. */
  isOverdue?: boolean;
  /** Renders the card at reduced opacity (used for historical/cancelled items). */
  dimmed?: boolean;
  onClick?: () => void;
  /** Rendered inside .item-card-actions - use for action buttons. */
  actions?: ReactNode;
  className?: string;
}

/**
 * Shared clickable entity card.
 *
 * Used across Planning (TasksTab, PlansTab, RoutinesTab) and Area detail related-work.
 * All interactive state (buttons, handlers) is passed in by the caller via `actions`
 * and `onClick`; this component only owns the visual shell.
 */
export function EntityCard({
  title,
  titleStrike,
  subtitle,
  accentColor,
  isOverdue,
  dimmed,
  onClick,
  actions,
  className,
}: EntityCardProps) {
  const clickable = !!onClick;

  function handleKeyDown(e: KeyboardEvent<HTMLDivElement>) {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      onClick?.();
    }
  }

  return (
    <div
      className={`item-card${isOverdue ? " overdue" : ""}${className ? ` ${className}` : ""}`}
      style={{
        borderLeft: `3px solid ${accentColor}`,
        opacity: dimmed ? 0.6 : undefined,
      }}
      onClick={clickable ? onClick : undefined}
      role={clickable ? "button" : undefined}
      tabIndex={clickable ? 0 : undefined}
      onKeyDown={clickable ? handleKeyDown : undefined}
    >
      <div className="item-card-body">
        <div
          className="item-card-title"
          style={titleStrike ? { textDecoration: "line-through" } : undefined}
        >
          {title}
        </div>
        {subtitle != null && <div className="item-card-subtitle">{subtitle}</div>}
      </div>
      {actions != null && <div className="item-card-actions">{actions}</div>}
    </div>
  );
}

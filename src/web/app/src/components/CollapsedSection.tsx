import { useState } from "react";

interface CollapsedSectionProps {
  label: string;
  count?: number;
  children: React.ReactNode;
  defaultExpanded?: boolean;
}

/**
 * CollapsedSection — collapsible content section.
 *
 * Used for completed items, archived items, or any secondary content
 * that should be accessible but not dominant in the default view.
 *
 * Usage:
 *   <CollapsedSection label="Completed" count={completedItems.length}>
 *     {completedItems.map(...)}
 *   </CollapsedSection>
 */
export function CollapsedSection({
  label,
  count,
  children,
  defaultExpanded = false,
}: CollapsedSectionProps) {
  const [expanded, setExpanded] = useState(defaultExpanded);

  return (
    <div className="collapsed-section">
      <button
        type="button"
        className="collapsed-section-toggle"
        onClick={() => setExpanded((e) => !e)}
        aria-expanded={expanded}
      >
        <span className="collapsed-section-label">{label}</span>
        {count !== undefined && (
          <span className="collapsed-section-count">{count}</span>
        )}
        <span className="collapsed-section-chevron" aria-hidden="true">
          {expanded ? "▲" : "▼"}
        </span>
      </button>
      {expanded && <div className="collapsed-section-body">{children}</div>}
    </div>
  );
}

interface ContextChipProps {
  label: string;
  onClick?: () => void;
  className?: string;
}

/**
 * ContextChip — small chip for showing area/plan context links.
 *
 * Used to surface contextual associations (area, linked plan, category)
 * in a compact, scannable form inside rows or inspectors.
 *
 * Usage:
 *   <ContextChip label="Kitchen" onClick={() => nav("/areas/...")} />
 *   <ContextChip label="Weekly shop" />   // read-only
 */
export function ContextChip({ label, onClick, className }: ContextChipProps) {
  if (onClick) {
    return (
      <button
        type="button"
        className={`context-chip${className ? ` ${className}` : ""}`}
        onClick={onClick}
      >
        {label}
      </button>
    );
  }
  return (
    <span className={`context-chip${className ? ` ${className}` : ""}`}>
      {label}
    </span>
  );
}

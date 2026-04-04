interface InspectorPanelProps {
  children: React.ReactNode;
  title?: string;
  onClose?: () => void;
  className?: string;
}

/**
 * InspectorPanel — right contextual panel for desktop detail inspection.
 *
 * Place this alongside main content inside .l-surface-body to create
 * a split content+inspector layout. On mobile, suppress it and use
 * BottomSheetDetail instead.
 *
 * Usage:
 *   <div className="l-surface-body">
 *     <div className="l-surface-content">...</div>
 *     {selectedItem && (
 *       <InspectorPanel title="Details" onClose={() => setSelected(null)}>
 *         ...
 *       </InspectorPanel>
 *     )}
 *   </div>
 */
export function InspectorPanel({ children, title, onClose, className }: InspectorPanelProps) {
  return (
    <aside className={`inspector-panel${className ? ` ${className}` : ""}`}>
      {(title || onClose) && (
        <div className="inspector-panel-header">
          {title && <h2 className="inspector-panel-title">{title}</h2>}
          {onClose && (
            <button
              className="inspector-panel-close"
              onClick={onClose}
              aria-label="Close inspector"
              type="button"
            >
              ✕
            </button>
          )}
        </div>
      )}
      <div className="inspector-panel-body">{children}</div>
    </aside>
  );
}

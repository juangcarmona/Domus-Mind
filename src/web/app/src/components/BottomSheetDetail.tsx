import { useEffect } from "react";

interface BottomSheetDetailProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
}

/**
 * BottomSheetDetail — mobile contextual detail overlay.
 *
 * Replaces full-page navigation or desktop inspector for item detail on mobile.
 * On desktop, suppress this and use InspectorPanel instead.
 *
 * Usage:
 *   <BottomSheetDetail open={!!selected} onClose={() => setSelected(null)} title="Details">
 *     ...
 *   </BottomSheetDetail>
 */
export function BottomSheetDetail({ open, onClose, title, children }: BottomSheetDetailProps) {
  useEffect(() => {
    if (!open) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <>
      <div className="bottom-sheet-backdrop" aria-hidden="true" onClick={onClose} />
      <div className="bottom-sheet" role="dialog" aria-modal="true">
        <div className="bottom-sheet-handle" aria-hidden="true" />
        {title && (
          <div className="bottom-sheet-header">
            <h2 className="bottom-sheet-title">{title}</h2>
            <button
              className="bottom-sheet-close"
              onClick={onClose}
              aria-label="Close"
              type="button"
            >
              ✕
            </button>
          </div>
        )}
        <div className="bottom-sheet-body">{children}</div>
      </div>
    </>
  );
}

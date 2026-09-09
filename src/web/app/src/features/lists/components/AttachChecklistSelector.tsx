import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { linkSharedListToEvent } from "../../../store/listsSlice";

interface AttachChecklistSelectorProps {
  eventId: string;
  onAttached: () => void;
  onCancel: () => void;
}

/**
 * Lightweight inline selector for attaching an existing family list to an event.
 * Shows only unlinked lists (no existing link) to avoid duplicate linkage.
 */
export function AttachChecklistSelector({
  eventId,
  onAttached,
  onCancel,
}: AttachChecklistSelectorProps) {
  const { t } = useTranslation("lists");
  const dispatch = useAppDispatch();
  const lists = useAppSelector((s) => s.lists.lists);

  const [selectedId, setSelectedId] = useState("");
  const [attaching, setAttaching] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Only offer lists that are not already linked
  const unlinkedLists = lists.filter((l) => !l.linkedEntityId);

  async function handleAttach() {
    if (!selectedId) return;
    setAttaching(true);
    setError(null);
    const result = await dispatch(linkSharedListToEvent({ listId: selectedId, eventId }));
    setAttaching(false);
    if (linkSharedListToEvent.fulfilled.match(result)) {
      onAttached();
    } else {
      setError((result.payload as string) ?? t("attachError"));
    }
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
      <select
        className="form-control"
        value={selectedId}
        onChange={(e) => setSelectedId(e.target.value)}
        autoFocus
      >
        <option value="">{t("attachSelectPlaceholder")}</option>
        {unlinkedLists.map((l) => (
          <option key={l.id} value={l.id}>{l.name}</option>
        ))}
      </select>
      <div style={{ display: "flex", gap: "0.5rem" }}>
        <button
          type="button"
          className="btn btn-ghost"
          onClick={onCancel}
          disabled={attaching}
        >
          {/* use common cancel translation */}
          ✕
        </button>
        <button
          type="button"
          className="btn"
          onClick={handleAttach}
          disabled={attaching || !selectedId}
        >
          {attaching ? t("attachAttaching") : t("attachChecklist")}
        </button>
      </div>
      {error && <p className="error-msg">{error}</p>}
    </div>
  );
}

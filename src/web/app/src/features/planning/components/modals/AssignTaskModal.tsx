import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { EnrichedTimelineEntry } from "../../../../api/domusmindApi";

interface AssignTaskModalProps {
  entry: EnrichedTimelineEntry;
  members: { memberId: string; name: string }[];
  onAssign: (taskId: string, memberId: string) => Promise<void>;
  onClose: () => void;
}

export function AssignTaskModal({
  entry,
  members,
  onAssign,
  onClose,
}: AssignTaskModalProps) {
  const { t } = useTranslation("tasks");
  const { t: tCommon } = useTranslation("common");
  const [memberId, setMemberId] = useState("");
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!memberId) return;
    setSubmitting(true);
    await onAssign(entry.entryId, memberId);
    setSubmitting(false);
    onClose();
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>{t("assign")} - {entry.title}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="planning-assign-select">{t("assignTo")}</label>
            <select
              id="planning-assign-select"
              className="form-control"
              value={memberId}
              onChange={(e) => setMemberId(e.target.value)}
              required
              autoFocus
            >
              <option value="">{tCommon("selectPerson")}</option>
              {members.map((m) => (
                <option key={m.memberId} value={m.memberId}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {tCommon("cancel")}
            </button>
            <button type="submit" className="btn" disabled={submitting || !memberId}>
              {submitting ? t("assigning") : t("assign")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

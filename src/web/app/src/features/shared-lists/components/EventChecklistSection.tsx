import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { sharedListsApi } from "../../../api/sharedListsApi";
import type { GetSharedListByLinkedEntityResponse } from "../../../api/types/sharedListTypes";

interface EventChecklistSectionProps {
  eventId: string;
  familyId: string;
}

export function EventChecklistSection({ eventId, familyId }: EventChecklistSectionProps) {
  const { t } = useTranslation("sharedLists");
  const navigate = useNavigate();

  const [linked, setLinked] = useState<GetSharedListByLinkedEntityResponse | null>(null);
  const [loadingLinked, setLoadingLinked] = useState(true);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadLinked = useCallback(async () => {
    setLoadingLinked(true);
    try {
      const result = await sharedListsApi.getSharedListByLinkedEntity("CalendarEvent", eventId);
      setLinked(result);
    } catch {
      setLinked(null);
    } finally {
      setLoadingLinked(false);
    }
  }, [eventId]);

  useEffect(() => {
    loadLinked();
  }, [loadLinked]);

  async function handleCreate() {
    setCreating(true);
    setError(null);
    try {
      const result = await sharedListsApi.createLinkedSharedListForEvent(eventId, { familyId });
      navigate(`/lists/${result.listId}`);
    } catch (err) {
      setError((err as { message?: string }).message ?? t("checklistError"));
      setCreating(false);
    }
  }

  if (loadingLinked) return null;

  return (
    <div className="form-group">
      <label>{t("checklistSection")}</label>
      {linked ? (
        <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
          <span>{linked.name}</span>
          <button
            type="button"
            className="btn btn-ghost"
            onClick={() => navigate(`/lists/${linked.listId}`)}
          >
            {t("openChecklist")}
          </button>
        </div>
      ) : (
        <>
          <div>
            <button
              type="button"
              className="btn btn-ghost"
              onClick={handleCreate}
              disabled={creating}
            >
              {creating ? t("checklistCreating") : t("createChecklist")}
            </button>
          </div>
          {error && <p className="error-msg">{error}</p>}
        </>
      )}
    </div>
  );
}

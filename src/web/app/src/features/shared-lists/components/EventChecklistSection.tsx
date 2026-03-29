import { useState, useEffect, useCallback, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useAppDispatch } from "../../../store/hooks";
import { unlinkSharedList } from "../../../store/sharedListsSlice";
import { sharedListsApi } from "../../../api/sharedListsApi";
import type { GetSharedListByLinkedEntityResponse } from "../../../api/types/sharedListTypes";
import { AttachChecklistSelector } from "./AttachChecklistSelector";

interface EventChecklistSectionProps {
  eventId: string;
  familyId: string;
}

export function EventChecklistSection({ eventId, familyId }: EventChecklistSectionProps) {
  const { t } = useTranslation("sharedLists");
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const [linked, setLinked] = useState<GetSharedListByLinkedEntityResponse | null>(null);
  const [loadingLinked, setLoadingLinked] = useState(true);
  const [creating, setCreating] = useState(false);
  const [unlinking, setUnlinking] = useState(false);
  const [showAttach, setShowAttach] = useState(false);
  const [showNameInput, setShowNameInput] = useState(false);
  const [newListName, setNewListName] = useState("");
  const [error, setError] = useState<string | null>(null);

  const loadLinked = useCallback(async () => {
    setLoadingLinked(true);
    try {
      const result = await sharedListsApi.getSharedListByLinkedEntity("CalendarEvent", eventId);
      setLinked(result);
    } catch (err) {
      // 404 = no list linked — this is a normal empty state, not an error.
      // Treat any failure as "no list linked" so the UI stays calm.
      void err;
      setLinked(null);
    } finally {
      setLoadingLinked(false);
    }
  }, [eventId]);

  useEffect(() => {
    loadLinked();
  }, [loadLinked]);

  async function handleCreate(name?: string) {
    setCreating(true);
    setError(null);
    try {
      const result = await sharedListsApi.createLinkedSharedListForEvent(eventId, {
        familyId,
        name: name?.trim() || undefined,
      });
      navigate(`/lists/${result.listId}`);
    } catch (err) {
      setError((err as { message?: string }).message ?? t("checklistError"));
      setCreating(false);
    }
  }

  function handleNameKey(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter") { e.preventDefault(); handleCreate(newListName); }
    else if (e.key === "Escape") { e.preventDefault(); setShowNameInput(false); setNewListName(""); }
  }

  async function handleUnlink() {
    if (!linked) return;
    setUnlinking(true);
    setError(null);
    const result = await dispatch(unlinkSharedList(linked.listId));
    setUnlinking(false);
    if (unlinkSharedList.fulfilled.match(result)) {
      setLinked(null);
    } else {
      setError((result.payload as string) ?? t("checklistError"));
    }
  }

  function handleAttached() {
    setShowAttach(false);
    loadLinked();
  }

  if (loadingLinked) return null;

  return (
    <div className="form-group">
      <label>{t("checklistSection")}</label>

      {linked ? (
        <div style={{ display: "flex", alignItems: "center", gap: "0.75rem", flexWrap: "wrap" }}>
          <span>{linked.name}</span>
          <button
            type="button"
            className="btn btn-ghost"
            onClick={() => navigate(`/lists/${linked.listId}`)}
          >
            {t("openChecklist")}
          </button>
          <button
            type="button"
            className="btn btn-ghost"
            onClick={handleUnlink}
            disabled={unlinking}
          >
            {unlinking ? "…" : t("unlinkChecklist")}
          </button>
        </div>
      ) : showAttach ? (
        <AttachChecklistSelector
          eventId={eventId}
          onAttached={handleAttached}
          onCancel={() => setShowAttach(false)}
        />
      ) : showNameInput ? (
        <div style={{ display: "flex", gap: "0.5rem", alignItems: "center", flexWrap: "wrap" }}>
          <input
            type="text"
            className="shared-list-add-input"
            placeholder={t("createChecklistName")}
            value={newListName}
            onChange={(e) => setNewListName(e.target.value)}
            onKeyDown={handleNameKey}
            autoFocus
            aria-label={t("createChecklistName")}
            style={{ flex: "1", minWidth: "12rem" }}
          />
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => handleCreate(newListName)}
            disabled={creating}
          >
            {creating ? t("checklistCreating") : t("createChecklist")}
          </button>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => { setShowNameInput(false); setNewListName(""); }}
          >
            {t("cancel")}
          </button>
        </div>
      ) : (
        <>
          <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap" }}>
            <button
              type="button"
              className="btn btn-ghost"
              onClick={() => setShowNameInput(true)}
              disabled={creating}
            >
              {t("createChecklist")}
            </button>
            <button
              type="button"
              className="btn btn-ghost"
              onClick={() => setShowAttach(true)}
            >
              {t("attachChecklist")}
            </button>
          </div>
          {error && <p className="error-msg">{error}</p>}
        </>
      )}

      {linked && error && <p className="error-msg">{error}</p>}
    </div>
  );
}

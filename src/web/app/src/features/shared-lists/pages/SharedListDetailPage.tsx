import { useEffect, useState, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import {
  fetchSharedListDetail,
  addItemToSharedList,
  toggleSharedListItem,
  optimisticToggleItem,
  clearDetail,
} from "../../../store/sharedListsSlice";

export function SharedListDetailPage() {
  const { listId } = useParams<{ listId: string }>();
  const { t } = useTranslation("sharedLists");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const detail = useAppSelector((s) => s.sharedLists.detail);
  const detailStatus = useAppSelector((s) => s.sharedLists.detailStatus);
  const [addItemName, setAddItemName] = useState("");
  const [addError, setAddError] = useState<string | null>(null);

  useEffect(() => {
    if (listId) {
      dispatch(fetchSharedListDetail(listId));
    }
    return () => {
      dispatch(clearDetail());
    };
  }, [listId, dispatch]);

  async function handleToggle(itemId: string) {
    if (!listId) return;
    const item = detail?.items.find((i) => i.itemId === itemId);
    if (!item) return;
    dispatch(optimisticToggleItem({ itemId }));
    await dispatch(toggleSharedListItem({ listId, itemId }));
  }

  async function handleAddItem(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key !== "Enter") return;
    const name = addItemName.trim();
    if (!name || !listId) return;
    setAddError(null);
    const result = await dispatch(addItemToSharedList({ listId, name }));
    if (addItemToSharedList.fulfilled.match(result)) {
      setAddItemName("");
    } else {
      setAddError((result.payload as string) ?? t("addError"));
    }
  }

  if (detailStatus === "loading" && !detail) {
    return (
      <div className="page-wrap">
        <div className="loading-wrap">{t("loadingDetail")}</div>
      </div>
    );
  }

  if (detailStatus === "error" || !detail) {
    return (
      <div className="page-wrap">
        <p className="error-msg">{tCommon("failed")}</p>
      </div>
    );
  }

  const sorted = [...detail.items].sort((a, b) => a.order - b.order);

  return (
    <div className="page-wrap">
      <div className="page-header">
        <div>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => navigate("/lists")}
          >
            ← {t("backToLists")}
          </button>
          <h1 style={{ marginTop: "0.5rem" }}>{detail.name}</h1>
        </div>
      </div>

      <div className="shared-list-items-wrap">
        {sorted.map((item) => (
          <div
            key={item.itemId}
            className={`shared-list-item${item.checked ? " shared-list-item--checked" : ""}`}
          >
            <input
              type="checkbox"
              className="shared-list-item__checkbox"
              checked={item.checked}
              onChange={() => handleToggle(item.itemId)}
              aria-label={item.name}
            />
            <span className="shared-list-item__name">{item.name}</span>
          </div>
        ))}

        {sorted.length === 0 && (
          <div className="shared-list-item">
            <span className="shared-list-item__name" style={{ color: "var(--muted)" }}>
              {t("noItems")}
            </span>
          </div>
        )}

        <div className="shared-list-add-row">
          <input
            type="text"
            className="shared-list-add-input"
            placeholder={t("addItemPlaceholder")}
            value={addItemName}
            onChange={(e) => setAddItemName(e.target.value)}
            onKeyDown={handleAddItem}
          />
        </div>
      </div>

      {addError && <p className="error-msg" style={{ marginTop: "0.5rem" }}>{addError}</p>}
    </div>
  );
}

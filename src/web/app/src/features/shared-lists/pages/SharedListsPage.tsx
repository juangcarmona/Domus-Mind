import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchFamilySharedLists } from "../../../store/sharedListsSlice";
import { CreateSharedListModal } from "../components/CreateSharedListModal";

export function SharedListsPage() {
  const { t } = useTranslation("sharedLists");
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const familyId = useAppSelector((s) => s.household.family?.familyId);
  const lists = useAppSelector((s) => s.sharedLists.lists);
  const listsStatus = useAppSelector((s) => s.sharedLists.listsStatus);
  const [showCreate, setShowCreate] = useState(false);

  useEffect(() => {
    if (familyId) {
      dispatch(fetchFamilySharedLists(familyId));
    }
  }, [familyId, dispatch]);

  return (
    <div className="page-wrap">
      <div className="page-header">
        <h1>{t("title")}</h1>
        <button className="btn btn-sm" onClick={() => setShowCreate(true)}>
          {t("newList")}
        </button>
      </div>

      {listsStatus === "loading" && lists.length === 0 && (
        <div className="loading-wrap">{t("loading")}</div>
      )}

      {listsStatus !== "loading" && lists.length === 0 && (
        <div className="empty-state">
          <p className="empty-state-headline">{t("emptyHeadline")}</p>
          <p>{t("emptyHint")}</p>
          <button className="btn" onClick={() => setShowCreate(true)}>
            {t("newList")}
          </button>
        </div>
      )}

      {lists.length > 0 && (
        <ul className="item-list">
          {lists.map((list) => (
            <li
              key={list.id}
              className="item-card"
              role="button"
              tabIndex={0}
              style={{ cursor: "pointer" }}
              onClick={() => navigate(`/lists/${list.id}`)}
              onKeyDown={(e) => {
                if (e.key === "Enter" || e.key === " ") {
                  e.preventDefault();
                  navigate(`/lists/${list.id}`);
                }
              }}
            >
              <div className="item-card-body">
                <div className="item-card-title">{list.name}</div>
                <div className="item-card-subtitle">{list.kind}</div>
              </div>
              <div className="item-card-actions">
                <span
                  className={`shared-list-card-count${list.uncheckedCount > 0 ? " shared-list-card-count--has-remaining" : ""}`}
                >
                  {list.uncheckedCount > 0
                    ? t("uncheckedCount_other", { count: list.uncheckedCount })
                    : t("allDone")}
                </span>
              </div>
            </li>
          ))}
        </ul>
      )}

      {showCreate && <CreateSharedListModal onClose={() => setShowCreate(false)} />}
    </div>
  );
}

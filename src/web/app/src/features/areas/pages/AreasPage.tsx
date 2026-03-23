import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchAreas } from "../../../store/areasSlice";

export function AreasPage() {
  const { t } = useTranslation("areas");
  const dispatch = useAppDispatch();
  const familyId = useAppSelector((s) => s.household.family?.familyId);
  const { items: areas, status, error } = useAppSelector((s) => s.areas);

  useEffect(() => {
    if (familyId) {
      void dispatch(fetchAreas(familyId));
    }
  }, [dispatch, familyId]);

  if (!familyId) {
    return null;
  }

  return (
    <div>
      <div className="page-header">
        <h1>{t("title")}</h1>
      </div>

      <div className="card">
        <p>{t("emptyHint")}</p>
      </div>

      {status === "loading" && <div className="loading-wrap">{t("loading")}</div>}
      {status === "error" && <p className="error-msg">{error}</p>}

      {status === "success" && areas.length === 0 && (
        <div className="empty-state">
          <p>{t("empty")}</p>
        </div>
      )}

      {areas.length > 0 && (
        <div className="item-list">
          {areas.map((area) => (
            <div key={area.areaId} className="item-card">
              <div className="item-card-body">
                <div className="item-card-title">{area.name}</div>
                {area.color && (
                  <div className="item-card-subtitle">
                    <span
                      aria-hidden="true"
                      style={{
                        display: "inline-block",
                        width: "0.75rem",
                        height: "0.75rem",
                        borderRadius: "999px",
                        backgroundColor: area.color,
                        marginRight: "0.5rem",
                        verticalAlign: "middle",
                      }}
                    />
                    {area.color}
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

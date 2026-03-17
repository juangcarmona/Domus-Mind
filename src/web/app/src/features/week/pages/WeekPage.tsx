import { useTranslation } from "react-i18next";
import { useAppSelector } from "../../../store/hooks";
import { useWeeklyGrid } from "../hooks/useWeeklyGrid";
import { WeeklyGrid } from "../components/WeeklyGrid";

export function WeekPage() {
  const { t, i18n } = useTranslation("week");
  const { t: tCommon } = useTranslation("common");
  const familyId = useAppSelector((s) => s.household.family?.familyId ?? "");
  const firstDayOfWeek = useAppSelector((s) => s.household.family?.firstDayOfWeek);
  const { grid, loading, error, weekStart, prevWeek, nextWeek } = useWeeklyGrid(familyId, firstDayOfWeek);

  const weekLabel = weekStart.toLocaleDateString(i18n.language, {
    month: "long",
    day: "numeric",
    year: "numeric",
  });

  return (
    <div className="page-content">
      <div className="page-header">
        <h1>{t("title")}</h1>
        <div className="week-nav">
          <button className="btn btn-ghost btn-sm" onClick={prevWeek}>
            {t("prevWeek")}
          </button>
          <span className="week-label">{weekLabel}</span>
          <button className="btn btn-ghost btn-sm" onClick={nextWeek}>
            {t("nextWeek")}
          </button>
        </div>
      </div>

      {loading && <p className="loading-wrap">{tCommon("loading")}</p>}
      {error && <p className="error-msg">{error}</p>}
      {!loading && !error && grid && <WeeklyGrid grid={grid} />}
      {!loading && !error && grid && grid.members.length === 0 && (
        <p className="empty-note">{t("empty")}</p>
      )}
    </div>
  );
}

import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../week/types";
import { WeeklyGrid } from "../../week/components/WeeklyGrid";

interface CoordinationWeekViewProps {
  grid: WeeklyGridResponse | null;
  loading: boolean;
  error: string | null;
  selectedDate: string;
  onDayClick: (date: string) => void;
}

export function CoordinationWeekView({
  grid,
  loading,
  error,
  selectedDate,
  onDayClick,
}: CoordinationWeekViewProps) {
  const { t } = useTranslation("coordination");
  const { t: tCommon } = useTranslation("common");

  if (loading) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }
  if (error) {
    return <p className="error-msg">{error}</p>;
  }
  if (!grid) {
    return <div className="loading-wrap">{tCommon("loading")}</div>;
  }

  return (
    <div className="coord-week-view">
      <WeeklyGrid
        grid={grid}
        selectedDate={selectedDate}
        onDayClick={onDayClick}
      />
    </div>
  );
}

import { useTranslation } from "react-i18next";
import type { WeeklyGridResponse } from "../../types";
import { WeeklyGrid } from "./WeeklyGrid";

interface WeeklyHouseholdGridProps {
  grid: WeeklyGridResponse | null;
  loading: boolean;
  error: string | null;
  selectedDate: string;
  onDayClick: (date: string) => void;
  onItemClick: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

export function WeeklyHouseholdGrid({
  grid,
  loading,
  error,
  selectedDate,
  onDayClick,
  onItemClick,
}: WeeklyHouseholdGridProps) {
  const { t } = useTranslation("today");
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
        onItemClick={onItemClick}
      />
    </div>
  );
}

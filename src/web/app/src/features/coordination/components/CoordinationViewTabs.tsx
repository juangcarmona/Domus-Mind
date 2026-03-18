import { useTranslation } from "react-i18next";
import type { ViewMode } from "../../../store/coordinationSlice";

interface CoordinationViewTabsProps {
  viewMode: ViewMode;
  onSelect: (mode: ViewMode) => void;
}

const TABS: { mode: ViewMode; key: string }[] = [
  { mode: "timeline", key: "tabs.timeline" },
  { mode: "day", key: "tabs.day" },
  { mode: "week", key: "tabs.week" },
  { mode: "month", key: "tabs.month" },
];

export function CoordinationViewTabs({ viewMode, onSelect }: CoordinationViewTabsProps) {
  const { t } = useTranslation("coordination");

  return (
    <div className="coord-view-tabs" role="tablist">
      {TABS.map(({ mode, key }) => (
        <button
          key={mode}
          role="tab"
          aria-selected={viewMode === mode}
          className={`coord-tab${viewMode === mode ? " coord-tab--active" : ""}`}
          onClick={() => onSelect(mode)}
          type="button"
        >
          {t(key as never)}
        </button>
      ))}
    </div>
  );
}

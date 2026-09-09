import { useTranslation } from "react-i18next";

/** Returns the week start shifted by ±7 days from the given ISO date. */
export function shiftWeek(weekStart: string, direction: "prev" | "next"): string {
  const d = new Date(weekStart + "T00:00:00");
  d.setDate(d.getDate() + (direction === "next" ? 7 : -7));
  return d.toISOString().slice(0, 10);
}

/** Format a "YYYY-MM-DD" date as a short localized label. */
export function formatShortDate(iso: string): string {
  try {
    const d = new Date(iso + "T00:00:00");
    return d.toLocaleDateString(undefined, { month: "short", day: "numeric" });
  } catch {
    return iso;
  }
}

interface WeekNavigatorProps {
  weekStart: string;
  onPrev: () => void;
  onNext: () => void;
}

/**
 * WeekNavigator — compact prev/week-label/next control for the meal planning header.
 */
export function WeekNavigator({ weekStart, onPrev, onNext }: WeekNavigatorProps) {
  const { t } = useTranslation("mealPlanning");
  const label = t("weekOf", { date: formatShortDate(weekStart) });

  return (
    <div className="mp-week-nav">
      <button
        type="button"
        className="mp-week-nav-btn"
        onClick={onPrev}
        aria-label={t("prevWeek")}
      >
        ‹
      </button>
      <span className="mp-week-nav-label">{label}</span>
      <button
        type="button"
        className="mp-week-nav-btn"
        onClick={onNext}
        aria-label={t("nextWeek")}
      >
        ›
      </button>
    </div>
  );
}

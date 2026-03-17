import { useTranslation } from "react-i18next";

interface WeekHeaderProps {
  days: string[]; // ISO date strings for 7 days
}

export function WeekHeader({ days }: WeekHeaderProps) {
  const { i18n } = useTranslation("week");

  return (
    <div className="wg-header-row">
      <div className="wg-member-label" />
      {days.map((iso) => {
        const d = new Date(iso);
        const weekday = d.toLocaleDateString(i18n.language, { weekday: "short" });
        const dayNum = d.toLocaleDateString(i18n.language, {
          day: "numeric",
          month: "short",
        });
        return (
          <div key={iso} className="wg-day-header">
            <span className="wg-day-name">{weekday}</span>
            <span className="wg-day-date">{dayNum}</span>
          </div>
        );
      })}
    </div>
  );
}

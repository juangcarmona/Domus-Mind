import { useTranslation } from "react-i18next";

interface WeekHeaderProps {
  days: string[]; // ISO date strings for 7 days
  today: string;  // ISO date string for today
  selectedDate?: string; // Optional: highlight selected date
  onDayClick?: (date: string) => void; // Optional: handle day click
  dayCounts?: Record<string, number>; // Optional: per-day item count for load indicator
}

export function WeekHeader({ days, today, selectedDate, onDayClick, dayCounts }: WeekHeaderProps) {
  const { i18n } = useTranslation("today");

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
        const isToday = iso.slice(0, 10) === today;
        const isSelected = selectedDate ? iso.slice(0, 10) === selectedDate : false;
        const classNames = [
          "wg-day-header",
          isToday ? "wg-day-header--today" : "",
          isSelected && !isToday ? "wg-day-header--selected" : "",
          onDayClick ? "wg-day-header--clickable" : "",
        ]
          .filter(Boolean)
          .join(" ");
        return (
          <div
            key={iso}
            className={classNames}
            onClick={onDayClick ? () => onDayClick(iso.slice(0, 10)) : undefined}
            role={onDayClick ? "button" : undefined}
            tabIndex={onDayClick ? 0 : undefined}
            onKeyDown={
              onDayClick
                ? (e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      onDayClick(iso.slice(0, 10));
                    }
                  }
                : undefined
            }
          >
            <span className="wg-day-name">{weekday}</span>
            <span className="wg-day-date">{dayNum}</span>
            {isToday && <span className="wg-today-dot" aria-hidden="true" />}
            {(() => {
              const count = dayCounts?.[iso.slice(0, 10)] ?? 0;
              if (count === 0) return null;
              const tier = count <= 2 ? "low" : count <= 5 ? "medium" : "high";
              return <span className={`wg-load-bar wg-load-bar--${tier}`} aria-hidden="true" />;
            })()}
          </div>
        );
      })}
    </div>
  );
}

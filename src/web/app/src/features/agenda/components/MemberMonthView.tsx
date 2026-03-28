import { useTranslation } from "react-i18next";
import type { WeeklyGridMember } from "../../today/types";

interface MemberMonthViewProps {
  member: WeeklyGridMember;
  /** ISO YYYY-MM-DD — anchor month. */
  selectedDate: string;
  onItemClick: (type: "event" | "task" | "routine", id: string) => void;
}

/**
 * Month-level member agenda view.
 *
 * V1: placeholder that shows a coming-soon message.
 * The component is wired into the page view switcher so the
 * tab is always accessible; full calendar grid is deferred.
 *
 * TODO: implement full month calendar grid using the month grid
 *       cache pattern already established in TodayPage.
 */
export function MemberMonthView({ member, selectedDate, onItemClick }: MemberMonthViewProps) {
  const { t } = useTranslation("agenda");

  // Suppress unused-variable warnings until implementation.
  void member;
  void selectedDate;
  void onItemClick;

  return (
    <div className="member-month-view">
      <span className="mday-empty">{t("month.empty")}</span>
    </div>
  );
}

import { useAppSelector } from "../../../../store/hooks";
import type { WeeklyGridMember } from "../../types";
import { WeeklyGridCell } from "./WeeklyGridCell";
import { MemberAvatar } from "../../../settings/components/avatar/MemberAvatar";

interface WeeklyGridRowProps {
  member: WeeklyGridMember;
  today: string; // ISO date string
  onItemClick?: (type: "event" | "task" | "routine" | "list-item", id: string) => void;
}

export function WeeklyGridRow({ member, today, onItemClick }: WeeklyGridRowProps) {
  const householdMember = useAppSelector((s) =>
    s.household.members.find((m) => m.memberId === member.memberId),
  );
  const displayName = householdMember?.preferredName || member.name;

  return (
    <div className="wg-row">
      <div className="wg-member-label">
        <MemberAvatar
          initial={householdMember?.avatarInitial ?? displayName[0]?.toUpperCase() ?? "?"}
          avatarIconId={householdMember?.avatarIconId}
          avatarColorId={householdMember?.avatarColorId}
          size={24}
        />
        <span className="wg-member-name">{displayName}</span>
      </div>
      {member.cells.map((cell) => (
        <WeeklyGridCell
          key={cell.date}
          cell={cell}
          isToday={cell.date.slice(0, 10) === today}
          onItemClick={onItemClick}
        />
      ))}
    </div>
  );
}

import type { FamilyMemberResponse } from "../../../api/domusmindApi";
import { MemberCard, type MemberCardProps } from "./MemberCard";

interface MemberGroupProps extends Omit<MemberCardProps, "m"> {
  title: string;
  members: FamilyMemberResponse[];
}

export function MemberGroup({ title, members, ...rest }: MemberGroupProps) {
  if (members.length === 0) return null;
  return (
    <div style={{ marginBottom: "1.25rem" }}>
      <div
        style={{
          fontSize: "0.75rem",
          fontWeight: 600,
          textTransform: "uppercase",
          letterSpacing: "0.06em",
          color: "var(--muted)",
          marginBottom: "0.4rem",
          paddingLeft: "0.25rem",
        }}
      >
        {title}
      </div>
      <div className="item-list">
        {members.map((m) => (
          <MemberCard key={m.memberId} m={m} {...rest} />
        ))}
      </div>
    </div>
  );
}

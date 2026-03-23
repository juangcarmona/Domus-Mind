import type { ReactNode } from "react";

interface DetailSectionProps {
  title: string;
  action?: ReactNode;
  children: ReactNode;
}

export function DetailSection({ title, action, children }: DetailSectionProps) {
  return (
    <div className="member-detail-section">
      <div className="member-detail-section-header">
        <h2 className="member-detail-section-title">{title}</h2>
        {action}
      </div>
      {children}
    </div>
  );
}

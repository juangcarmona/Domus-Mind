import { useTranslation } from "react-i18next";
import { HouseholdLogo } from "../../../components/HouseholdLogo";
import type { OnboardingMember } from "./onboardingTypes";

interface Step4MembersProps {
  members: OnboardingMember[];
  memberName: string;
  memberType: string;
  memberIsManager: boolean;
  memberError: string | null;
  membersError: string | null;
  submitting: boolean;
  dots: React.ReactNode;
  back: React.ReactNode;
  onMemberNameChange: (v: string) => void;
  onMemberTypeChange: (v: string) => void;
  onMemberIsManagerChange: (v: boolean) => void;
  onAdd: () => void;
  onRemove: (i: number) => void;
  onComplete: () => void;
}

export function Step4Members({
  members, memberName, memberType, memberIsManager,
  memberError, membersError, submitting, dots, back,
  onMemberNameChange, onMemberTypeChange,
  onMemberIsManagerChange, onAdd, onRemove, onComplete,
}: Step4MembersProps) {
  const { t } = useTranslation("onboarding");

  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        {back}
        <div className="logo-wrap"><HouseholdLogo size={48} /></div>
        {dots}
        <p className="onboarding-step-label">{t("members.step")}</p>
        <h1>{t("members.title")}</h1>
        <p>{t("members.subtitle")}</p>

        {members.length > 0 && (
          <div className="people-chips">
            {members.map((m, i) => (
              <span key={i} className="people-chip">
                {m.name}
                <span style={{ fontSize: "0.75rem", opacity: 0.7, marginLeft: "0.1rem" }}>
                  ({t(`members.types.${m.type}` as never, m.type)}{m.manager ? ` · ${t("members.managerLabel")}` : ""})
                </span>
                <button type="button" onClick={() => onRemove(i)} aria-label={`Remove ${m.name}`}>×</button>
              </span>
            ))}
          </div>
        )}

        <div className="inline-form" style={{ marginBottom: "0.5rem" }}>
          <div className="form-group" style={{ flex: 2 }}>
            <input
              className="form-control"
              type="text"
              placeholder={t("members.namePlaceholder")}
              value={memberName}
              onChange={(e) => onMemberNameChange(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter") { e.preventDefault(); onAdd(); } }}
            />
          </div>
          <div className="form-group" style={{ flex: 1 }}>
            <select className="form-control" value={memberType} onChange={(e) => { onMemberTypeChange(e.target.value); if (e.target.value !== "adult") onMemberIsManagerChange(false); }}>
              <option value="adult">{t("members.types.adult")}</option>
              <option value="child">{t("members.types.child")}</option>
              <option value="pet">{t("members.types.pet")}</option>
            </select>
          </div>
          <button type="button" className="btn btn-ghost" onClick={onAdd} disabled={!memberName.trim()}>{t("members.add")}</button>
        </div>

        {memberType === "adult" && (
          <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", marginBottom: "1rem" }}>
            <input id="member-manager" type="checkbox" checked={memberIsManager} onChange={(e) => onMemberIsManagerChange(e.target.checked)} />
            <label htmlFor="member-manager" style={{ fontSize: "0.875rem" }}>{t("members.managerLabel")}</label>
          </div>
        )}

        {memberError && <p className="error-msg">{memberError}</p>}
        {membersError && <p className="error-msg">{membersError}</p>}

        <div style={{ display: "flex", gap: "0.5rem" }}>
          <button className="btn" style={{ flex: 1, justifyContent: "center" }} onClick={onComplete} disabled={submitting}>
            {submitting ? t("members.completing") : members.length > 0 ? t("members.complete") : t("members.skip")}
          </button>
        </div>
      </div>
    </div>
  );
}

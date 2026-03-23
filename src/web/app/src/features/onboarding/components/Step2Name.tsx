import { type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Step2NameProps {
  householdName: string;
  submitting: boolean;
  error: string | null;
  dots: React.ReactNode;
  back: React.ReactNode;
  onChange: (name: string) => void;
  onSubmit: (e: FormEvent) => void;
}

export function Step2Name({ householdName, submitting, error, dots, back, onChange, onSubmit }: Step2NameProps) {
  const { t } = useTranslation("onboarding");
  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        {back}
        <div className="logo-wrap"><HouseholdLogo size={48} /></div>
        {dots}
        <p className="onboarding-step-label">{t("name.step")}</p>
        <h1>{t("name.title")}</h1>
        <p>{t("name.subtitle")}</p>
        <form onSubmit={onSubmit}>
          <div className="form-group">
            <input className="form-control" type="text" placeholder={t("name.placeholder")} value={householdName} onChange={(e) => onChange(e.target.value)} required autoFocus />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn" style={{ width: "100%", justifyContent: "center" }} disabled={submitting || !householdName.trim()}>
            {submitting ? t("name.creating") : t("name.create")}
          </button>
        </form>
      </div>
    </div>
  );
}

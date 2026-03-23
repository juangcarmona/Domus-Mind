import { useTranslation } from "react-i18next";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Step3SelfProps {
  selfName: string;
  selfBirthDate: string;
  dots: React.ReactNode;
  back: React.ReactNode;
  onNameChange: (v: string) => void;
  onBirthDateChange: (v: string) => void;
  onNext: () => void;
}

export function Step3Self({ selfName, selfBirthDate, dots, back, onNameChange, onBirthDateChange, onNext }: Step3SelfProps) {
  const { t } = useTranslation("onboarding");
  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        {back}
        <div className="logo-wrap"><HouseholdLogo size={48} /></div>
        {dots}
        <p className="onboarding-step-label">{t("self.step")}</p>
        <h1>{t("self.title")}</h1>
        <p>{t("self.subtitle")}</p>
        <div className="form-group">
          <input className="form-control" type="text" placeholder={t("self.namePlaceholder")} value={selfName} onChange={(e) => onNameChange(e.target.value)} autoFocus />
        </div>
        <div className="form-group">
          <label style={{ display: "block", marginBottom: "0.25rem", fontSize: "0.875rem" }}>{t("self.birthdateLabel")}</label>
          <input className="form-control" type="date" value={selfBirthDate} onChange={(e) => onBirthDateChange(e.target.value)} />
        </div>
        <button className="btn" style={{ width: "100%", justifyContent: "center" }} disabled={!selfName.trim()} onClick={onNext}>
          {t("self.next")}
        </button>
      </div>
    </div>
  );
}

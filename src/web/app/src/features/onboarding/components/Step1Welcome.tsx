import { useTranslation } from "react-i18next";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Step1WelcomeProps {
  dots: React.ReactNode;
  back: React.ReactNode;
  onStart: () => void;
}

export function Step1Welcome({ dots, back, onStart }: Step1WelcomeProps) {
  const { t } = useTranslation("onboarding");
  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        {back}
        <div className="logo-wrap"><HouseholdLogo size={48} /></div>
        {dots}
        <h1>{t("welcome.title")}</h1>
        <p>{t("welcome.subtitle")}</p>
        <button className="btn" style={{ width: "100%", justifyContent: "center" }} onClick={onStart}>
          {t("welcome.start")}
        </button>
      </div>
    </div>
  );
}

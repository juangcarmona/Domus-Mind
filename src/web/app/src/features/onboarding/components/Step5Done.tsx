import { useTranslation } from "react-i18next";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

interface Step5DoneProps {
  familyName: string | undefined;
  dots: React.ReactNode;
  onFinish: () => void;
}

export function Step5Done({ familyName, dots, onFinish }: Step5DoneProps) {
  const { t } = useTranslation("onboarding");
  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        <div className="logo-wrap"><HouseholdLogo size={48} /></div>
        {dots}
        <h1>{t("done.title")}</h1>
        <p>{familyName} {t("done.subtitle")}</p>
        <button className="btn" style={{ width: "100%", justifyContent: "center" }} onClick={onFinish}>
          {t("done.open")}
        </button>
      </div>
    </div>
  );
}

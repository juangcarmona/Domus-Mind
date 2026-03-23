import { useTranslation } from "react-i18next";
import { HouseholdLogo } from "../../../components/HouseholdLogo";
import type { LanguageItem } from "./onboardingTypes";

interface Step0LanguageProps {
  selectedLang: string;
  languages: LanguageItem[];
  loading: boolean;
  dots: React.ReactNode;
  onSelect: (code: string) => void;
  onContinue: () => void;
}

const FALLBACK_LANGUAGES: LanguageItem[] = [
  { code: "en", displayName: "English", nativeDisplayName: "English" },
  { code: "de", displayName: "German", nativeDisplayName: "Deutsch" },
  { code: "es", displayName: "Spanish", nativeDisplayName: "Español" },
  { code: "fr", displayName: "French", nativeDisplayName: "Français" },
  { code: "it", displayName: "Italian", nativeDisplayName: "Italiano" },
  { code: "ja", displayName: "Japanese", nativeDisplayName: "日本語" },
  { code: "zh", displayName: "Chinese", nativeDisplayName: "中文" },
];

export function Step0Language({ selectedLang, languages, loading, dots, onSelect, onContinue }: Step0LanguageProps) {
  const { t } = useTranslation("lang");
  const { t: tCommon } = useTranslation("common");
  const items = languages.length > 0 ? languages : (!loading ? FALLBACK_LANGUAGES : []);

  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        <div className="logo-wrap"><HouseholdLogo size={48} /></div>
        {dots}
        <h1>{t("select")}</h1>
        <p>{t("subtitle")}</p>
        {loading && <p className="muted-text">{tCommon("loading")}</p>}
        {items.length > 0 && (
          <div className="lang-grid">
            {items.map((lang) => (
              <button
                key={lang.code}
                type="button"
                className={`lang-option${selectedLang === lang.code ? " selected" : ""}`}
                onClick={() => onSelect(lang.code)}
              >
                <span className="lang-native">{lang.nativeDisplayName}</span>
                <span className="lang-display">{lang.displayName}</span>
              </button>
            ))}
          </div>
        )}
        <button className="btn" style={{ width: "100%", justifyContent: "center", marginTop: "1rem" }} onClick={onContinue}>
          {t("continue")}
        </button>
      </div>
    </div>
  );
}

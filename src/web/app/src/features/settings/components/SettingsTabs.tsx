import { useTranslation } from "react-i18next";

export type SettingsTab = "household" | "account" | "about";

interface SettingsTabsProps {
  active: SettingsTab;
  onChange: (tab: SettingsTab) => void;
}

export function SettingsTabs({ active, onChange }: SettingsTabsProps) {
  const { t } = useTranslation("settings");
  return (
    <div className="settings-tabs" role="tablist">
      <button
        role="tab"
        aria-selected={active === "household"}
        className={`settings-tab${active === "household" ? " active" : ""}`}
        onClick={() => onChange("household")}
        type="button"
      >
        {t("household.title")}
      </button>
      <button
        role="tab"
        aria-selected={active === "account"}
        className={`settings-tab${active === "account" ? " active" : ""}`}
        onClick={() => onChange("account")}
        type="button"
      >
        {t("account.title")}
      </button>
      <button
        role="tab"
        aria-selected={active === "about"}
        className={`settings-tab${active === "about" ? " active" : ""}`}
        onClick={() => onChange("about")}
        type="button"
      >
        {t("about.title")}
      </button>
    </div>
  );
}

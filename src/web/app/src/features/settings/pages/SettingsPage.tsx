import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchSupportedLanguages } from "../../../store/languagesSlice";
import { SettingsTabs, type SettingsTab } from "../components/SettingsTabs";
import { AccountSettingsSection } from "../components/AccountSettingsSection";
import { HouseholdSettingsSection } from "../components/HouseholdSettingsSection";
import { MembersSettingsSection } from "../components/MembersSettingsSection";
import { AboutSection } from "../components/AboutSection";
import { ConnectedCalendarsSection } from "../components/ConnectedCalendarsSection";
import { AreasPage } from "../../areas/pages/AreasPage";

export function SettingsPage({ defaultTab = "household" }: { defaultTab?: SettingsTab }) {
  const { t } = useTranslation("settings");
  const dispatch = useAppDispatch();
  const languagesStatus = useAppSelector((s) => s.languages.status);
  const [activeTab, setActiveTab] = useState<SettingsTab>(defaultTab);

  useEffect(() => {
    if (languagesStatus === "idle") {
      dispatch(fetchSupportedLanguages());
    }
  }, [dispatch, languagesStatus]);

  return (
    <div className="page-wrap">
      <h1 className="page-title">{t("title")}</h1>
      <SettingsTabs active={activeTab} onChange={setActiveTab} />
      <div role="tabpanel">
        {activeTab === "household" && <HouseholdSettingsSection />}
        {activeTab === "people" && <MembersSettingsSection />}
        {activeTab === "areas" && <AreasPage />}
        {activeTab === "account" && <AccountSettingsSection />}
        {activeTab === "connectedCalendars" && <ConnectedCalendarsSection />}
        {activeTab === "about" && <AboutSection />}
      </div>
    </div>
  );
}

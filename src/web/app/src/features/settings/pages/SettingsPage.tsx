import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchSupportedLanguages } from "../../../store/languagesSlice";
import { SettingsTabs, type SettingsTab } from "../components/SettingsTabs";
import { AccountSettingsSection } from "../components/AccountSettingsSection";
import { HouseholdSettingsSection } from "../components/HouseholdSettingsSection";

export function SettingsPage() {
  const { t } = useTranslation("settings");
  const dispatch = useAppDispatch();
  const languagesStatus = useAppSelector((s) => s.languages.status);
  const [activeTab, setActiveTab] = useState<SettingsTab>("account");

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
        {activeTab === "account" ? <AccountSettingsSection /> : <HouseholdSettingsSection />}
      </div>
    </div>
  );
}

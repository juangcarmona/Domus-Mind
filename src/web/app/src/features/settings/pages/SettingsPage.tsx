import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchSupportedLanguages } from "../../../store/languagesSlice";
import { useAuth } from "../../../auth/AuthProvider";
import { SettingsTabs, type SettingsTab } from "../components/SettingsTabs";
import { AccountSettingsSection } from "../components/AccountSettingsSection";
import { HouseholdSettingsSection } from "../components/HouseholdSettingsSection";

export function SettingsPage() {
  const { t } = useTranslation("settings");
  const dispatch = useAppDispatch();
  const languagesStatus = useAppSelector((s) => s.languages.status);
  const [activeTab, setActiveTab] = useState<SettingsTab>("household");
  const { user } = useAuth();
  const navigate = useNavigate();

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
        {activeTab === "account" && <AccountSettingsSection />}
      </div>
      {user?.isOperator && (
        <div style={{ marginTop: "2rem", paddingTop: "1.5rem", borderTop: "1px solid var(--border, #e0e0e0)" }}>
          <p style={{ fontSize: "0.75rem", color: "var(--muted, #888)", marginBottom: "0.5rem", textTransform: "uppercase", letterSpacing: "0.05em" }}>
            Platform
          </p>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => void navigate("/admin")}
          >
            Admin area
          </button>
        </div>
      )}
    </div>
  );
}

import { useTranslation } from "react-i18next";
import {
  APP_VERSION,
  APP_RELEASE_DATE,
  APP_COMMIT_SHA,
  APP_IS_PRERELEASE,
} from "../../../generated/version";

export function AboutSection() {
  const { t } = useTranslation("settings");

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">{t("about.title")}</h2>
      <div className="settings-field-group">
        <div className="settings-field">
          <span className="settings-field-label">{t("about.version")}</span>
          <span className="settings-field-value">{APP_VERSION}</span>
        </div>
        <div className="settings-field">
          <span className="settings-field-label">{t("about.releaseDate")}</span>
          <span className="settings-field-value">{APP_RELEASE_DATE}</span>
        </div>
        <div className="settings-field">
          <span className="settings-field-label">{t("about.commit")}</span>
          <span className="settings-field-value">{APP_COMMIT_SHA}</span>
        </div>
        <div className="settings-field">
          <span className="settings-field-label">{t("about.channel")}</span>
          <span className="settings-field-value">
            {APP_IS_PRERELEASE ? t("about.prerelease") : t("about.stable")}
          </span>
        </div>
      </div>
    </section>
  );
}

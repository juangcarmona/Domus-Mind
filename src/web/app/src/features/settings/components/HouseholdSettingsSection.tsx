import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { updateHouseholdSettings } from "../../../store/householdSlice";
import { setUiLanguage } from "../../../i18n/index";
import { FIRST_DAY_OPTIONS, DATE_FORMAT_OPTIONS } from "../types";

export function HouseholdSettingsSection() {
  const { t } = useTranslation("settings");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const family = useAppSelector((s) => s.household.family);
  const allLanguages = useAppSelector((s) => s.languages.items);

  const [name, setName] = useState(family?.name ?? "");
  const [languageCode, setLanguageCode] = useState(family?.primaryLanguageCode ?? "");
  const [firstDayOfWeek, setFirstDayOfWeek] = useState(family?.firstDayOfWeek ?? "");
  const [dateFormat, setDateFormat] = useState(family?.dateFormatPreference ?? "");

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  if (!family) return null;

  async function handleSave(e: FormEvent) {
    e.preventDefault();
    if (!family) return;
    setError(null);
    setSuccess(null);
    setSaving(true);

    const result = await dispatch(
      updateHouseholdSettings({
        familyId: family.familyId,
        name: name.trim(),
        primaryLanguageCode: languageCode || null,
        firstDayOfWeek: firstDayOfWeek || null,
        dateFormatPreference: dateFormat || null,
      }),
    );

    setSaving(false);

    if (updateHouseholdSettings.fulfilled.match(result)) {
      setSuccess(t("household.saved"));
      // Immediately apply language change in the UI
      if (languageCode) {
        setUiLanguage(languageCode);
      }
    } else {
      setError((result.payload as string) ?? tCommon("failed"));
    }
  }

  return (
    <>
      <section className="settings-section">
        <h2 className="settings-section-title">{t("household.title")}</h2>
        <form onSubmit={handleSave} className="settings-form">
          <div className="form-group">
            <label htmlFor="household-name">{t("household.name")}</label>
            <input
              id="household-name"
              type="text"
              className="form-control"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              maxLength={100}
            />
          </div>

          <div className="form-group">
            <label htmlFor="household-lang">{t("household.language")}</label>
            <select
              id="household-lang"
              className="form-control"
              value={languageCode}
              onChange={(e) => setLanguageCode(e.target.value)}
            >
              <option value="">—</option>
              {allLanguages.map((l) => (
                <option key={l.code} value={l.code}>
                  {l.nativeDisplayName} ({l.code})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="first-day">{t("household.firstDayOfWeek")}</label>
            <select
              id="first-day"
              className="form-control"
              value={firstDayOfWeek}
              onChange={(e) => setFirstDayOfWeek(e.target.value)}
            >
              <option value="">—</option>
              {FIRST_DAY_OPTIONS.map((d) => (
                <option key={d} value={d}>
                  {t(`household.days.${d}` as never)}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="date-format">{t("household.dateFormat")}</label>
            <select
              id="date-format"
              className="form-control"
              value={dateFormat}
              onChange={(e) => setDateFormat(e.target.value)}
            >
              <option value="">—</option>
              {DATE_FORMAT_OPTIONS.map((f) => (
                <option key={f} value={f}>
                  {f}
                </option>
              ))}
            </select>
          </div>

          {error && <p className="error-msg">{error}</p>}
          {success && <p className="success-msg">{success}</p>}

          <button type="submit" className="btn" disabled={saving || !name.trim()}>
            {saving ? t("household.saving") : t("household.save")}
          </button>
        </form>
      </section>
    </>
  );
}

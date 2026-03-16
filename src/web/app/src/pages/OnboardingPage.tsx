import { useState, useEffect, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { createFamily, addMember } from "../store/householdSlice";
import { fetchSupportedLanguages } from "../store/languagesSlice";
import { setUiLanguage } from "../i18n/index";
import { HouseholdLogo } from "../components/HouseholdLogo";

// Step 0: language selection
// Step 1: welcome
// Step 2: name household
// Step 3: add people
// Step 4: done
type Step = 0 | 1 | 2 | 3 | 4;

const STEP_COUNT = 5;

export function OnboardingPage() {
  const dispatch = useAppDispatch();
  const nav = useNavigate();
  const { t, i18n } = useTranslation();
  const household = useAppSelector((s) => s.household);
  const languages = useAppSelector((s) => s.languages);

  const [step, setStep] = useState<Step>(0);
  const [selectedLang, setSelectedLang] = useState<string>(
    i18n.language?.split("-")[0] ?? "en",
  );
  const [householdName, setHouseholdName] = useState("");
  const [people, setPeople] = useState<{ name: string; role: string }[]>([]);
  const [personName, setPersonName] = useState("");
  const [personRole, setPersonRole] = useState("Adult");
  const [addError, setAddError] = useState<string | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const familyId = household.family?.familyId;

  // Fetch available languages for step 0
  useEffect(() => {
    if (languages.status === "idle") {
      dispatch(fetchSupportedLanguages());
    }
  }, [dispatch, languages.status]);

  function handleLangSelect(code: string) {
    setSelectedLang(code);
    setUiLanguage(code);
  }

  function handleLangContinue() {
    setStep(1);
  }

  async function handleCreateHousehold(e: FormEvent) {
    e.preventDefault();
    if (!householdName.trim()) return;
    setSubmitting(true);
    setCreateError(null);
    const result = await dispatch(
      createFamily({
        name: householdName.trim(),
        primaryLanguageCode: selectedLang,
      }),
    );
    setSubmitting(false);
    if (createFamily.fulfilled.match(result)) {
      setStep(3);
    } else {
      setCreateError((result.payload as string) ?? t("common.error"));
    }
  }

  function handleAddPerson() {
    const name = personName.trim();
    if (!name) return;
    setPeople((prev) => [...prev, { name, role: personRole }]);
    setPersonName("");
    setAddError(null);
  }

  function handleRemovePerson(i: number) {
    setPeople((prev) => prev.filter((_, idx) => idx !== i));
  }

  async function handleSavePeople() {
    if (!familyId) return;
    setSubmitting(true);
    for (const p of people) {
      await dispatch(addMember({ familyId, name: p.name, role: p.role }));
    }
    setSubmitting(false);
    setStep(4);
  }

  function handleFinish() {
    nav("/timeline");
  }

  function renderDots() {
    return (
      <div className="progress-dots">
        {Array.from({ length: STEP_COUNT }).map((_, i) => (
          <div
            key={i}
            className={`progress-dot ${i < step ? "done" : i === step ? "active" : ""}`}
          />
        ))}
      </div>
    );
  }

  function handleBack() {
    setStep((prev) => (prev > 0 ? ((prev - 1) as Step) : prev));
  }

  function renderBackButton(currentStep: Step) {
    if (currentStep <= 0 || currentStep >= 4) return null;

    return (
      <button
        type="button"
        className="onboarding-back"
        onClick={handleBack}
        aria-label={t("common.back")}
        title={t("common.back")}
      >
                  ❮
              
      </button>
    );
  }

  /* ---- Step 0: Language selection ---- */
  if (step === 0) {
    return (
      <div className="onboarding-wrap">
        <div className="onboarding-card">
          {renderBackButton(step)}
          <div className="logo-wrap">
            <HouseholdLogo size={48} />
          </div>
          {renderDots()}
          <h1>{t("lang.select")}</h1>
          <p>{t("lang.subtitle")}</p>

          {languages.status === "loading" && (
            <p className="muted-text">{t("common.loading")}</p>
          )}

          {languages.items.length > 0 && (
            <div className="lang-grid">
              {languages.items.map((lang) => (
                <button
                  key={lang.code}
                  type="button"
                  className={`lang-option${selectedLang === lang.code ? " selected" : ""}`}
                  onClick={() => handleLangSelect(lang.code)}
                >
                  <span className="lang-native">{lang.nativeDisplayName}</span>
                  <span className="lang-display">{lang.displayName}</span>
                </button>
              ))}
            </div>
          )}

          {/* Fallback: show static list if backend unavailable */}
          {languages.status !== "loading" && languages.items.length === 0 && (
            <div className="lang-grid">
              {[
                {
                  code: "en",
                  displayName: "English",
                  nativeDisplayName: "English",
                },
                {
                  code: "de",
                  displayName: "German",
                  nativeDisplayName: "Deutsch",
                },
                {
                  code: "es",
                  displayName: "Spanish",
                  nativeDisplayName: "Español",
                },
                {
                  code: "fr",
                  displayName: "French",
                  nativeDisplayName: "Français",
                },
                {
                  code: "it",
                  displayName: "Italian",
                  nativeDisplayName: "Italiano",
                },
                {
                  code: "ja",
                  displayName: "Japanese",
                  nativeDisplayName: "日本語",
                },
                {
                  code: "zh",
                  displayName: "Chinese",
                  nativeDisplayName: "中文",
                },
              ].map((lang) => (
                <button
                  key={lang.code}
                  type="button"
                  className={`lang-option${selectedLang === lang.code ? " selected" : ""}`}
                  onClick={() => handleLangSelect(lang.code)}
                >
                  <span className="lang-native">{lang.nativeDisplayName}</span>
                  <span className="lang-display">{lang.displayName}</span>
                </button>
              ))}
            </div>
          )}

          <button
            className="btn"
            style={{
              width: "100%",
              justifyContent: "center",
              marginTop: "1rem",
            }}
            onClick={handleLangContinue}
          >
            {t("lang.continue")}
          </button>
        </div>
      </div>
    );
  }

  /* ---- Step 1: Welcome ---- */
  if (step === 1) {
    return (
      <div className="onboarding-wrap">
        <div className="onboarding-card">
          {renderBackButton(step)}
          <div className="logo-wrap">
            <HouseholdLogo size={48} />
          </div>
          {renderDots()}
          <h1>{t("onboarding.welcome.title")}</h1>
          <p>{t("onboarding.welcome.subtitle")}</p>
          <button
            className="btn"
            style={{ width: "100%", justifyContent: "center" }}
            onClick={() => setStep(2)}
          >
            {t("onboarding.welcome.start")}
          </button>
        </div>
      </div>
    );
  }

  /* ---- Step 2: Name household ---- */
  if (step === 2) {
    return (
      <div className="onboarding-wrap">
        <div className="onboarding-card">
          {renderBackButton(step)}
          <div className="logo-wrap">
            <HouseholdLogo size={48} />
          </div>
          {renderDots()}
          <p className="onboarding-step-label">{t("onboarding.name.step")}</p>
          <h1>{t("onboarding.name.title")}</h1>
          <p>{t("onboarding.name.subtitle")}</p>
          <form onSubmit={handleCreateHousehold}>
            <div className="form-group">
              <input
                className="form-control"
                type="text"
                placeholder={t("onboarding.name.placeholder")}
                value={householdName}
                onChange={(e) => setHouseholdName(e.target.value)}
                required
                autoFocus
              />
            </div>
            {createError && <p className="error-msg">{createError}</p>}
            <button
              type="submit"
              className="btn"
              style={{ width: "100%", justifyContent: "center" }}
              disabled={submitting || !householdName.trim()}
            >
              {submitting
                ? t("onboarding.name.creating")
                : t("onboarding.name.create")}
            </button>
          </form>
        </div>
      </div>
    );
  }

  /* ---- Step 3: Add people ---- */
  if (step === 3) {
    return (
      <div className="onboarding-wrap">
        <div className="onboarding-card">
          {renderBackButton(step)}
          <div className="logo-wrap">
            <HouseholdLogo size={48} />
          </div>
          {renderDots()}
          <p className="onboarding-step-label">{t("onboarding.people.step")}</p>
          <h1>{t("onboarding.people.title")}</h1>
          <p>{t("onboarding.people.subtitle")}</p>

          {people.length > 0 && (
            <div className="people-chips">
              {people.map((p, i) => (
                <span key={i} className="people-chip">
                  {p.name}
                  <span
                    style={{
                      fontSize: "0.75rem",
                      opacity: 0.7,
                      marginLeft: "0.1rem",
                    }}
                  >
                    ({t(`onboarding.people.roles.${p.role}` as never, p.role)})
                  </span>
                  <button
                    type="button"
                    onClick={() => handleRemovePerson(i)}
                    aria-label={`Remove ${p.name}`}
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          )}

          <div className="inline-form" style={{ marginBottom: "1rem" }}>
            <div className="form-group" style={{ flex: 2 }}>
              <input
                className="form-control"
                type="text"
                placeholder={t("onboarding.people.namePlaceholder")}
                value={personName}
                onChange={(e) => setPersonName(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    handleAddPerson();
                  }
                }}
              />
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <select
                className="form-control"
                value={personRole}
                onChange={(e) => setPersonRole(e.target.value)}
              >
                <option value="Adult">
                  {t("onboarding.people.roles.Adult")}
                </option>
                <option value="Child">
                  {t("onboarding.people.roles.Child")}
                </option>
                <option value="Teen">
                  {t("onboarding.people.roles.Teen")}
                </option>
              </select>
            </div>
            <button
              type="button"
              className="btn btn-ghost"
              onClick={handleAddPerson}
              disabled={!personName.trim()}
            >
              {t("onboarding.people.add")}
            </button>
          </div>

          {addError && <p className="error-msg">{addError}</p>}

          <div style={{ display: "flex", gap: "0.5rem" }}>
            <button
              className="btn"
              style={{ flex: 1, justifyContent: "center" }}
              onClick={handleSavePeople}
              disabled={submitting}
            >
              {submitting
                ? t("onboarding.people.saving")
                : people.length > 0
                  ? t("onboarding.people.save")
                  : t("onboarding.people.skip")}
            </button>
          </div>
        </div>
      </div>
    );
  }

  /* ---- Step 4: Done ---- */
  return (
    <div className="onboarding-wrap">
      <div className="onboarding-card">
        <div className="logo-wrap">
          <HouseholdLogo size={48} />
        </div>
        {renderDots()}
        <h1>{t("onboarding.done.title")}</h1>
        <p>
          {household.family?.name} {t("onboarding.done.subtitle")}
        </p>
        <button
          className="btn"
          style={{ width: "100%", justifyContent: "center" }}
          onClick={handleFinish}
        >
          {t("onboarding.done.open")}
        </button>
      </div>
    </div>
  );
}

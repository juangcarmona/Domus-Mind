import { useState, useEffect, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { createFamily, completeOnboarding } from "../../../store/householdSlice";
import { fetchSupportedLanguages } from "../../../store/languagesSlice";
import { setUiLanguage } from "../../../i18n/index";
import { HouseholdLogo } from "../../../components/HouseholdLogo";

// Step 0: language selection
// Step 1: welcome
// Step 2: name household
// Step 3: who are you?
// Step 4: add household members
// Step 5: done
type Step = 0 | 1 | 2 | 3 | 4 | 5;

const STEP_COUNT = 6;

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
  const [selfName, setSelfName] = useState("");
  const [selfBirthDate, setSelfBirthDate] = useState("");
  const [members, setMembers] = useState<
    { name: string; birthDate: string; type: string; manager: boolean }[]
  >([]);
  const [memberName, setMemberName] = useState("");
  const [memberBirthDate, setMemberBirthDate] = useState("");
  const [memberType, setMemberType] = useState("adult");
  const [memberIsManager, setMemberIsManager] = useState(false);
  const [memberError, setMemberError] = useState<string | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);
  const [membersError, setMembersError] = useState<string | null>(null);
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

  function handleAddMember() {
    const name = memberName.trim();
    if (!name) return;
    if (memberType !== "adult" && memberIsManager) {
      setMemberError(t("onboarding.members.managerAdultOnly"));
      return;
    }
    setMembers((prev) => [
      ...prev,
      { name, birthDate: memberBirthDate, type: memberType, manager: memberIsManager },
    ]);
    setMemberName("");
    setMemberBirthDate("");
    setMemberIsManager(false);
    setMemberError(null);
  }

  function handleRemoveMember(i: number) {
    setMembers((prev) => prev.filter((_, idx) => idx !== i));
  }

  async function handleCompleteOnboarding() {
    if (!familyId || !selfName.trim()) return;
    setSubmitting(true);
    setMembersError(null);
    const result = await dispatch(
      completeOnboarding({
        familyId,
        selfName: selfName.trim(),
        selfBirthDate: selfBirthDate || null,
        additionalMembers: members.map((m) => ({
          name: m.name,
          birthDate: m.birthDate || null,
          type: m.type,
          manager: m.manager,
        })),
      }),
    );
    setSubmitting(false);
    if (completeOnboarding.fulfilled.match(result)) {
      setStep(5);
    } else {
      setMembersError((result.payload as string) ?? t("common.error"));
    }
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
    if (currentStep <= 0 || currentStep >= 5) return null;

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

  /* ---- Step 3: Who are you? ---- */
  if (step === 3) {
    return (
      <div className="onboarding-wrap">
        <div className="onboarding-card">
          {renderBackButton(step)}
          <div className="logo-wrap">
            <HouseholdLogo size={48} />
          </div>
          {renderDots()}
          <p className="onboarding-step-label">{t("onboarding.self.step")}</p>
          <h1>{t("onboarding.self.title")}</h1>
          <p>{t("onboarding.self.subtitle")}</p>
          <div className="form-group">
            <input
              className="form-control"
              type="text"
              placeholder={t("onboarding.self.namePlaceholder")}
              value={selfName}
              onChange={(e) => setSelfName(e.target.value)}
              autoFocus
            />
          </div>
          <div className="form-group">
            <label style={{ display: "block", marginBottom: "0.25rem", fontSize: "0.875rem" }}>
              {t("onboarding.self.birthdateLabel")}
            </label>
            <input
              className="form-control"
              type="date"
              value={selfBirthDate}
              onChange={(e) => setSelfBirthDate(e.target.value)}
            />
          </div>
          <button
            className="btn"
            style={{ width: "100%", justifyContent: "center" }}
            disabled={!selfName.trim()}
            onClick={() => setStep(4)}
          >
            {t("onboarding.self.next")}
          </button>
        </div>
      </div>
    );
  }

  /* ---- Step 4: Add household members ---- */
  if (step === 4) {
    return (
      <div className="onboarding-wrap">
        <div className="onboarding-card">
          {renderBackButton(step)}
          <div className="logo-wrap">
            <HouseholdLogo size={48} />
          </div>
          {renderDots()}
          <p className="onboarding-step-label">{t("onboarding.members.step")}</p>
          <h1>{t("onboarding.members.title")}</h1>
          <p>{t("onboarding.members.subtitle")}</p>

          {members.length > 0 && (
            <div className="people-chips">
              {members.map((m, i) => (
                <span key={i} className="people-chip">
                  {m.name}
                  <span style={{ fontSize: "0.75rem", opacity: 0.7, marginLeft: "0.1rem" }}>
                    ({t(`onboarding.members.types.${m.type}` as never, m.type)}
                    {m.manager ? ` · ${t("onboarding.members.managerLabel")}` : ""})
                  </span>
                  <button
                    type="button"
                    onClick={() => handleRemoveMember(i)}
                    aria-label={`Remove ${m.name}`}
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          )}

          <div className="inline-form" style={{ marginBottom: "0.5rem" }}>
            <div className="form-group" style={{ flex: 2 }}>
              <input
                className="form-control"
                type="text"
                placeholder={t("onboarding.members.namePlaceholder")}
                value={memberName}
                onChange={(e) => setMemberName(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    handleAddMember();
                  }
                }}
              />
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <select
                className="form-control"
                value={memberType}
                onChange={(e) => {
                  setMemberType(e.target.value);
                  if (e.target.value !== "adult") setMemberIsManager(false);
                }}
              >
                <option value="adult">{t("onboarding.members.types.adult")}</option>
                <option value="child">{t("onboarding.members.types.child")}</option>
                <option value="pet">{t("onboarding.members.types.pet")}</option>
              </select>
            </div>
            <button
              type="button"
              className="btn btn-ghost"
              onClick={handleAddMember}
              disabled={!memberName.trim()}
            >
              {t("onboarding.members.add")}
            </button>
          </div>

          {memberType === "adult" && (
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "0.5rem",
                marginBottom: "1rem",
              }}
            >
              <input
                id="member-manager"
                type="checkbox"
                checked={memberIsManager}
                onChange={(e) => setMemberIsManager(e.target.checked)}
              />
              <label htmlFor="member-manager" style={{ fontSize: "0.875rem" }}>
                {t("onboarding.members.managerLabel")}
              </label>
            </div>
          )}

          {memberError && <p className="error-msg">{memberError}</p>}
          {membersError && <p className="error-msg">{membersError}</p>}

          <div style={{ display: "flex", gap: "0.5rem" }}>
            <button
              className="btn"
              style={{ flex: 1, justifyContent: "center" }}
              onClick={handleCompleteOnboarding}
              disabled={submitting}
            >
              {submitting
                ? t("onboarding.members.completing")
                : members.length > 0
                  ? t("onboarding.members.complete")
                  : t("onboarding.members.skip")}
            </button>
          </div>
        </div>
      </div>
    );
  }

  /* ---- Step 5: Done ---- */
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

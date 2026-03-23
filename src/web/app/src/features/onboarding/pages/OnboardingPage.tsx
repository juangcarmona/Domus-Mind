import { useState, useEffect, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { createFamily, completeOnboarding } from "../../../store/householdSlice";
import { fetchSupportedLanguages } from "../../../store/languagesSlice";
import { setUiLanguage } from "../../../store/uiSlice";
import { useTranslation } from "react-i18next";
import { Step0Language } from "../components/Step0Language";
import { Step1Welcome } from "../components/Step1Welcome";
import { Step2Name } from "../components/Step2Name";
import { Step3Self } from "../components/Step3Self";
import { Step4Members } from "../components/Step4Members";
import { Step5Done } from "../components/Step5Done";
import type { OnboardingMember } from "../components/onboardingTypes";

type Step = 0 | 1 | 2 | 3 | 4 | 5;
const STEP_COUNT = 6;

export function OnboardingPage() {
  const dispatch = useAppDispatch();
  const nav = useNavigate();
  const { i18n } = useTranslation();
  const { t: tCommon } = useTranslation("common");
  const household = useAppSelector((s) => s.household);
  const languages = useAppSelector((s) => s.languages);

  const [step, setStep] = useState<Step>(0);
  const [selectedLang, setSelectedLang] = useState<string>(i18n.language?.split("-")[0] ?? "en");
  const [householdName, setHouseholdName] = useState("");
  const [selfName, setSelfName] = useState("");
  const [selfBirthDate, setSelfBirthDate] = useState("");
  const [members, setMembers] = useState<OnboardingMember[]>([]);
  const [memberName, setMemberName] = useState("");
  const [memberBirthDate, setMemberBirthDate] = useState("");
  const [memberType, setMemberType] = useState("adult");
  const [memberIsManager, setMemberIsManager] = useState(false);
  const [memberError, setMemberError] = useState<string | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);
  const [membersError, setMembersError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const familyId = household.family?.familyId;
  const { t: tOnboarding } = useTranslation("onboarding");

  useEffect(() => {
    if (languages.status === "idle") dispatch(fetchSupportedLanguages());
  }, [dispatch, languages.status]);

  function handleLangSelect(code: string) {
    setSelectedLang(code);
    dispatch(setUiLanguage(code));
  }

  async function handleCreateHousehold(e: FormEvent) {
    e.preventDefault();
    if (!householdName.trim()) return;
    setSubmitting(true); setCreateError(null);
    const result = await dispatch(createFamily({ name: householdName.trim(), primaryLanguageCode: selectedLang }));
    setSubmitting(false);
    if (createFamily.fulfilled.match(result)) { setStep(3); }
    else { setCreateError((result.payload as string) ?? tCommon("error")); }
  }

  function handleAddMember() {
    const name = memberName.trim();
    if (!name) return;
    if (memberType !== "adult" && memberIsManager) { setMemberError(tOnboarding("members.managerAdultOnly")); return; }
    setMembers((prev) => [...prev, { name, birthDate: memberBirthDate, type: memberType, manager: memberIsManager }]);
    setMemberName(""); setMemberBirthDate(""); setMemberIsManager(false); setMemberError(null);
  }

  async function handleCompleteOnboarding() {
    if (!familyId || !selfName.trim()) return;
    setSubmitting(true); setMembersError(null);
    const result = await dispatch(completeOnboarding({
      familyId, selfName: selfName.trim(), selfBirthDate: selfBirthDate || null,
      additionalMembers: members.map((m) => ({ name: m.name, birthDate: m.birthDate || null, type: m.type, manager: m.manager })),
    }));
    setSubmitting(false);
    if (completeOnboarding.fulfilled.match(result)) { setStep(5); }
    else { setMembersError((result.payload as string) ?? tCommon("error")); }
  }

  const dots = (
    <div className="progress-dots">
      {Array.from({ length: STEP_COUNT }).map((_, i) => (
        <div key={i} className={`progress-dot ${i < step ? "done" : i === step ? "active" : ""}`} />
      ))}
    </div>
  );

  const back = step > 0 && step < 5 ? (
    <button type="button" className="onboarding-back" onClick={() => setStep((prev) => (prev > 0 ? prev - 1 as Step : prev))} aria-label={tCommon("back")}>❮</button>
  ) : null;

  if (step === 0) return <Step0Language selectedLang={selectedLang} languages={languages.items} loading={languages.status === "loading"} dots={dots} onSelect={handleLangSelect} onContinue={() => setStep(1)} />;
  if (step === 1) return <Step1Welcome dots={dots} back={back} onStart={() => setStep(2)} />;
  if (step === 2) return <Step2Name householdName={householdName} submitting={submitting} error={createError} dots={dots} back={back} onChange={setHouseholdName} onSubmit={handleCreateHousehold} />;
  if (step === 3) return <Step3Self selfName={selfName} selfBirthDate={selfBirthDate} dots={dots} back={back} onNameChange={setSelfName} onBirthDateChange={setSelfBirthDate} onNext={() => setStep(4)} />;
  if (step === 4) return (
    <Step4Members
      members={members} memberName={memberName}
      memberType={memberType} memberIsManager={memberIsManager} memberError={memberError}
      membersError={membersError} submitting={submitting} dots={dots} back={back}
      onMemberNameChange={setMemberName}
      onMemberTypeChange={setMemberType} onMemberIsManagerChange={setMemberIsManager}
      onAdd={handleAddMember} onRemove={(i) => setMembers((prev) => prev.filter((_, idx) => idx !== i))}
      onComplete={handleCompleteOnboarding}
    />
  );
  return <Step5Done familyName={household.family?.name} dots={dots} onFinish={() => nav("/timeline")} />;
}

import i18n from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

// English
import enAuth from "./locales/en/auth";
import enCommon from "./locales/en/common";
import enLang from "./locales/en/lang";
import enNav from "./locales/en/nav";
import enOnboarding from "./locales/en/onboarding";
import enTimeline from "./locales/en/timeline";
import enPeople from "./locales/en/people";
import enAreas from "./locales/en/areas";
import enPlans from "./locales/en/plans";
import enTasks from "./locales/en/tasks";
import enRoutines from "./locales/en/routines";
import enSettings from "./locales/en/settings";
import enWeek from "./locales/en/week";
import enCoordination from "./locales/en/coordination";

// German
import deAuth from "./locales/de/auth";
import deCommon from "./locales/de/common";
import deLang from "./locales/de/lang";
import deNav from "./locales/de/nav";
import deOnboarding from "./locales/de/onboarding";
import deTimeline from "./locales/de/timeline";
import dePeople from "./locales/de/people";
import deAreas from "./locales/de/areas";
import dePlans from "./locales/de/plans";
import deTasks from "./locales/de/tasks";
import deRoutines from "./locales/de/routines";
import deSettings from "./locales/de/settings";
import deWeek from "./locales/de/week";
import deCoordination from "./locales/de/coordination";

// Spanish
import esAuth from "./locales/es/auth";
import esCommon from "./locales/es/common";
import esLang from "./locales/es/lang";
import esNav from "./locales/es/nav";
import esOnboarding from "./locales/es/onboarding";
import esTimeline from "./locales/es/timeline";
import esPeople from "./locales/es/people";
import esAreas from "./locales/es/areas";
import esPlans from "./locales/es/plans";
import esTasks from "./locales/es/tasks";
import esRoutines from "./locales/es/routines";
import esSettings from "./locales/es/settings";
import esWeek from "./locales/es/week";
import esCoordination from "./locales/es/coordination";

// French
import frAuth from "./locales/fr/auth";
import frCommon from "./locales/fr/common";
import frLang from "./locales/fr/lang";
import frNav from "./locales/fr/nav";
import frOnboarding from "./locales/fr/onboarding";
import frTimeline from "./locales/fr/timeline";
import frPeople from "./locales/fr/people";
import frAreas from "./locales/fr/areas";
import frPlans from "./locales/fr/plans";
import frTasks from "./locales/fr/tasks";
import frRoutines from "./locales/fr/routines";
import frSettings from "./locales/fr/settings";
import frWeek from "./locales/fr/week";
import frCoordination from "./locales/fr/coordination";

// Italian
import itAuth from "./locales/it/auth";
import itCommon from "./locales/it/common";
import itLang from "./locales/it/lang";
import itNav from "./locales/it/nav";
import itOnboarding from "./locales/it/onboarding";
import itTimeline from "./locales/it/timeline";
import itPeople from "./locales/it/people";
import itAreas from "./locales/it/areas";
import itPlans from "./locales/it/plans";
import itTasks from "./locales/it/tasks";
import itRoutines from "./locales/it/routines";
import itSettings from "./locales/it/settings";
import itWeek from "./locales/it/week";
import itCoordination from "./locales/it/coordination";

// Japanese
import jaAuth from "./locales/ja/auth";
import jaCommon from "./locales/ja/common";
import jaLang from "./locales/ja/lang";
import jaNav from "./locales/ja/nav";
import jaOnboarding from "./locales/ja/onboarding";
import jaTimeline from "./locales/ja/timeline";
import jaPeople from "./locales/ja/people";
import jaAreas from "./locales/ja/areas";
import jaPlans from "./locales/ja/plans";
import jaTasks from "./locales/ja/tasks";
import jaRoutines from "./locales/ja/routines";
import jaSettings from "./locales/ja/settings";
import jaWeek from "./locales/ja/week";
import jaCoordination from "./locales/ja/coordination";

// Chinese
import zhAuth from "./locales/zh/auth";
import zhCommon from "./locales/zh/common";
import zhLang from "./locales/zh/lang";
import zhNav from "./locales/zh/nav";
import zhOnboarding from "./locales/zh/onboarding";
import zhTimeline from "./locales/zh/timeline";
import zhPeople from "./locales/zh/people";
import zhAreas from "./locales/zh/areas";
import zhPlans from "./locales/zh/plans";
import zhTasks from "./locales/zh/tasks";
import zhRoutines from "./locales/zh/routines";
import zhSettings from "./locales/zh/settings";
import zhWeek from "./locales/zh/week";
import zhCoordination from "./locales/zh/coordination";

// The explicit UI language choice key stored in localStorage.
export const UI_LANG_KEY = "dm_ui_lang";

export const SUPPORTED_LANG_CODES = ["en", "de", "es", "fr", "it", "ja", "zh"] as const;
export type SupportedLangCode = (typeof SUPPORTED_LANG_CODES)[number];

const resources = {
  en: { auth: enAuth, common: enCommon, lang: enLang, nav: enNav, onboarding: enOnboarding, timeline: enTimeline, people: enPeople, areas: enAreas, plans: enPlans, tasks: enTasks, routines: enRoutines, settings: enSettings, week: enWeek, coordination: enCoordination },
  de: { auth: deAuth, common: deCommon, lang: deLang, nav: deNav, onboarding: deOnboarding, timeline: deTimeline, people: dePeople, areas: deAreas, plans: dePlans, tasks: deTasks, routines: deRoutines, settings: deSettings, week: deWeek, coordination: deCoordination },
  es: { auth: esAuth, common: esCommon, lang: esLang, nav: esNav, onboarding: esOnboarding, timeline: esTimeline, people: esPeople, areas: esAreas, plans: esPlans, tasks: esTasks, routines: esRoutines, settings: esSettings, week: esWeek, coordination: esCoordination },
  fr: { auth: frAuth, common: frCommon, lang: frLang, nav: frNav, onboarding: frOnboarding, timeline: frTimeline, people: frPeople, areas: frAreas, plans: frPlans, tasks: frTasks, routines: frRoutines, settings: frSettings, week: frWeek, coordination: frCoordination },
  it: { auth: itAuth, common: itCommon, lang: itLang, nav: itNav, onboarding: itOnboarding, timeline: itTimeline, people: itPeople, areas: itAreas, plans: itPlans, tasks: itTasks, routines: itRoutines, settings: itSettings, week: itWeek, coordination: itCoordination },
  ja: { auth: jaAuth, common: jaCommon, lang: jaLang, nav: jaNav, onboarding: jaOnboarding, timeline: jaTimeline, people: jaPeople, areas: jaAreas, plans: jaPlans, tasks: jaTasks, routines: jaRoutines, settings: jaSettings, week: jaWeek, coordination: jaCoordination },
  zh: { auth: zhAuth, common: zhCommon, lang: zhLang, nav: zhNav, onboarding: zhOnboarding, timeline: zhTimeline, people: zhPeople, areas: zhAreas, plans: zhPlans, tasks: zhTasks, routines: zhRoutines, settings: zhSettings, week: zhWeek, coordination: zhCoordination },
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    // i18next-browser-languagedetector order: localStorage → navigator
    detection: {
      order: ["localStorage", "navigator"],
      lookupLocalStorage: UI_LANG_KEY,
      caches: ["localStorage"],
    },
    fallbackLng: "en",
    supportedLngs: SUPPORTED_LANG_CODES,
    // If detected lang is e.g. "fr-FR", strip region to "fr"
    nonExplicitSupportedLngs: true,
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;

/** Persists an explicit language selection. */
export function setUiLanguage(code: string): void {
  localStorage.setItem(UI_LANG_KEY, code);
  i18n.changeLanguage(code);
}

/** Returns the currently active language code. */
export function getCurrentLanguage(): string {
  return i18n.language?.split("-")[0] ?? "en";
}

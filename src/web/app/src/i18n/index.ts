import i18n from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

// English
import enAuth from "./locales/en/auth";
import enCommon from "./locales/en/common";
import enLang from "./locales/en/lang";
import enNav from "./locales/en/nav";
import enOnboarding from "./locales/en/onboarding";
import enSetup from "./locales/en/setup";
import enAreas from "./locales/en/areas";
import enPlans from "./locales/en/plans";
import enTasks from "./locales/en/tasks";
import enRoutines from "./locales/en/routines";
import enSettings from "./locales/en/settings";
import enMembers from "./locales/en/members";
import enToday from "./locales/en/today";
import enSharedLists from "./locales/en/sharedLists";

// German
import deAuth from "./locales/de/auth";
import deCommon from "./locales/de/common";
import deLang from "./locales/de/lang";
import deNav from "./locales/de/nav";
import deOnboarding from "./locales/de/onboarding";
import deSetup from "./locales/de/setup";
import deAreas from "./locales/de/areas";
import dePlans from "./locales/de/plans";
import deTasks from "./locales/de/tasks";
import deRoutines from "./locales/de/routines";
import deSettings from "./locales/de/settings";
import deMembers from "./locales/de/members";
import deToday from "./locales/de/today";
import deSharedLists from "./locales/de/sharedLists";

// Spanish
import esAuth from "./locales/es/auth";
import esCommon from "./locales/es/common";
import esLang from "./locales/es/lang";
import esNav from "./locales/es/nav";
import esOnboarding from "./locales/es/onboarding";
import esSetup from "./locales/es/setup";
import esAreas from "./locales/es/areas";
import esPlans from "./locales/es/plans";
import esTasks from "./locales/es/tasks";
import esRoutines from "./locales/es/routines";
import esSettings from "./locales/es/settings";
import esMembers from "./locales/es/members";
import esToday from "./locales/es/today";
import esSharedLists from "./locales/es/sharedLists";

// French
import frAuth from "./locales/fr/auth";
import frCommon from "./locales/fr/common";
import frLang from "./locales/fr/lang";
import frNav from "./locales/fr/nav";
import frOnboarding from "./locales/fr/onboarding";
import frAreas from "./locales/fr/areas";
import frPlans from "./locales/fr/plans";
import frTasks from "./locales/fr/tasks";
import frRoutines from "./locales/fr/routines";
import frSettings from "./locales/fr/settings";
import frMembers from "./locales/fr/members";
import frSetup from "./locales/fr/setup";
import frToday from "./locales/fr/today";
import frSharedLists from "./locales/fr/sharedLists";

// Italian
import itAuth from "./locales/it/auth";
import itCommon from "./locales/it/common";
import itLang from "./locales/it/lang";
import itNav from "./locales/it/nav";
import itOnboarding from "./locales/it/onboarding";
import itSetup from "./locales/it/setup";
import itAreas from "./locales/it/areas";
import itPlans from "./locales/it/plans";
import itTasks from "./locales/it/tasks";
import itRoutines from "./locales/it/routines";
import itSettings from "./locales/it/settings";
import itMembers from "./locales/it/members";
import itToday from "./locales/it/today";
import itSharedLists from "./locales/it/sharedLists";

// Japanese
import jaAuth from "./locales/ja/auth";
import jaCommon from "./locales/ja/common";
import jaLang from "./locales/ja/lang";
import jaNav from "./locales/ja/nav";
import jaOnboarding from "./locales/ja/onboarding";
import jaSetup from "./locales/ja/setup";
import jaAreas from "./locales/ja/areas";
import jaPlans from "./locales/ja/plans";
import jaTasks from "./locales/ja/tasks";
import jaRoutines from "./locales/ja/routines";
import jaSettings from "./locales/ja/settings";
import jaMembers from "./locales/ja/members";
import jaToday from "./locales/ja/today";
import jaSharedLists from "./locales/ja/sharedLists";

// Chinese
import zhAuth from "./locales/zh/auth";
import zhCommon from "./locales/zh/common";
import zhLang from "./locales/zh/lang";
import zhNav from "./locales/zh/nav";
import zhOnboarding from "./locales/zh/onboarding";
import zhSetup from "./locales/zh/setup";
import zhAreas from "./locales/zh/areas";
import zhPlans from "./locales/zh/plans";
import zhTasks from "./locales/zh/tasks";
import zhRoutines from "./locales/zh/routines";
import zhSettings from "./locales/zh/settings";
import zhMembers from "./locales/zh/members";
import zhToday from "./locales/zh/today";
import zhSharedLists from "./locales/zh/sharedLists";

export const SUPPORTED_LANG_CODES = ["en", "de", "es", "fr", "it", "ja", "zh"] as const;
export type SupportedLangCode = (typeof SUPPORTED_LANG_CODES)[number];

const resources = {
  en: { auth: enAuth, common: enCommon, lang: enLang, nav: enNav, onboarding: enOnboarding, setup: enSetup, areas: enAreas, plans: enPlans, tasks: enTasks, routines: enRoutines, settings: enSettings, members: enMembers, today: enToday, sharedLists: enSharedLists },
  de: { auth: deAuth, common: deCommon, lang: deLang, nav: deNav, onboarding: deOnboarding, setup: deSetup, areas: deAreas, plans: dePlans, tasks: deTasks, routines: deRoutines, settings: deSettings, members: deMembers, today: deToday, sharedLists: deSharedLists },
  es: { auth: esAuth, common: esCommon, lang: esLang, nav: esNav, onboarding: esOnboarding, setup: esSetup, areas: esAreas, plans: esPlans, tasks: esTasks, routines: esRoutines, settings: esSettings, members: esMembers, today: esToday, sharedLists: esSharedLists },
  fr: { auth: frAuth, common: frCommon, lang: frLang, nav: frNav, onboarding: frOnboarding, setup: frSetup, areas: frAreas, plans: frPlans, tasks: frTasks, routines: frRoutines, settings: frSettings, members: frMembers, today: frToday, sharedLists: frSharedLists },
  it: { auth: itAuth, common: itCommon, lang: itLang, nav: itNav, onboarding: itOnboarding, setup: itSetup, areas: itAreas, plans: itPlans, tasks: itTasks, routines: itRoutines, settings: itSettings, members: itMembers, today: itToday, sharedLists: itSharedLists },
  ja: { auth: jaAuth, common: jaCommon, lang: jaLang, nav: jaNav, onboarding: jaOnboarding, setup: jaSetup, areas: jaAreas, plans: jaPlans, tasks: jaTasks, routines: jaRoutines, settings: jaSettings, members: jaMembers, today: jaToday, sharedLists: jaSharedLists },
  zh: { auth: zhAuth, common: zhCommon, lang: zhLang, nav: zhNav, onboarding: zhOnboarding, setup: zhSetup, areas: zhAreas, plans: zhPlans, tasks: zhTasks, routines: zhRoutines, settings: zhSettings, members: zhMembers, today: zhToday, sharedLists: zhSharedLists },
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    // i18next-browser-languagedetector order: localStorage → navigator
    // caches is intentionally omitted: we write UI_LANG_KEY ourselves via
    // setUiLanguage(), so the detector must not auto-persist browser language
    // into that key (it would be treated as an explicit user choice).
    detection: {
      order: ["navigator"],
    },
    fallbackLng: "en",
    supportedLngs: SUPPORTED_LANG_CODES,
    // If detected lang is e.g. "fr-FR", strip region to "fr"
    nonExplicitSupportedLngs: true,
    interpolation: {
      escapeValue: false,
    },
  });

// Keep <html lang> in sync so <input type="date"> and other browser
// features that read the lang attribute use the correct locale.
i18n.on("languageChanged", (lng) => {
  document.documentElement.lang = lng;
});

export default i18n;

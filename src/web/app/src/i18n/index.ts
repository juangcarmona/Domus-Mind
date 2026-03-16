import i18n from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

import de from "./locales/de";
import en from "./locales/en";
import es from "./locales/es";
import fr from "./locales/fr";
import it from "./locales/it";
import ja from "./locales/ja";
import zh from "./locales/zh";

// The explicit UI language choice key stored in localStorage.
export const UI_LANG_KEY = "dm_ui_lang";

export const SUPPORTED_LANG_CODES = ["en", "de", "es", "fr", "it", "ja", "zh"] as const;
export type SupportedLangCode = (typeof SUPPORTED_LANG_CODES)[number];

const resources = {
  en: { translation: en },
  de: { translation: de },
  es: { translation: es },
  fr: { translation: fr },
  it: { translation: it },
  ja: { translation: ja },
  zh: { translation: zh },
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

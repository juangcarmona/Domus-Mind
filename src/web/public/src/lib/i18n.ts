export const locales = ['en', 'es', 'fr', 'de', 'it', 'zh', 'ja'] as const;
export type Locale = (typeof locales)[number];

export const defaultLocale: Locale = 'en';

export function isLocale(value: string): value is Locale {
  return (locales as readonly string[]).includes(value);
}

export function normalizeLocale(value?: string): Locale {
  if (value && isLocale(value)) {
    return value;
  }
  return defaultLocale;
}

/**
 * Build a locale-prefixed path. English uses root paths (e.g. `/`, `/concept`).
 * Other locales get a prefix (e.g. `/es/`, `/es/concept`).
 */
export function withLocale(locale: Locale, path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  if (locale === defaultLocale) {
    return normalized === '/' ? '/' : normalized;
  }
  return normalized === '/' ? `/${locale}/` : `/${locale}${normalized}`;
}

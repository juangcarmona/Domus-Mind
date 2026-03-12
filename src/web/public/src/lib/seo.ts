import { defaultLocale, type Locale } from './i18n';

const baseUrl = 'https://domusmind.org';

export function pageTitle(title: string): string {
  return `${title} · DomusMind`;
}

function normalizePath(pathname: string): string {
  if (pathname === '/') {
    return '/';
  }

  return pathname.startsWith('/') ? pathname : `/${pathname}`;
}

export function localeUrl(locale: Locale, pathname: string): string {
  const path = normalizePath(pathname);
  if (locale === defaultLocale) {
    return `${baseUrl}${path}`;
  }
  return `${baseUrl}/${locale}${path}`;
}

export function canonicalUrl(locale: Locale, pathname: string): string {
  return localeUrl(locale, pathname);
}

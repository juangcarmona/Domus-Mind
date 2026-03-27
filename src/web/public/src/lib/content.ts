import { defaultLocale, type Locale } from './i18n';
import type { UiContent } from './content-types';

interface MarkdownModule<T extends object = Record<string, unknown>> {
  frontmatter: T;
  Content: unknown;
}

const pageModules = import.meta.glob('../content/*/*.md', { eager: true }) as Record<string, MarkdownModule>;
const uiModules = import.meta.glob('../content/*/ui.json', { eager: true, import: 'default' }) as Record<string, UiContent>;

function getPagePath(locale: Locale, key: string): string {
  return `../content/${locale}/${key}.md`;
}

function getUiPath(locale: Locale): string {
  return `../content/${locale}/ui.json`;
}

export function getUiContent(locale: Locale): UiContent {
  const localizedPath = getUiPath(locale);
  const canonicalPath = getUiPath(defaultLocale);
  return uiModules[localizedPath] ?? uiModules[canonicalPath];
}

export function getPageContent<T extends object>(locale: Locale, key: string): {
  content: MarkdownModule<T>;
  sourceLocale: Locale;
} {
  const localizedPath = getPagePath(locale, key);
  const canonicalPath = getPagePath(defaultLocale, key);

  if (pageModules[localizedPath]) {
    return { content: pageModules[localizedPath] as MarkdownModule<T>, sourceLocale: locale };
  }

  return { content: pageModules[canonicalPath] as MarkdownModule<T>, sourceLocale: defaultLocale };
}

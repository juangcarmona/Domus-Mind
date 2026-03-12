export interface UiContent {
  siteName: string;
  tagline: string;
  nav: Record<string, string>;
  themeLabel: string;
  languageLabel: string;
  footer: Record<string, string>;
}

export interface HomeFrontmatter {
  title: string;
  description: string;
  heroTitle: string;
  heroBody: string;
  primaryCtaLabel: string;
  primaryCtaHref: string;
  secondaryCtaLabel: string;
  secondaryCtaHref: string;
  developerCtaTitle: string;
  developerCtaBody: string;
  developerCtaLabel: string;
  developerCtaHref: string;
  familyCtaTitle: string;
  familyCtaBody: string;
  familyCtaLabel: string;
  familyCtaHref: string;
}

export interface PageFrontmatter {
  title: string;
  description: string;
}

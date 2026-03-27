/**
 * Canonical demo family data for public-site examples.
 * Source of truth: docs/00_product/public-site.md — Localized Family Examples table.
 *
 * All visible names in proof panels, screenshots captions, and homepage examples
 * must come from this table. No ad-hoc invented names.
 *
 * Default locale is `en` → The Smith Family (UK).
 */

export interface DemoFamilyMember {
  name: string;
}

export interface DemoPet {
  name: string;
  type: string;
}

export interface DemoFamily {
  locale: string;
  country: string;
  familyName: string;
  parent1: DemoFamilyMember;
  parent2: DemoFamilyMember;
  children: DemoFamilyMember[];
  pets: DemoPet[];
}

export const DEMO_FAMILIES: Record<string, DemoFamily> = {
  en: {
    locale: 'en',
    country: 'UK',
    familyName: 'The Smith Family',
    parent1: { name: 'James' },
    parent2: { name: 'Sarah' },
    children: [{ name: 'Noah' }, { name: 'Olivia' }],
    pets: [{ name: 'Bella', type: 'dog' }],
  },
  es: {
    locale: 'es',
    country: 'Spain',
    familyName: 'Familia García Martínez',
    parent1: { name: 'David' },
    parent2: { name: 'Laura' },
    children: [{ name: 'Hugo' }, { name: 'Martina' }],
    pets: [{ name: 'Luna', type: 'dog' }, { name: 'Coco', type: 'cat' }],
  },
  fr: {
    locale: 'fr',
    country: 'France',
    familyName: 'Famille Martin',
    parent1: { name: 'Nicolas' },
    parent2: { name: 'Camille' },
    children: [{ name: 'Gabriel' }, { name: 'Emma' }],
    pets: [{ name: 'Nala', type: 'dog' }, { name: 'Tigrou', type: 'cat' }],
  },
  de: {
    locale: 'de',
    country: 'Germany',
    familyName: 'Familie Müller',
    parent1: { name: 'Michael' },
    parent2: { name: 'Anna' },
    children: [{ name: 'Noah' }, { name: 'Emilia' }],
    pets: [{ name: 'Luna', type: 'cat' }],
  },
  it: {
    locale: 'it',
    country: 'Italy',
    familyName: 'Famiglia Rossi',
    parent1: { name: 'Marco' },
    parent2: { name: 'Giulia' },
    children: [{ name: 'Leonardo' }],
    pets: [{ name: 'Leo', type: 'dog' }, { name: 'Mia', type: 'cat' }],
  },
  ja: {
    locale: 'ja',
    country: 'Japan',
    familyName: '田中家',
    parent1: { name: 'Takashi' },
    parent2: { name: 'Yuki' },
    children: [{ name: 'Haruto' }],
    pets: [{ name: 'Momo', type: 'dog' }],
  },
  zh: {
    locale: 'zh',
    country: 'China',
    familyName: '李家',
    parent1: { name: 'Wei' },
    parent2: { name: 'Li' },
    children: [{ name: 'Yuchen' }],
    pets: [],
  },
};

/** Default demo family for English (UK). Use this for en-locale proof panels and examples. */
export const DEFAULT_DEMO_FAMILY = DEMO_FAMILIES.en;

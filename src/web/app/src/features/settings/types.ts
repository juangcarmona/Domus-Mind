export const FIRST_DAY_OPTIONS = [
  "monday",
  "sunday",
] as const;

export const DATE_FORMAT_OPTIONS = [
  "dd/MM/yyyy",
  "MM/dd/yyyy",
  "yyyy-MM-dd",
  "d/M/yyyy",
  "M/d/yyyy",
] as const;

export type FirstDayOfWeek = (typeof FIRST_DAY_OPTIONS)[number];
export type DateFormatOption = (typeof DATE_FORMAT_OPTIONS)[number];

export interface HouseholdSettingsFormData {
  name: string;
  primaryLanguageCode: string;
  firstDayOfWeek: string;
  dateFormatPreference: string;
}

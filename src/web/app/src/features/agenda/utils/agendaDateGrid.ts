import { DAY_ORDER, toIsoDate, startOfWeek, addDays } from "../../agenda-today/utils/dateUtils";

/**
 * Build a calendar grid for a given year + month.
 *
 * Returns an array of ISO-date strings (YYYY-MM-DD) in row-major week order,
 * padded with leading / trailing days from adjacent months to fill complete weeks.
 *
 * Uses local-date-safe toIsoDate (avoids toISOString() UTC drift).
 */
export function buildCalendarGrid(
  year: number,
  month: number, // 0-indexed
  firstDayOfWeek: string,
): string[][] {
  const firstDayIdx = Math.max(0, DAY_ORDER.indexOf(firstDayOfWeek.toLowerCase()));
  const firstOfMonth = new Date(year, month, 1);
  const lastOfMonth = new Date(year, month + 1, 0);

  let startPad = firstOfMonth.getDay() - firstDayIdx;
  if (startPad < 0) startPad += 7;

  const dates: string[] = [];
  for (let i = -startPad; i <= lastOfMonth.getDate() - 1; i++) {
    dates.push(toIsoDate(new Date(year, month, 1 + i)));
  }

  const remaining = dates.length % 7 === 0 ? 0 : 7 - (dates.length % 7);
  for (let i = 1; i <= remaining; i++) {
    dates.push(toIsoDate(new Date(year, month + 1, i)));
  }

  const weeks: string[][] = [];
  for (let i = 0; i < dates.length; i += 7) {
    weeks.push(dates.slice(i, i + 7));
  }
  return weeks;
}

/**
 * Compute the ISO week range [weekStart, weekEnd] that contains the given date.
 * weekEnd is the last day of the 7-day span (inclusive).
 */
export function weekRangeFor(
  iso: string,
  firstDayOfWeek: string | null,
): { weekStart: string; weekEnd: string } {
  const d = new Date(iso + "T00:00:00");
  const ws = startOfWeek(d, firstDayOfWeek);
  const weekStart = toIsoDate(ws);
  const weekEnd = addDays(weekStart, 6);
  return { weekStart, weekEnd };
}

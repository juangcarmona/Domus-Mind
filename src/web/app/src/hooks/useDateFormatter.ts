import { useAppSelector } from "../store/hooks";

/**
 * Returns date formatter functions that respect household UI settings:
 * - dateFormat from uiSlice (set from household.dateFormatPreference after bootstrap)
 * - locale from uiSlice (set from household.primaryLanguageCode after bootstrap)
 */
export function useDateFormatter() {
  const dateFormat = useAppSelector((s) => s.ui.dateFormat);
  const locale = useAppSelector((s) => s.ui.language);

  function formatDate(iso: string | null): string {
    if (!iso) return "";
    const d = new Date(iso);
    if (dateFormat) {
      return applyDateFormat(d, dateFormat);
    }
    return new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(d);
  }

  function formatDateTime(iso: string | null): string {
    if (!iso) return "";
    const d = new Date(iso);
    const time = new Intl.DateTimeFormat(locale, { hour: "2-digit", minute: "2-digit" }).format(d);
    if (dateFormat) {
      return `${applyDateFormat(d, dateFormat)} ${time}`;
    }
    return new Intl.DateTimeFormat(locale, { dateStyle: "medium", timeStyle: "short" }).format(d);
  }

  return { formatDate, formatDateTime };
}

function pad(n: number): string {
  return n.toString().padStart(2, "0");
}

function applyDateFormat(d: Date, fmt: string): string {
  const day = pad(d.getDate());
  const month = pad(d.getMonth() + 1);
  const year = d.getFullYear().toString();
  return fmt
    .replace("dd", day)
    .replace("d", d.getDate().toString())
    .replace("MM", month)
    .replace("M", (d.getMonth() + 1).toString())
    .replace("yyyy", year);
}

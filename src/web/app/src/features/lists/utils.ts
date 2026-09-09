// Shared utilities for the Lists surface.

export const WEEK_DAYS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"] as const;

/** Converts an ISO datetime string to "YYYY-MM-DDTHH:MM" for <input type="datetime-local">. */
export function toLocalInput(iso: string | null): string {
  if (!iso) return "";
  try {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  } catch {
    return "";
  }
}

/** Converts a "YYYY-MM-DDTHH:MM" local string back to ISO, or null if empty. */
export function fromLocalInput(localStr: string): string | null {
  if (!localStr) return null;
  try {
    return new Date(localStr).toISOString();
  } catch {
    return null;
  }
}

// Canonical repeat format: "Daily" | "Weekly:1,3,5" | "Monthly" | "Yearly"
// Days of week: 0=Sun, 1=Mon … 6=Sat

export function parseRepeat(val: string | null): { freq: string; days: number[] } {
  if (!val) return { freq: "", days: [] };
  const [freq, daysPart] = val.split(":");
  const days = daysPart
    ? daysPart.split(",").map(Number).filter((d) => d >= 0 && d <= 6)
    : [];
  return { freq, days };
}

export function serializeRepeat(freq: string, days: number[]): string | null {
  if (!freq) return null;
  if (freq === "Weekly" && days.length > 0) {
    return `Weekly:${[...days].sort((a, b) => a - b).join(",")}`;
  }
  return freq;
}

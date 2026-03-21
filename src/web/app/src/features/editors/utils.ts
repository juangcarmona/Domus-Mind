/**
 * Normalizes any ISO date/datetime string to the YYYY-MM-DD format expected
 * by <input type="date">. Returns "" if the value is empty or time-only.
 */
export function toLocalDateInput(value?: string | null): string {
  if (!value) return "";
  if (value.includes("T")) return value.slice(0, 10);
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
  if (/^\d{2}:\d{2}/.test(value)) return ""; // time-only string
  return value.slice(0, 10);
}

/**
 * Normalizes any ISO date/datetime or HH:mm string to the HH:mm format
 * expected by <input type="time">. Returns "" if the value is empty or
 * date-only.
 */
export function toLocalTimeInput(value?: string | null): string {
  if (!value) return "";
  if (/^\d{2}:\d{2}/.test(value)) return value.slice(0, 5);
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return ""; // date-only string
  if (value.includes("T")) {
    const d = new Date(value);
    return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
  }
  return value.slice(0, 5);
}

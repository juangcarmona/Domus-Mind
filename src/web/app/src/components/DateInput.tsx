import { useState } from "react";
import { useAppSelector } from "../store/hooks";

/**
 * Converts an ISO date string (YYYY-MM-DD) to the household display format.
 * Works for all supported format tokens: yyyy, MM, dd, M, d.
 * Replacements are ordered from longest to shortest to avoid partial matches.
 */
function applyFmt(iso: string, fmt: string): string {
  if (!iso) return "";
  const parts = iso.split("-");
  if (parts.length !== 3) return iso;
  const [y, rawM, rawD] = parts;
  if (!y || !rawM || !rawD) return iso;
  return fmt
    .replace("yyyy", y)
    .replace("MM", rawM.padStart(2, "0"))
    .replace("dd", rawD.padStart(2, "0"))
    .replace("M", String(parseInt(rawM, 10)))
    .replace("d", String(parseInt(rawD, 10)));
}

/**
 * Parses a user-typed date string back to ISO YYYY-MM-DD.
 * Returns "" for a cleared input, null for an unparseable/incomplete string.
 */
function parseToIso(display: string, fmt: string | null): string | null {
  if (!display.trim()) return "";
  if (!fmt) {
    return /^\d{4}-\d{2}-\d{2}$/.test(display) ? display : null;
  }
  // Detect the field separator (first non-format character in the format string)
  const sep = fmt.replace(/[dMy]/g, "").charAt(0);
  if (!sep) return null;

  const fmtParts = fmt.split(sep);
  const valParts = display.split(sep);
  if (valParts.length !== fmtParts.length) return null;

  let y = "", m = "", d = "";
  fmtParts.forEach((part, i) => {
    if (part.includes("y")) y = valParts[i];
    else if (part.includes("M")) m = valParts[i].padStart(2, "0");
    else if (part.includes("d")) d = valParts[i].padStart(2, "0");
  });

  if (!y || !m || !d) return null;
  const yn = parseInt(y, 10), mn = parseInt(m, 10), dn = parseInt(d, 10);
  if (isNaN(yn) || isNaN(mn) || isNaN(dn)) return null;
  if (mn < 1 || mn > 12 || dn < 1 || dn > 31) return null;

  return `${y.padStart(4, "0")}-${m}-${d}`;
}

interface DateInputProps {
  id?: string;
  className?: string;
  value: string;
  onChange: (iso: string) => void;
  required?: boolean;
}

/**
 * A date input that respects the household dateFormatPreference.
 *
 * When a format is configured (e.g. "dd/MM/yyyy"), renders as a plain text
 * input so the chosen format is shown and respected on all browsers.
 * Chrome ignores the `lang` attribute on <input type="date">, making the
 * native picker useless for non-US locales.
 *
 * When no format is configured, falls back to the native <input type="date">.
 *
 * Value contract: always reads and writes ISO YYYY-MM-DD (or "") from/to
 * the parent - identical to a native date input.
 */
export function DateInput({ id, className, value, onChange, required }: DateInputProps) {
  const dateFormat = useAppSelector((s) => s.ui.dateFormat);

  // rawInput is non-null while the user is actively editing the field.
  const [rawInput, setRawInput] = useState<string | null>(null);

  const formatted = value && dateFormat ? applyFmt(value, dateFormat) : value;
  const displayValue = rawInput !== null ? rawInput : formatted;

  if (!dateFormat) {
    return (
      <input
        id={id}
        className={className}
        type="date"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
      />
    );
  }

  function handleFocus() {
    setRawInput(formatted);
  }

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const raw = e.target.value;
    setRawInput(raw);
    if (!raw.trim()) {
      onChange("");
      return;
    }
    const iso = parseToIso(raw, dateFormat);
    if (iso) onChange(iso);
  }

  function handleBlur() {
    // If the field was cleared propagate the empty string (onChange may not have
    // fired yet if the user cleared the field and immediately tabbed away).
    if (rawInput !== null && !rawInput.trim()) {
      onChange("");
    }
    // Stop editing - displayValue reverts to the formatted version of the prop value.
    setRawInput(null);
  }

  return (
    <input
      id={id}
      className={className}
      type="text"
      inputMode="numeric"
      value={displayValue}
      placeholder={dateFormat.toLowerCase()}
      onFocus={handleFocus}
      onChange={handleChange}
      onBlur={handleBlur}
      required={required}
      autoComplete="off"
      maxLength={10}
    />
  );
}

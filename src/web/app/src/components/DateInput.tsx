interface DateInputProps {
  id?: string;
  className?: string;
  value: string;
  onChange: (iso: string) => void;
  required?: boolean;
}

/**
 * A date-only input backed by the native browser date picker.
 *
 * Value contract: always reads and writes ISO YYYY-MM-DD (or "") from/to
 * the parent. The browser controls display formatting; no manual text entry
 * is supported.
 */
export function DateInput({ id, className, value, onChange, required }: DateInputProps) {
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

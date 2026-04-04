import { useState } from "react";

interface QuickAddBarProps {
  placeholder?: string;
  onAdd: (value: string) => void;
  disabled?: boolean;
}

/**
 * QuickAddBar — always-visible inline capture bar.
 *
 * Replaces the inline quick-add inputs scattered in feature pages.
 * Optimized for sequential capture: submits on Enter, clears and refocuses.
 *
 * Usage:
 *   <QuickAddBar placeholder="Add item..." onAdd={(value) => dispatch(addItem(value))} />
 */
export function QuickAddBar({ placeholder = "Add item...", onAdd, disabled }: QuickAddBarProps) {
  const [value, setValue] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const trimmed = value.trim();
    if (!trimmed) return;
    onAdd(trimmed);
    setValue("");
  }

  return (
    <form className="quick-add-bar" onSubmit={handleSubmit}>
      <input
        type="text"
        className="quick-add-bar-input"
        placeholder={placeholder}
        value={value}
        onChange={(e) => setValue(e.target.value)}
        disabled={disabled}
        autoComplete="off"
      />
      <button
        type="submit"
        className="quick-add-bar-submit"
        disabled={disabled || !value.trim()}
        aria-label="Add"
      >
        +
      </button>
    </form>
  );
}

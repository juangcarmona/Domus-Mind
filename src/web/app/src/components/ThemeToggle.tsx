import { useAppDispatch, useAppSelector } from "../store/hooks";
import { setTheme, type Theme } from "../store/uiSlice";

/** Returns the effective resolved theme (light or dark) given the Redux theme preference. */
function useResolvedTheme(): "light" | "dark" {
  const theme = useAppSelector((s) => s.ui.theme);
  if (theme === "system") {
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }
  return theme;
}

export function ThemeToggle() {
  const dispatch = useAppDispatch();
  const resolved = useResolvedTheme();
  const isDark = resolved === "dark";

  function toggle() {
    const next: Theme = isDark ? "light" : "dark";
    dispatch(setTheme(next));
  }

  return (
    <button
      type="button"
      className="theme-toggle"
      aria-label="Toggle theme"
      aria-pressed={isDark}
      onClick={toggle}
      title={isDark ? "Switch to light mode" : "Switch to dark mode"}
    >
      {isDark ? (
        /* Sun icon - shown when dark, clicking switches to light */
        <svg
          aria-hidden="true"
          xmlns="http://www.w3.org/2000/svg"
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <circle cx="12" cy="12" r="5" />
          <line x1="12" y1="1" x2="12" y2="3" />
          <line x1="12" y1="21" x2="12" y2="23" />
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
          <line x1="1" y1="12" x2="3" y2="12" />
          <line x1="21" y1="12" x2="23" y2="12" />
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
        </svg>
      ) : (
        /* Moon icon - shown when light, clicking switches to dark */
        <svg
          aria-hidden="true"
          xmlns="http://www.w3.org/2000/svg"
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
        </svg>
      )}
    </button>
  );
}

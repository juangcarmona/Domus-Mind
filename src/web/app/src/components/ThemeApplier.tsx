import { useEffect } from "react";
import { useAppSelector } from "../store/hooks";

/**
 * Keeps document.documentElement[data-theme] in sync with Redux ui.theme.
 * Renders nothing — pure side-effect component.
 */
export function ThemeApplier() {
  const theme = useAppSelector((s) => s.ui.theme);

  useEffect(() => {
    const root = document.documentElement;
    if (theme === "dark") {
      root.setAttribute("data-theme", "dark");
    } else if (theme === "light") {
      root.setAttribute("data-theme", "light");
    } else {
      root.removeAttribute("data-theme");
    }
  }, [theme]);

  return null;
}

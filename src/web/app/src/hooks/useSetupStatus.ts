import { useState, useEffect } from "react";
import { setupApi } from "../api/setupApi";

type SetupStatus = "loading" | "needed" | "done";

/**
 * Checks whether the system has been initialized via the setup API.
 * Returns the status and a setter so the setup page can mark itself done.
 */
export function useSetupStatus(): [SetupStatus, (s: SetupStatus) => void] {
  const [status, setStatus] = useState<SetupStatus>("loading");

  useEffect(() => {
    setupApi
      .getStatus()
      .then(({ isInitialized }) => setStatus(isInitialized ? "done" : "needed"))
      .catch(() => setStatus("done")); // on API error, fall through to normal auth flow
  }, []);

  return [status, setStatus];
}

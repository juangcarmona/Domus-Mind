import { useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import i18nSingleton from "../i18n";
import { bootstrapHousehold } from "../store/householdSlice";
import { AppShell } from "../components/AppShell";
import { SplashScreen } from "../components/SplashScreen";
import { OnboardingPage } from "../features/onboarding/pages/OnboardingPage";
import { AreaDetailPage } from "../features/areas/pages/AreaDetailPage";
import { AreasPage } from "../features/areas/pages/AreasPage";
import { SettingsPage } from "../features/settings/pages/SettingsPage";
import { AgendaPage } from "../features/agenda/pages/AgendaPage";
import { SharedListsPage } from "../features/shared-lists/pages/SharedListsPage";

/**
 * Manages household bootstrap and i18n sync, then renders the shell-wrapped route tree.
 * Only rendered after the user is authenticated.
 */
export function AuthedRoutes() {
  const dispatch = useAppDispatch();
  const bootstrapStatus = useAppSelector((s) => s.household.bootstrapStatus);
  const uiLanguage = useAppSelector((s) => s.ui.language);

  useEffect(() => {
    dispatch(bootstrapHousehold());
  }, [dispatch]);

  // Keep i18n in sync with Redux language state.
  // Uses the stable singleton — NOT useTranslation's i18n — to avoid re-firing on
  // every language switch (the hook reference changes, which would create a loop).
  useEffect(() => {
    i18nSingleton.changeLanguage(uiLanguage);
  }, [uiLanguage]); // eslint-disable-line react-hooks/exhaustive-deps

  if (bootstrapStatus === "idle" || bootstrapStatus === "loading") {
    return <SplashScreen />;
  }

  if (bootstrapStatus === "needsOnboarding") {
    return (
      <Routes>
        <Route path="*" element={<OnboardingPage />} />
      </Routes>
    );
  }

  if (bootstrapStatus !== "ready") {
    return <SplashScreen />;
  }

  return (
    <AppShell>
      <Routes>
        {/* Primary surfaces */}
        <Route path="/agenda" element={<AgendaPage />} />
        <Route path="/agenda/members/:memberId" element={<AgendaPage />} />
        <Route path="/lists" element={<SharedListsPage />} />
        <Route path="/lists/:listId" element={<SharedListsPage />} />
        <Route path="/areas" element={<AreasPage />} />
        <Route path="/areas/:areaId" element={<AreaDetailPage />} />
        <Route path="/settings" element={<SettingsPage />} />

        {/* Default redirect */}
        <Route path="/" element={<Navigate to="/agenda" replace />} />

        {/* Legacy redirects — consolidated in one place */}
        <Route path="/planning" element={<Navigate to="/agenda?mode=week" replace />} />
        <Route path="/agenda/shared" element={<Navigate to="/agenda" replace />} />
        <Route path="/members" element={<Navigate to="/settings" replace />} />
        <Route path="/members/:memberId" element={<Navigate to="/settings" replace />} />
        <Route path="/timeline" element={<Navigate to="/agenda?mode=week" replace />} />
        <Route path="/coordination" element={<Navigate to="/agenda" replace />} />
        <Route path="/plans" element={<Navigate to="/agenda" replace />} />
        <Route path="/tasks" element={<Navigate to="/agenda" replace />} />

        {/* Catch-all */}
        <Route path="*" element={<Navigate to="/agenda" replace />} />
      </Routes>
    </AppShell>
  );
}

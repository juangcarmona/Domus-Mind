import { useEffect, useState } from "react";
import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  useNavigate,
} from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AuthProvider, useAuth } from "./auth/AuthProvider";
import { useAppDispatch, useAppSelector } from "./store/hooks";
import { bootstrapHousehold } from "./store/householdSlice";
import { AppShell } from "./components/AppShell";
import { LoginPage } from "./features/auth/pages/LoginPage";
import { RegisterPage } from "./features/auth/pages/RegisterPage";
import { SetupPage } from "./features/setup/pages/SetupPage";
import { OnboardingPage } from "./features/onboarding/pages/OnboardingPage";
import { AreasPage } from "./features/areas/pages/AreasPage";
import { PlanningPage } from "./features/planning/pages/PlanningPage";
import { SettingsPage } from "./features/settings/pages/SettingsPage";
import { TodayPage } from "./features/today/pages/TodayPage";
import { setupApi } from "./api/setupApi";

function AuthedApp() {
  const dispatch = useAppDispatch();
  const { bootstrapStatus } = useAppSelector((s) => s.household);
  const uiLanguage = useAppSelector((s) => s.ui.language);
  const { i18n } = useTranslation("common");

  useEffect(() => {
    dispatch(bootstrapHousehold());
  }, [dispatch]);

  // Keep i18n in sync with the language resolved by uiSlice
  // (set from household.primaryLanguageCode after bootstrap / settings save).
  useEffect(() => {
    i18n.changeLanguage(uiLanguage);
  }, [uiLanguage, i18n]);

  if (bootstrapStatus === "idle" || bootstrapStatus === "loading") {
    return <div className="loading-wrap">Loading your household…</div>;
  }

  if (bootstrapStatus === "needsOnboarding") {
    return (
      <Routes>
        <Route path="*" element={<OnboardingPage />} />
      </Routes>
    );
  }

  if (bootstrapStatus !== "ready") {
    return <div className="loading-wrap">Loading your household…</div>;
  }

  return (
    <AppShell>
      <Routes>
        <Route path="/areas" element={<AreasPage />} />
        <Route path="/planning" element={<PlanningPage />} />
        <Route path="/settings" element={<SettingsPage />} />
        <Route path="/timeline" element={<Navigate to="/planning" replace />} />
        <Route path="/" element={<TodayPage />} />
        <Route path="/agenda" element={<Navigate to="/" replace />} />
        <Route path="/coordination" element={<Navigate to="/" replace />} />
        <Route path="/plans" element={<Navigate to="/planning" replace />} />
        <Route path="/tasks" element={<Navigate to="/planning" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AppShell>
  );
}

type AuthPage = "login" | "register";

function UnauthApp() {
  const nav = useNavigate();
  const [page, setPage] = useState<AuthPage>("login");

  if (page === "register") {
    return (
      <RegisterPage
        onSuccess={() => setPage("login")}
        onGoToLogin={() => setPage("login")}
      />
    );
  }

  return (
    <LoginPage
      onSuccess={() => nav("/")}
      onGoToRegister={() => setPage("register")}
    />
  );
}

function AppRoutes() {
  const { user, isLoading } = useAuth();
  const [setupStatus, setSetupStatus] = useState<"loading" | "needed" | "done">("loading");

  useEffect(() => {
    setupApi
      .getStatus()
      .then(({ isInitialized }) => setSetupStatus(isInitialized ? "done" : "needed"))
      .catch(() => setSetupStatus("done")); // on API error, fall through to normal auth flow
  }, []);

  if (setupStatus === "loading" || isLoading) {
    return <div className="loading-wrap">Loading\u2026</div>;
  }

  if (setupStatus === "needed") {
    return (
      <Routes>
        <Route
          path="*"
          element={<SetupPage onInitialized={() => setSetupStatus("done")} />}
        />
      </Routes>
    );
  }

  if (!user) {
    return (
      <Routes>
        <Route path="*" element={<UnauthApp />} />
      </Routes>
    );
  }

  return <AuthedApp />;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  );
}

import { useEffect } from "react";
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
import { UI_LANG_KEY, setUiLanguage } from "./i18n/index";
import { AppShell } from "./components/AppShell";
import { LoginPage } from "./features/auth/pages/LoginPage";
import { RegisterPage } from "./features/auth/pages/RegisterPage";
import { OnboardingPage } from "./features/onboarding/pages/OnboardingPage";
import { PeoplePage } from "./features/people/pages/PeoplePage";
import { AreasPage } from "./features/areas/pages/AreasPage";
import { PlanningPage } from "./features/planning/pages/PlanningPage";
import { SettingsPage } from "./features/settings/pages/SettingsPage";
import { WeekPage } from "./features/week/pages/WeekPage";
import { CoordinationPage } from "./features/coordination/pages/CoordinationPage";

function AuthedApp() {
  const dispatch = useAppDispatch();
  const { bootstrapStatus, family } = useAppSelector((s) => s.household);
  const { i18n } = useTranslation("common");

  useEffect(() => {
    dispatch(bootstrapHousehold());
  }, [dispatch]);

  // Sync language precedence: explicit UI choice > household language > browser/fallback
  useEffect(() => {
    const explicitChoice = localStorage.getItem(UI_LANG_KEY);
    if (!explicitChoice && family?.primaryLanguageCode) {
      setUiLanguage(family.primaryLanguageCode);
    }
  }, [family?.primaryLanguageCode, i18n]);

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
        <Route path="/people" element={<PeoplePage />} />
        <Route path="/areas" element={<AreasPage />} />
        <Route path="/planning" element={<PlanningPage />} />
        <Route path="/week" element={<WeekPage />} />
        <Route path="/settings" element={<SettingsPage />} />
        <Route path="/timeline" element={<Navigate to="/planning" replace />} />
        <Route path="/agenda" element={<CoordinationPage />} />
        <Route path="/coordination" element={<Navigate to="/agenda" replace />} />
        <Route path="/plans" element={<Navigate to="/planning" replace />} />
        <Route path="/tasks" element={<Navigate to="/planning" replace />} />
        <Route path="*" element={<Navigate to="/planning" replace />} />
      </Routes>
    </AppShell>
  );
}

type AuthPage = "login" | "register";

function UnauthApp() {
  const nav = useNavigate();
  const [page, setPage] = [
    (sessionStorage.getItem("dm_auth_page") as AuthPage) ?? "login",
    (p: AuthPage) => {
      sessionStorage.setItem("dm_auth_page", p);
      nav(".", { replace: true });
    },
  ];

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
      onSuccess={() => nav("/planning")}
      onGoToRegister={() => setPage("register")}
    />
  );
}

function AppRoutes() {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return <div className="loading-wrap">Loading\u2026</div>;
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

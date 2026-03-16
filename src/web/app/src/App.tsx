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
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { OnboardingPage } from "./pages/OnboardingPage";
import { TimelinePage } from "./pages/TimelinePage";
import { PeoplePage } from "./pages/PeoplePage";
import { AreasPage } from "./pages/AreasPage";
import { PlansPage } from "./pages/PlansPage";
import { TasksPage } from "./pages/TasksPage";

function AuthedApp() {
  const dispatch = useAppDispatch();
  const { bootstrapStatus, family } = useAppSelector((s) => s.household);
  const { i18n } = useTranslation();

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
        <Route path="/timeline" element={<TimelinePage />} />
        <Route path="/people" element={<PeoplePage />} />
        <Route path="/areas" element={<AreasPage />} />
        <Route path="/plans" element={<PlansPage />} />
        <Route path="/tasks" element={<TasksPage />} />
        <Route path="*" element={<Navigate to="/timeline" replace />} />
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
      onSuccess={() => nav("/timeline")}
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

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
import i18nSingleton from "./i18n";
import { bootstrapHousehold } from "./store/householdSlice";
import { AppShell } from "./components/AppShell";
import { LoginPage } from "./features/auth/pages/LoginPage";
import { RegisterPage } from "./features/auth/pages/RegisterPage";
import { ForceChangePasswordPage } from "./features/auth/pages/ForceChangePasswordPage";
import { CloudHostedNoAccessPage } from "./features/auth/pages/CloudHostedNoAccessPage";
import { SetupPage } from "./features/setup/pages/SetupPage";
import { OnboardingPage } from "./features/onboarding/pages/OnboardingPage";
import { AreasPage } from "./features/areas/pages/AreasPage";
import { AreaDetailPage } from "./features/areas/pages/AreaDetailPage";
import { PlanningPage } from "./features/planning/pages/PlanningPage";
import { SettingsPage } from "./features/settings/pages/SettingsPage";
import { TodayPage } from "./features/today/pages/TodayPage";
import { MembersPage } from "./features/members/pages/MembersPage";
import { MemberDetailPage } from "./features/members/pages/MemberDetailPage";
import { SharedListsPage } from "./features/shared-lists/pages/SharedListsPage";
import { SharedListDetailPage } from "./features/shared-lists/pages/SharedListDetailPage";
import { MemberAgendaPage } from "./features/agenda/pages/MemberAgendaPage";
import { AdminShell } from "./features/admin/components/AdminShell";
import { AdminOverviewPage } from "./features/admin/pages/AdminOverviewPage";
import { AdminHouseholdsPage } from "./features/admin/pages/AdminHouseholdsPage";
import { AdminUsersPage } from "./features/admin/pages/AdminUsersPage";
import { AdminInvitationsPage } from "./features/admin/pages/AdminInvitationsPage";
import { setupApi } from "./api/setupApi";
import { platformApi } from "./api/platformApi";
import type { DeploymentModeResponse } from "./api/platformApi";
import { SplashScreen } from "./components/SplashScreen";

/** Keeps document.documentElement[data-theme] in sync with Redux ui.theme. */
function ThemeApplier() {
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

function AuthedApp({ deploymentMode }: { deploymentMode: DeploymentModeResponse["deploymentMode"] }) {
  const dispatch = useAppDispatch();
  const { bootstrapStatus } = useAppSelector((s) => s.household);
  const uiLanguage = useAppSelector((s) => s.ui.language);
  const { i18n } = useTranslation("common");
  const { user } = useAuth();

  useEffect(() => {
    dispatch(bootstrapHousehold());
  }, [dispatch]);

  // Keep i18n in sync with the language in Redux.
  // Use the stable singleton import - NOT `i18n` from useTranslation —
  // because the hook reference changes on every language switch, which
  // would re-fire this effect and override an in-progress selection.
  useEffect(() => {
    i18nSingleton.changeLanguage(uiLanguage);
  }, [uiLanguage]); // eslint-disable-line react-hooks/exhaustive-deps

  // Keep the i18n hook reference in sync with the singleton (no-op in practice,
  // but satisfies linters that reference `i18n`).
  void i18n;

  if (bootstrapStatus === "idle" || bootstrapStatus === "loading") {
    return <SplashScreen />;
  }

  if (bootstrapStatus === "needsOnboarding") {
    if (user?.isOperator) {
      return <Navigate to="/admin" replace />;
    }
    if (deploymentMode === "CloudHosted") {
      return (
        <Routes>
          <Route path="*" element={<CloudHostedNoAccessPage />} />
        </Routes>
      );
    }
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
        <Route path="/areas" element={<AreasPage />} />
        <Route path="/areas/:areaId" element={<AreaDetailPage />} />
        <Route path="/planning" element={<PlanningPage />} />
        <Route path="/members" element={<MembersPage />} />
        <Route path="/members/:memberId" element={<MemberDetailPage />} />
        <Route path="/agenda/members/:memberId" element={<MemberAgendaPage />} />
        <Route path="/lists" element={<SharedListsPage />} />
        <Route path="/lists/:listId" element={<SharedListDetailPage />} />
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
  const [deploymentMode, setDeploymentMode] = useState<DeploymentModeResponse["deploymentMode"]>("SingleInstance");

  useEffect(() => {
    Promise.all([setupApi.getStatus(), platformApi.getDeploymentMode()])
      .then(([{ isInitialized }, { deploymentMode: mode }]) => {
        setDeploymentMode(mode);
        setSetupStatus(isInitialized ? "done" : "needed");
      })
      .catch(() => setSetupStatus("done")); // on API error, fall through to normal auth flow
  }, []);

  if (setupStatus === "loading" || isLoading) {
    return <SplashScreen />;
  }

  // In CloudHosted the system is operator-initialized; skip self-service admin setup.
  if (setupStatus === "needed" && deploymentMode !== "CloudHosted") {
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

  if (user.mustChangePassword) {
    return (
      <Routes>
        <Route path="*" element={<ForceChangePasswordPage />} />
      </Routes>
    );
  }

  return <AuthedApp deploymentMode={deploymentMode} />;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <ThemeApplier />
        <Routes>
          <Route path="/admin/*" element={<AdminRoutes />} />
          <Route path="*" element={<AppRoutes />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

function AdminRoutes() {
  const { user, isLoading } = useAuth();

  if (isLoading) return <SplashScreen />;
  if (!user) return <Navigate to="/" replace />;
  if (!user.isOperator) return <Navigate to="/" replace />;

  return (
    <Routes>
      <Route element={<AdminShell />}>
        <Route index element={<AdminOverviewPage />} />
        <Route path="households" element={<AdminHouseholdsPage />} />
        <Route path="users" element={<AdminUsersPage />} />
        <Route path="invitations" element={<AdminInvitationsPage />} />
      </Route>
    </Routes>
  );
}

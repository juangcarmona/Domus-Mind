import { Routes, Route } from "react-router-dom";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider, useAuth } from "./auth/AuthProvider";
import { ThemeApplier } from "./components/ThemeApplier";
import { SplashScreen } from "./components/SplashScreen";
import { ForceChangePasswordPage } from "./features/auth/pages/ForceChangePasswordPage";
import { SetupPage } from "./features/setup/pages/SetupPage";
import { useSetupStatus } from "./hooks/useSetupStatus";
import { AuthedRoutes } from "./app/AuthedRoutes";
import { UnauthRoutes } from "./app/UnauthRoutes";

function AppRoutes() {
  const { user, isLoading } = useAuth();
  const [setupStatus, setSetupStatus] = useSetupStatus();

  if (setupStatus === "loading" || isLoading) return <SplashScreen />;

  if (setupStatus === "needed") {
    return (
      <Routes>
        <Route path="*" element={<SetupPage onInitialized={() => setSetupStatus("done")} />} />
      </Routes>
    );
  }

  if (!user) {
    return (
      <Routes>
        <Route path="*" element={<UnauthRoutes />} />
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

  return <AuthedRoutes />;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <ThemeApplier />
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  );
}

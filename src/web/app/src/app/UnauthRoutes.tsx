import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { RegisterPage } from "../features/auth/pages/RegisterPage";

type AuthPage = "login" | "register";

/**
 * Unauthenticated app state machine.
 * Handles switching between the login and register pages.
 */
export function UnauthRoutes() {
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

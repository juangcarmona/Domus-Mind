const BASE_URL = "";

export interface ApiError {
  status: number;
  message: string;
}

async function request<T>(
  path: string,
  options?: RequestInit,
  token?: string | null
): Promise<T> {
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options?.headers,
  };

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      if (body?.error) message = body.error;
    } catch {
      // ignore parse errors
    }
    const err: ApiError = { status: res.status, message };
    throw err;
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

// ── Auth endpoint shapes ──────────────────────────────────────────────────────

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  email: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

export interface MeResponse {
  userId: string;
  email: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

// ── API client ────────────────────────────────────────────────────────────────

export const authApi = {
  register: (body: RegisterRequest) =>
    request<RegisterResponse>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  login: (body: LoginRequest) =>
    request<LoginResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  refresh: (refreshToken: string) =>
    request<RefreshTokenResponse>("/api/auth/refresh", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    }),

  me: (token: string) =>
    request<MeResponse>("/api/auth/me", {}, token),

  changePassword: (token: string, body: ChangePasswordRequest) =>
    request<void>("/api/auth/change-password", {
      method: "POST",
      body: JSON.stringify(body),
    }, token),

  logout: (refreshToken: string) =>
    request<void>("/api/auth/logout", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    }),
};

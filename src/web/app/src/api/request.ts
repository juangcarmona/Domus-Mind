import { getStoredToken } from "../lib/tokenStorage";

export { getStoredToken };

const BASE_URL = "";

export interface ApiError {
  status: number;
  message: string;
}

export async function request<T>(
  path: string,
  options?: RequestInit,
  token?: string | null,
): Promise<T> {
  const resolvedToken = token ?? getStoredToken();
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...(resolvedToken ? { Authorization: `Bearer ${resolvedToken}` } : {}),
    ...options?.headers,
  };
  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });
  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      if (body?.error) message = body.error;
    } catch {
      // ignore
    }
    throw { status: res.status, message } as ApiError;
  }
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

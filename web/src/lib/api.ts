// Typed API client for the Circles backend.
//
// All data endpoints require a JWT bearer token. The token is kept in
// localStorage at prototype level (see lib/auth.tsx). This module is the single
// place that knows how to talk HTTP to the backend, so switching to httpOnly
// cookies later only touches `authHeader()` and the token source.

const API_URL =
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") || "http://localhost:5292";

const TOKEN_KEY = "circles.token";

// ---- Token storage --------------------------------------------------------

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return window.localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(TOKEN_KEY);
}

// ---- Response contracts (mirror the backend DTOs) -------------------------

export interface AuthTokenResponse {
  token: string;
  expiresAt: string;
  userAccountId: string;
  personId: string | null;
  email: string;
  fullName: string | null;
}

export interface MeResponse {
  userAccountId: string;
  email: string;
  personId: string | null;
  fullName: string | null;
  isLinkedToPerson: boolean;
}

export interface MagicLinkResponse {
  message: string;
  devToken: string | null;
  devLoginUrl: string | null;
}

export type CircleType = "Team" | "Board" | "Officials" | "General";
export type AccessKind = "Direct" | "Derived";

export interface CircleAccess {
  circleId: string;
  name: string;
  slug: string;
  type: CircleType;
  parentCircleId: string | null;
  accessKind: AccessKind;
}

export type MembershipRole =
  | "Player"
  | "Guardian"
  | "Coach"
  | "Leader"
  | "Administrator"
  | "Member";

export interface Member {
  personId: string;
  fullName: string;
  role: MembershipRole;
  validFrom: string;
}

export interface Organization {
  id: string;
  name: string;
  slug: string;
}

// ---- Error type -----------------------------------------------------------

/** Thrown for any non-2xx response, carrying the HTTP status and a Swedish message. */
export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

// ---- Core request helper --------------------------------------------------

interface RequestOptions {
  method?: string;
  body?: unknown;
  /** When true, attaches the stored bearer token (required for data endpoints). */
  auth?: boolean;
}

async function request<T>(path: string, opts: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, auth = false } = opts;

  const headers: Record<string, string> = {};
  if (body !== undefined) headers["Content-Type"] = "application/json";
  if (auth) {
    const token = getToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;
  }

  let res: Response;
  try {
    res = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      cache: "no-store",
    });
  } catch {
    throw new ApiError(0, "Kunde inte nå servern. Kontrollera din anslutning.");
  }

  if (res.status === 204) return undefined as T;

  const text = await res.text();
  const data = text ? safeJson(text) : null;

  if (!res.ok) {
    throw new ApiError(res.status, extractError(res.status, data));
  }

  return data as T;
}

function safeJson(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

/** Turns a FastEndpoints error payload into a single Swedish message. */
function extractError(status: number, data: unknown): string {
  if (data && typeof data === "object") {
    const obj = data as {
      errors?: Record<string, string[]>;
      message?: string;
    };
    if (obj.errors) {
      const first = Object.values(obj.errors).flat()[0];
      if (first) return first;
    }
    if (obj.message && obj.message !== "One or more errors occurred!") {
      return obj.message;
    }
  }
  if (status === 401) return "Fel e-postadress eller lösenord.";
  if (status === 403) return "Du har inte behörighet till detta.";
  if (status === 404) return "Kunde inte hittas.";
  return "Något gick fel. Försök igen.";
}

// ---- Public API surface ---------------------------------------------------

export const api = {
  login(email: string, password: string) {
    return request<AuthTokenResponse>("/api/auth/login", {
      method: "POST",
      body: { email, password },
    });
  },

  requestMagicLink(email: string) {
    return request<MagicLinkResponse>("/api/auth/magic-link", {
      method: "POST",
      body: { email },
    });
  },

  consumeMagicLink(token: string) {
    return request<AuthTokenResponse>("/api/auth/magic-link/consume", {
      method: "POST",
      body: { token },
    });
  },

  me() {
    return request<MeResponse>("/api/auth/me", { auth: true });
  },

  personCircles(personId: string) {
    return request<CircleAccess[]>(`/api/persons/${personId}/circles`, {
      auth: true,
    });
  },

  circleMembers(circleId: string) {
    return request<Member[]>(`/api/circles/${circleId}/members`, {
      auth: true,
    });
  },

  organizations() {
    return request<Organization[]>("/api/organizations", { auth: true });
  },
};

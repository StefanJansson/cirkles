"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { api, ApiError } from "@/lib/api";
import { Spinner } from "@/components/Spinner";
import { LinkIcon } from "@/components/icons";

export default function LoginPage() {
  const router = useRouter();
  const { isAuthenticated, loading, login, loginWithToken } = useAuth();

  // Already signed in? Go home.
  useEffect(() => {
    if (!loading && isAuthenticated) router.replace("/hem");
  }, [loading, isAuthenticated, router]);

  // ---- Password login ----
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [pwLoading, setPwLoading] = useState(false);
  const [pwError, setPwError] = useState<string | null>(null);

  async function handlePasswordLogin(e: React.FormEvent) {
    e.preventDefault();
    setPwError(null);
    setPwLoading(true);
    try {
      await login(email.trim(), password);
      router.replace("/hem");
    } catch (err) {
      setPwError(
        err instanceof ApiError
          ? err.message
          : "Något gick fel. Försök igen.",
      );
    } finally {
      setPwLoading(false);
    }
  }

  // ---- Passwordless (magic link) ----
  const [mlEmail, setMlEmail] = useState("");
  const [mlLoading, setMlLoading] = useState(false);
  const [mlMessage, setMlMessage] = useState<string | null>(null);
  const [mlError, setMlError] = useState<string | null>(null);
  const [devToken, setDevToken] = useState<string | null>(null);
  const [consuming, setConsuming] = useState(false);

  async function handleMagicLink(e: React.FormEvent) {
    e.preventDefault();
    setMlError(null);
    setMlMessage(null);
    setDevToken(null);
    setMlLoading(true);
    try {
      const res = await api.requestMagicLink(mlEmail.trim());
      setMlMessage(res.message);
      setDevToken(res.devToken); // populated only in the backend's Development mode
    } catch (err) {
      setMlError(
        err instanceof ApiError ? err.message : "Något gick fel. Försök igen.",
      );
    } finally {
      setMlLoading(false);
    }
  }

  async function handleConsume() {
    if (!devToken) return;
    setConsuming(true);
    setMlError(null);
    try {
      await loginWithToken((await api.consumeMagicLink(devToken)).token);
      router.replace("/hem");
    } catch (err) {
      setMlError(
        err instanceof ApiError
          ? err.message
          : "Länken kunde inte användas. Försök igen.",
      );
      setConsuming(false);
    }
  }

  // Avoid flashing the form while we check for an existing session.
  if (loading || isAuthenticated) {
    return (
      <div className="mx-auto flex min-h-dvh max-w-app items-center justify-center bg-cream shadow-app">
        <Spinner className="h-7 w-7" />
      </div>
    );
  }

  return (
    <div className="mx-auto flex min-h-dvh max-w-app flex-col bg-cream px-6 py-12 shadow-app">
      <div className="animate-fade-in">
        <div className="mb-10 text-center">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-forest text-2xl font-semibold text-cream">
            C
          </div>
          <h1 className="text-2xl font-semibold text-navy">Circles</h1>
          <p className="mt-1 text-sm text-muted">Cirklar för din förening</p>
        </div>

        {/* Password login */}
        <form onSubmit={handlePasswordLogin} className="space-y-4">
          <div>
            <label htmlFor="email" className="mb-1 block text-sm font-medium text-navy">
              E-post
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-xl border border-hairline bg-white px-4 py-3 text-navy outline-none transition focus:border-forest focus:ring-2 focus:ring-forest/20"
              placeholder="namn@exempel.se"
            />
          </div>
          <div>
            <label htmlFor="password" className="mb-1 block text-sm font-medium text-navy">
              Lösenord
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-xl border border-hairline bg-white px-4 py-3 text-navy outline-none transition focus:border-forest focus:ring-2 focus:ring-forest/20"
              placeholder="••••••••"
            />
          </div>

          {pwError && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {pwError}
            </p>
          )}

          <button
            type="submit"
            disabled={pwLoading}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-forest px-4 py-3 font-medium text-cream transition hover:bg-forest-dark disabled:opacity-60"
          >
            {pwLoading && <Spinner className="h-5 w-5 text-cream" />}
            Logga in
          </button>
        </form>

        {/* Divider */}
        <div className="my-8 flex items-center gap-3 text-xs text-muted">
          <span className="h-px flex-1 bg-hairline" />
          eller
          <span className="h-px flex-1 bg-hairline" />
        </div>

        {/* Passwordless login */}
        <section>
          <h2 className="mb-1 flex items-center gap-2 text-sm font-semibold text-navy">
            <LinkIcon className="h-4 w-4 text-forest" />
            Logga in utan lösenord
          </h2>
          <p className="mb-3 text-xs text-muted">
            Vi skickar en inloggningslänk till din e-post.
          </p>
          <form onSubmit={handleMagicLink} className="space-y-3">
            <input
              type="email"
              required
              value={mlEmail}
              onChange={(e) => setMlEmail(e.target.value)}
              className="w-full rounded-xl border border-hairline bg-white px-4 py-3 text-navy outline-none transition focus:border-forest focus:ring-2 focus:ring-forest/20"
              placeholder="namn@exempel.se"
              aria-label="E-post för inloggningslänk"
            />
            <button
              type="submit"
              disabled={mlLoading}
              className="flex w-full items-center justify-center gap-2 rounded-xl border border-forest px-4 py-3 font-medium text-forest transition hover:bg-forest/5 disabled:opacity-60"
            >
              {mlLoading && <Spinner className="h-5 w-5" />}
              Skicka länk
            </button>
          </form>

          {mlMessage && (
            <div className="mt-3 rounded-lg bg-forest/5 px-3 py-2 text-sm text-navy">
              {mlMessage}
            </div>
          )}

          {/* Development convenience: the backend echoes the token so the flow is
              testable without a real email provider. */}
          {devToken && (
            <button
              onClick={handleConsume}
              disabled={consuming}
              className="mt-3 flex w-full items-center justify-center gap-2 rounded-xl bg-navy px-4 py-3 text-sm font-medium text-cream transition hover:opacity-90 disabled:opacity-60"
            >
              {consuming && <Spinner className="h-5 w-5 text-cream" />}
              Klicka här för att logga in
            </button>
          )}

          {mlError && (
            <p className="mt-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {mlError}
            </p>
          )}
        </section>
      </div>
    </div>
  );
}

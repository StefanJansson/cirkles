"use client";

import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { LogoutIcon } from "@/components/icons";

export default function ProfilPage() {
  const { user, logout } = useAuth();
  const router = useRouter();

  function handleLogout() {
    logout();
    router.replace("/login");
  }

  const initials = getInitials(user?.fullName ?? user?.email ?? "");

  return (
    <div className="animate-fade-in">
      <header className="mb-6">
        <h1 className="text-2xl font-semibold text-navy">Profil</h1>
      </header>

      <section className="rounded-2xl border border-hairline bg-white p-5 shadow-card">
        <div className="flex items-center gap-4">
          <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-forest/10 text-lg font-semibold text-forest">
            {initials}
          </div>
          <div className="min-w-0">
            <p className="truncate text-lg font-semibold text-navy">
              {user?.fullName ?? "Namn saknas"}
            </p>
            <p className="truncate text-sm text-muted">{user?.email}</p>
          </div>
        </div>

        <dl className="mt-5 space-y-4 border-t border-hairline pt-5">
          <div>
            <dt className="text-xs font-semibold uppercase tracking-wide text-muted">
              E-post
            </dt>
            <dd className="mt-0.5 text-sm text-navy">{user?.email}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase tracking-wide text-muted">
              Personkoppling
            </dt>
            <dd className="mt-0.5 text-sm text-navy">
              {user?.isLinkedToPerson ? (
                <span className="inline-flex items-center gap-1.5">
                  <span
                    className="h-2 w-2 rounded-full bg-forest"
                    aria-hidden="true"
                  />
                  Kontot är länkat till en person
                </span>
              ) : (
                <span className="inline-flex items-center gap-1.5">
                  <span
                    className="h-2 w-2 rounded-full bg-amber-500"
                    aria-hidden="true"
                  />
                  Kontot saknar personkoppling
                </span>
              )}
            </dd>
          </div>
        </dl>
      </section>

      <button
        type="button"
        onClick={handleLogout}
        className="mt-6 flex w-full items-center justify-center gap-2 rounded-xl border border-hairline bg-white px-4 py-3 text-sm font-semibold text-navy transition-colors hover:bg-red-50 hover:text-red-700"
      >
        <LogoutIcon className="h-5 w-5" />
        Logga ut
      </button>
    </div>
  );
}

function getInitials(source: string): string {
  const trimmed = source.trim();
  if (!trimmed) return "?";
  const parts = trimmed.split(/\s+/);
  if (parts.length === 1) {
    return parts[0].slice(0, 2).toUpperCase();
  }
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

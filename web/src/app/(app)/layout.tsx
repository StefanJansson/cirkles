"use client";

// Layout + auth guard for all protected screens (/hem, /cirklar, /profil).
//
// The JWT is stored in localStorage, which server-side Next.js middleware cannot
// read — so route protection is enforced here on the client: while the session
// rehydrates we show a loader, and unauthenticated users are redirected to
// /login. This is the intended pattern for a localStorage-based token; when the
// prototype later moves the token to an httpOnly cookie, this guard can be
// promoted to real middleware.

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { BottomNav } from "@/components/BottomNav";
import { PageLoader } from "@/components/Spinner";

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [loading, isAuthenticated, router]);

  return (
    <div className="mx-auto flex min-h-dvh max-w-app flex-col bg-cream shadow-app">
      {loading || !isAuthenticated ? (
        <PageLoader />
      ) : (
        <>
          <main className="flex-1 px-5 pb-6 pt-6">{children}</main>
          <BottomNav />
        </>
      )}
    </div>
  );
}

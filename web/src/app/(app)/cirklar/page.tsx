"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth";
import { api, ApiError, type CircleAccess } from "@/lib/api";
import { CircleCard } from "@/components/CircleCard";
import { Spinner } from "@/components/Spinner";

export default function CirklarPage() {
  const { user } = useAuth();
  const [circles, setCircles] = useState<CircleAccess[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      if (!user?.personId) {
        setCircles([]);
        return;
      }
      try {
        const data = await api.personCircles(user.personId);
        if (!cancelled) setCircles(data);
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof ApiError
              ? err.message
              : "Kunde inte hämta dina cirklar.",
          );
        }
      }
    }
    load();
    return () => {
      cancelled = true;
    };
  }, [user?.personId]);

  return (
    <div className="animate-fade-in">
      <header className="mb-6">
        <h1 className="text-2xl font-semibold text-navy">Cirklar</h1>
        <p className="mt-1 text-sm text-muted">
          Cirklar du har tillgång till
        </p>
      </header>

      {error ? (
        <p className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700" role="alert">
          {error}
        </p>
      ) : circles === null ? (
        <div className="flex justify-center py-10">
          <Spinner className="h-6 w-6" />
        </div>
      ) : circles.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-hairline bg-white/60 px-5 py-10 text-center">
          <p className="font-medium text-navy">Inga cirklar än</p>
          <p className="mt-1 text-sm text-muted">
            Du är inte med i någon cirkel för tillfället.
          </p>
        </div>
      ) : (
        <ul className="space-y-3">
          {circles.map((c) => (
            <li key={c.circleId}>
              <CircleCard circle={c} />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

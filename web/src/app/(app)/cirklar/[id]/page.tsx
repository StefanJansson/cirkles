"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/auth";
import {
  api,
  ApiError,
  type CircleAccess,
  type Member,
} from "@/lib/api";
import { circleTypeLabel, roleLabel } from "@/lib/labels";
import { TypeBadge } from "@/components/CircleCard";
import { Spinner } from "@/components/Spinner";
import { ArrowLeftIcon } from "@/components/icons";

type Tab = "medlemmar" | "diskussioner" | "uppgifter";

export default function CircleDetailPage() {
  const params = useParams<{ id: string }>();
  const circleId = params.id;
  const { user } = useAuth();

  const [circle, setCircle] = useState<CircleAccess | null>(null);
  const [members, setMembers] = useState<Member[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [noAccess, setNoAccess] = useState(false);
  const [tab, setTab] = useState<Tab>("medlemmar");

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        // Resolve the circle from the user's accessible set — this both gives us
        // the circle's name/type and enforces backend-driven access (a circle the
        // user cannot reach simply won't be present).
        const accessible = user?.personId
          ? await api.personCircles(user.personId)
          : [];
        const found = accessible.find((c) => c.circleId === circleId) ?? null;
        if (!found) {
          if (!cancelled) {
            setNoAccess(true);
            setLoading(false);
          }
          return;
        }
        const memberList = await api.circleMembers(circleId);
        if (!cancelled) {
          setCircle(found);
          setMembers(memberList);
          setLoading(false);
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof ApiError
              ? err.message
              : "Kunde inte hämta cirkeln.",
          );
          setLoading(false);
        }
      }
    }
    load();
    return () => {
      cancelled = true;
    };
  }, [circleId, user?.personId]);

  return (
    <div className="animate-fade-in">
      <Link
        href="/cirklar"
        className="mb-4 inline-flex items-center gap-1.5 text-sm text-muted transition hover:text-navy"
      >
        <ArrowLeftIcon className="h-4 w-4" />
        Tillbaka
      </Link>

      {loading ? (
        <div className="flex justify-center py-14">
          <Spinner className="h-6 w-6" />
        </div>
      ) : noAccess ? (
        <div className="rounded-2xl border border-dashed border-hairline bg-white/60 px-5 py-10 text-center">
          <p className="font-medium text-navy">Ingen åtkomst</p>
          <p className="mt-1 text-sm text-muted">
            Du har inte tillgång till den här cirkeln.
          </p>
        </div>
      ) : error ? (
        <p className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700" role="alert">
          {error}
        </p>
      ) : circle ? (
        <>
          <header className="mb-5">
            <h1 className="text-2xl font-semibold text-navy">{circle.name}</h1>
            <div className="mt-2 flex items-center gap-2">
              <TypeBadge type={circle.type} />
              {circle.accessKind === "Derived" && (
                <span className="text-xs text-muted">Härledd åtkomst</span>
              )}
            </div>
          </header>

          {/* Tabs: Members is live; the rest are future features. */}
          <div className="mb-4 flex gap-1 border-b border-hairline">
            <TabButton active={tab === "medlemmar"} onClick={() => setTab("medlemmar")}>
              Medlemmar
            </TabButton>
            <TabButton disabled>Diskussioner</TabButton>
            <TabButton disabled>Uppgifter</TabButton>
          </div>

          {tab === "medlemmar" && <MembersList members={members ?? []} />}
        </>
      ) : null}
    </div>
  );
}

function TabButton({
  children,
  active = false,
  disabled = false,
  onClick,
}: {
  children: React.ReactNode;
  active?: boolean;
  disabled?: boolean;
  onClick?: () => void;
}) {
  if (disabled) {
    return (
      <span className="relative -mb-px cursor-not-allowed px-3 py-2 text-sm text-muted/50">
        {children}
        <span className="ml-1 align-super text-[9px] font-medium text-muted/60">
          Kommer snart
        </span>
      </span>
    );
  }
  return (
    <button
      onClick={onClick}
      className={`-mb-px border-b-2 px-3 py-2 text-sm font-medium transition ${
        active
          ? "border-forest text-forest"
          : "border-transparent text-muted hover:text-navy"
      }`}
    >
      {children}
    </button>
  );
}

function MembersList({ members }: { members: Member[] }) {
  if (members.length === 0) {
    return (
      <div className="rounded-2xl border border-dashed border-hairline bg-white/60 px-5 py-10 text-center">
        <p className="font-medium text-navy">Inga aktiva medlemmar</p>
        <p className="mt-1 text-sm text-muted">
          Den här cirkeln har inga aktiva medlemmar just nu.
        </p>
      </div>
    );
  }

  return (
    <ul className="space-y-2">
      {members.map((m) => (
        <li
          key={m.personId}
          className="flex items-center gap-3 rounded-xl border border-hairline bg-white p-3 shadow-card"
        >
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-navy/5 text-sm font-medium text-navy">
            {initials(m.fullName)}
          </div>
          <div className="min-w-0 flex-1">
            <p className="truncate font-medium text-navy">{m.fullName}</p>
          </div>
          <span className="rounded-full bg-navy/5 px-2.5 py-0.5 text-xs font-medium text-navy">
            {roleLabel[m.role]}
          </span>
        </li>
      ))}
    </ul>
  );
}

function initials(fullName: string): string {
  const parts = fullName.trim().split(/\s+/);
  const first = parts[0]?.charAt(0) ?? "";
  const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : "";
  return (first + last).toUpperCase();
}

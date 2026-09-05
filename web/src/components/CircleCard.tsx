import Link from "next/link";
import type { CircleAccess } from "@/lib/api";
import { circleTypeLabel } from "@/lib/labels";
import { ChevronRightIcon } from "./icons";

export function TypeBadge({ type }: { type: CircleAccess["type"] }) {
  return (
    <span className="rounded-full bg-forest/10 px-2.5 py-0.5 text-xs font-medium text-forest-dark">
      {circleTypeLabel[type]}
    </span>
  );
}

export function CircleCard({ circle }: { circle: CircleAccess }) {
  return (
    <Link
      href={`/cirklar/${circle.circleId}`}
      className="flex items-center gap-3 rounded-2xl border border-hairline bg-white p-4 shadow-card transition hover:border-forest/40 hover:shadow-md"
    >
      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-forest/10 text-lg font-semibold text-forest-dark">
        {circle.name.charAt(0).toUpperCase()}
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <h3 className="truncate font-medium text-navy">{circle.name}</h3>
        </div>
        <div className="mt-1 flex items-center gap-2">
          <TypeBadge type={circle.type} />
          {circle.accessKind === "Derived" && (
            <span className="text-xs text-muted">Härledd åtkomst</span>
          )}
        </div>
      </div>
      <ChevronRightIcon className="h-5 w-5 shrink-0 text-muted" />
    </Link>
  );
}

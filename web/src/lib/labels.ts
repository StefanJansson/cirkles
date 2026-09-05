// Swedish display labels for backend enum values. Kept in one place so every
// screen shows the same wording.

import type { CircleType, MembershipRole } from "./api";

export const circleTypeLabel: Record<CircleType, string> = {
  Team: "Lag",
  Board: "Styrelse",
  Officials: "Funktionärer",
  General: "Allmän",
};

export const roleLabel: Record<MembershipRole, string> = {
  Player: "Spelare",
  Guardian: "Vårdnadshavare",
  Coach: "Tränare",
  Leader: "Ledare",
  Administrator: "Administratör",
  Member: "Medlem",
};

export function firstName(fullName: string | null): string | null {
  if (!fullName) return null;
  return fullName.trim().split(/\s+/)[0] || null;
}

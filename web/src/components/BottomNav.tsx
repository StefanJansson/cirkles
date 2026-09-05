"use client";

// Fixed mobile-style bottom navigation. Sits inside the 430px app column so it
// stays aligned with the content both on mobile and centered on desktop.

import Link from "next/link";
import { usePathname } from "next/navigation";
import { CirclesIcon, HomeIcon, ProfileIcon } from "./icons";
import type { ComponentType, SVGProps } from "react";

interface NavItem {
  href: string;
  label: string;
  Icon: ComponentType<SVGProps<SVGSVGElement>>;
  /** Extra path prefixes that should also mark this tab active. */
  match?: string[];
}

const items: NavItem[] = [
  { href: "/hem", label: "Hem", Icon: HomeIcon },
  { href: "/cirklar", label: "Cirklar", Icon: CirclesIcon, match: ["/cirklar"] },
  { href: "/profil", label: "Profil", Icon: ProfileIcon },
];

export function BottomNav() {
  const pathname = usePathname();

  return (
    <nav className="sticky bottom-0 z-10 border-t border-hairline bg-cream/95 backdrop-blur">
      <ul className="mx-auto flex max-w-app items-stretch justify-around">
        {items.map(({ href, label, Icon, match }) => {
          const active =
            pathname === href ||
            (match?.some((m) => pathname.startsWith(m)) ?? false);
          return (
            <li key={href} className="flex-1">
              <Link
                href={href}
                aria-current={active ? "page" : undefined}
                className={`flex flex-col items-center gap-1 py-2.5 text-[11px] font-medium transition-colors ${
                  active ? "text-forest" : "text-muted hover:text-navy"
                }`}
              >
                <Icon className={`h-6 w-6 ${active ? "stroke-[2]" : ""}`} />
                {label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}

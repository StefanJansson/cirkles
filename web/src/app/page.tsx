"use client";

// Landing route: send people to /hem (the guard there bounces them to /login if
// they are not authenticated).

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { PageLoader } from "@/components/Spinner";

export default function IndexPage() {
  const router = useRouter();
  useEffect(() => {
    router.replace("/hem");
  }, [router]);

  return (
    <div className="mx-auto flex min-h-dvh max-w-app flex-col bg-cream shadow-app">
      <PageLoader />
    </div>
  );
}

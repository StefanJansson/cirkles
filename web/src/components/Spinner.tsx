// Small, calm loading spinner in the brand accent colour.

export function Spinner({ className = "" }: { className?: string }) {
  return (
    <svg
      className={`animate-spin text-forest ${className}`}
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      role="status"
      aria-label="Laddar"
    >
      <circle
        className="opacity-20"
        cx="12"
        cy="12"
        r="9"
        stroke="currentColor"
        strokeWidth="3"
      />
      <path
        className="opacity-90"
        d="M21 12a9 9 0 0 0-9-9"
        stroke="currentColor"
        strokeWidth="3"
        strokeLinecap="round"
      />
    </svg>
  );
}

/** Full-height centered spinner for whole-page loading states. */
export function PageLoader() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <Spinner className="h-7 w-7" />
    </div>
  );
}

import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/app/**/*.{ts,tsx}",
    "./src/components/**/*.{ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Scandinavian, calm palette (from Task 1 design exploration).
        cream: "#FAFAF8",
        navy: "#1C2B3A",
        forest: "#4A7C59",
        "forest-dark": "#3C6449",
        hairline: "#E8E6E1",
        muted: "#6B7280",
      },
      fontFamily: {
        sans: [
          "-apple-system",
          "BlinkMacSystemFont",
          "Segoe UI",
          "Roboto",
          "Helvetica Neue",
          "Arial",
          "sans-serif",
        ],
      },
      maxWidth: {
        app: "430px",
      },
      boxShadow: {
        app: "0 0 40px rgba(28, 43, 58, 0.08)",
        card: "0 1px 2px rgba(28, 43, 58, 0.06)",
      },
    },
  },
  plugins: [],
};

export default config;

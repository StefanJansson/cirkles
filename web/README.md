# Circles – Webbfrontend

Mobilanpassad frontend för Circles, byggd med **Next.js 15**, **TypeScript** och
**Tailwind CSS**. Gränssnittet är helt på svenska och följer en skandinavisk,
avskalad design.

## Kom igång

```bash
npm install
cp .env.local.example .env.local   # peka NEXT_PUBLIC_API_URL mot ditt API
npm run dev                        # http://localhost:3000
```

Bygg för produktion:

```bash
npm run build
npm run start
```

## Miljövariabler

| Variabel              | Beskrivning                          | Standard                |
| --------------------- | ------------------------------------ | ----------------------- |
| `NEXT_PUBLIC_API_URL` | Bas-URL till Circles-API:t (backend) | `http://localhost:5292` |

## Struktur

```
src/
  app/
    layout.tsx              # Rot-layout, <html lang="sv">, AuthProvider
    page.tsx               # Omdirigerar till /hem
    login/page.tsx         # Inloggning (lösenord + lösenordsfri magisk länk)
    (app)/                 # Skyddad zon – kräver inloggning
      layout.tsx           # Klientbaserad auth-guard + skal + bottennavigering
      hem/page.tsx         # Startsida med "Mina cirklar"
      cirklar/page.tsx     # Lista över cirklar
      cirklar/[id]/page.tsx# Cirkeldetalj med medlemmar
      profil/page.tsx      # Profil + utloggning
  lib/
    api.ts                 # Typad API-klient med Bearer-token
    auth.tsx               # Auth-context (JWT i localStorage)
    labels.ts              # Svenska etiketter för roller/cirkeltyper
  components/
    BottomNav.tsx          # Bottennavigering (Hem/Cirklar/Profil)
    CircleCard.tsx         # Kort för en cirkel
    icons.tsx              # Inline-SVG-ikoner
    Spinner.tsx            # Laddningsindikatorer
```

## Autentisering & behörighet

- JWT lagras i `localStorage` och skickas som `Authorization: Bearer <token>`.
- Sessionen återupprättas vid sidladdning via `GET /api/auth/me`.
- Skyddet av `/hem`, `/cirklar` och `/profil` sker med en **klientbaserad guard**
  i `(app)/layout.tsx`. Server/edge-middleware kan inte läsa `localStorage`, så
  guarden körs i klienten och omdirigerar till `/login` när token saknas.
- Behörighet är backend-styrd: gränssnittet visar endast det som API:t returnerar.
  Härledd åtkomst markeras diskret med "Härledd åtkomst".

## Design

Skandinavisk palett: gräddvit bakgrund (`#FAFAF8`), marinblå text (`#1C2B3A`),
skogsgrön accent (`#4A7C59`) och tunna hårlinjer (`#E8E6E1`). Innehållet är
centrerat i en 430 px bred kolumn med mjuk skugga på större skärmar.

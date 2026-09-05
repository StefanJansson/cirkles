# Circles

Circles is a communication and identity platform for organizational contexts such
as sports clubs and teams. This repository contains the C# backend: an
**ASP.NET Core Web API** built on **EF Core** and **PostgreSQL**, structured as a
**modular monolith**.

The first use case is **Uppsala IK**, a fictional Swedish sports club, which the
project seeds with realistic demo data.

---

## The domain model

The single most important idea in Circles is that **User Account, Person and
Membership are three distinct concepts**. Conflating them is the mistake this
model is specifically designed to avoid.

| Concept | Answers the question | Notes |
| --- | --- | --- |
| **Person** | *Who is this human being?* | Identity only. A person can exist with **no** way to log in. |
| **UserAccount** | *How does someone authenticate?* | Email + password hash. Links to a Person via a **nullable** `PersonId`. |
| **Membership** | *What does this human belong to, and in what role?* | Time-based association of a Person with a Circle. |

### Why the separation matters

- **A Person may exist without a UserAccount.** Children are registered as players
  long before they ever have a login. In the seed data, *Alexander Andersson* (10)
  and *Lisa Berg* are people with memberships but **no** user account.
- **Authentication is not identity.** A `UserAccount` is only a set of
  credentials; it points *at* a Person so that, once logged in, the system can
  resolve the human and derive what they may do.
- **Membership is not identity either.** People join and leave circles over time;
  the Person stays the same.

### Entities

- **Person** — `Id, FirstName, LastName, CreatedAt`
- **UserAccount** — `Id, Email, PasswordHash, PersonId (FK, nullable), CreatedAt`
- **Relationship** — `Id, FromPersonId, ToPersonId, Type, ValidFrom, ValidUntil?`
  - explicit, time-based links between people:
    `GuardianOf`, `ChildOf`, `LeaderOf`, `ContactPerson`
- **Organization** — `Id, Name, Slug, CreatedAt` — owns its circles
- **Circle** — `Id, OrganizationId, ParentCircleId?, Name, Slug, Type, CreatedAt`
  - types: `Team`, `Board`, `Officials`, `General`; nestable via `ParentCircleId`
- **Membership** — `Id, PersonId, CircleId, Role, ValidFrom, ValidUntil?`
  - roles: `Player, Guardian, Coach, Leader, Administrator, Member`
- **RolePermission** — maps a role to a `PermissionType`
  - permissions: `ReadPosts, CreateDiscussion, Comment, CreatePoll, Vote,
    CreateTask, AdministerMembers, PublishAnnouncements, ViewMemberList,
    ViewHistoricalInfo`

### Time-based and historical by design

Both **memberships** and **relationships** carry `ValidFrom` / `ValidUntil`.

> Memberships are **never deleted** (neither hard- nor soft-deleted). When a
> membership ends, its `ValidUntil` is set and the row is kept, so **historical
> membership remains fully representable**.

### Organizations own their circles

Circles belong to the organization, not to the people currently in them. When a
coach leaves, their membership expires — **the circle (e.g. the team) persists**.

---

## The authorization model

Access is **never** stored as a flag on a user. It is always *derived* from the
domain model:

```
Person → active membership  OR  derived relationship access
       → circle → role → permission → resource
```

`IAuthorizationService` (implemented in `Circles.Application`) answers:

- `CanPersonAccessCircleAsync(personId, circleId)` — true if the person has an
  active direct membership **or** derived access to the circle.
- `GetPersonPermissionsInCircleAsync(personId, circleId)` — the effective set of
  permissions, combining all active memberships and any derived access.
- `GetAccessibleCircleIdsAsync(personId)` — every circle the person can reach.

### Derived guardian access

A **guardian of a child who is an active member of a circle automatically gets
derived, read-only access to that circle** — without any membership of their own.

In the seed data, *Johan Andersson* is the guardian of *Alexander*, who plays in
**P2016**. Johan therefore gains **derived** access to P2016
(`ReadPosts`, `ViewMemberList`) even though he has **no direct membership** there.
His only direct membership is in *Funktionärer*.

The role → permission mapping lives in data (`role_permissions`) rather than in
code, so it is auditable and adjustable. See
`Circles.Infrastructure/Seeding/RolePermissionMap.cs`.

---

## Project structure (modular monolith)

```
Circles.sln
src/
  Circles.Domain          # Entities, enums, domain interfaces (no dependencies)
  Circles.Infrastructure  # EF Core DbContext, configurations, migrations, seeding
  Circles.Application     # Authorization service, query services, DTOs
  Circles.API             # ASP.NET Core Web API: FastEndpoints, startup, DI
    Features/             # Vertical slices — one folder per feature area
      Persons/           #   ListPersons, GetPersonCircles, GetPersonPermissions
      Organizations/     #   ListOrganizations, GetOrganizationCircles
      Circles/           #   GetCircleMembers
      Health/            #   Health
```

Dependency direction: `API → Application → Infrastructure → Domain`
(Domain depends on nothing).

### API layer: FastEndpoints (REPR pattern)

The API is built with **[FastEndpoints](https://fast-endpoints.com/)** (v8.3.0) rather
than MVC controllers. Each endpoint is a single self-contained class following the
**REPR** pattern (Request → Endpoint → Response) and lives in its own file under
`Features/<Area>/`, so the request contract, route, and handler for one operation
are always together. Endpoints delegate to `CirclesQueryService` in the
Application layer and reuse the same DTOs as before.

### Technology stack

- **.NET 10.0** (latest)
- **EF Core 10.0.0** with **Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0**
- **FastEndpoints 8.3.0** + **FastEndpoints.Swagger 8.3.0**
- **PostgreSQL 14+**

---

## Running locally

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) 14+ running and reachable

### 1. Create the database

```bash
createdb circles          # or: psql -c "CREATE DATABASE circles;"
```

### 2. Configure the connection string

The default (in `src/Circles.API/appsettings.json`) is:

```
Host=localhost;Port=5432;Database=circles;Username=postgres;Password=postgres
```

Override it without editing files via an environment variable:

```bash
export ConnectionStrings__Circles="Host=localhost;Port=5432;Database=circles;Username=postgres;Password=postgres"
```

### 3. Run

```bash
dotnet run --project src/Circles.API
```

On startup the API **applies EF Core migrations** and **seeds the Uppsala IK demo
data** automatically (both are idempotent). Swagger UI is available in the
Development environment at `/swagger`.

> To manage migrations manually you need the EF tools:
> `dotnet tool install --global dotnet-ef --version 8.0.11`
> then e.g.
> `dotnet ef migrations add <Name> --project src/Circles.Infrastructure --startup-project src/Circles.API`

---

## Seed data (Uppsala IK)

**Organization:** Uppsala IK

**Circles (hierarchy):**

```
Uppsala IK (root)
├── P2016        (Team)
├── P2014        (Team)
├── F2016        (Team)
├── Styrelsen    (Board)
└── Funktionärer (General / Officials)
```

**People & accounts:**

| Person | User account | Note |
| --- | --- | --- |
| Johan Andersson | `johan@example.com` | Guardian of Alexander |
| Alexander Andersson | *(none)* | Child, 10 years old |
| Lisa Berg | *(none)* | Child |
| Anna Berg | `anna@example.com` | Guardian of Lisa |
| Erik Svensson | `erik@example.com` | Coach |
| Maria Lindgren | `maria@example.com` | Club administrator |

> Demo accounts use a placeholder password hash — this seed is **not** a real
> credential store.

**Relationships (active):** Johan `GuardianOf` Alexander · Anna `GuardianOf` Lisa

**Memberships (active):**

| Person | Circle | Role |
| --- | --- | --- |
| Alexander | P2016 | Player |
| Lisa | F2016 | Player |
| Erik | P2016 | Coach |
| Maria | Uppsala IK (root) | Administrator |
| Johan | Funktionärer | Member |

Johan has **no direct membership in P2016**; his access there is **derived** from
being Alexander's guardian.

---

## REST API endpoints

| Method & path | Description |
| --- | --- |
| `GET /health` | Health check |
| `GET /api/persons` | List all persons (with account info) |
| `GET /api/persons/{id}/circles` | Circles the person can access (Direct / Derived) |
| `GET /api/persons/{id}/permissions/{circleId}` | Person's permissions in a circle |
| `GET /api/organizations` | List organizations |
| `GET /api/organizations/{id}/circles` | Circle hierarchy (nested tree) |
| `GET /api/circles/{id}/members` | Active members of a circle |

### Quick verification

```bash
# List people — note Alexander and Lisa have no account
curl localhost:5000/api/persons

# Johan's circles — Funktionärer (Direct) and P2016 (Derived, as guardian)
curl localhost:5000/api/persons/<johan-id>/circles

# Johan's permissions in P2016 — derived read-only: ReadPosts, ViewMemberList
curl localhost:5000/api/persons/<johan-id>/permissions/<p2016-id>
```

Enums are serialized as strings throughout the API for readability.

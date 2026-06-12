# Shva — Transaction Approval Simulator

Full-stack take-home assignment: a web app that simulates credit transactions and decides
whether each one is **Approved** or **Rejected** based on the **local banking hours of the
selected region**.

![Stack](https://img.shields.io/badge/React-TypeScript-blue) ![Stack](https://img.shields.io/badge/.NET-10-purple) ![Stack](https://img.shields.io/badge/MSSQL-EF%20Core-red)

---

## Tech Stack

| Layer    | Technology |
|----------|------------|
| Frontend | React 18 + TypeScript + Vite, CSS Modules (no UI framework) |
| Backend  | .NET 10 Web API (C#), controller-based |
| ORM      | Entity Framework Core (code-first migrations) |
| Database | MSSQL Server 2022 |
| Infra    | Docker Compose (client + api + db) |
| Auth     | JWT (signup / login) — bonus |
| i18n     | English / Hebrew with LTR / RTL — bonus |

## Architecture

```
React client (nginx :8080)
      |  /api/* proxied by nginx (no CORS needed in Docker)
      v
.NET Web API (:5000 -> container :8080)
      |  EF Core
      v
MSSQL (:1433)
```

- The **backend owns all business logic** — the frontend never decides approval.
- All submitted transactions (approved **and** rejected) are persisted.
- Layered API project: `Controllers` → `Application/Services` → `Infrastructure/Persistence`,
  with `Contracts` (request/response DTOs) and `Domain` (entities, enums).

## Business Rule

> A transaction is **Approved** only if the local time in the selected region falls within
> standard banking hours — **08:00 (inclusive) to 18:00 (exclusive)**. Otherwise **Rejected**.

### Timezone handling (key design decision)

The assignment requires the backend to *"accurately determine what the local time was in the
selected country at that exact moment."* Therefore the submitted time is treated as an
**instant**, not as region-local time:

1. The client builds an ISO-8601 instant from **today's date + the picked HH:mm in the
   browser's time zone** (offset included), e.g. `2026-06-11T20:00:00+03:00`.
2. The backend converts that instant to the selected region's IANA time zone
   (via `TimeZoneConverter`, which works on both Windows and Linux) and applies the rule.

So picking *20:00* in Israel for region *Japan* checks what time it is **in Japan** at that
moment (~03:00 → Rejected). Real time-zone rules (including DST) are used — never fixed UTC
offsets. The decision logic is isolated in
`TransactionApprovalService` so the alternative interpretation ("the picked time *is* the
region's local time") is a one-line change — a deliberate discussion point.

### Supported regions

| Id   | Region  | IANA Time Zone   |
|------|---------|------------------|
| IL   | Israel  | Asia/Jerusalem   |
| FR   | France  | Europe/Paris     |
| CY   | Cyprus  | Asia/Nicosia     |
| IT   | Italy   | Europe/Rome      |
| JP   | Japan   | Asia/Tokyo       |
| US   | USA     | America/New_York |

## Run with Docker Compose (recommended)

Prerequisite: Docker Desktop.

```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| Client  | http://localhost:8080 |
| API + Swagger | http://localhost:5000/swagger |
| MSSQL   | localhost,1433 (sa / `Shva_Local_Dev_Pa55!`) |

The API applies EF Core migrations automatically on startup (with retries while MSSQL boots).
First boot of the MSSQL container can take ~30–60 seconds.

**Using the app:** sign up (any username ≥3 chars, password ≥6 chars), pick a region and a
time, press **OK**. Approved transactions appear in the bottom cards.

## Run manually (local dev)

Prerequisites: .NET 10 SDK, Node.js 20+, an MSSQL instance.

```bash
# 1. Database — either your own MSSQL, or just the db service:
docker compose up db -d

# 2. API (applies migrations automatically; listens on http://localhost:5000)
dotnet run --project server/Shva.TransactionSimulator.Api
# adjust ConnectionStrings:DefaultConnection in appsettings.json if needed

# 3. Client (dev server on :3000, proxies /api to http://localhost:5000)
cd client
npm install
npm run dev
```

Migrations are managed with the local tool manifest:

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project server/Shva.TransactionSimulator.Api
```

## Run tests

```bash
dotnet test server/ShvaTransactionSimulator.slnx
```

15 tests cover: banking-hour boundaries (07:59 / 08:00 / 17:59 / 18:00 / 18:01),
instant→region-local conversion (incl. DST and date rollover), offset handling, unknown
region rejection, persistence of both statuses, approved-only retrieval, ordering, and limits.

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET  | `/api/regions` | — | Supported regions (drives the dropdown) |
| POST | `/api/transactions/simulate` | JWT | Simulate; persists and returns the decision |
| GET  | `/api/transactions/approved?limit=20` | JWT | Approved only, newest first (max 100) |
| POST | `/api/auth/signup` | — | Create user, returns JWT |
| POST | `/api/auth/login` | — | Returns JWT |

Notes:
- A **rejected** transaction is still a successful simulation → `200 OK` with
  `status: "Rejected"`. `400` (ProblemDetails) is returned only for invalid input
  (e.g. unknown `regionId`).
- Example request:
  ```json
  POST /api/transactions/simulate
  { "regionId": "JP", "submittedAt": "2026-06-11T20:00:00+03:00" }
  ```

## Database

Table `Transactions`: region snapshot (id, name, tz), the submitted UTC instant, the computed
region-local date/time, status (stored as string for easy inspection), decision reason, and
creation timestamp. Indexed on `(Status, CreatedAtUtc)` to serve the approved-list query.
Table `Users`: username (unique) + ASP.NET Identity password hash.

## Bonus Features Implemented

- ✅ **Docker Compose** — client, API and MSSQL with a single command.
- ✅ **Localization** — ENG/Hebrew toggle (top-right); `dir`/`lang` switch with full RTL
  layout flip via CSS logical properties. Illustration text is intentionally untouched.
- ✅ **Authentication** — signup/login with JWT; transaction endpoints require a token.

## Assumptions

- A transaction consists of **region + time** only (per the Figma) — no amount/currency.
- Banking hours: **08:00 inclusive, 18:00 exclusive** (constants in `TransactionApprovalService`).
- The picked time is interpreted as an **instant in the user's time zone** (see Timezone
  handling above) — switchable to "region-local input" with a one-line change.
- All transactions are stored; only approved ones are displayed.
- Dev-only credentials/JWT key live in `appsettings.json` / `docker-compose.yml` for
  reviewer convenience — in production these would come from a secret store.

## Known Limitations

- No refresh tokens / password reset — auth is intentionally minimal (bonus scope).
- The region list is hardcoded server-side (assignment scope); it is served via
  `GET /api/regions` so a DB-backed list is a drop-in change.
- `npm install` (not `npm ci`) in the client image — no lockfile is committed.

## Project Structure

```
├── docker-compose.yml
├── server/
│   ├── Shva.TransactionSimulator.Api/
│   │   ├── Controllers/          # Regions, Transactions, Auth
│   │   ├── Contracts/            # Request/response DTOs
│   │   ├── Domain/               # Entities, enums, region model
│   │   ├── Application/          # Interfaces + services (business logic)
│   │   ├── Infrastructure/       # EF Core DbContext + configurations
│   │   └── Migrations/
│   └── Shva.TransactionSimulator.Tests/
└── client/
    └── src/
        ├── api/                  # fetch wrapper + endpoint clients
        ├── components/           # Header, RegionSelect, TimeInput, cards…
        ├── hooks/                # useRegions, useApprovedTransactions, useSimulation
        ├── i18n/                 # translations + language context (RTL/LTR)
        ├── auth/                 # auth context (JWT)
        └── pages/                # HomePage, AuthPage
```


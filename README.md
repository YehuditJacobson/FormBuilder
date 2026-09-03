# Form Builder & Milestone Management — PoC

A proof of concept for the HR department: build organizational form templates and attach a
dynamic approval route (milestones) to each.

- **Server** — .NET 8 Web API, Clean Architecture (Domain / Application / Infrastructure / Api)
- **Client** — Angular 22 standalone (`client/`), Reactive Forms *(feature UI from Step 08)*
- **Data** — EF Core 8; SQL Server in production, SQLite / InMemory for the PoC

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full design, the data model, the API contract,
and the staged build plan.

## Prerequisites

- .NET SDK 8.0 or later (`global.json` pins the band; roll-forward to a newer major is allowed)

## Build & test

```bash
# backend
dotnet build FormBuilder.sln
dotnet test  FormBuilder.sln

# run the API  (http://localhost:5052, Swagger at /swagger)
dotnet run --project src/FormBuilder.Api

# client
cd client
npm start        # ng serve on :4200, proxies /api -> http://localhost:5052
npm run build
npm test
```

The API is configured for SQL Server LocalDB (`(localdb)\MSSQLLocalDB`, database
`FormBuilder`). Set `Database__Provider=Sqlite` or `=InMemory` (see `appsettings.json`) to
switch. Details: [docs/running-locally.md](docs/running-locally.md).

## Solution layout

```
src/
  FormBuilder.Domain          entities, enums, domain rules — no dependencies
  FormBuilder.Application     CQRS use cases, DTOs, validators, abstractions
  FormBuilder.Infrastructure  EF Core: DbContext, configurations, repositories, migrations
  FormBuilder.Api             controllers, middleware, composition root
tests/
  FormBuilder.Domain.Tests
  FormBuilder.Application.Tests
  FormBuilder.Api.IntegrationTests
```

NuGet versions are managed centrally in `Directory.Packages.props`; shared build settings
live in `Directory.Build.props`.

## Progress

- [x] **Step 00** — solution scaffolding
- [x] **Step 01** — domain layer *(31 tests)*
- [x] **Step 02** — application layer — CQRS / MediatR *(14 tests)*
- [x] **Step 03** — infrastructure layer — EF Core + InitialCreate migration *(5 tests)*
- [x] **Step 04** — API layer — 3 endpoints, ProblemDetails, CORS, Swagger *(verified live)*
- [x] **Step 05** — backend integration tests + dev seeder *(5 tests → 55 backend total)*
- [x] **Step 06** — Angular scaffold *(in `client/`)*
- [x] **Step 07** — core & shared — models, API service, error interceptor, RTL shell *(7 tests)*
- [x] **Step 08** — form-builder page — dynamic fields + approval route + live preview
- [x] **Step 09** — list & view pages + signal store *(23 client tests)*
- [x] **Step 10** — integration & polish — full `create → list → view` verified end-to-end
- [ ] Step 11 — theory write-up & recording

UI language: **Hebrew, RTL** (see `design/` mockups).

Per-step notes: [docs/](docs/)

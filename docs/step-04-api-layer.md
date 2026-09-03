# Step 04 — API layer

**Status:** complete · solution builds clean · all 3 endpoints exercised live (201 / 200 / 200 / 400 / 404)
**Scope:** `src/FormBuilder.Api`
**Depends on:** Application (Step 02), Infrastructure (Step 03).

---

## Goal

A thin HTTP surface over the CQRS use cases: three endpoints, RFC 7807 problem responses,
CORS for the Angular dev origin, Swagger, structured logging, and provider-aware DB startup.

---

## Files

| File | Role |
|------|------|
| `Program.cs` | composition root — Serilog, controllers + `JsonStringEnumConverter`, API versioning, Swagger, ProblemDetails + `GlobalExceptionHandler`, CORS, `AddApplication()` + `AddInfrastructure(config)`, DB init, `public partial class Program` for tests |
| `Controllers/FormsController.cs` | `POST /api/v1/forms`, `GET /api/v1/forms`, `GET /api/v1/forms/{id:guid}` — build a command/query, `sender.Send`, map the `Result` |
| `Contracts/CreateFormTemplateResponse.cs` | `{ id }` body for the 201 |
| `Extensions/ResultExtensions.cs` | `Result<T>` → `IActionResult`: value on success; 404 / 400 / 409 / 500 problem by `ErrorType` on failure |
| `Infrastructure/GlobalExceptionHandler.cs` | `IExceptionHandler` — `ValidationException` → 400 `ValidationProblemDetails` (grouped by property); anything else → logged 500 `ProblemDetails` |
| `Infrastructure/HttpContextCurrentUser.cs` | `ICurrentUser` from a name-identifier claim → `X-User-Id` header → `"system"`; registered last so it beats the infrastructure default |
| `Infrastructure/DatabaseInitializer.cs` | SQL Server → `MigrateAsync()`; SQLite / InMemory → `EnsureCreatedAsync()` |
| `appsettings.json` | `Database:Provider` (`Sqlite`), `ConnectionStrings:Default`, `Cors:AllowedOrigins`, Serilog levels |

Removed the template's `WeatherForecast*` earlier (Step 00).

---

## Contract

| Method | Route | Body | Success | Errors |
|--------|-------|------|---------|--------|
| POST | `/api/v1/forms` | `CreateFormTemplateRequest` | `201` + `Location` + `{ id }` | `400` `ValidationProblemDetails` |
| GET | `/api/v1/forms` | — | `200` + `FormTemplateSummary[]` (newest first) | — |
| GET | `/api/v1/forms/{id}` | — | `200` + `FormTemplateDetail` | `404` `ProblemDetails` |

Enums serialize as strings (`"Text"`, `"Approve"`, `"Published"`). Field/step **order is not
sent** — the server assigns it from list position.

### Verified live (InMemory provider)

```
POST valid            -> 201  {"id":"1685baec-…"}
GET  /api/v1/forms    -> 200  [{ …, "status":"Draft", "fieldCount":2, "approvalStepCount":2 }]
GET  /api/v1/forms/{id} -> 200  full graph, fields & steps ordered, enums as strings
POST { name:"", fields:[], approvalSteps:[] } -> 400  errors: Request.Name, Request.Fields, Request.ApprovalSteps
GET  /api/v1/forms/{zeros} -> 404  ProblemDetails "Form template '…' was not found."
```

(Hebrew text in responses is correct UTF-8; the `?` glyphs seen in a Windows console are a
terminal code-page artefact, not the payload.)

---

## Decisions

- **Thin controller, `Result` mapping in one place.** `ResultExtensions.ToActionResult`
  translates domain-expected failures; the exception handler covers validation and the
  unexpected. Controllers never see a `DbContext`.
- **`ICurrentUser` split.** Infrastructure keeps a no-dependency `"system"` default; the API
  registers `HttpContextCurrentUser` after `AddInfrastructure`, so the request-aware one wins
  at runtime and tests still get the default.
- **Provider-aware startup.** One code path migrates SQL Server, another creates SQLite /
  InMemory from the model — matches the "model it as SQL Server, run it on SQLite" brief.
- **API versioning** via URL segment (`/api/v1/…`), default `1.0`, one Swagger doc. Small
  overhead, keeps a v2 door open.
- **Run profiles**: `dotnet run --project src/FormBuilder.Api` → `http://localhost:5052`
  (`https://localhost:7104`). The Angular dev proxy already points at `5052`.

---

## Next backend step — Step 05

`WebApplicationFactory<Program>` integration tests for the three endpoints against a SQLite /
InMemory test host, plus an optional sample-data seeder.

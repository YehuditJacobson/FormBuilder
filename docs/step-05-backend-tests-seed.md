# Step 05 — Backend integration tests & seed

**Status:** complete · `dotnet test` → **Api.IntegrationTests 5 passed** (Domain 31, Application 14, Infrastructure 5 → **55 total**)
**Scope:** `tests/FormBuilder.Api.IntegrationTests` + `src/FormBuilder.Api/Infrastructure/DataSeeder.cs`

---

## Goal

Prove the three endpoints work through the real HTTP pipeline (routing, model binding,
JSON, validation, ProblemDetails, EF), and give Development a sample row to show.

---

## Files

| File | Role |
|------|------|
| `FormBuilderApiFactory.cs` | `WebApplicationFactory<Program>`: environment `Testing` (seeder + Swagger off); swaps `AppDbContext` for a **per-factory** in-memory database (one db name built once, not per request); exposes `Json` options matching the API |
| `FormsEndpointsTests.cs` | 5 tests via `HttpClient` |
| `src/FormBuilder.Api/Infrastructure/DataSeeder.cs` | inserts one published "בקשת חופשה" template (3 fields, 2 steps) on first run when Development and the table is empty; wired into `Program.cs` after `DatabaseInitializer` |

## Tests

| Test | Asserts |
|------|---------|
| `Post_valid_returns_201_with_id_and_location` | `201`, non-empty id, `Location` header contains the id |
| `Post_then_get_by_id_returns_the_full_ordered_graph` | fields & steps in `Order`, enums as strings, `Status = Draft` |
| `Get_list_returns_created_templates_newest_first_with_counts` | two POSTs come back newest-first with `FieldCount` / `ApprovalStepCount` |
| `Post_invalid_returns_400_with_field_errors` | `400`, `errors` keys `Request.Name` / `Request.Fields` / `Request.ApprovalSteps` |
| `Get_unknown_id_returns_404` | `404` |

---

## Decisions & gotchas

- **In-memory DB name built once.** `AddDbContext(o => o.UseInMemoryDatabase($"…{Guid.NewGuid()}"))`
  runs the lambda **per `DbContextOptions` resolution — i.e. per HTTP request** — so a Guid
  built *inside* the lambda gives every request its own empty database (POST writes to one,
  GET reads another). Fixed by computing the name once as a factory field. This was the
  cause of the first run's 2 failures.
- **Environment `Testing`** keeps the dev seeder and Swagger out of the test host, so the
  list tests start from an empty table.
- **`Program` is `public partial`** (added in Step 04) so `WebApplicationFactory<Program>` can host it.
- Requests are sent with the API's `JsonSerializerOptions` (string enums) for symmetry with
  how the client will call it.

---

## Next backend step — Step 10

Run the API and the Angular client together and confirm the create → list → get round-trip
in the browser; wire a `README` run section (already drafted).

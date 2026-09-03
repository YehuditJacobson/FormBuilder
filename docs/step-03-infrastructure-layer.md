# Step 03 — Infrastructure layer

**Status:** complete · `dotnet test` → **Infrastructure.Tests 5 passed** (Domain 31, Application 14)
**Scope:** `src/FormBuilder.Infrastructure` + `tests/FormBuilder.Infrastructure.Tests`
**Depends on:** Application (Step 02). Adds package: `Microsoft.Extensions.Configuration.Abstractions`.

---

## Goal

Put EF Core behind the Application abstractions: the `DbContext`, entity mappings,
repository / query / unit-of-work implementations, the provider switch, and the first
migration.

---

## Files

### `src/FormBuilder.Infrastructure`

| Area | File | Role |
|------|------|------|
| Persistence | `AppDbContext.cs` | `DbSet`s for `FormTemplate`, `FormField`, `ApprovalStep`; `ApplyConfigurationsFromAssembly` |
| Persistence/Configurations | `FormTemplateConfiguration.cs` | keys, lengths, `Status` as string, cascade to children, **navigations mapped to the private backing fields** (`PropertyAccessMode.Field`) |
| Persistence/Configurations | `FormFieldConfiguration.cs` | lengths, `FieldType` as string, unique index on `(FormTemplateId, Order)` |
| Persistence/Configurations | `ApprovalStepConfiguration.cs` | lengths, `ActionType` as string, unique index on `(FormTemplateId, Order)` |
| Persistence/Repositories | `FormTemplateRepository.cs` | `AddAsync` |
| Persistence/Queries | `FormTemplateQueries.cs` | `GetSummariesAsync` (projection + child counts, `AsNoTracking`, newest first); `GetDetailAsync` (full graph projection, children ordered by `Order`) |
| Persistence | `UnitOfWork.cs` | wraps `SaveChangesAsync` |
| Persistence | `AppDbContextFactory.cs` | `IDesignTimeDbContextFactory` (SQL Server) so `dotnet ef` works without the API host |
| Persistence/Migrations | `20260903072033_InitialCreate.*` + `AppDbContextModelSnapshot` | the SQL Server schema |
| Time | `SystemDateTimeProvider.cs` | `DateTime.UtcNow` |
| Identity | `SystemCurrentUser.cs` | returns `"system"` — the API replaces this with an HTTP-aware one in Step 04 |
| — | `DependencyInjection.cs` | `AddInfrastructure(config)` — provider switch + service registrations |

### `tests/FormBuilder.Infrastructure.Tests` (5 tests, SQLite in-memory)

- repository persists the template with its fields and steps;
- `GetDetailAsync` projects the graph with children in `Order`, enums round-tripped;
- `GetDetailAsync` returns `null` for an unknown id;
- `GetSummariesAsync` returns child counts, newest first;
- deleting a template cascades to `FormFields` and `ApprovalSteps`.

`InternalsVisibleTo("FormBuilder.Infrastructure.Tests")` exposes the `internal` repository /
query classes to the test project.

---

## Decisions

- **The migration targets SQL Server**, so the checked-in schema is the canonical relational
  one the exam asks for (`uniqueidentifier`, `nvarchar(n)`, `datetime2`, `bit`, cascade FKs,
  unique composite indexes). At runtime:
  - `Database:Provider = SqlServer` → `Database.Migrate()` applies it (wired in Step 04);
  - `Sqlite` / `InMemory` → `EnsureCreated()` builds the schema from the model instead.
- **Provider switch** lives in one method: `configuration["Database:Provider"]` selects
  `UseSqlServer` / `UseSqlite` / `UseInMemoryDatabase`; unknown value throws at startup.
  Default is `Sqlite` with `Data Source=formbuilder.db`.
- **Read model projected in SQL.** The query class builds `FormTemplateDetailDto` /
  `FormTemplateSummaryDto` directly in the `Select`, so a list call is one query with
  `COUNT` sub-selects and a detail call is one query with ordered collection sub-selects — no
  aggregate materialisation. Verified translatable by the SQLite tests.
- **Aggregate encapsulation preserved.** The read-only `Fields` / `ApprovalSteps` collections
  are mapped to the `_fields` / `_approvalSteps` backing fields, so EF writes them without a
  public setter.
- **`ICurrentUser` default here, real one in the API.** Infrastructure has no ASP.NET
  dependency; `SystemCurrentUser` is the fallback for design-time / tests, and Step 04
  registers an `IHttpContextAccessor`-based implementation that wins at runtime.

---

## EF tooling

Local tool manifest added (`.config/dotnet-tools.json`) pinning `dotnet-ef 8.0.10`.

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <Name> --project src/FormBuilder.Infrastructure --startup-project src/FormBuilder.Infrastructure --output-dir Persistence/Migrations
```

---

## Next backend step — Step 04: API layer

Composition root + `FormsController` (3 endpoints), ProblemDetails, CORS, Serilog,
`JsonStringEnumConverter`, and provider-aware DB init (`Migrate()` for SQL Server,
`EnsureCreated()` otherwise).

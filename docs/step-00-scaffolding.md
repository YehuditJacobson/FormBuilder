# Step 00 — Solution scaffolding

**Status:** complete · `dotnet build FormBuilder.sln` → **0 warnings, 0 errors** (7/7 projects)
**Git:** no repository initialised, nothing committed or pushed (per instruction)
**Target framework:** `net8.0` (LTS) — builds on SDK 8.x and 9.x

---

## Goal of this step

Stand up an empty but correctly wired Clean Architecture solution: the four backend
projects, three test projects, the dependency graph, and the shared build / package
configuration. No domain code, no EF model, no controllers yet.

---

## Files created

| File | Purpose |
|------|---------|
| `FormBuilder.sln` | Solution, all 7 projects added |
| `global.json` | Pins the SDK to the 8.0 band; `rollForward: latestMajor` so a newer SDK still works |
| `Directory.Build.props` | Shared MSBuild settings for every project (see below) |
| `Directory.Packages.props` | Central Package Management — every NuGet version declared once |
| `tests/Directory.Build.props` | Test-only settings + the common test package set (xunit, FluentAssertions, coverlet) |
| `.editorconfig` | C# coding conventions (file-scoped namespaces, `_camelCase` fields, `var` rules, analyzer severities) |
| `.gitignore` | .NET + Angular/Node + SQLite + IDE artefacts |
| `README.md` | Build instructions and the step checklist |
| `ARCHITECTURE.md` | Full design (created earlier, unchanged this step) |

## Projects

```
src/
  FormBuilder.Domain          net8.0 classlib   — 0 project refs, 0 packages
  FormBuilder.Application     net8.0 classlib   — ref: Domain
  FormBuilder.Infrastructure  net8.0 classlib   — ref: Application
  FormBuilder.Api             net8.0 web        — ref: Application, Infrastructure
tests/
  FormBuilder.Domain.Tests            — ref: Domain
  FormBuilder.Application.Tests       — ref: Application
  FormBuilder.Api.IntegrationTests    — ref: Api
```

Dependency direction matches the Clean Architecture rule in `ARCHITECTURE.md` §3:
`Api → Infrastructure → Application → Domain`, and `Api → Application` directly.
`Domain` depends on nothing.

## Package references (by project)

| Project | Packages |
|---------|----------|
| `Domain` | *none* |
| `Application` | `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions` |
| `Infrastructure` | `Microsoft.EntityFrameworkCore` (+ `.SqlServer`, `.Sqlite`, `.InMemory`, `.Design`) |
| `Api` | `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`, `Asp.Versioning.Mvc`, `Asp.Versioning.Mvc.ApiExplorer`, `EFCore.Design` |
| test projects | `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`, `FluentAssertions`; `NSubstitute` (Application.Tests); `Microsoft.AspNetCore.Mvc.Testing` (Api.IntegrationTests) |

Versions live only in `Directory.Packages.props`; `.csproj` files reference packages by name.

## `Directory.Build.props` — shared settings

- `TargetFramework=net8.0`, `LangVersion=latest`, `Nullable=enable`, `ImplicitUsings=enable`
- `TreatWarningsAsErrors=true` — clean-code discipline
- `WarningsNotAsErrors=NU1901;NU1902;NU1903;NU1904` — NuGet vulnerability advisories stay
  visible but do not block local iteration
- `EnableNETAnalyzers=true`, `AnalysisLevel=latest-minimum`
- `GenerateDocumentationFile=true` with `CS1591` suppressed — the XML file is produced for
  Swagger without forcing a doc comment on every member
- `Deterministic=true`, `ManagePackageVersionsCentrally=true`

---

## Boilerplate removed

`Class1.cs` (×3), `WeatherForecast.cs`, `WeatherForecastController.cs`,
`FormBuilder.Api.http`, `UnitTest1.cs` (×3).

## Deliberately deferred

- `src/FormBuilder.Api/Program.cs` is still the template's minimal host. The real
  composition root (`AddApplication()`, `AddInfrastructure()`, CORS, Serilog,
  ProblemDetails, API versioning) is **Step 04**.
- Test projects contain no test files yet — they build clean; `dotnet test` reports
  "no tests available".

---

## Next — Step 01: Domain layer

- Entities: `FormTemplate` (aggregate root), `FormField`, `ApprovalStep`, optional `Approver`
- Enums: `FieldType`, `ApprovalActionType`, `TemplateStatus`
- Encapsulated collections (`IReadOnlyCollection` + private backing list); constructors
  enforce required data; `AddField` / `AddApprovalStep` / `RemoveStep` / `Reorder` methods
  keep `Order` contiguous
- `DomainException` type
- Unit tests for every invariant

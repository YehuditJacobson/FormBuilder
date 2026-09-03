# Form Builder & Milestone Management — Architecture

Reference blueprint for the HR forms proof of concept: an organizational **form-template
builder** paired with a **dynamic approval route** (milestones) per template.

| Layer  | Technology |
|--------|------------|
| Client | Angular 17+ · standalone components · Reactive Forms · signal store |
| Server | .NET 8 Web API · MediatR · FluentValidation |
| Data   | EF Core 8 · SQL Server *(SQLite / InMemory for the PoC)* |

---

## 1. System overview

The HR department composes organizational forms (vacation request, sick-leave report, and
the like) and attaches a configurable chain of approvals to each. This PoC delivers the
**authoring** side of that story: create a template, persist it whole, and read it back.

### Two capabilities

- **Form template** — an envelope (name, description, creation date, author, lifecycle
  status) plus an ordered set of dynamic fields the author adds one at a time.
- **Approval route (milestones)** — an ordered list of steps; each step carries its
  position, a name, the approver identity, and the action the approver is permitted to take.

### Guiding principles

- **Clean Architecture** — dependencies point inward; the domain has zero framework references.
- **One class per use case** — CQRS commands and queries keep controllers thin and behaviour testable.
- **The aggregate is the transaction boundary** — a template with its fields and steps is saved atomically.
- **Reactive Forms as the source of truth** on the client — typed `FormGroup` / `FormArray`, no ad-hoc mutable arrays.
- **Server-side validation is authoritative** — the client mirrors it for UX only.

---

## 2. Architecture map

```
Angular SPA  ──HTTPS/JSON──►  API Layer            (Controllers · Middleware · Swagger)
                                   │
                                   ▼
                              Application Layer     (CQRS handlers · Validators · DTOs · Abstractions)
                                   │
                                   ▼
                              Domain Layer          (FormTemplate aggregate · Entities · Enums)
                                   ▲
                              Infrastructure  ──────┘   (depends on Application abstractions)
                              (EF Core · DbContext · Repositories · Unit of Work)
                                   │
                                   ▼
                              SQL Server             (InMemory / SQLite for PoC)
```

Arrows point toward the thing depended upon. The domain and application layers can be
unit-tested with no database and no web host. Swapping SQL Server for SQLite or EF InMemory
is a single line in `AddInfrastructure()`.

---

## 3. Solution structure

```
FormBuilder.sln
├─ Directory.Build.props              # nullable enable, warnings as errors, langversion latest
├─ Directory.Packages.props          # central package version management
├─ src/
│  ├─ FormBuilder.Domain             # entities, enums, value objects, domain errors — no dependencies
│  ├─ FormBuilder.Application        # use cases (CQRS), DTOs, validators, abstractions → refs Domain
│  ├─ FormBuilder.Infrastructure     # EF Core, DbContext, repositories, migrations → refs Application
│  └─ FormBuilder.Api                # controllers, middleware, DI composition → refs Application + Infrastructure
└─ tests/
   ├─ FormBuilder.Domain.Tests        # aggregate invariants
   ├─ FormBuilder.Application.Tests   # handlers against a fake repository
   └─ FormBuilder.Api.IntegrationTests# WebApplicationFactory → 3 endpoints
```

### Project references

| Project          | References                    | Key packages |
|------------------|-------------------------------|--------------|
| `Domain`         | *nothing*                     | — |
| `Application`    | `Domain`                      | MediatR, FluentValidation |
| `Infrastructure` | `Application`                 | EF Core, EF Core.SqlServer, EF Core.Sqlite, EF Core.InMemory |
| `Api`            | `Application`, `Infrastructure`| Swashbuckle, Serilog.AspNetCore, Asp.Versioning |

**Composition root** — `Api` references `Infrastructure` only so `Program.cs` can call
`AddInfrastructure(config)`. Controllers never touch a `DbContext`; they depend on
`ISender` (MediatR) and the abstractions live in `Application`.

---

## 4. Domain model

`FormTemplate` is the **aggregate root**. It owns its fields and its approval steps; nothing
outside the aggregate holds a reference to a child. Collections are encapsulated (private
backing list, exposed as `IReadOnlyCollection`) and mutated only through
intention-revealing methods that enforce invariants.

### FormTemplate — aggregate root

| Field          | Type                        | Notes |
|----------------|-----------------------------|-------|
| `Id`           | `Guid`                      | |
| `Name`         | `string` ≤200, required     | |
| `Description`  | `string?`                   | |
| `CreatedAtUtc` | `DateTime`                  | set from `IDateTimeProvider` |
| `CreatedBy`    | `string`                    | from `ICurrentUser` |
| `Status`       | `TemplateStatus`            | `Draft` / `Published` |
| `Fields`       | `IReadOnlyCollection<FormField>`   | |
| `Steps`        | `IReadOnlyCollection<ApprovalStep>`| |

### FormField — entity

| Field         | Type          | Notes |
|---------------|---------------|-------|
| `Id`          | `Guid`        | |
| `Label`       | `string`, req | |
| `FieldType`   | `FieldType`   | enum |
| `Order`       | `int`         | contiguous from 0 |
| `IsRequired`  | `bool`        | |
| `Placeholder` | `string?`     | |
| `Options`     | `string?`     | JSON, for dropdown |

### ApprovalStep — entity

| Field        | Type                  | Notes |
|--------------|-----------------------|-------|
| `Id`         | `Guid`                | |
| `Order`      | `int`                 | position in route, contiguous from 0 |
| `Name`       | `string`, req         | e.g. "Direct manager approval" |
| `ApproverId` | `Guid?` / `string`    | free text for PoC, FK-ready |
| `ActionType` | `ApprovalActionType`  | enum |

### Approver — entity (optional)

| Field         | Type     |
|---------------|----------|
| `Id`          | `Guid`   |
| `DisplayName` | `string` |
| `Email`       | `string` |

### Enums (Domain)

- `FieldType` — `Text`, `Date`, `Number`, `Checkbox`, `Dropdown`
- `ApprovalActionType` — `Approve`, `Reject`, `ReturnForRevision`, `Sign`, `Acknowledge`
- `TemplateStatus` — `Draft`, `Published`

### Relationships & rules

- `FormTemplate` **1 → \*** `FormField` — required FK, cascade delete.
- `FormTemplate` **1 → \*** `ApprovalStep` — required FK, cascade delete.
- `ApprovalStep` **\* → 0..1** `Approver` — optional FK, restrict delete.
- Unique index on `(FormTemplateId, Order)` for both children — no duplicate positions.
- Invariants in the aggregate: `Order` is contiguous from 0; `AddField` / `AddApprovalStep`
  / `RemoveStep` / `Reorder` keep it so.
- Enums persisted **as strings** via `HasConversion<string>()` — readable rows, safe to
  reorder the enum.

### ER diagram

```
FORM_TEMPLATE ||--o{ FORM_FIELD    : contains
FORM_TEMPLATE ||--o{ APPROVAL_STEP : defines
APPROVER      ||--o{ APPROVAL_STEP : "assigned to"
```

---

## 5. Application layer — CQRS

One request record and one handler per use case, dispatched through MediatR. Cross-cutting
concerns (validation, logging) are pipeline behaviours wrapped around every handler.

### Commands

- `CreateFormTemplateCommand` → `Result<Guid>`
  Payload: name, description, fields[], steps[]. Builds the aggregate via a factory,
  persists through repository + Unit of Work in one transaction.
- `PublishFormTemplateCommand` *(bonus)*
- `DeleteFormTemplateCommand` *(bonus)*

### Queries

- `GetFormTemplatesQuery` → `IReadOnlyList<FormTemplateSummaryDto>`
  id, name, createdAtUtc, createdBy, fieldCount, stepCount — projected, `AsNoTracking`.
- `GetFormTemplateByIdQuery` → `Result<FormTemplateDetailDto>`
  full graph with ordered fields + steps, or `NotFound`.

### Building blocks

- **Abstractions** (interfaces here, implemented in Infrastructure): `IFormTemplateRepository`,
  `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUser`.
- **Validation**: one `AbstractValidator<T>` per command, run by a
  `ValidationBehavior<TReq,TRes>` pipeline behaviour — failures return `400` before the
  handler executes.
- **Mapping**: explicit static `ToDto()` / `ToEntity()` helpers — no reflection-based mapper
  for a PoC; the mapping is visible and debuggable.
- **Result pattern**: `Result<T>` carries typed failures (`NotFound`, `Validation`) so
  handlers don't throw for expected outcomes.
- **Factory**: `FormTemplateFactory.Create(...)` centralises aggregate construction and
  invariant checks.

### Pattern → purpose

| Pattern | Where | Why |
|---------|-------|-----|
| Mediator / CQRS | MediatR handlers | Thin controllers, isolated use cases, easy to test |
| Repository + Unit of Work | Application abstraction, Infra impl | Keeps EF out of the domain; aggregate = transaction boundary |
| Factory | `FormTemplateFactory` | Single place that knows how to assemble a valid template |
| Decorator (pipeline behaviours) | Validation, logging | Cross-cutting concerns without touching handlers |
| Result / Either | Handler return types | Explicit, exception-free error flow |
| Options | Typed config binding | Strongly-typed settings, validated at startup |

---

## 6. Infrastructure layer

EF Core lives here and nowhere else.

- `AppDbContext : DbContext` — `DbSet` for `FormTemplate`, `FormField`, `ApprovalStep`, `Approver`.
- **One `IEntityTypeConfiguration<T>` per entity** — keys, max lengths, required columns,
  relationships, cascade behaviour, enum-to-string conversions, the unique
  `(FormTemplateId, Order)` index. Applied via `ApplyConfigurationsFromAssembly`.
- `FormTemplateRepository : IFormTemplateRepository` — `AddAsync`, `GetByIdAsync`
  (`Include` + `OrderBy(Order)` on both children), `ListSummariesAsync` (`Select`
  projection, `AsNoTracking`).
- `UnitOfWork : IUnitOfWork` — wraps `SaveChangesAsync`; the handler calls it once.
- `SystemDateTimeProvider`, `CurrentUser` — trivial implementations of the abstractions.
- **Provider switch** in `AddInfrastructure(config)`: `UseSqlServer(conn)` in production;
  `UseSqlite` / `UseInMemoryDatabase` when `Database:Provider` says so.
- Startup: `Database.Migrate()` for a relational provider, `EnsureCreated()` for InMemory.
  One EF migration checked in.

---

## 7. API layer & contract

`FormsController` is deliberately dull: build the command or query, `await _sender.Send(...)`,
translate the `Result` to an `IActionResult`.

| Method | Route                    | Request                     | Success                          | Errors |
|--------|--------------------------|-----------------------------|----------------------------------|--------|
| POST   | `/api/v1/forms`          | `CreateFormTemplateRequest` | `201 Created` + `Location` + `{ id }` | `400` ValidationProblem |
| GET    | `/api/v1/forms`          | —                           | `200` + `FormTemplateSummary[]`  | — |
| GET    | `/api/v1/forms/{id}`     | —                           | `200` + `FormTemplateDetail`     | `404` ProblemDetails |

### Request payload shape

```json
{
  "name": "Vacation Request",
  "description": "Annual leave form for all staff",
  "fields": [
    { "label": "Employee name", "fieldType": "Text",   "order": 0, "isRequired": true },
    { "label": "Start date",    "fieldType": "Date",   "order": 1, "isRequired": true },
    { "label": "Days",          "fieldType": "Number", "order": 2, "isRequired": true }
  ],
  "approvalSteps": [
    { "name": "Direct manager",  "actionType": "Approve", "order": 0, "approverId": null },
    { "name": "HR verification", "actionType": "Sign",    "order": 1, "approverId": null }
  ]
}
```

### Cross-cutting concerns

- **Global exception handler** → RFC 7807 `ProblemDetails`; FluentValidation failures → `ValidationProblemDetails`.
- **Serilog** structured logging + request logging middleware.
- **Swagger / OpenAPI** (Swashbuckle) with XML doc comments.
- **CORS** policy allowing `http://localhost:4200` in development.
- **API versioning** (`Asp.Versioning`) — `v1` from day one.
- `Program.cs` stays ~20 lines: `AddApplication()`, `AddInfrastructure(config)`, controllers,
  Swagger, CORS, exception handler, migrate, run.

---

## 8. Angular client

Angular 17+ standalone, `strict` mode, `OnPush` everywhere. The "Create new form" screen is
one typed Reactive Form; two `FormArray`s carry the dynamic parts.

```
src/app/
├─ core/
│  ├─ api/form-template.api.ts        # typed HttpClient wrapper: create / list / getById
│  ├─ interceptors/base-url.interceptor.ts
│  ├─ interceptors/error.interceptor.ts  # normalises ProblemDetails
│  └─ models/                          # interfaces mirroring the API DTOs
├─ shared/ui/                          # dumb, reusable primitives
└─ features/form-builder/
   ├─ pages/
   │  ├─ form-list.page.ts             # smart — consumes the signal store
   │  ├─ form-builder.page.ts          # smart — owns the root FormGroup
   │  └─ form-view.page.ts             # smart — loads by id, read-only
   ├─ components/
   │  ├─ form-details.component.ts      # the form-name control
   │  ├─ field-builder.component.ts     # add / remove fields  (FormArray)
   │  ├─ field-row.component.ts         # one field editor    (FormGroup)
   │  ├─ approval-route.component.ts    # add / remove steps  (FormArray)
   │  ├─ approval-step-row.component.ts
   │  └─ form-preview.component.ts      # renders current state read-only
   ├─ state/form-builder.store.ts       # signal-based facade
   └─ form-builder.routes.ts            # lazy-loaded
```

### The Reactive Form model

```ts
builderForm = fb.nonNullable.group({
  name:          ['', [Validators.required, Validators.maxLength(200)]],
  description:    [''],
  fields:        fb.array<FieldGroup>([], atLeastOne),      // dynamic
  approvalSteps: fb.array<StepGroup>([], atLeastOne),       // dynamic
});

// each field group
{ label: ['', Validators.required], fieldType: ['Text'], isRequired: [false], placeholder: [''] }

// each step group — the brief's minimum: name + action type
{ name: ['', Validators.required], actionType: ['Approve', Validators.required] }
```

- `addField(type)` pushes a pre-shaped `FormGroup` onto the `fields` array; `removeField(i)`
  removes it. Same for steps. No Drag & Drop — buttons plus `moveUp` / `moveDown` that swap controls.
- `Order` is derived from array index at submit time, so it is always contiguous.
- Form-level validators: at least one field, at least one approval step. Submit disabled
  while `invalid`; each control shows its own error text.
- On submit: `builderForm.getRawValue()` → `CreateFormTemplateRequest` → `api.create()` →
  navigate to the list; API `ProblemDetails` surfaced inline on failure.
- `form-preview` subscribes to `builderForm.valueChanges` and renders the form as the
  end-user would see it.

### State & patterns

- **Signal store** as a Facade: `FormBuilderStore` holds `templates`, `loading`, `error`,
  `selected` via `signal()` / `computed()`. No NgRx boilerplate for a PoC.
- **Container / Presentational**: pages own the form and the store; child components take a
  `FormGroup` / `FormArray` `@Input` and emit `@Output` events only.
- **Adapter**: the `api` service maps wire DTOs ↔ view models.
- **HTTP concerns in interceptors**: base URL, error normalisation, optional loading flag.
- Routes: `/forms` (list) · `/forms/new` (builder) · `/forms/:id` (view).

---

## 9. Decision points

### How to store the form structure

- **Option A — Normalized table.** One `FormFields` row per field. Queryable, validated
  column-by-column, migrates cleanly, diffable.
- **Option B — Serialized blob.** A single `StructureJson` or raw-HTML column on
  `FormTemplate`. Fastest to build and fully flexible, but opaque to the database — no
  referential integrity, no querying, manual versioning, stored HTML must be sanitised on render.
- **Recommendation:** Option A for fields (the brief lists per-field attributes explicitly
  and it demonstrates relational modelling), with an optional cached `RenderedHtml` column.
  The approval route is always normalized — its per-step attributes are required data.

### Template vs. running instance

This PoC stores the approval route *definition*. A production workflow adds `FormInstance`
and `ApprovalStepInstance` (status, actor, timestamp, comment) so a submitted form travels
the route independently of the template. Out of scope here, worth stating aloud.

### State management on the client

The Reactive Form is local component state — correct, it is transient editor state. Shared
app state (the template list) goes in a lightweight signal store. NgRx is deferred until
there are many features, effects, or a need for time-travel debugging.

### Approver identity

For the PoC, `ApprovalStep.ApproverId` can be a free-text name. The model keeps an optional
`Approver` entity and FK so it upgrades to real user references (later, Entra ID object IDs)
without a schema rewrite.

---

## 10. Cloud & theory — talking points (Part 3)

### Why this architecture

- Layering gives testability and dependency inversion — swap EF or the database without touching domain logic.
- CQRS makes each use case a single, nameable class; the pipeline handles validation and logging once.
- The aggregate boundary makes "save the whole form" a single transaction, which is exactly requirement 1.

### Deploying to Azure

| Concern       | Service                                    | Note |
|---------------|--------------------------------------------|------|
| API host      | Azure App Service / Container Apps          | Stateless → horizontal autoscale is trivial |
| SPA host      | Azure Static Web Apps / Blob + Front Door CDN | Built Angular is static assets |
| Database      | Azure SQL Database                          | Same EF provider as local SQL Server |
| Secrets       | Azure Key Vault + Managed Identity          | No connection strings in config |
| Observability | Application Insights                        | Serilog sink for traces & requests |
| CI/CD         | GitHub Actions / Azure DevOps               | Build → test → EF migration step → slot swap |
| Auth          | Microsoft Entra ID (OIDC)                   | API: JWT bearer; Angular: MSAL |

### Security & integrity

- Server-side validation is authoritative; any stored HTML is sanitised before render (XSS).
- Authorization on who may create or publish templates; HTTPS only; CORS locked to known origins.
- Unique index on `(FormTemplateId, Order)`; optimistic concurrency (`rowversion`) on
  `FormTemplate` for future edit flows.

---

## 11. Build sequence

Twelve steps, ordered by dependency. Steps 00–04 and 06–08 are the minimum viable
submission; 05, 09–11 raise the grade.

| Step | Title | Scope |
|------|-------|-------|
| 00 | Solution scaffolding | backend |
| 01 | Domain layer | backend |
| 02 | Application layer | backend |
| 03 | Infrastructure layer | backend |
| 04 | API layer | backend |
| 05 | Backend tests & seed | backend |
| 06 | Angular scaffold | client |
| 07 | Core & shared | client |
| 08 | Form builder page | client |
| 09 | List & view pages | client |
| 10 | Integration & polish | full stack |
| 11 | Theory write-up & recording | submission |

### Step 00 — Solution scaffolding

- `dotnet new sln`; create the four `src` projects and three test projects; set project
  references per the dependency rule.
- `Directory.Build.props`: `Nullable=enable`, `TreatWarningsAsErrors`, `LangVersion=latest`.
- `Directory.Packages.props`: central package version management.
- `.gitignore`, `.editorconfig`.
- Add NuGet packages: MediatR, FluentValidation, EF Core (+ SqlServer, Sqlite, InMemory),
  Swashbuckle, Serilog, Asp.Versioning.
- **Done when** the empty solution builds and the reference graph matches section 3.

### Step 01 — Domain layer

- Entities: `FormTemplate` (aggregate root), `FormField`, `ApprovalStep`, optional `Approver`.
- Enums: `FieldType`, `ApprovalActionType`, `TemplateStatus`.
- Encapsulate collections; constructors enforce required data; behaviour methods `AddField` /
  `AddApprovalStep` / `RemoveStep` / `Reorder` with invariant checks; domain exception type.
- Unit tests for the invariants.
- **Done when** Domain.Tests are green and Domain has no package references.

### Step 02 — Application layer

- Abstractions: `IFormTemplateRepository`, `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUser`; `Result<T>` type.
- DTOs / request records; explicit `ToDto` / `ToEntity` mappers; `FormTemplateFactory`.
- `CreateFormTemplateCommand` + handler + validator.
- `GetFormTemplatesQuery` and `GetFormTemplateByIdQuery` + handlers.
- `ValidationBehavior` pipeline; `AddApplication()` DI extension.
- Handler unit tests with an in-memory fake repository.
- **Done when** all three use cases pass tests without a database.

### Step 03 — Infrastructure layer

- `AppDbContext`; one `IEntityTypeConfiguration` per entity.
- `FormTemplateRepository`, `UnitOfWork`, `SystemDateTimeProvider`, `CurrentUser`.
- `AddInfrastructure(config)` with the provider switch.
- First EF migration checked in; `Database.Migrate()` / `EnsureCreated()` helper.
- **Done when** a handler test can run against SQLite and produce a real row.

### Step 04 — API layer

- `Program.cs` composition root.
- `FormsController` with the three endpoints; map `Result` → `IActionResult`.
- ProblemDetails exception handler.
- **Done when** the three endpoints work end-to-end from the Swagger UI.

### Step 05 — Backend tests & seed

- `WebApplicationFactory` integration tests hitting all three endpoints.
- Optional seeder: one sample "Vacation Request" template.
- **Done when** `create → list → get-by-id` round-trips in a test.

### Step 06 — Angular scaffold

- `ng new` — standalone, routing, strict, SCSS.
- `environment.ts` with `apiBaseUrl`; `provideHttpClient(withInterceptors(...))`; base route table; dev proxy.
- **Done when** the app serves and can `GET /api/v1/forms` through the proxy.

### Step 07 — Core & shared

- Model interfaces mirroring the DTOs.
- `FormTemplateApi` service: `create`, `list`, `getById`.
- Base-URL and error interceptors; a few shared UI primitives.
- **Done when** a smoke component can list templates from the running API.

### Step 08 — Form builder page (the centrepiece)

- `form-builder.page` builds the typed root `FormGroup` with `fields` and `approvalSteps` `FormArray`s.
- `field-builder` + `field-row`: "Add text field" / "Add date field" buttons push shaped groups; remove; move up / down.
- `approval-route` + `approval-step-row`: add / remove steps; each has name + action type at minimum.
- Form-level validators (≥1 field, ≥1 step); submit disabled while invalid; inline error messages.
- `form-preview` renders the current form state read-only.
- Submit → map `getRawValue()` → request → `api.create()` → navigate to list.
- **Done when** a form with 3 fields and 2 steps saves and returns a `201` with an id.

### Step 09 — List & view pages

- `form-list.page` consumes the signal store; "New form" button; row → `/forms/:id`.
- `form-view.page` loads by id and renders via the preview component.
- **Done when** a freshly-saved form appears in the list and opens in the viewer.

### Step 10 — Integration & polish

- Run API + Angular together; verify `create → list → get` round-trip.
- Display `ProblemDetails` from the API; empty / loading / error states; minimal SCSS.
- `README` with run instructions for both halves.
- **Done when** a reviewer can clone, run two commands, and build a form.

### Step 11 — Theory write-up & recording

- One page of notes from section 10.
- Rehearse the screen recording covering architecture, the DB model, and cloud deployment.
- **Done when** the recording covers all of Part 3 in a few clear minutes.

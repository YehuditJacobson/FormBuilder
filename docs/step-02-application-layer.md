# Step 02 — Application layer

**Status:** complete · `dotnet test` → **Application.Tests 14 passed** (Domain 31, Infrastructure 5)
**Scope:** `src/FormBuilder.Application` + `tests/FormBuilder.Application.Tests`
**Depends on:** Domain (Step 01). Adds packages: MediatR, FluentValidation.

---

## Goal

One class per use case, dispatched through MediatR, with validation as a pipeline concern
and a typed `Result` instead of exceptions for expected failures.

---

## Files

### `src/FormBuilder.Application`

| Area | File | Role |
|------|------|------|
| Common | `Error.cs` | `Error` record + `ErrorType` enum (`Validation` / `NotFound` / `Conflict` / `Failure`) |
| Common | `Result.cs` | `Result` and `Result<T>` — success or an `Error`; `Value` throws on a failed result |
| Common | `IDateTimeProvider.cs` | current UTC time (keeps handlers deterministic) |
| Common | `ICurrentUser.cs` | `Id` of the acting user |
| Common | `IUnitOfWork.cs` | `SaveChangesAsync` — one commit per use case |
| Common/Behaviors | `ValidationBehavior.cs` | runs every `IValidator<TRequest>` before the handler; throws `ValidationException` on failure |
| FormTemplates/Contracts | `CreateFormTemplateRequest.cs` | `CreateFormTemplateRequest` + `CreateFormFieldInput` + `CreateApprovalStepInput` — **no `Order`**, taken from list position |
| FormTemplates/Contracts | `FormTemplateDtos.cs` | `FormTemplateSummaryDto`, `FormTemplateDetailDto`, `FormFieldDto`, `ApprovalStepDto` — enums kept as enums, serialized as strings by the API |
| FormTemplates/Abstractions | `IFormTemplateRepository.cs` | write side: `AddAsync` |
| FormTemplates/Abstractions | `IFormTemplateQueries.cs` | read side: `GetSummariesAsync`, `GetDetailAsync` (DTO projections) |
| FormTemplates | `FormTemplateFactory.cs` | builds the aggregate from a request |
| FormTemplates/Create | `CreateFormTemplateCommand.cs` | `IRequest<Result<Guid>>` |
| FormTemplates/Create | `CreateFormTemplateCommandHandler.cs` | factory → repository → unit of work; catches `DomainException` → `Result.Failure(Validation)` |
| FormTemplates/Create | `CreateFormTemplateCommandValidator.cs` | FluentValidation: name required, ≥1 field, ≥1 step, per-child rules |
| FormTemplates/GetList | `GetFormTemplatesQuery.cs` | query + handler → `IReadOnlyList<FormTemplateSummaryDto>` |
| FormTemplates/GetById | `GetFormTemplateByIdQuery.cs` | query + handler → `Result<FormTemplateDetailDto>` (NotFound if missing) |
| — | `DependencyInjection.cs` | `AddApplication()` — MediatR + `ValidationBehavior` + validators |

### `tests/FormBuilder.Application.Tests` (14 tests)

- `CreateFormTemplateCommandHandlerTests` — builds the aggregate from the request, returns
  its id, commits once, and turns a `DomainException` into a `Validation` failure without saving.
- `CreateFormTemplateCommandValidatorTests` — accepts a good request; rejects missing name,
  no fields, no steps, blank child label / step name (via `FluentValidation.TestHelper`).
- `GetFormTemplateByIdQueryHandlerTests` — returns detail when present, `NotFound` when absent.
- `ValidationBehaviorTests` — passes through with no validators / on success; throws on failure.

Fakes: a hand-written `FakeFormTemplateRepository` (captures the aggregate) plus
`FixedDateTimeProvider` / `StubCurrentUser`; NSubstitute for `IUnitOfWork` and `IFormTemplateQueries`.

---

## Decisions

- **CQRS split at the interface.** `IFormTemplateRepository` is write-only (`AddAsync`).
  Reads go through `IFormTemplateQueries`, which returns DTOs projected in Infrastructure —
  no aggregate load, no tracking. The GetById handler never materialises the aggregate.
- **`Order` is not in the request.** Position comes from the list order; the factory calls
  `AddField` / `AddApprovalStep` in sequence and the aggregate numbers them 0..n.
- **Two layers of validation.** The request validator (FluentValidation, in the pipeline) is
  the user-facing gate; the aggregate's guards are the backstop the handler translates to a
  `Result` failure. Belt and braces, by design.
- **Enums stay enums in DTOs.** `JsonStringEnumConverter` (wired in the API, Step 04) renders
  them as `"Text"`, `"Approve"`, `"Published"`. Keeps the EF projection free of `.ToString()`.
- **Result, not exceptions, for expected outcomes.** NotFound and rule violations are
  `Result.Failure`; the API maps them to 404 / 400. Unexpected errors still throw.

### Open question for the user

Server-side validation messages (`CreateFormTemplateCommandValidator`, and the
`ProblemDetails` the API will return) are currently **English**. The Angular client owns the
Hebrew user-facing validation text. Say if you want the server messages in Hebrew too.

---

## Next backend step — Step 04: API layer

`Program.cs` composition root (`AddApplication` + `AddInfrastructure`, CORS for the Angular
origin, Serilog, `JsonStringEnumConverter`, a ProblemDetails exception handler that maps
`ValidationException` → 400 and `Result` NotFound → 404), and `FormsController` with the
three endpoints.

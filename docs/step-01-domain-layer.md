# Step 01 — Domain layer

**Status:** complete · `dotnet test` → **31 passed, 0 failed** (`FormBuilder.Domain.Tests`)
**Scope:** `src/FormBuilder.Domain` + `tests/FormBuilder.Domain.Tests`
**Dependencies:** none — the project has no NuGet packages and no project references

---

## Goal of this step

Model the form template and its approval route as a single aggregate that protects its own
invariants, with no dependency on EF Core, ASP.NET, or any framework.

---

## Files created

### `src/FormBuilder.Domain`

| File | Role |
|------|------|
| `Common/Entity.cs` | Base class; identity + equality by `Id` and runtime type |
| `Common/DomainException.cs` | Raised when an operation would break a domain rule |
| `Common/DomainGuard.cs` | `internal` guard helpers (`RequiredText`, `OptionalText`, length checks) that return the normalised value |
| `Enums/FieldType.cs` | `Text`, `Date`, `Number`, `Checkbox`, `Dropdown` |
| `Enums/ApprovalActionType.cs` | `Approve`, `Reject`, `ReturnForRevision`, `Sign`, `Acknowledge` |
| `Enums/TemplateStatus.cs` | `Draft`, `Published` |
| `FormTemplates/FormField.cs` | Entity — a dynamic field; `internal` constructor, `private` setters |
| `FormTemplates/ApprovalStep.cs` | Entity — one milestone; `internal` constructor, `private` setters |
| `FormTemplates/FormTemplate.cs` | **Aggregate root** — envelope + fields + approval route |

### `tests/FormBuilder.Domain.Tests`

| File | Covers |
|------|--------|
| `Common/EntityTests.cs` | equality by id + type, inequality, null-safety |
| `FormTemplates/FormTemplateTests.cs` | construction, trimming, guard clauses on name / author |
| `FormTemplates/FormTemplateFieldTests.cs` | add appends & numbers from 0, remove re-indexes, reorder, label guard |
| `FormTemplates/FormTemplateApprovalStepTests.cs` | same rules for the approval route + approver normalisation |
| `FormTemplates/FormTemplatePublishTests.cs` | publish requires ≥1 field and ≥1 step |

---

## Design decisions

- **Aggregate root owns its children.** `FormField` and `ApprovalStep` have `internal`
  constructors, so they can only be created via `FormTemplate.AddField(...)` /
  `AddApprovalStep(...)`. Callers outside the domain never new one up.
- **Encapsulated collections.** Backing `List<T>` fields are exposed as
  `IReadOnlyList<T>` via `AsReadOnly()`. All mutation goes through aggregate methods.
- **`Order` is always contiguous from zero.** `Add*` appends at `count`; `Remove*` and
  `Reorder*` call a private `Reindex` that rewrites every position. This is a domain
  invariant, independent of how the API or UI happens to call it.
- **Deterministic domain.** The `FormTemplate` constructor takes `createdAtUtc` as a
  parameter (supplied later by `IDateTimeProvider`) rather than reading the clock itself.
- **Validation vs. invariant.** The domain allows an empty `Draft` (no fields / no steps);
  it only enforces "≥1 field and ≥1 step" on `Publish()`. Requiring a non-empty form at the
  API boundary is a product rule and belongs in the Step 02 command validator.
- **Guard helpers return the cleaned value** so constructors read as
  `Name = DomainGuard.RequiredText(name, MaxNameLength, nameof(Name));`.

## Not included (deliberately)

- **No `Approver` entity yet.** `ApprovalStep.ApproverId` is free-text (`string?`), matching
  the brief ("identity of the approver") and the PoC decision in `ARCHITECTURE.md` §9. It is
  shaped to become a foreign key later without a rewrite.
- No `Update`/`Rename` methods — the required API has no edit endpoint. `Publish()` is kept
  because it is where the non-empty invariant lives and it is fully tested.

---

## Public API of the aggregate

```csharp
new FormTemplate(name, description, createdBy, createdAtUtc)

FormField     AddField(label, fieldType, isRequired = false, placeholder = null, options = null)
void          RemoveField(Guid fieldId)
void          ReorderFields(IReadOnlyList<Guid> orderedFieldIds)

ApprovalStep  AddApprovalStep(name, actionType, approverId = null)
void          RemoveApprovalStep(Guid stepId)
void          ReorderApprovalSteps(IReadOnlyList<Guid> orderedStepIds)

void          Publish()                       // requires ≥1 field and ≥1 step

IReadOnlyList<FormField>     Fields
IReadOnlyList<ApprovalStep>  ApprovalSteps
```

---

## Next backend step — Step 02: Application layer

Abstractions (`IFormTemplateRepository`, `IUnitOfWork`, `IDateTimeProvider`,
`ICurrentUser`), `Result<T>`, DTOs, `FormTemplateFactory`, the three CQRS use cases
(`CreateFormTemplateCommand`, `GetFormTemplatesQuery`, `GetFormTemplateByIdQuery`) with
validators and a `ValidationBehavior` pipeline, plus handler unit tests.

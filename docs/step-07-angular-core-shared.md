# Step 07 — Angular core & shared

**Status:** complete · `npm run build` clean · `ng test` → **7 passed** (3 spec files)
**Scope:** `client/src`
**Depends on:** Angular scaffold (Step 06). Consumes the DTO shapes from Step 02.

---

## Goal

The client foundation the feature screens will build on: typed models, a typed API service,
a normalising error interceptor, the RTL + Hebrew shell, the token/primitive stylesheet, and
a shared icon component.

---

## Files

| File | Role |
|------|------|
| `src/index.html` | `<html lang="he" dir="rtl">`, title **בונה טפסים**, Assistant font `<link>` + preconnect |
| `src/styles/_tokens.scss` | the approved "clean enterprise" tokens as CSS custom properties (oklch palette, radii, shadows, font stack) |
| `src/styles/_primitives.scss` | reusable classes — `.card`, `.btn`(`--primary`/`--ghost`), `.icon-btn`, `.input`/`.select`/`.textarea` (+ `.is-invalid`), `.field-label`, `.field-error`, `.tag`, `.badge`(`--published`/`--draft`) — RTL-aware |
| `src/styles.scss` | `@use` tokens + primitives; global reset; `html { direction: rtl }`; `.rtl-flip` helper |
| `src/app/core/models/form-template.models.ts` | TS mirrors of the API DTOs; `FieldType` / `ApprovalActionType` as string-literal unions with `const` arrays for iteration |
| `src/app/core/api/form-template-api.service.ts` | `FormTemplateApiService` — `list()`, `getById(id)`, `create(request)` against `${environment.apiBaseUrl}/v1/forms` |
| `src/app/core/errors/api-error.ts` | `ApiError` interface + `toApiError()` — parses RFC 7807 `ProblemDetails` / `ValidationProblemDetails`; Hebrew fallback titles; `status 0` → "לא ניתן להתחבר לשרת" |
| `src/app/core/interceptors/error.interceptor.ts` | functional `errorInterceptor` — every `HttpErrorResponse` → `ApiError` before it reaches a component |
| `src/app/shared/ui/icon/icon.component.ts` | `<app-icon name="…" [size] [strokeWidth] [flip]>` — 17 inline stroke icons via `@switch`; `flip` mirrors direction-sensitive ones in RTL |
| `src/app/app.config.ts` | `provideHttpClient(withFetch(), withInterceptors([errorInterceptor]))` |

### Tests (`ng test` — Vitest)

- `form-template-api.service.spec.ts` — `list` / `getById` / `create` hit the right URL and method, `create` sends the body (`HttpTestingController`).
- `error.interceptor.spec.ts` — a `ValidationProblemDetails` body becomes `ApiError.fieldErrors`; a `status 0` failure becomes a connection error.
- `app.spec.ts` — shell renders a `router-outlet` (from Step 06).

---

## Decisions

- **Tokens + primitive classes, not a component per element.** The design is settled and
  RTL; a shared SCSS layer keeps the feature components lean. The one shared *component* is
  `app-icon`, because inline SVGs would otherwise be copy-pasted across every screen.
- **Deviation from the Step 06 plan:** no base-URL interceptor. The API service already
  prepends `environment.apiBaseUrl`, so an interceptor would be redundant. Kept just the
  error interceptor.
- **`ApiError` is the only error shape components see.** Field errors are exposed as
  `Record<field, string[]>` so the builder screen (Step 08) can attach server-side messages
  to the matching controls if client validation is ever bypassed.
- **Enums as string-literal unions** (`'Text' | 'Date' | …`) with `FIELD_TYPES` /
  `APPROVAL_ACTION_TYPES` const arrays — matches the API's `JsonStringEnumConverter` output
  and gives the builder its `<select>` options for free.
- **RTL icon mirroring** via a `flip` input + `[dir="rtl"] .rtl-flip { transform: scaleX(-1) }`,
  so `arrow-back` and `chevron-left` point the right way.

---

## Next client step — Step 08

The form-builder page: the typed root `FormGroup` with `fields` / `approvalSteps`
`FormArray`s, the add/remove/reorder row components, the live preview, form-level validators,
and submit → `FormTemplateApiService.create`.

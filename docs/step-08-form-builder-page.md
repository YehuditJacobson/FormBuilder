# Step 08 — Form-builder page (the centrepiece)

**Status:** complete · `npm run build` clean (`strictTemplates`) · `ng test` → **15 passed** (5 files)
**Scope:** `client/src/app/features/form-builder/` + app shell
**Visual / browser check:** deferred to Step 10 (integration pass)

---

## Goal

The "יצירת טופס חדש" screen from the approved mockup: one typed reactive form with two
dynamic `FormArray`s, add / remove / reorder rows, a live preview, form-level validation,
and submit → `POST /api/v1/forms`.

---

## Files

### `features/form-builder/model/`

| File | Role |
|------|------|
| `builder-form.ts` | typed `BuilderForm` = `{ name, description, fields: FormArray<FieldGroup>, approvalSteps: FormArray<StepGroup> }`; `createBuilderForm`, `newFieldGroup(type)`, `newStepGroup()`; `BuilderValue` type |
| `builder-form.validators.ts` | `minLengthArray(n)` — fails a `FormArray` with fewer than `n` controls |
| `builder-form.mapper.ts` | `toCreateRequest(value)` — trims text, blank → `null`, `options: null`, order = array position |
| `labels.ts` | Hebrew labels for `FieldType` / `ApprovalActionType`, the `<select>` options, and the five "add field" buttons |

### `features/form-builder/components/` (all standalone, `OnPush`)

| Component | Role |
|-----------|------|
| `field-row.component.ts` | one field: order badge, type tag, ↑ ↓ 🗑, label (+ inline error), placeholder, "שדה חובה" checkbox |
| `approval-step-row.component.ts` | one step: order badge, ↑ ↓ 🗑, step name (+ inline error), action-type `<select>`, approver |
| `form-preview.component.ts` | read-only render of the current value — fields by type (text / date / checkbox / dropdown), a disabled submit, the approval route as a numbered stepper with action tags |

### `features/form-builder/pages/`

| File | Role |
|------|------|
| `form-builder.page.ts` / `.html` / `.scss` | container — owns `form`, `addField` / `removeField` / `moveField` (+ step equivalents), `submit`, `startNew`, `cancel`; `value` signal from `valueChanges` → `getRawValue()`; `hasStarted` / `errorSummary` computed; `saving` / `savedId` / `serverError` signals |

### Wiring

- `form-builder.routes.ts` → `'' → new`, `new → FormBuilderPage`. *(Step 09 adds `'' → list`, `':id' → view`.)*
- `app.ts` / `app.html` / `app.scss` → persistent topbar ("בונה טפסים", links to `/forms`) + `<router-outlet>`.
- `app.spec.ts` updated for the router.

### Tests (15 total)

- `builder-form.mapper.spec.ts` — trims, nulls blanks, keeps order.
- `form-builder.page.spec.ts` — starts invalid; add / remove / move field; API not called while invalid; valid submit calls `create` once and sets `savedId`; server error surfaces and stays on the form.
- plus Step 07's api-service / interceptor / app-shell specs.

---

## Behaviour

- **No drag & drop** — ↑ / ↓ buttons move a control within its `FormArray` (keeps its
  touched / dirty state). Order is derived from position at submit time.
- **Validation.** `name` required (≤200); each row's label / step-name required; each
  `FormArray` needs ≥ 1 entry (`minLengthArray`). **Save is disabled while the form is
  invalid**; the summary banner ("יש לתקן N בעיות…") appears once the user has started
  (name typed, or a field / step added) and lists exactly what's missing — matching the
  Validation mockup.
- **Live preview** updates from `form.valueChanges` (mapped to `getRawValue()` so the signal
  always holds a complete `BuilderValue`).
- **Submit** → `toCreateRequest` → `FormTemplateApiService.create` → on success show a
  "נשמר בהצלחה" card with the id + "יצירת טופס נוסף" (resets); on failure show the
  `ApiError` banner and stay on the form.

## Deviations from the plan

- On success the page shows an **inline success card** rather than navigating — the list /
  view pages don't exist yet. Step 09 will switch this to `router.navigate(['/forms', id])`.
- No separate `FieldListComponent` / `ApprovalRouteComponent` wrappers — the page loops the
  `FormArray`s directly and renders the row components. Fewer layers for a PoC.
- Signal store deferred to Step 09 (it holds the *list* state; the builder form is correctly
  local component state).

---

## Next client step — Step 09

`FormListPage` (signal store over `FormTemplateApiService.list`) and `FormViewPage`
(read-only render by id), then point the builder's success path at the list.

# Step 09 — List & view pages

**Status:** complete · `npm run build` clean · `ng test` → **23 passed** (8 files)
**Scope:** `client/src/app/features/form-builder/` (state + two pages) + routing + preview refactor

---

## Goal

The remaining two screens from the mockup — the template list (`/forms`) and the read-only
view (`/forms/:id`) — plus pointing the builder's success path at the list.

---

## Files

| File | Role |
|------|------|
| `state/form-list.store.ts` | `FormListStore` — signal facade: `templates` / `loading` / `error` / `isEmpty`; `load()` (once) and `refresh()` (force) over `FormTemplateApiService.list` |
| `pages/form-list.page.ts` | smart page — calls `store.load()` on init; renders loading / error+retry / empty-state / table (name link, field & step counts, status badge, author, `dd/MM/yyyy`, chevron); row navigates to `/forms/:id` |
| `pages/form-view.page.ts` | smart page — `id` as a routed signal input (`withComponentInputBinding`), `effect` loads it; renders back-arrow + name + status badge + meta line + the read-only form; a 404 shows "הטופס לא נמצא" |
| `components/form-preview.component.ts` | **refactored** to normalised inputs `heading` / `fields: PreviewField[]` / `steps: PreviewStep[]`, so it is shared by the builder's live preview and the saved-form view |
| `form-builder.routes.ts` | `'' → FormListPage`, `new → FormBuilderPage`, `:id → FormViewPage` |
| `app.config.ts` | added `withComponentInputBinding()` |
| `pages/form-builder.page.*` | success now `router.navigate(['/forms', id])` instead of the inline card; feeds the preview the mapped `PreviewField[]` / `PreviewStep[]` |

### Tests added (→ 23 total)

- `form-list.store.spec.ts` — `load()` fetches once; empty state; error captured; `refresh()` re-fetches.
- `form-list.page.spec.ts` — empty state renders; one `<tr>` per template with status labels.
- `form-view.page.spec.ts` — loads by id and renders the detail; 404 → "הטופס לא נמצא".
- `form-builder.page.spec.ts` — updated: valid submit now asserts `router.navigate(['/forms', id])`.

---

## Decisions

- **`FormListStore` is `providedIn: 'root'`** and guards against duplicate loads (`_loaded`);
  `refresh()` bypasses the guard for use after a create (the builder navigates to the view,
  so the list re-loads naturally on next visit).
- **`form-view` uses a routed signal input** (`id = input.required<string>()`) via
  `withComponentInputBinding()` + an `effect` — no `ActivatedRoute` boilerplate, and it
  re-loads if the id ever changes.
- **One shared read-only renderer.** Collapsing the builder preview and the view screen onto
  `FormPreviewComponent<PreviewField[], PreviewStep[]>` removed a near-duplicate component;
  each page maps its own source (form value / detail DTO) to the shared shape.
- **Dates** shown with `DatePipe`'s `dd/MM/yyyy` — a numeric pattern, so no locale
  registration is needed.

---

## Next client step

Nothing functional — the three screens and the round-trip are done. Step 10 runs it all
together; Step 11 is the theory write-up.

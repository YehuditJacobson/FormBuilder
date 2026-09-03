# Step 10 — Integration & polish

**Status:** complete · full stack verified end-to-end through the Angular dev proxy

---

## Goal

Run the API and the Angular client together and confirm the whole `create → list → view`
round-trip works over the real network path (browser → dev-server proxy → API → EF).

---

## What was run

```
Terminal 1:  Database__Provider=InMemory ASPNETCORE_ENVIRONMENT=Development \
             dotnet run --project src/FormBuilder.Api        # http://localhost:5052
Terminal 2:  cd client && npm start                          # http://localhost:4200  (proxies /api -> 5052)
```

## Verified (requests made to `http://localhost:4200`, i.e. through the proxy)

| Check | Result |
|-------|--------|
| `GET /api/v1/forms` on first run | `200` — one row: the seeded **"בקשת חופשה"** (3 fields, 2 steps, `Published`) |
| `POST /api/v1/forms` (new "דיווח ימי מחלה") | `201` + `{ id }` |
| `GET /api/v1/forms` again | `200` — **2 rows, newest first** |
| `GET /api/v1/forms/{id}` | `200` — full graph, fields `order` 0,1, step `order` 0, enums as strings, `status` `Draft` |
| `GET /api/v1/forms/{zeros}` | `404` |
| `OPTIONS /api/v1/forms` with `Origin: localhost:4200` (direct to API) | `204`, `Access-Control-Allow-Origin: http://localhost:4200` |
| `GET /forms` (SPA shell) | `<html lang="he" dir="rtl">`, `<title>בונה טפסים</title>` |

Hebrew payloads are correct UTF-8 end-to-end (the `?` glyphs in a Windows console are a
terminal code-page artefact only).

## Polish

- Loading / empty / error+retry states are built into `FormListPage`; 404 handled in
  `FormViewPage`; server `ProblemDetails` surfaced on the builder via the error interceptor.
- Dead code from Step 08's inline success card removed (`.saved` styles, `savedId`, `startNew`).
- `README.md` run section covers both halves and the `Database__Provider` switch.
- Dev seeder gives a first-run demo row (Development only, empty table only).

## How to demo

1. `dotnet run --project src/FormBuilder.Api` (SQLite by default; Swagger at `/swagger`).
2. `cd client && npm start`, open `http://localhost:4200`.
3. The list shows the seeded template → **טופס חדש** → add fields / steps, watch the live
   preview → **שמירת טופס** → lands on the read-only view → back to the list shows it.

---

## Remaining

**Step 11** — theory write-up (`ARCHITECTURE.md` §10 already drafts it) and the screen
recording covering architecture, the DB model, and cloud deployment.

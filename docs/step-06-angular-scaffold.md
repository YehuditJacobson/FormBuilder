# Step 06 — Angular scaffold

**Status:** complete · `npm run build` → **bundle generated, 0 errors** · `ng test` → **2 passed**
**Scope:** `client/`
**Runs in parallel with:** Step 01 (backend Domain layer) — no shared code

---

## Goal of this step

Stand up the Angular client: a standalone, strict, SCSS project with routing, an HTTP
client, environment configuration, and a dev-server proxy to the .NET API.

---

## What was generated

`npx @angular/cli@latest new client --style=scss --routing --ssr=false --skip-git --defaults`

- **Angular 22.1** (latest; required for Node 24 which is installed) — standalone APIs, no
  NgModules, new file naming (`app.ts`, not `app.component.ts`)
- **TypeScript 6.0**, target ES2022
- Test runner: **Vitest + jsdom** (Angular 22's default; replaces Karma/Jasmine)
- Build: `@angular/build:application` (esbuild)

## Changes made on top of the scaffold

| File | Change |
|------|--------|
| `src/environments/environment.ts` | **new** — production config: `{ production: true, apiBaseUrl: '/api' }` |
| `src/environments/environment.development.ts` | **new** — dev config: `{ production: false, apiBaseUrl: '/api' }` |
| `proxy.conf.json` | **new** — forwards `/api` → `http://localhost:5052` (the API's HTTP profile) |
| `angular.json` | `serve.options.proxyConfig` → `proxy.conf.json`; `build.configurations.development.fileReplacements` swaps in `environment.development.ts` |
| `tsconfig.json` | added `"strict": true` and `angularCompilerOptions.strictTemplates: true` |
| `src/app/app.config.ts` | added `provideHttpClient(withFetch())` |
| `src/app/app.routes.ts` | base route table: `'' → forms`, `forms` lazy-loads the feature, `** → forms` |
| `src/app/features/form-builder/form-builder.routes.ts` | **new** — empty `formBuilderRoutes`, filled in Step 08/09 |
| `src/app/app.html` | replaced the 344-line CLI welcome page with `<router-outlet />` |
| `src/app/app.ts` | trimmed to a bare shell component |
| `src/app/app.spec.ts` | rewritten: "creates the app shell" + "renders a router outlet" |

## How dev wiring fits together

```
ng serve  ──►  dev-server :4200
                  │  request to /api/...
                  ▼
             proxy.conf.json  ──►  http://localhost:5052  (dotnet run, API http profile)
```

The client only ever calls `/api` (from `environment.apiBaseUrl`); the proxy removes any
need for the backend's absolute URL or for CORS during local development. In production the
same `/api` path is expected to be served from the client's own origin.

## Deferred

- `core/` (typed API service, base-URL + error interceptors) and `shared/ui/` — **Step 07**
- Feature pages and components (`form-list`, `form-builder`, `form-view`, the reactive form,
  the signal store) — **Step 08 / 09**
- `provideHttpClient` currently has `withFetch()` only; `withInterceptors([...])` is added in
  Step 07

---

## Commands

```bash
cd client
npm start            # ng serve with the API proxy on http://localhost:4200
npm run build        # production build → client/dist/client
npm test             # vitest, single run
```

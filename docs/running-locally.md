# Running locally

Two processes: the **.NET API** (port 5052) and the **Angular client** (port 4200). Start
the API first — the client's first request is a proxied `/api/...` call.

---

## API — `src/FormBuilder.Api`

### Visual Studio

1. Open `FormBuilder.sln`.
2. Set **FormBuilder.Api** as the startup project.
3. Choose the **`http`** launch profile (dropdown by the Run button) and press **Ctrl+F5**
   (run without debugging) or **F5**.
   - Runs on `http://localhost:5052`, opens **Swagger** at `/swagger`.
   - The Angular proxy targets `http://localhost:5052`, so use `http`, not `https` / `IIS Express`.
   - To use `https` instead: run the `https` profile (`https://localhost:7104`) and change
     `client/proxy.conf.json` → `"target": "https://localhost:7104"`, `"secure": false`.

### Command line

```bash
dotnet run --project src/FormBuilder.Api          # http://localhost:5052

# override the provider without touching appsettings.json:
Database__Provider=InMemory  dotnet run --project src/FormBuilder.Api
Database__Provider=SqlServer ConnectionStrings__Default="Server=(localdb)\mssqllocaldb;Database=FormBuilder;Trusted_Connection=True;TrustServerCertificate=True" dotnet run --project src/FormBuilder.Api
```

In **Development** the API seeds one sample template ("בקשת חופשה") when the table is empty.

---

## Client — `client/`

Open the **`client`** folder in VS Code (so `package.json` and `.vscode/` resolve).

```bash
npm install      # first time / fresh clone
npm start        # ng serve on http://localhost:4200 (watch mode)
npm run build    # production build -> client/dist/client
npm test         # vitest, single run
```

- **Run and Debug** (Ctrl+Shift+D) → **"ng serve"** → F5 launches the dev server and opens
  Chrome with `.ts` breakpoint debugging.
- The `/api` proxy is configured in `angular.json` (`serve.options.proxyConfig` →
  `proxy.conf.json`), so no CORS is needed in development.
- `client/src/environments/environment.development.ts` sets `apiBaseUrl: '/api'`; the proxy
  forwards `/api` to `http://localhost:5052`.

Recommended VS Code extensions: **Angular Language Service** (`angular.ng-template`),
**SQLite Viewer** (`qwtel.sqlite-viewer`).

---

## The database

Controlled by `Database:Provider` in `src/FormBuilder.Api/appsettings.json`. Currently set to
**`SqlServer`** against `(localdb)\MSSQLLocalDB` / database `FormBuilder`. Change it back to
`Sqlite` or `InMemory` at any time.

### Sqlite (default)

- Connection: `ConnectionStrings:Default = "Data Source=formbuilder.db"`.
- The file is created **relative to the API's working directory**, i.e.
  **`src/FormBuilder.Api/formbuilder.db`**.
- Schema is created from the EF model via `EnsureCreated()` on first run (SQLite does not
  use the checked-in SQL Server migration).
- **View it:**
  - VS Code *SQLite Viewer* extension — click `formbuilder.db`, browse `FormTemplates`,
    `FormFields`, `ApprovalSteps`.
  - **DB Browser for SQLite** (free GUI, sqlitebrowser.org) → *Open Database* → *Browse Data*.
  - CLI: `sqlite3 src/FormBuilder.Api/formbuilder.db` → `.tables`, `SELECT * FROM FormTemplates;`
- **Reset:** stop the API, delete `formbuilder.db` (and `formbuilder.db-shm` / `-wal` if
  present), restart. The file is gitignored.

### InMemory

Nothing on disk. Data lives only while the process runs. Inspect it only through the API /
Swagger.

### SqlServer / LocalDB

- Set `"Provider": "SqlServer"` and a real connection string, e.g.
  `Server=(localdb)\mssqllocaldb;Database=FormBuilder;Trusted_Connection=True;TrustServerCertificate=True`.
- The app applies the checked-in migration (`Migrate()`) on startup.
- **View it in Visual Studio:** *View → SQL Server Object Explorer* → `(localdb)\MSSQLLocalDB`
  → *Databases* → `FormBuilder` → *Tables* → right-click a table → *View Data*.
- Or **SSMS** / **Azure Data Studio** connected to `(localdb)\MSSQLLocalDB`.
- EF tooling (from the repo root): `dotnet tool restore` then
  `dotnet dotnet-ef migrations list` / `dotnet dotnet-ef database update`.

### Any provider

`http://localhost:5052/swagger` — call `GET /api/v1/forms` etc. to see what is stored.

---

## Troubleshooting

### `npm : File ...\npm.ps1 cannot be loaded because running scripts is disabled`

PowerShell's execution policy is blocking the `npm` script. Fix it once for your user:

```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

Alternatives that need no policy change: run `npm.cmd start`, or use **Command Prompt**
instead of PowerShell (in VS Code: *Terminal: Select Default Profile* → *Command Prompt*).

### `npm start` fails with "no start script" / missing `package.json`

You are in the repo root. `cd client` first — the Angular project (and its `package.json`)
lives in `client/`.


# codebase/

The code lives in `wheelhouse-canary.backend-services/` (.NET) and `wheelhouse-canary.frontend-services/` (Vue).

## Backend

`WheelhouseCanary.BackendServices.slnx` holds five Clean Architecture projects and three test tiers.

| Project | Holds |
|---|---|
| `WheelhouseCanary.Domain` | `PingEntity` |
| `WheelhouseCanary.Application` | the add-ping command, the list query, their model, validator and repository contract |
| `WheelhouseCanary.Infrastructure` | the command and query handlers |
| `WheelhouseCanary.Persistence` | the EF context, the repository and `Migrations/NNN-name/{Apply,Rollback}.sql` |
| `WheelhouseCanary.Api` | host composition, `PingsController` (`/api/pings`), `SystemController` (`/api/system/status`), `/health` |
| `tests/WheelhouseCanary.Tests.Unit` | handlers and validator; no I/O |
| `tests/WheelhouseCanary.Tests.Migrations` | apply, idempotency and the Apply → Rollback → Apply round trip over PostgreSQL |
| `tests/WheelhouseCanary.Tests.E2E` | the real host over PostgreSQL: pings, status, health and the served app |

- Run `dotnet test` from the backend folder. The Migrations and E2E tiers need Docker.
- The Api build runs `pnpm run deploy`, which builds the Vue app into `WheelhouseCanary.Api/wwwroot/` (gitignored); `-p:BuildSpa=false` skips it.
- The connection string is `DatabaseSettings:ConnectionString` (or `DB_CONNECTION`); the environment label is `Canary:Label`. A deployed environment supplies both through `appsettings.Local.json`.
- `appsettings.Development.json` points an IDE run at the compose database on loopback `8374` with local-only credentials.

## Frontend

The frontend is a private pnpm workspace. Its product app is `apps/web/`; optional shared code uses `packages/`.
Installation and root `dev`, `typecheck`, `test`, `build`, and `deploy` commands run from that workspace root.
The app owns its dependencies, Vite and TypeScript configuration, five-layer `src/`, tests, and `dist/` output.
`scripts/deploy.mjs` copies `apps/web/dist/` to `wheelhouse-canary.backend-services/WheelhouseCanary.Api/wwwroot/`.
CI passes the product version as `APP_VERSION`; Vite stamps it into the page as `__APP_VERSION__` (`dev` locally).

Rules: wow-two-ws/conventions/development/repo/structure/repo-structure.md

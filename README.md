# wow-two-platform.wheelhouse-canary

The smallest real app Wheelhouse can deploy: a Vue page over a .NET API and a PostgreSQL database, served from one host.
Wheelhouse dev deploys it to its own dev and prod environments to prove every deployment flow before a Wheelhouse release is promoted.

The page lists the latest twenty pings, adds one, and shows the running app and API versions and the environment's label.

## Layout

```
engineering/
├── codebase/
│   ├── wheelhouse-canary.backend-services/    ← .NET Clean Arch: Api · Application · Domain · Infrastructure · Persistence (+ tests/)
│   └── wheelhouse-canary.frontend-services/   ← private pnpm workspace; Vue app in apps/web/
├── deployment/                                ← deploy.yml · Dockerfile · docker-compose.yml
└── planning/version-track/v0.1/               ← the product version CI reads
```

## What it exercises

| Flow | Where |
|---|---|
| CI, image build and publish | `.github/workflows/publish-docker-image.yml` → shared `delivery.yml@v0.11.0` (checks, then publish; docs-only pushes run nothing); no deploy job |
| Release bundle and version | newest `version-track/v{X.Y}`; the page and `GET /api/system/status` report the running version |
| Per-environment settings | `Canary:Label` and the connection string, from the environment's settings file |
| Database and migrations | two SQL migrations in `WheelhouseCanary.Persistence/Migrations`, applied at boot |
| Health and rollout checks | `GET /health` answers 200 only while the database answers |
| Private site | one `app` site, `exposure: private`, served at `/` and `/api` (descriptor 2) |

## Run it (dev)

```bash
# Database (loopback 8374; local-only credentials)
cd engineering/deployment && docker compose up -d database

# Backend — also builds the Vue app into wwwroot (needs node and pnpm); https://localhost:8370
cd engineering/codebase/wheelhouse-canary.backend-services && dotnet run --project WheelhouseCanary.Api --launch-profile https

# Frontend dev server with hot reload — https://localhost:8372, proxying /api to the backend
cd engineering/codebase/wheelhouse-canary.frontend-services && pnpm install --frozen-lockfile && pnpm dev
```

## Test it

```bash
cd engineering/codebase/wheelhouse-canary.backend-services && dotnet test   # Docker required
cd engineering/codebase/wheelhouse-canary.frontend-services && pnpm test && pnpm build
```

Conventions: `wow-two-ws/conventions/`.

# deployment/

How WheelhouseCanary ships. Descriptor format: wow-two-ws/conventions/deployment/descriptor/deploy-descriptor.md.

| File | Role |
|---|---|
| `deploy.yml` | one service, `canary`: `/health` check, 256 MB, settings keys, `needs: [postgres]`, `dependsOn: []`, one private `app` site (descriptor 2) served at `/` and `/api`; `dotnetEnvironment: true` and the sample `verify` check |
| `Dockerfile` | three stages: build the Vue app, publish the .NET host with the app in `wwwroot`, run on `aspnet` with `curl` for the health check |
| `docker-compose.yml` | local verification: the image plus a PostgreSQL; `docker compose up -d database` alone serves an IDE run, which reads `WheelhouseCanary.Api/appsettings.Local.json` (gitignored; the `https` launch profile sets `LOCAL_SETTINGS=true`): `DatabaseSettings:ConnectionString` `Host=localhost;Port=8374;Database=wheelhouse_canary;Username=canary;Password=canary`, `Canary:Label` `local` |

## Contract with Wheelhouse

- Image: `ghcr.io/wow-two-platform/wow-two-platform.wheelhouse-canary/canary`, published by `.github/workflows/publish-docker-image.yml` through the shared `delivery.yml@v0.9.0`, which runs the backend and frontend checks first; CI never deploys.
- Version: `X.Y` from the newest `planning/version-track/v{X.Y}/`; the page, the image tag and `GET /api/system/status` report the same `X.Y.Z`.
- Settings a target must provide in the service's settings file (`/app/appsettings.Target.json`, pipelines v0.7.0): `DatabaseSettings:ConnectionString`, `Canary:Label`. The label (`dev`, `prod`) shows on the page, proving each environment read its own file.
- Database: the host creates the database if missing and applies pending `Migrations/` before it serves, so a first deploy builds the schema and an upgrade applies only the new migrations.
- Health: `GET /health` answers 200 only while PostgreSQL answers; an unreachable database fails the rollout check.
- No volumes and no host ports; the ingress routes the `app` site (private exposure) to port 8080: one router for
  `/` and one for `/api` (`/api` and `/api/…`, priority 104 over the root's 101), both to the `canary` service.
- Environment: dev runs as .NET `Development`, prod as `Production` (`dotnetEnvironment`); the image holds no
  `appsettings.Development.json`, since the canary has no Development default.
- Dev check: after a dev rollout succeeds, the runner runs `verify` — `curl` from the canary's own image against
  `$VERIFY_SERVICE_CANARY/health` and `/api/pings` — and auto-promote takes the release only when it passed.
  The URL carries the service's platform-network name, which a target's `AllowedHosts` (public host plus `localhost`)
  does not allow, so each request sends `Host: localhost`; without it host filtering answers 400.
  `HostFilteringTests` runs the E2E host under such an `AllowedHosts` and replays the descriptor's requests.

## Local image check

```bash
docker build --build-arg APP_VERSION=0.1.99 -f engineering/deployment/Dockerfile -t wheelhouse-canary:local engineering/codebase
```

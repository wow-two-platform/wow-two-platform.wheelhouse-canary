# deployment/

How WheelhouseCanary ships. Descriptor format: wow-two-ws/conventions/deployment/descriptor/deploy-descriptor.md.

| File | Role |
|---|---|
| `deploy.yml` | one service, `canary`: `/health` check, 256 MB, settings keys, `needs: [postgres]`, one private `app` site |
| `Dockerfile` | three stages: build the Vue app, publish the .NET host with the app in `wwwroot`, run on `aspnet` with `curl` for the health check |
| `docker-compose.yml` | local verification: the image plus a PostgreSQL; `docker compose up -d database` alone serves an IDE run |

## Contract with Wheelhouse

- Image: `ghcr.io/wow-two-platform/wow-two-platform.wheelhouse-canary/canary`, published by `.github/workflows/publish-docker-image.yml` through the shared `publish.yml@v0.3.0`; CI never deploys.
- Version: `X.Y` from the newest `planning/version-track/v{X.Y}/`; the page, the image tag and `GET /api/system/status` report the same `X.Y.Z`.
- Settings a target must provide in the service's settings file (`/app/appsettings.Local.json`): `DatabaseSettings:ConnectionString`, `Canary:Label`. The label (`dev`, `prod`) shows on the page, proving each environment read its own file.
- Database: the host creates the database if missing and applies pending `Migrations/` before it serves, so a first deploy builds the schema and an upgrade applies only the new migrations.
- Health: `GET /health` answers 200 only while PostgreSQL answers; an unreachable database fails the rollout check.
- No volumes and no host ports; the ingress routes the `app` site (private exposure) to port 8080.

## Local image check

```bash
docker build --build-arg APP_VERSION=0.1.99 -f engineering/deployment/Dockerfile -t wheelhouse-canary:local engineering/codebase
```

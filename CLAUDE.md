# wow-two-platform.wheelhouse-canary

The minimal sample app (Vue + .NET + PostgreSQL) that Wheelhouse deploys to its own dev, test and prod environments
to prove every deployment flow before a Wheelhouse release is promoted. Keep it tiny: a feature belongs here only
when a deployment flow needs it to prove something.

## Lazy loading

- Do NOT pre-read files at conversation start; open a file only when the task needs it.
- Navigate code under `engineering/codebase/` with `tree`/`find`/`grep`.

## Structure

```
engineering/
├── codebase/
│   ├── wheelhouse-canary.backend-services/    ← .NET Clean Arch, WheelhouseCanary.BackendServices.slnx (+ tests/)
│   └── wheelhouse-canary.frontend-services/   ← private pnpm workspace; Vue app in apps/web/
├── deployment/                                ← deploy.yml · Dockerfile · docker-compose.yml · deployment.md
└── planning/version-track/v0.1/               ← CI derives the product version X.Y from the newest folder
```

## Rules

- Conventions: wow-two-ws/conventions/. This repo adds no deltas of its own.
- `engineering/deployment/deploy.yml` follows wow-two-ws/conventions/deployment/descriptor/deploy-descriptor.md; hostnames, secrets and environments live in Wheelhouse targets, never here.
- CI has no deploy job: Wheelhouse deploys the published bundle. Do not add one.
- Migrations are `Migrations/NNN-name/{Apply,Rollback}.sql` in `WheelhouseCanary.Persistence`; every `Rollback.sql` is a real inverse.
- The backend tests need Docker (PostgreSQL via Testcontainers) and, because the Api build builds the Vue app, node and pnpm.
- Opening a new `version-track/v{X.Y}/` folder is the version bump; nobody edits a patch number.

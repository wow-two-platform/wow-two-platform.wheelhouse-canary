# syntax=docker/dockerfile:1

# One image serving the .NET API and the Vue app. The build context MUST be engineering/codebase/, which holds
# both wheelhouse-canary.backend-services and wheelhouse-canary.frontend-services.
#
#   docker build -f engineering/deployment/Dockerfile -t wheelhouse-canary:local engineering/codebase
#
# The image holds no environment: the database connection string and the label arrive at run time, through the
# environment's settings file (/app/appsettings.Target.json) or the DB_CONNECTION variable.

# ---- Stage 1: build the Vue workspace ----
FROM node:24.11.0-alpine AS spa
RUN npm install --global pnpm@10.33.2
WORKDIR /src/wheelhouse-canary.frontend-services
COPY wheelhouse-canary.frontend-services/package.json wheelhouse-canary.frontend-services/pnpm-lock.yaml wheelhouse-canary.frontend-services/pnpm-workspace.yaml ./
COPY wheelhouse-canary.frontend-services/apps/web/package.json ./apps/web/package.json
RUN pnpm install --frozen-lockfile
COPY wheelhouse-canary.frontend-services/ ./
# The build's version from the shared publish workflow (X.Y.Z or a pre-release); unset in a local build. Declared
# after the dependency layers, since a changed build argument misses the cache of every RUN after it.
ARG APP_VERSION
RUN pnpm build

# ---- Stage 2: publish the .NET API (the SPA arrives from stage 1) ----
# Stays in global.json's 10.0.3xx band; the floating 10.0 tag moved to 10.0.4xx, which the pin refuses.
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
ARG TARGETARCH
WORKDIR /src
COPY wheelhouse-canary.backend-services/ ./
COPY --from=spa /src/wheelhouse-canary.frontend-services/apps/web/dist ./WheelhouseCanary.Api/wwwroot
# MSBuild reads APP_VERSION from the environment; Directory.Build.props turns it into the product version.
ARG APP_VERSION
RUN case "$TARGETARCH" in amd64) rid=linux-x64 ;; arm64) rid=linux-arm64 ;; *) exit 1 ;; esac \
    && dotnet publish WheelhouseCanary.Api/WheelhouseCanary.Api.csproj -c Release -o /app -r "$rid" --self-contained false -p:BuildSpa=false

# ---- Stage 3: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_HTTP_PORTS=8080
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=60s --retries=4 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "WheelhouseCanary.Api.dll"]

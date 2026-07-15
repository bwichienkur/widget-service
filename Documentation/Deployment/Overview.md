# Deployment

## Build Process

### Solution
`WidgetProviderService.sln` — 3 projects, `Release|Any CPU` configuration.

### Local Build
```bash
dotnet restore WidgetProviderService.sln --configfile nuget.config
dotnet build WidgetProviderService.sln -c Release
dotnet publish EDDY.IS.WidgetProvider.Service/EDDY.IS.WidgetProvider.Service.csproj -c Release
```

### NuGet Feed
`nuget.config` at solution root — likely private EducationDynamics feed (check file for source URLs).

### TypeScript Compilation
`Microsoft.TypeScript.MSBuild` compiles `scripts/AdAggregator.ts` and `scripts/MouseTracker.ts` at build time.

---

## CI/CD Pipelines

### Feature Branch CI (`ci-pipelines.yaml`)

| Property | Value |
|----------|-------|
| Trigger | `feature/*` branches |
| PR trigger | `development`, `qa`, `uat` |
| Pool | `Back Office` (Windows) |
| Template | `pipelines/templates/ci-build.yaml@PipelineTemplates` |
| Solution | `WidgetProviderService.sln` |
| Configuration | `Release` |
| Publish Profile | `NonProd` |

### Deploy Pipeline (`azure-pipelines.yaml`)

| Property | Value |
|----------|-------|
| Trigger branches | `development`, `qa`, `uat` |
| Pool | `Back Office` (Windows) |
| Template | `pipelines/templates/build-deploy.yaml@PipelineTemplates` |
| App pool | `widget.educationdynamics.com` |
| Deploy path | `F:\inetpub\wwwroot\widget.educationdynamics.com` |
| Rollback | Manual via `runRollback` parameter |

### Source Control
Azure DevOps: `https://educationdynamics.visualstudio.com/` (from `.sln` TFS section)

---

## Infrastructure

| Component | Technology |
|-----------|------------|
| Web server | **IIS** on Windows Server |
| App pool | `widget.educationdynamics.com` |
| Database | SQL Server (`*-sql1` servers per environment) |
| Cache | Azure Redis (`*.redis.cache.windows.net`) |
| CDN | External (Bootstrap, jQuery, Forms Engine, CMS CSS) |

**No Docker/Kubernetes** configuration found. Confidence: high.

---

## Environment Promotion

| Branch | Pipeline | Environment (inferred) |
|--------|----------|------------------------|
| `feature/*` | CI only | — |
| `development` | CI/CD | Dev |
| `qa` | CI/CD | QA |
| `uat` | CI/CD | UAT |

`appsettings.NonProd.json` uses `PROJECT_NAME` token replaced during deploy for environment-specific URLs.

---

## Configuration Differences

| Setting | Local (`appsettings.json`) | NonProd Template |
|---------|---------------------------|------------------|
| `SERVICEURL` | `/api` | `https://widget.PROJECT_NAME.educationdynamics.local/api` |
| SQL Server | `ads-sql1` | `PROJECT_NAME-sql1` |
| Redis cache enabled | `false` | `true` |
| Redis prefix | `WS_ntide` | `WS_PROJECT_NAME` |
| GP Listing URL | `localhost:7174` | `ags.PROJECT_NAME.educationdynamics.local` |
| Style/script URLs | Mix of localhost + microgp | `widget.PROJECT_NAME` hostnames |

---

## Publish Profiles

`Properties/PublishProfiles/` — `NonProd` profile referenced by pipeline (contents not fully analyzed; standard IIS folder publish expected).

---

## Post-Deploy Verification

1. Hit `/Header` — verify IP forwarding headers
2. Load `/testclients/testadlistingapi.html` — integration smoke test
3. Call `GET /api/WidgetProvider/GetWidgetJs?vendorToken={valid-guid}`
4. Check `Logs/app-logs.txt` on server
5. Verify NLog DB exceptions not spiking in `EddyLogging.dbo.Exception`

---

## Monitoring

| Signal | Mechanism |
|--------|-----------|
| File logs | `Logs/app-logs.txt` (90-day archive) |
| DB exceptions | NLog → `EddyLogging.dbo.Exception` |
| Console | Development lifetime console |
| APM/Telemetry | **Not configured** — no Application Insights |

---

## IIS Considerations

- .NET 8 hosting bundle required
- `web.config` generated on publish
- Static files served from `css/`, `images/`, `testclients/` via custom `UseStaticFiles` mappings
- Windows Authentication to SQL Server (app pool identity needs DB access)

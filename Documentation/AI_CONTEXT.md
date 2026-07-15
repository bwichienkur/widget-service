# AI Knowledge Base — Widget Provider Service

> **Purpose:** Enable an AI agent or new engineer to immediately understand this codebase for maintenance, extension, or modernization.

## One-Line Summary

ASP.NET Core 8 IIS-hosted service that renders embeddable education marketing widgets (ads, forms, QDF) as HTML/JS for publisher websites, configured via Nexus SQL Server.

## Repository Layout

```
WidgetProviderService.sln
├── EDDY.IS.WidgetProvider.Service/   # Web host, controllers, Razor templates, static JS/CSS
├── EDDY.IS.WidgetProvider.Core/    # Business logic, widget models, external API clients
└── EDDY.IS.WidgetProvider.Data/    # EF Core, repositories
```

**Dependency:** Service → Data → Core

## Key Entry Points

| Entry | File |
|-------|------|
| Host | `Service/Program.cs` |
| DI | `Service/Startup.cs` |
| Main API | `Service/Controllers/WidgetProviderController.cs` |
| Orchestrator | `Core/Services/WidgetPackageService.cs` |
| Widget factory | `Core/Services/ModelInstantiationService.cs` |
| Config load | `Data/WidgetRepository.cs` → `GetWidgetConfig` |

## Architecture Pattern

**Layered monolith** + **Strategy pattern** (`IRenderable` per `WidgetType`).

Adding a new widget type requires:
1. Add enum value to `WidgetType` in `Core/Entities/VendorWidgetConfig.cs`
2. Create `ComponentModels/{Name}Model.cs` implementing `IRenderable`
3. Add case in `ModelInstantiationService.CreateComponent`
4. Add Razor templates under `Service/WidgetTemplates/{WidgetType}/`
5. Register widget in Nexus DB (`WS.Widget`, settings, vendor instances)

## Business Rules (Critical)

1. **VendorToken** = `VendorWidget.VendorWidgetToken` GUID — currently only format-validated, NOT looked up
2. **ContainerName** = `VendorWidget.VendorWidgetName` — must match DOM element on publisher page
3. **WidgetId** in DB = `WidgetType` enum integer (not auto-increment)
4. **Widget components:** AdServer=1, FormsEngine=2, Custom=3
5. **Admin pages** (`admin.educationdynamics.com`) skip analytics tracking
6. **TrackId override:** When `checktrackidinsessioncookies=true` in settings, client `CookieTrackId` wins
7. **URL prefill:** `VendorWidgetUrlParameterConfig` maps page URL → default categories/subcategories/etc.
8. **Exit pop:** Exactly one ExitPop/GPExitPop per page request
9. **GP listing:** Merges Forms Engine Redis session (`{prefix}.FE.Session[{id}]`) when `fesessionid` provided

## Widget Types Quick Reference

| ID | Name | Model | External Deps |
|----|------|-------|---------------|
| 1 | AdStackSolo | AdStackSoloModel | Ad Aggregator CDN |
| 2 | SmartListing | SmartListingModel | Forms Engine + Ad Server |
| 7 | AdListingApi | AdListingApiModel | Ad Listing API (GET) |
| 8 | QDFLight | QDFLightModel | Matching Engine WCF |
| 11 | GPListingApi | GPListingApiModel | GP Listing API (POST) + Redis |
| 9/12 | ExitPop/GPExitPop | ExitPopModel/GPExitPopModel | Listing APIs on exit |

## API Surface

```
GET  /api/WidgetProvider/GetWidgetJs?vendorToken=&checkJquery=
POST /api/WidgetProvider/GetWidgetPackage          (WidgetRequest body)
POST /api/WidgetProvider/UpdateWidgetPackage       (WidgetRequest body, no external resources)
GET  /api/WidgetProvider/SaveWidgetImpression?widgetSessionGuid=
POST /api/QDF/RetrieveQDFData                      (QDFDataRequest body)
POST /api/ExitPop/CanRender                        (trackId string body)
POST /api/ExitPop/RenderAd                         (WidgetRequest body)
POST /api/ExitPop/GetWidgetTrackId                 (WidgetRequest body)
GET  /Header/Index                                 (debug headers)
```

**No authentication on any endpoint.**

## Database

| DB | Context | Purpose |
|----|---------|---------|
| Nexus | NexusContext | Widget config (read) |
| EddyTrackingIS | EddyTrackingISContext | WidgetRequest + WidgetImpression (write) |
| EddyLogging | NLog only | Exception table |

Key views: `WS.VW_VendorWidgetConfiguration`, `WS.VW_QDFTemplateConfiguration`

**No EF migrations** — schema managed externally.

## External Systems

| System | Config Key | Protocol |
|--------|------------|----------|
| Ad Listing API | AdListingApiURL | HTTP GET |
| GP Listing API | GPListingApiURL | HTTP POST JSON |
| Matching Engine | MatchingServiceURL | WCF/SOAP |
| Forms Engine | ScriptSources:Bundled* | CDN JS |
| Azure Redis | RedisConnection | Redis strings |

## Configuration Sections

- `ScriptSources`, `StyleSources` — CDN URLs
- `WidgetJsReplaceValues` — token replacement in client JS
- `RedisSettings` — cache toggle, prefix, duration
- `ConnectionStrings` — SQL + Redis
- `MinifyJavascript`, `HttpRequestTimeout`

## Naming Conventions

- Interfaces: `I{Name}` in `Core/Interfaces/`
- Widget renderers: `{Type}Model` in `Core/ComponentModels/`
- Razor views: `Service/WidgetTemplates/{WidgetType}/{ViewName}.cshtml`
- DB schema: `WS` for widget tables
- Site settings keys: lowercase (`categories`, `subcategories`)
- Forms Engine field codes: PascalCase with underscores (`Desired_Degree_Level`)
- Mapping: `WidgetSettingsMappingService` + `SettingsMap` static dictionaries

## DI Lifetimes

- **Singleton:** `IFESessionRedisService`
- **Scoped:** Everything else registered in `ConfigureDependencies`
- **Transient:** `IGPListingApiService`
- **NOT in DI:** `AdListingApiService`, `MatchingServiceClient`, `*Model` components

## Known Bugs / Tech Debt

1. `WidgetRepository.GetWidgetConfig` — settings not filtered per component (line 50-53)
2. Vendor token not validated against DB
3. Redis password in appsettings.json
4. `.Result` blocking in QDFService
5. `new HttpClient()` in AdListingApiService
6. `MapAdditionalSettings` — `ContainsKey` before `Add` inverted (line 21)
7. Program.cs log message says "Ad Reporting Service" (legacy name)

## Testing

**No automated tests.** Manual test pages in `Service/testclients/*.html`.

## Deployment

- Azure DevOps → IIS `widget.educationdynamics.com`
- Branches: development/qa/uat deploy; feature/* CI only
- Path: `F:\inetpub\wwwroot\widget.educationdynamics.com`

## Safe Change Guidelines for AI Agents

1. **Do not** commit secrets to appsettings
2. **Do not** break `WidgetRequest` JSON contract — client JS depends on it
3. **Preserve** `##SERVICEURL##` token replacement in client JS files
4. **Test** with `testclients/` HTML pages after changes
5. **WidgetType enum values** must match DB `WidgetId` — never renumber
6. **Razor view paths** are hardcoded in `ViewRenderService` — keep folder names in sync with enum
7. When adding async, update full chain (interface → implementation → caller)

## Documentation Index

| Topic | Path |
|-------|------|
| Executive summary | `Documentation/ExecutiveSummary.md` |
| Architecture | `Documentation/Architecture.md` |
| Business processes | `Documentation/BusinessProcesses.md` |
| APIs | `Documentation/APIs/Endpoints.md` |
| Database | `Documentation/Database/Overview.md` |
| Services | `Documentation/Services/Overview.md` |
| Security | `Documentation/Security/Review.md` |
| Refactoring | `Documentation/Refactoring/Recommendations.md` |
| Diagrams | `Documentation/Diagrams/` |

## Organization Context

- **Owner:** EducationDynamics (Eddy)
- **TFS/Azure DevOps:** educationdynamics.visualstudio.com
- **Product:** Inbound Science (IS) widget delivery for education lead generation

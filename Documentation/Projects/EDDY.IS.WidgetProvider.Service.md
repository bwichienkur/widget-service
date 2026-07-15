# EDDY.IS.WidgetProvider.Service

## Purpose

ASP.NET Core 8 **web host** — HTTP entry point, DI composition root, Razor view location for widget templates, static assets (JS/CSS/images), and test client pages.

## Responsibilities

- Register all services and DbContexts (`Startup.cs`)
- Expose REST/MVC endpoints (`Controllers/`)
- Host Razor templates (`WidgetTemplates/{WidgetType}/`)
- Serve static widget assets (`js/`, `css/`, `images/`, `scripts/`)
- Bootstrap NLog logging (`Program.cs`, `nlog.config`)
- Pre-warm URL configuration cache on startup

## Project References

| Reference | Path |
|-----------|------|
| `EDDY.IS.WidgetProvider.Data` | `../EDDY.IS.WidgetProvider.Data/` |

Core is used transitively via Data → Core.

## NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| jquery.TypeScript.DefinitelyTyped | 3.1.2 | TS definitions |
| Microsoft.TypeScript.MSBuild | 4.6.4 | Compile `scripts/*.ts` |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 6.0.2 | Scaffolding |
| NLog.Appsettings.Standard | 2.1.0 | NLog config from appsettings |
| NLog.Extensions.Logging | 1.7.4 | MS logging bridge |
| NLog.Web.AspNetCore | 4.14.0 | ASP.NET Core NLog |
| System.Data.SqlClient | 4.8.3 | Legacy SQL (NLog DB target) |

## Important Classes

| Class | File | Role |
|-------|------|------|
| `Program` | `Program.cs` | Host builder, NLog init |
| `Startup` | `Startup.cs` | DI registration, middleware pipeline |
| `WidgetProviderController` | `Controllers/WidgetProviderController.cs` | Main widget API |
| `QDFController` | `Controllers/QDFController.cs` | QDF AJAX data |
| `ExitPopController` | `Controllers/ExitPopController.cs` | Exit pop API |
| `HeaderController` | `Controllers/HeaderController.cs` | Debug headers view |

## Configuration Files

| File | Environment |
|------|-------------|
| `appsettings.json` | Base |
| `appsettings.Development.json` | Development overlay |
| `appsettings.NonProd.json` | Non-prod deploy template (`PROJECT_NAME` placeholders) |
| `nlog.config` | Logging targets |
| `Properties/launchSettings.json` | Local dev URLs |
| `tsconfig.json` | TypeScript compile settings |

## Folder Structure

| Folder | Purpose |
|--------|---------|
| `Controllers/` | API and MVC controllers |
| `WidgetTemplates/` | Razor views per widget type |
| `js/` | Client widget runtime (`eddywidget.js`, `adlisting.js`, `qdf.js`) |
| `scripts/` | TypeScript sources (`AdAggregator.ts`, `MouseTracker.ts`) |
| `css/` | Widget stylesheets |
| `testclients/` | HTML integration test pages |
| `Views/Header/` | Header debug view |

## External Services

Configured via appsettings — see `Documentation/Deployment/Configuration.md`.

## Potential Improvements

1. Add direct project reference to Core (explicit dependency)
2. Extract duplicated `GetClientIPAddress()` to shared middleware/filter
3. Enable HTTPS redirection and authorization
4. Register `IAdListingApiService` in DI consistently
5. Add health check endpoints (`/health`)
6. Upgrade EF Core packages to 8.x

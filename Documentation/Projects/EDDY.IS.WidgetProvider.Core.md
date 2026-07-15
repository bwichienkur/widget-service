# EDDY.IS.WidgetProvider.Core

## Purpose

**Domain and application logic library** — widget rendering components, external service integrations, DTOs, mapping, caching, and view rendering abstraction.

## Responsibilities

- Define widget type models implementing `IRenderable`
- Orchestrate widget package assembly (`WidgetPackageService`)
- Integrate with Matching Engine (WCF), Ad/GP Listing APIs (HTTP), Redis
- Map site settings to API parameters (`WidgetSettingsMappingService`)
- Render Razor views to strings (`ViewRenderService`)
- Provide repository interfaces consumed by Data layer

## Project References

None (standalone class library).

## NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Mvc.Razor | 2.2.0 | View rendering |
| Microsoft.Extensions.Http | 6.0.0 | HttpClientFactory |
| NUglify | 1.19.2 | JS minification |
| StackExchange.Redis | 2.5.61 | Redis cache |
| System.ServiceModel.* | 4.9.0 | WCF Matching Engine client |

## Folder Structure

| Folder | Pattern | Key Types |
|--------|---------|-----------|
| `ComponentModels/` | Strategy/Component | `*Model` implementing `IRenderable` |
| `Services/` | Application services | `WidgetPackageService`, `QDFService`, etc. |
| `Interfaces/` | Contracts | `IWidgetRepository`, `IRenderable`, `ICacheable` |
| `Entities/` | Domain models | `VendorWidgetConfig`, `QDFTemplate`, placements |
| `DTO/` | API transport | `WidgetRequest`, `QDFDataRequest/Response` |
| `Settings/` | View models for includes | `FormsEngineIncludesModel`, `AdServerIncludesModel` |
| `Extensions/` | Utilities | `EncodingExtension`, `BooleanConverter` |
| `Connected Services/MatchingEngine/` | WCF proxy | `MatchingServiceClient`, `Reference.cs` |

## Important Classes

See `Documentation/Services/Overview.md` and `Documentation/Entities/Overview.md`.

## Configuration Consumed

Read via `IConfiguration` injection — sections: `ScriptSources`, `StyleSources`, `WidgetJsReplaceValues`, `RedisSettings`, `AdListingApiURL`, `GPListingApiURL`, `MatchingServiceURL`, `MinifyJavascript`, `HttpRequestTimeout`.

## External Services

| Service | Client Class |
|---------|-------------|
| Matching Engine WCF | `MatchingEngine.MatchingServiceClient` |
| Ad Listing API | `AdListingApiService` |
| GP Listing API | `GPListingApiService` |
| Azure Redis | `CacheService`, `FESessionRedisService` |

## Potential Improvements

1. Remove ASP.NET Razor dependency from Core — move `ViewRenderService` to Service project
2. Register all HTTP services in DI; eliminate `new AdListingApiService()`
3. Replace `.Result` blocking calls in `QDFService` with async throughout
4. Add strongly-typed options classes (`IOptions<RedisSettings>`)
5. Extract WCF client factory for connection reuse
6. Fix `MapAdditionalSettings` logic bug (checks `ContainsKey` before `Add` — line 21)

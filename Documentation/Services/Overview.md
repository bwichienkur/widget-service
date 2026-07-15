# Services Overview

All services registered in `Startup.ConfigureDependencies()` unless noted.

## Registration Summary

| Service | Interface | Lifetime | File |
|---------|-----------|----------|------|
| `WidgetPackageService` | `IWidgetPackageService` | Scoped | `Services/WidgetPackageService.cs` |
| `FileSerializeService` | `IFileSerializeService` | Scoped | `Services/FileSerializeService.cs` |
| `ViewRenderService` | `IViewRenderService` | Scoped | `Services/ViewRenderService.cs` |
| `WidgetRepository` | `IWidgetRepository` | Scoped | `Data/WidgetRepository.cs` |
| `CampaignRepository` | `ICampaignRepository` | Scoped | `Data/CampaignRepository.cs` |
| `MinificationService` | `IMinificationService` | Scoped | `Services/MinificationService.cs` |
| `QDFService` | `IQDFService` | Scoped | `Services/QDFService.cs` |
| `CacheService` | `ICacheService` | Scoped | `Services/CacheService.cs` |
| `ModelInstantiationService` | `IModelInstantiationService` | Scoped | `Services/ModelInstantiationService.cs` |
| `GPListingApiService` | `IGPListingApiService` | **Transient** | `Services/GPListingApiService.cs` |
| `FESessionRedisService` | `IFESessionRedisService` | **Singleton** | `Services/FESessionRedisService.cs` |
| `AdListingApiService` | `IAdListingApiService` | **Not registered** | Instantiated with `new` |

---

## WidgetPackageService

**Responsibilities:** Orchestrate full widget package — load configs, parallel render, minify, async save tracking.

**Dependencies:** `IWidgetRepository`, `IViewRenderService`, `IMinificationService`, `IConfiguration`, `ICacheService`, `IModelInstantiationService`, `ILogger`

**Consumers:** `WidgetProviderController`

**Caching:** Reads/writes `URLCONFIGS` via `ICacheService`; delegates per-widget cache to `ICacheable` models

**Error handling:** Logs and returns partial/empty string

**Issues:** `Task.Run` for save — no error propagation; `t.Result` after `WhenAll` (acceptable post-await)

---

## QDFService

**Responsibilities:** Build QDF templates with populated dropdown options; proxy Matching Engine for categories/subjects/specialties/program levels.

**Dependencies:** `IWidgetRepository`, `IConfiguration`, `ICacheService`

**Consumers:** `QDFController`, `QDFLightModel`, `GPQDFLightModel`

**External API:** WCF `MatchingServiceClient` — new client per method call

**Retry:** None

**Issues:** Blocking `.Result` on async WCF calls; new WCF client per request

---

## GPListingApiService

**Responsibilities:** POST JSON to GP Listing API; build request from filter settings + widget metadata.

**Dependencies:** `IHttpClientFactory`, `IConfiguration`

**Consumers:** `GPListingApiModel`, `ExitPopController` (GP exit pop)

**Retry:** None

**Timeout:** `HttpRequestTimeout` config (default 300s)

**Caching:** None at service level

---

## AdListingApiService

**Responsibilities:** GET query string to legacy Ad Listing API.

**Dependencies:** `IConfiguration` (static field)

**Consumers:** `AdListingApiModel`, `ExitPopController` (via `new`)

**Issues:** Creates new `HttpClient` per call (socket exhaustion risk); static config; blocking `GetAwaiter().GetResult()`

---

## ViewRenderService

**Responsibilities:** Render Razor `.cshtml` templates to HTML strings outside MVC request pipeline.

**Dependencies:** `IRazorViewEngine`, `ITempDataProvider`, `IServiceProvider`

**Consumers:** All `IRenderable` models, `ExitPopController`

**Template path:** `~/WidgetTemplates/{WidgetType}/{viewName}.cshtml`

---

## CacheService

**Responsibilities:** Dual cache — Redis (string, optional) + `IMemoryCache` (object).

**Dependencies:** `IConfiguration`, `IMemoryCache`

**Redis:** Lazy `ConnectionMultiplexer`; key prefix from `RedisSettings:CachePrefix`

**Enabled:** `RedisSettings:CacheEnabled` (false in default appsettings.json)

**Issues:** `RemoveCacheItem` not implemented; static `_config`/`_cache`; distributed get not implemented

---

## FESessionRedisService

**Responsibilities:** Read Forms Engine session from Redis; filter to supported keys.

**Dependencies:** `IConfiguration`, `ILogger`

**Lifetime:** Singleton (shared Redis connection)

**Redis key:** `{FormsEngineRedisCachePrefix}.FE.Session[{sessionId}]`

**Consumers:** `GPListingApiModel`

---

## FileSerializeService

**Responsibilities:** Read JS files from disk; replace `##SERVICEURL##`, `##JQUERY_VERSION##` tokens; optional minify.

**Dependencies:** `IMinificationService`, `IConfiguration`

**Consumers:** `WidgetProviderController`, multiple component models

---

## MinificationService

**Responsibilities:** NUglify JavaScript minification.

**Dependencies:** None

---

## ModelInstantiationService

**Responsibilities:** Factory — map `WidgetType` enum to `IRenderable` component model.

**Dependencies:** `IConfiguration`, `IWidgetRepository`, `IQDFService`, `IFileSerializeService`, `IServiceProvider`, `IFESessionRedisService`

**Pattern:** Simple Factory (switch on enum)

---

## WidgetSettingsMappingService

**Responsibilities:** Static mapping dictionaries — site settings keys → Ad Server / Forms Engine / Listing API parameter names.

**Type:** Static class (no DI)

**Consumers:** All listing and form widget models, `ExitPopController.CleanFilters`

---

## CampaignRepository

**Responsibilities:** Check `AllowExitPops` for campaign track ID.

**External API:** WCF Matching Engine `GetCampaignDetailByTrackIDAsync`

**Note:** Lives in Data project but calls external WCF service.

---

## WidgetRepository

See `Documentation/Database/Overview.md` and `Documentation/Projects/EDDY.IS.WidgetProvider.Data.md`.

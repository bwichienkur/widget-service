# Refactoring Recommendations

Ranked by combined business and technical impact.

---

## R01: Validate Vendor Tokens Against Database

| Attribute | Value |
|-----------|-------|
| **Priority** | P0 — Critical |
| **Problem** | Any valid GUID is accepted as vendor token (`WidgetProviderController.cs:33` TODO) |
| **Risk** | Unauthorized widget rendering, analytics pollution, potential data leakage via misconfigured widgets |
| **Effort** | Small (1–2 days) |
| **Business Impact** | Prevents unauthorized publishers from consuming ad inventory |
| **Technical Impact** | Adds one DB lookup per bootstrap request; cache token validity |
| **Implementation** | Add `IsValidVendorToken(Guid)` to `IWidgetRepository`; query `VendorWidget` where `VendorWidgetToken=@token AND IsEnabled=1`; return 401 if invalid |

---

## R02: Remove Secrets from Source Control

| Attribute | Value |
|-----------|-------|
| **Priority** | P0 — Critical |
| **Problem** | Redis password in `appsettings.json` |
| **Risk** | Credential compromise |
| **Effort** | Small |
| **Business Impact** | Compliance, security audit pass |
| **Technical Impact** | Azure Key Vault provider or env var binding |
| **Implementation** | Rotate Redis key; use `ConnectionStrings__RedisConnection` env var; add secret scanning to CI |

---

## R03: Fix GetWidgetConfig Settings Grouping Bug

| Attribute | Value |
|-----------|-------|
| **Priority** | P0 — High |
| **Problem** | Inner loop adds ALL settings to each component dictionary (`WidgetRepository.cs:50-53`) |
| **Risk** | Wrong widget behavior, duplicate key exceptions |
| **Effort** | Small (hours) |
| **Business Impact** | Correct ad targeting and form configuration |
| **Technical Impact** | Filter `configList` by `component.Key` or use `component` grouping properly |
| **Implementation** | `foreach (var config in component)` instead of `configList` |

---

## R04: Eliminate Async Blocking (.Result)

| Attribute | Value |
|-----------|-------|
| **Priority** | P1 — High |
| **Problem** | `QDFService` and `AdListingApiService` block on async calls |
| **Risk** | Thread pool starvation under concurrent QDF dropdown loads |
| **Effort** | Medium |
| **Business Impact** | Widget timeout failures on high-traffic publisher pages |
| **Technical Impact** | Async all the way through `IQDFService`, `IRenderable`, controllers |
| **Implementation** | `await msc.GetCategoriesAsync()`; change `Configure` to async if needed |

---

## R05: Register AdListingApiService in DI with HttpClientFactory

| Attribute | Value |
|-----------|-------|
| **Priority** | P1 — High |
| **Problem** | `new HttpClient()` per request in `AdListingApiService` |
| **Risk** | Socket exhaustion, DNS issues |
| **Effort** | Small |
| **Business Impact** | Reliability of legacy ad listing widgets |
| **Technical Impact** | `services.AddHttpClient<IAdListingApiService, AdListingApiService>()` |
| **Implementation** | Mirror `GPListingApiService` pattern; remove `new` from models/controllers |

---

## R06: Add Automated Test Suite

| Attribute | Value |
|-----------|-------|
| **Priority** | P1 — High |
| **Problem** | Zero test projects in solution |
| **Risk** | Regressions on every deploy |
| **Effort** | Large (ongoing) |
| **Business Impact** | Confidence in widget rendering for revenue-critical flows |
| **Technical Impact** | xUnit + WebApplicationFactory for integration; unit tests for mapping services |
| **Implementation** | Start with `WidgetSettingsMappingService`, `GetWidgetConfig`, API contract tests |

---

## R07: Extract Shared IP Resolution Middleware

| Attribute | Value |
|-----------|-------|
| **Priority** | P2 — Medium |
| **Problem** | `GetClientIPAddress()` duplicated in 3 controllers (~30 lines each) |
| **Risk** | Inconsistent IP logic (WidgetProvider has Cloudflare header; ExitPop does not) |
| **Effort** | Small |
| **Business Impact** | Accurate geo targeting for ads |
| **Technical Impact** | `IClientIpResolver` service or middleware setting `WidgetRequest.IPAddress` |
| **Implementation** | Single implementation with Cloudflare + X-Forwarded-For support |

---

## R08: Upgrade EF Core to 8.x

| Attribute | Value |
|-----------|-------|
| **Priority** | P2 — Medium |
| **Problem** | EF 6.0.3 on .NET 8 runtime |
| **Risk** | Missing performance fixes, security patches |
| **Effort** | Medium |
| **Business Impact** | Long-term maintainability |
| **Technical Impact** | Package upgrade + regression test |
| **Implementation** | `Microsoft.EntityFrameworkCore.SqlServer` 8.x |

---

## R09: Re-architect to Clean Architecture Boundaries

| Attribute | Value |
|-----------|-------|
| **Priority** | P3 — Strategic |
| **Problem** | Core depends on Razor; Data contains WCF client; templates in Service but paths in Core |
| **Risk** | Hard to test, hard to split services |
| **Effort** | Very Large |
| **Business Impact** | Enables future microservice extraction |
| **Technical Impact** | Move `ViewRenderService` to Service; interfaces only in Core; Infrastructure project for EF+WCF |
| **Implementation** | Phased — start with moving WCF out of Data |

---

## R10: Add Health Checks and Observability

| Attribute | Value |
|-----------|-------|
| **Priority** | P2 — Medium |
| **Problem** | No health endpoints, no Application Insights |
| **Risk** | Slow incident detection |
| **Effort** | Small–Medium |
| **Business Impact** | Uptime for publisher embeds |
| **Technical Impact** | `/health` with SQL + Redis checks; OpenTelemetry |
| **Implementation** | `AddHealthChecks().AddSqlServer().AddRedis()` |

---

## R11: Sanitize Error Responses

| Attribute | Value |
|-----------|-------|
| **Priority** | P1 — Medium |
| **Problem** | Exception messages returned to clients (`ExitPopController`, `WidgetProviderController`) |
| **Risk** | Information disclosure |
| **Effort** | Small |
| **Business Impact** | Security compliance |
| **Technical Impact** | Global exception handler returning generic `WidgetError` codes |
| **Implementation** | `IExceptionHandler` middleware (.NET 8) |

---

## R12: Batch Widget Config Database Queries

| Attribute | Value |
|-----------|-------|
| **Priority** | P2 — Medium |
| **Problem** | N+1 queries per container |
| **Risk** | Latency on multi-widget pages |
| **Effort** | Medium |
| **Business Impact** | Faster page loads → better conversion |
| **Technical Impact** | `GetWidgetConfigs(IEnumerable<string> containers, Guid token)` |
| **Implementation** | Single LINQ `Where` with `Contains` on container names |

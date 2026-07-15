# Cross-Cutting Concerns

## Authentication & Authorization

**Status: Not implemented**

- `app.UseAuthorization()` commented out (`Startup.cs:75`)
- No `[Authorize]` attributes on controllers
- No JWT, cookie auth, or API key middleware
- Publisher identity = `VendorToken` GUID in request (format check only)

See `Documentation/Security/Review.md` for recommendations.

---

## Logging

### Framework
- **NLog** via `NLog.Web.AspNetCore` (`Program.cs:48`)
- MS logging cleared; minimum level `Trace` in host builder

### Targets (`nlog.config`)

| Target | Type | Level | Output |
|--------|------|-------|--------|
| `textFile` | File | Info+ | `Logs/app-logs.txt` (daily archive, 90 days) |
| `database` | SQL | Error+ | `EddyLogging.dbo.Exception` |
| `lifetimeConsole` | Console | Info+ | Structured MS layout |
| `blackHole` | Null | — | Swallows Microsoft.* Info |

### ILogger Usage
Controllers and services inject `ILogger<T>` for error logging in catch blocks.

### Structured Logging
**Minimal** — mostly `LogError(ex, ex.Message)` without structured properties.

### Correlation IDs
**Not implemented.** `WidgetRequestGuid` serves as per-request widget session ID but is not propagated to logs automatically.

### Telemetry / Application Insights
**Not configured.**

---

## Exception Handling

### Global Handlers
**None.** No `UseExceptionHandler`, `IExceptionHandler`, or middleware.

### Per-Controller Pattern
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, ex.Message);
    return Content($"WidgetError: {ex.Message}");
}
```

### Error Response Formats
| Pattern | Used By |
|---------|---------|
| `WidgetError: {message}` | WidgetProviderController |
| `WidgetError` (no detail) | UpdateWidgetPackage |
| `ex.Message` as body | ExitPopController |
| `false` | CanRender on error |
| Empty string | RenderAd on no ads |

### NLog DB Exception Schema
Inserts: Priority, Severity, Title, Timestamp, MachineName, AppDomainName, ProcessID, ProcessName, Message, ExceptionMessage, StackTrace

### Retry Logic
**None** for HTTP or WCF calls.

### Compensation
**None.** Failed widget renders return partial/empty content; tracking save failures are silent.

---

## Background Processing

| Mechanism | Present? |
|-----------|----------|
| Hangfire | No |
| Quartz | No |
| Azure Functions | No |
| IHostedService | No |
| Timers | No |
| Message queues | No |

**Fire-and-forget:** `Task.Run(() => SaveWidgetRequests(...))` only.

---

## Design Patterns

| Pattern | Where | How |
|---------|-------|-----|
| **Repository** | `IWidgetRepository`, `ICampaignRepository` | Interface in Core, impl in Data |
| **Strategy** | `IRenderable` + per-widget models | Selected by `WidgetType` |
| **Factory** | `ModelInstantiationService` | Switch creates concrete models |
| **Template Method** | `FormsEngineBaseModel` | Abstract `FormsEngineServiceUrl` |
| **Adapter** | `QDFService` | Wraps WCF ME types → domain entities |
| **Facade** | `WidgetPackageService` | Single entry for package assembly |
| **Cache-Aside** | `ICacheable`, `CacheService` | Check cache → render → set |
| **DTO** | `Core/DTO/` | API transport separate from entities |
| **Static Utility** | `WidgetSettingsMappingService`, `SettingsMap` | Key mapping dictionaries |

**Not used:** Mediator, CQRS, Unit of Work (EF default), Specification, Observer, Decorator (beyond middleware).

---

## Testing

| Type | Status |
|------|--------|
| Unit tests | **None** |
| Integration tests | **None** |
| E2E tests | Manual HTML in `testclients/` |
| Coverage | 0% |

### Recommended First Tests
1. `WidgetSettingsMappingService.Map` — all dictionary keys
2. `WidgetRepository.GetWidgetConfig` — after bug fix
3. `WidgetProviderController.GetWidgetJs` — invalid token returns 404
4. `QDFService.CreateRequest` — field mapping to ME request
5. Integration: `WebApplicationFactory` + in-memory or Testcontainers SQL

### Mocking Strategy (Recommended)
- Mock `IWidgetRepository`, `IGPListingApiService`, `IQDFService` for unit tests
- Use WireMock for HTTP API integration tests
- WCF client abstraction interface for ME tests

---

## Code Quality Issues

| Category | Examples |
|----------|----------|
| **DRY violations** | `GetClientIPAddress` x3, `MapServiceResponse` x3 |
| **God classes** | `ExitPopController` (388 lines), `QDFService` (435 lines) |
| **SOLID violations** | `AdListingApiService` static config; Data repo creates DbContext |
| **Dead code** | Commented QDF template cache in QDFService; unused `Campaign` DbSet |
| **Naming** | `marchingEngineClient` typo in CampaignRepository; `htlm` typo in ExitPopController |
| **Magic strings** | `"URLCONFIGS"`, `"WidgetError"` |
| **Inconsistent async** | Mix of sync/async in same flows |

---

## Folder Documentation Summary

### Service/Controllers
MVC/API endpoints. Pattern: thin controllers, fat services (mostly followed except ExitPopController).

### Service/WidgetTemplates
Razor views organized by `WidgetType` enum name. Each folder contains widget-specific `.cshtml` and sometimes partials.

### Core/ComponentModels
Strategy implementations. Each model: `Configure()` loads data, `RenderAsync()` produces output.

### Core/Services
Application services and static utilities.

### Core/Connected Services
Auto-generated WCF proxy for Matching Engine — do not hand-edit `Reference.cs`.

### Data/Entities
EF Core entity classes and DbContext configurations.

---

## Class Documentation — Key Classes

Detailed summaries for the most important classes. See source files for full method lists.

### WidgetPackageService
- **Complexity:** High — orchestration, parallel render, caching, URL augmentation
- **Thread safety:** Scoped per request — safe
- **Issues:** `Task.Run` fire-and-forget; string concat

### WidgetRepository.GetWidgetConfig
- **Complexity:** Medium
- **Bug:** Settings grouping (high confidence)
- **Methods:** 6 public, 3 private helpers

### QDFService
- **Complexity:** High — ME request building, macro replacement, template population
- **Thread safety:** Scoped; but static `macroValues` list (read-only, safe)
- **Issues:** Blocking async; new WCF client per call

### GPListingApiService
- **Complexity:** Medium
- **Thread safety:** Transient with HttpClientFactory — safe

### ModelInstantiationService
- **Complexity:** Low — switch factory
- **Extension point:** Add new case for new widget types

### ViewRenderService
- **Complexity:** Medium
- **Issue:** Creates synthetic HttpContext per render

---

## Performance Hot Paths

See `Documentation/Performance/Review.md`.

## Security

See `Documentation/Security/Review.md`.

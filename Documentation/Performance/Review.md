# Performance Review

## Hot Paths

1. **`POST /api/WidgetProvider/GetWidgetPackage`** — DB config load + parallel render + external API calls
2. **`WidgetRepository.GetWidgetConfig`** — View query per container
3. **`QDFService` Matching Engine calls** — WCF per dropdown population
4. **`GPListingApiService.GetDataAsync`** — HTTP POST per GP listing widget

---

## Memory Allocations

| Area | Issue | Location |
|------|-------|----------|
| String concatenation | `fullWidgetString += t.Result` builds large strings | `WidgetPackageService.cs:71` |
| Dictionary copies | Multiple `ToDictionary`, mapping passes per widget | `WidgetPackageService`, models |
| Razor render | `StringWriter` per view — acceptable | `ViewRenderService` |

**Recommendation:** Use `StringBuilder` for final package assembly.

---

## LINQ Inefficiencies

| Issue | Location |
|-------|----------|
| `GetWidgetConfig` loads all matching rows then groups in memory | `WidgetRepository.cs:31` |
| `OrderByDescending` on enum for render order | `WidgetPackageService.cs:67` — minor |

---

## N+1 Queries

**Present:** One `GetWidgetConfig` call per container in `WidgetPackageService` loop (line 47-49). Multiple containers → multiple DB round trips.

**Recommendation:** Batch load configs by vendor token + container names in single query.

---

## Async Correctness

| Issue | Severity | Location |
|-------|----------|----------|
| `.Result` on WCF async | High | `QDFService.cs:342,362,382,402` |
| `GetAwaiter().GetResult()` | High | `AdListingApiService.cs:31` |
| `Task.Run` for fire-and-forget save | Medium | `WidgetPackageService.cs:77` |
| `Task.WhenAll` + `.Result` | OK after await | `WidgetPackageService.cs:70-71` |

**Recommendation:** Propagate `async`/`await` through `IQDFService`, `IRenderable.Configure`, controllers.

---

## Blocking Calls

- WCF Matching Engine: 4 methods in `QDFService` block thread pool
- `AdListingApiService`: blocks on HTTP GET

---

## Database Performance

| Observation | Impact |
|-------------|--------|
| Keyless views — no tracking overhead | Good |
| No compiled queries | Minor overhead |
| Manual DbContext per tracking write | Connection churn | `WidgetRepository.cs:67-74` |
| No indexes documented in code | Unknown — check DBA scripts |

---

## Caching Opportunities

| Data | Current | Opportunity |
|------|---------|-------------|
| Vendor widget config | None per request | Cache by `token+container` with short TTL |
| QDF templates | Commented-out cache in QDFService | Re-enable Redis/memory cache |
| URL configs | Memory cache at startup | ✓ Implemented |
| QDFLight render output | `ICacheable` + Redis | ✓ When `CacheEnabled=true` |
| Matching Engine responses | None | Cache category lists per track+filters |

---

## HTTP Client Usage

| Service | Pattern | Issue |
|---------|---------|-------|
| `GPListingApiService` | `IHttpClientFactory` | Good |
| `AdListingApiService` | `new HttpClient()` per call | Socket exhaustion under load |

---

## Parallelization

`WidgetPackageService` parallelizes by widget type group — good. External API calls within a single `GPListingApiModel` are sequential per container.

---

## Large Object Allocations

Full widget package HTML/JS can be large (multiple CDN includes + ad HTML). Minification optional via `MinifyJavascript` config (default `false`).

---

## Recommendations

| Priority | Action | Impact |
|----------|--------|--------|
| P0 | Fix async blocking in QDFService | Thread pool starvation |
| P0 | Register AdListingApiService with HttpClientFactory | Connection stability |
| P1 | Batch widget config DB queries | Latency |
| P1 | StringBuilder for package assembly | GC pressure |
| P2 | Cache widget configs | DB load |
| P2 | Re-enable QDF template caching | ME/DB load |

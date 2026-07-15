# Configuration Reference

## appsettings.json Structure

### Logging
```json
"Logging": { "LogLevel": { "Default": "Information", "Microsoft": "Warning" } }
```

### ScriptSources
CDN URLs for external JavaScript dependencies (jQuery Validate, jQuery Modal, Forms Engine bundles, Ad Aggregator, Bootstrap, Select2).

**Bound via:** `_configuration.GetSection("ScriptSources")["{Key}"]`

### StyleSources
CDN URLs for CSS (Bootstrap, Font Awesome, widget-specific CSS, jQuery Modal).

### WidgetJsReplaceValues
| Key | Purpose |
|-----|---------|
| `SERVICEURL` | Replaces `##SERVICEURL##` in client JS |
| `BASEURL` | Base URL for modal CSS paths |
| `JQUERY_VERSION` | Replaces `##JQUERY_VERSION##` |

### ConnectionStrings
| Key | Database |
|-----|----------|
| `DefaultConnection` | Nexus |
| `EddyLoggingConnection` | EddyLogging (NLog) |
| `EddyTrackingISConnection` | EddyTrackingIS |
| `RedisConnection` | Azure Redis |

### RedisSettings
| Key | Default (local) | Purpose |
|-----|-----------------|---------|
| `CacheDuration` | `30` | Minutes for Redis string cache TTL |
| `CachePrefix` | `WS_ntide` | Redis key prefix |
| `CacheEnabled` | `false` | Enable Redis render cache |
| `FormsEngineRedisCachePrefix` | `microgp` | FE session key namespace |

### API URLs
| Key | Purpose |
|-----|---------|
| `AdListingApiURL` | Legacy listing GET endpoint |
| `GPListingApiURL` | GP listing POST endpoint |
| `MatchingServiceURL` | WCF Matching Engine endpoint |

### Other
| Key | Default | Purpose |
|-----|---------|---------|
| `MinifyJavascript` | `false` | NUglify output |
| `ApplicationName` | `WidgetProviderService` | App identifier |
| `HttpRequestTimeout` | `300` | HTTP client timeout (seconds) |
| `AllowedHosts` | `*` | Host filtering |

---

## Environment Variables

ASP.NET Core standard: `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`

No custom environment variable bindings found — all config via JSON files.

**Recommendation:** Use `ConnectionStrings__RedisConnection` env var override for secrets.

---

## Secrets

| Secret | Current Storage | Should Be |
|--------|-----------------|-----------|
| Redis password | appsettings.json | Key Vault / env var |
| SQL credentials | Windows integrated auth | OK if IIS identity managed |

User Secrets ID: `21afb0a2-9f85-405e-b349-0089ee8e7586` (local dev only)

---

## Feature Flags

No formal feature flag framework. Behavioral toggles via config:

- `MinifyJavascript`
- `RedisSettings:CacheEnabled`
- Per-widget DB settings (`testmode`, `passiveload`, etc.)

---

## Options Classes

**None.** Configuration accessed via raw `IConfiguration.GetSection()` strings throughout codebase.

**Recommendation:** Create:
- `RedisSettingsOptions`
- `ScriptSourcesOptions`
- `ApiEndpointsOptions`

---

## NLog Configuration Binding

`nlog.config` uses `${appsettings:name=ConnectionStrings.EddyLoggingConnection}` for DB target — requires `NLog.Appsettings.Standard` package.

---

## Environment Files

| File | When Loaded |
|------|-------------|
| `appsettings.json` | Always |
| `appsettings.{Environment}.json` | Based on `ASPNETCORE_ENVIRONMENT` |
| `appsettings.NonProd.json` | Likely copied/transformed at deploy (not standard ASP.NET naming) |

`appsettings.Development.json` only overrides logging — minimal.

---

## Configuration Binding in Startup

```csharp
// Startup.cs
Configuration.GetConnectionString("DefaultConnection")
Configuration.GetConnectionString("EddyTrackingISConnection")
Configuration.GetSection("MatchingServiceURL").Value  // CampaignRepository
```

No `services.Configure<T>()` calls.

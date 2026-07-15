# Security Review

## Executive Summary

The Widget Provider Service is designed as a **public embeddable endpoint** with **minimal security controls**. This is appropriate only if vendor tokens are treated as unguessable secrets and downstream APIs enforce authorization. Current implementation has **significant gaps**.

---

## Secrets & Credentials

| Finding | Location | Severity | Confidence |
|---------|----------|----------|------------|
| **Redis password in appsettings** | `appsettings.json:42`, `appsettings.NonProd.json:43` | Critical | High |
| SQL Windows auth connection strings | `appsettings.json:39-41` | Low (if deployed on trusted network) | High |
| No User Secrets usage in committed config | `UserSecretsId` in csproj but secrets in JSON | Medium | High |

**Recommendation:** Move Redis password and connection strings to Azure Key Vault / environment variables. Remove credentials from source control immediately.

---

## Authentication & Authorization

| Finding | Evidence | Severity |
|---------|----------|----------|
| No authentication middleware | `Startup.cs:75` — `UseAuthorization()` commented out | High |
| Vendor token not validated against DB | `WidgetProviderController.cs:33` — TODO comment | High |
| Any valid GUID accepted as vendor token | `Guid.TryParse` only | High |
| Open CORS policy | `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()` | Medium |

**Recommendation:** Validate `VendorWidgetToken` against `WS.VendorWidget` where `IsEnabled=1`. Consider API keys or signed JWTs for publishers.

---

## Injection Risks

| Vector | Assessment |
|--------|------------|
| **SQL Injection** | Low — EF Core parameterized queries; NLog DB target uses parameters |
| **XSS** | Medium — server renders HTML/JS returned to publisher pages; Razor output depends on ad API data sanitization (not verified in this codebase) |
| **Command Injection** | None identified |

---

## Authorization Gaps

- No role-based access
- `HeaderController` exposes all request headers — useful for debug, should not be public in production
- `ExitPopController` returns raw exception messages to client

---

## Input Validation

| Endpoint | Validation Level |
|----------|------------------|
| `GetWidgetJs` | GUID format only |
| `GetWidgetPackage` | None on `WidgetRequest` fields |
| `RetrieveQDFData` | `Guid.Parse` on TrackId — throws on bad input |
| `CanRender` | `new Guid(trackId)` — throws on bad input |

**Recommendation:** Add model validation attributes, global exception handler, consistent 400 responses.

---

## CSRF

Not applicable to primary API (JSON POST from cross-origin embed scripts). CORS allows any origin.

---

## SSRF

| Service | Risk |
|---------|------|
| Ad Listing API URL | Config-fixed — low |
| GP Listing API URL | Config-fixed — low |
| Matching Engine WCF | Config-fixed — low |
| `PageUrl` passed to listing APIs | Encoded via `WebUtility.UrlEncode` — medium (depends on downstream validation) |

---

## Open Redirects

`targeturl` settings from DB passed to client JS for QDF/forms — potential open redirect if Nexus config compromised. Confidence: medium (inferred from `QDFLightModel.DestinationURL`).

---

## Privilege Escalation

No user accounts — N/A. Vendor token scope not enforced means one publisher's token format works but may not load another's widgets (DB filters by token + container name).

---

## Dependency Vulnerabilities

| Package | Version | Note |
|---------|---------|------|
| EF Core | 6.0.3 | Behind .NET 8 — check CVEs |
| NLog.Web.AspNetCore | 4.14.0 | Older major |
| System.Data.SqlClient | 4.8.3 | Legacy; prefer Microsoft.Data.SqlClient |
| jQuery 1.12.4 (CDN) | Via config | Known outdated jQuery |

**Recommendation:** Run `dotnet list package --vulnerable` in CI.

---

## Rate Limiting

**None implemented.** Public endpoints vulnerable to abuse/DDoS.

**Recommendation:** Add ASP.NET rate limiting middleware or API gateway throttling.

---

## PII Handling

| Data | Storage | Retention |
|------|---------|-----------|
| IP Address | `WidgetRequest.Ipaddress` | Unknown |
| User Agent | `WidgetRequest.UserAgent` | Unknown |
| Form session data | Read from Redis, not persisted by this service | Transient |
| Email, phone (via FE session) | Passed to GP Listing API | Transient |

**Recommendation:** Document retention policy; consider IP anonymization for GDPR.

---

## Recommendations (Prioritized)

1. **Remove secrets from appsettings** — rotate Redis credentials
2. **Validate vendor tokens against database**
3. **Do not return exception messages to clients**
4. **Restrict or remove HeaderController in production**
5. **Add rate limiting**
6. **Enable HTTPS redirection**
7. **Tighten CORS** to known publisher domains if feasible
8. **Dependency audit in CI pipeline**

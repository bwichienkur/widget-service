# API Documentation

Base route pattern: `{controller=Home}/{action=Index}` with API controllers using `[Route("api/[controller]/[action]")]`.

**Authentication:** None on any endpoint (confidence: high — `UseAuthorization()` commented out in `Startup.cs:75`).

**CORS:** `AllowAnyOrigin`, `AllowAnyMethod`, `AllowAnyHeader` (`Startup.cs:46-51`).

---

## WidgetProviderController

Route prefix: `/api/WidgetProvider`

### GET GetWidgetJs

| Property | Value |
|----------|-------|
| **URL** | `/api/WidgetProvider/GetWidgetJs` |
| **Method** | GET |
| **Auth** | None |
| **Content-Type (response)** | `text/javascript` |

**Query Parameters:**
| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `vendorToken` | string (GUID) | Yes | — | Publisher vendor token |
| `checkJquery` | bool | No | `true` | Whether widget checks for jQuery |

**Success Response (200):**
```javascript
var checkJquery = true;
var globalVendorGuid = '{guid}';
// ... eddywidget.min.js contents ...
widget_setCookie('EddyVendorToken', '{guid}');
```

**Error Responses:**
| Condition | Status | Body |
|-----------|--------|------|
| Invalid GUID | 404 | `WidgetError: Invalid Vendor Token.` |
| Exception | 200* | `WidgetError: {exception.Message}` |

*Note: Status code not set to 500 on error (commented out).

**Business Logic:** Loads and token-replaces bootstrap JS; sets vendor cookie script.

**Dependencies:** `IWidgetPackageService`, `IFileSerializeService`, `ILogger`

---

### POST GetWidgetPackage

| Property | Value |
|----------|-------|
| **URL** | `/api/WidgetProvider/GetWidgetPackage` |
| **Method** | POST |
| **Auth** | None |
| **Content-Type (request)** | `application/json` (model binding) |
| **Content-Type (response)** | `text/html` |

**Request Body (`WidgetRequest`):**
```json
{
  "vendorToken": "00000000-0000-0000-0000-000000000000",
  "trackId": "00000000-0000-0000-0000-000000000000",
  "pageUrl": "https://publisher.com/page",
  "referrerUrl": "https://google.com",
  "userAgent": "Mozilla/5.0...",
  "jqueryVersionNumber": "1.12.4",
  "containerList": [
    {
      "containerName": "eddy-widget-1",
      "settings": { "categories": "1,2" }
    }
  ],
  "filterFields": { "age": "25" },
  "cookieTrackId": "00000000-0000-0000-0000-000000000000",
  "isModalLoad": false,
  "loadExternalResources": true
}
```

**Server-side mutations:**
- `IPAddress` ← derived from headers (`True-Client-IP`, `X-Forwarded-For`, etc.)
- `WidgetRequestGuid` ← `Guid.NewGuid()`
- `TrackId` ← `CookieTrackId` if present

**Success Response (200):**
HTML/JS fragment + `widget_setCookie('EddyWidgetSession', '{guid}');`

**Error Responses:**
| Condition | Body |
|-----------|------|
| No widgets found | `WidgetError: No widgets found for VendorToken - {token}` |
| Exception | `WidgetError: {message}` |

**Dependencies:** `IWidgetPackageService`

---

### POST UpdateWidgetPackage

| Property | Value |
|----------|-------|
| **URL** | `/api/WidgetProvider/UpdateWidgetPackage` |
| **Method** | POST |
| **Auth** | None |

Same as `GetWidgetPackage` but forces:
- `LoadExternalResources = false`
- `UpdateWidget = true`

**Response:** `text/html` (no session cookie script appended)

---

### GET SaveWidgetImpression

| Property | Value |
|----------|-------|
| **URL** | `/api/WidgetProvider/SaveWidgetImpression` |
| **Method** | GET |
| **Auth** | None |

**Query Parameters:**
| Name | Type | Required |
|------|------|----------|
| `widgetSessionGuid` | GUID | Yes |

**Response:** Empty (void action)

**Business Logic:** Inserts `WidgetImpression` record.

---

## QDFController

Route prefix: `/api/QDF`

### POST RetrieveQDFData

| Property | Value |
|----------|-------|
| **URL** | `/api/QDF/RetrieveQDFData` |
| **Method** | POST |
| **Auth** | None |
| **Content-Type (response)** | `application/json` |

**Request Body (`QDFDataRequest`):**
```json
{
  "trackId": "guid-string",
  "ignoreTrackId": false,
  "currentData": { "Categories": "1" },
  "fieldsToUpdate": {
    "SubCategories": "Categories"
  }
}
```

**Response (`QDFDataResponse`):**
```json
{
  "returnData": {
    "SubCategories": [
      { "id": 1, "name": "Nursing" }
    ]
  }
}
```

**Validation:** Minimal — invalid `TrackId` throws `FormatException`.

**Dependencies:** `IQDFService`, `IWidgetRepository`

---

## ExitPopController

Route prefix: `/api/ExitPop`

### POST CanRender

| Property | Value |
|----------|-------|
| **URL** | `/api/ExitPop/CanRender` |
| **Method** | POST |
| **Auth** | None |

**Request Body:** JSON string (GUID) — e.g. `"00000000-0000-0000-0000-000000000000"`

**Response:** `true` or `false` (JSON boolean)

**Business Logic:** Calls Matching Engine `GetCampaignDetailByTrackIDAsync`; returns `AllowExitPops`.

---

### POST RenderAd

| Property | Value |
|----------|-------|
| **URL** | `/api/ExitPop/RenderAd` |
| **Method** | POST |
| **Auth** | None |

**Request Body:** `WidgetRequest` (same shape as GetWidgetPackage)

**Response:** HTML string (ad markup) or empty string

**Business Logic:** Resolves exit pop config; fetches ads from Ad Listing or GP Listing API; renders Razor `ExitPop/default` or `GPExitPop/default` view.

**Error Handling:** Returns `ex.Message` as string body on failure.

---

### POST GetWidgetTrackId

| Property | Value |
|----------|-------|
| **URL** | `/api/ExitPop/GetWidgetTrackId` |
| **Method** | POST |
| **Auth** | None |

**Request Body:** `WidgetRequest`

**Response:** Track ID GUID string, `Guid.Empty`, or exception message

---

## HeaderController

### GET Index (MVC)

| Property | Value |
|----------|-------|
| **URL** | `/Header` or `/Header/Index` |
| **Method** | GET |
| **Auth** | None |
| **Response** | Razor view listing all request headers + derived IP |

**Purpose:** Debugging proxy/Cloudflare IP forwarding (internal/diagnostic tool).

---

## Static File Endpoints

| Path | Source Directory |
|------|------------------|
| `/testclients/*` | `testclients/` |
| `/css/*` | `css/` |
| `/images/*` | `images/` |
| Default static files | `wwwroot` (standard ASP.NET) |

Configured in `Startup.cs:77-93`.

---

## Client Integration Example

From test client `testclients/testadlistingapi.html` (inferred pattern):

1. Include `<script src="/api/WidgetProvider/GetWidgetJs?vendorToken=...">`
2. Client calls `GetWidgetPackage` with container names matching Nexus `VendorWidgetName`
3. Injected HTML targets DOM containers by name
4. Call `SaveWidgetImpression` when widgets visible

# Entity Documentation

Entities are split between **Core domain models** (used in rendering/business logic) and **Data EF entities** (database mapping).

---

## Core Domain Entities

### VendorWidgetConfig

**File:** `Core/Entities/VendorWidgetConfig.cs`

| Property | Business Meaning |
|----------|------------------|
| `VendorWidgetId` | DB primary key |
| `WidgetContainerName` | DOM element ID on publisher page |
| `WidgetType` | Determines renderer (enum 1–12) |
| `CSSText` | Custom CSS override |
| `SystemSettings` | DB-configured settings by component (AdServer/FormsEngine/Custom) |
| `SiteSettings` | Publisher-passed + URL-augmented settings |
| `ClientWidgetRequest` | Full request context |

### WidgetType Enum

Maps 1:1 to `WS.Widget.WidgetId` (non-identity PK).

### WidgetComponentType Enum

| Value | Name | Typical Settings |
|-------|------|------------------|
| 1 | AdServer | `trackid`, `placementtoken`, `creativeid` |
| 2 | FormsEngine | `templateid`, `targeturl`, `buttontext` |
| 3 | Custom | `targeturl`, `jqueryselector` |

### QDFTemplate / QDFField

**File:** `Core/Entities/QDFTemplate.cs`

- `QDFTemplate` — form definition with header, fields, unique render ID
- `QDFField` — single form field with control type, ME mapping, predecessors, cascading `UpdateFields`
- `MatchingEngineInput` — enum mapping field codes to ME API inputs

### Placement Models

| Class | Widget Types | Key Properties |
|-------|-------------|----------------|
| `AdPlacement` | AdStackSolo | `PlacementToken`, `CampaignTrackId`, `FilterSettings` |
| `AdListingApiPlacement` | AdListingApi, ExitPop | `AdItemList`, `Creative`, `FilterSettings` |
| `GPListingApiPlacement` | GPListingApi, GPExitPop | `AdItemList`, `RenderedAds`, `ImpressionPixels`, `Creative` |
| `QDFTemplatePlacement` | QDFLight, GPQDFLight | `Template`, `TrackId`, `FilterSettings` |

### QDF Data Entities (`Entities/QDFData/`)

| Class | Source | Properties |
|-------|--------|------------|
| `Category` | ME `Category` | `CategoryId`, `CategoryName` |
| `Subject` | ME `Subject` | `SubjectId`, `SubjectName` |
| `Specialty` | ME `Specialty` | `SpecialtyId`, `SpecialtyName` |
| `ProgramLevel` | ME `ProgramLevel` | `ProgramLevelId`, `ProgramLevelName` |
| `IMEField` | Interface | `Id`, `Name` |

---

## Data EF Entities (Nexus)

### VendorWidget

Publisher-specific widget instance. Links `VendorWidgetToken` (client GUID) to `WidgetId` and `VendorWidgetName` (container).

**Relationships:** → `Widget`, → `VendorWidgetSettingValue` collection

### Widget

Master widget type definition. `WidgetId` matches `WidgetType` enum.

### WidgetComponent

Component categories within a widget (Ad Server, Forms Engine, Custom).

### WidgetSetting

Defines configurable keys per widget+component.

### VendorWidgetSettingValue

Actual configured values per vendor widget instance.

### VendorWidgetUrlParameterConfig

URL → default parameter mappings for page-specific prefill.

### Campaign

Mapped to `dbo.Campaign` but **unused** in application code. Confidence: high — no queries reference `Campaigns` DbSet.

### VwVendorWidgetConfiguration (View)

Read-only denormalized configuration. Primary source for `GetWidgetConfig`.

### VwQDFTemplateConfiguration (View)

QDF template field rows ordered by `TemplateStepId`, `RowSequence`.

---

## Data EF Entities (EddyTrackingIS)

### WidgetRequest

One row per widget container rendered (excluding admin URLs). Stores page context, timing, settings JSON.

**Lifecycle:** Created on each `GetWidgetPackage` call; never updated.

### WidgetImpression

Created when client calls `SaveWidgetImpression` with session GUID.

**Lifecycle:** Insert-only; linked to `WidgetRequestGuid` (logical, no FK in EF model).

---

## DTOs

### WidgetRequest (Core/DTO)

API transport object — distinct from `Data.Entities.WidgetRequest` (persistence entity). Same name, different namespaces.

### QDFDataRequest / QDFDataResponse

AJAX payloads for cascading QDF dropdowns.

### AdListingApiResponse / GPListingApiResponse

Deserialized ad listing API responses. See `Core/DTO/` for full property lists.

---

## Validation

| Entity | Validation |
|--------|------------|
| API `WidgetRequest` | None (model binding only) |
| EF entities | DB constraints only (max lengths in `OnModelCreating`) |
| `VendorToken` | GUID format only at API layer |

---

## Naming Conventions

- EF entities: PascalCase, singular (`VendorWidget`)
- DbSets: Match entity name or pluralized (`Campaigns`)
- Schema prefix: `WS` for widget-specific tables
- DTOs suffixed or in `DTO/` namespace
- Component models suffixed `Model`

# EDDY.IS.WidgetProvider.Data

## Purpose

**Data access layer** — Entity Framework Core DbContexts, entity mappings, and repository implementations for Nexus and EddyTrackingIS databases.

## Responsibilities

- Map SQL Server tables/views to EF entities
- Implement `IWidgetRepository` — widget config, QDF templates, URL configs, tracking writes
- Implement `ICampaignRepository` — exit pop eligibility via Matching Engine WCF

## Project References

| Reference | Path |
|-----------|------|
| `EDDY.IS.WidgetProvider.Core` | `../EDDY.IS.WidgetProvider.Core/` |

## NuGet Packages

| Package | Version |
|---------|---------|
| Microsoft.EntityFrameworkCore.SqlServer | 6.0.3 |
| Microsoft.EntityFrameworkCore.Tools | 6.0.3 |

## Important Classes

| Class | Interface | File |
|-------|-----------|------|
| `WidgetRepository` | `IWidgetRepository` | `WidgetRepository.cs` |
| `CampaignRepository` | `ICampaignRepository` | `CampaignRepository.cs` |
| `NexusContext` | — | `Entities/NexusContext.cs` |
| `EddyTrackingISContext` | — | `Entities/EddyTrackingISContext.cs` |

## IWidgetRepository Methods

| Method | DB | Purpose |
|--------|-----|---------|
| `GetWidgetConfig` | Nexus view | Load vendor widget configuration |
| `GetQDFTemplate` | Nexus view | Build QDF template with fields |
| `GetVendorUrlConfigurations` | Nexus table | URL prefill map |
| `SaveWidgetRequests` | EddyTrackingIS | Analytics insert |
| `SaveWidgetImpression` | EddyTrackingIS | Impression insert |

## Configuration

Connection strings from `IConfiguration`:
- `DefaultConnection` → NexusContext
- `EddyTrackingISConnection` → manual context in repository writes

## External Services

`CampaignRepository` calls WCF Matching Engine (`MatchingServiceURL` config) — unusual placement in Data layer.

## Potential Improvements

1. Fix `GetWidgetConfig` component settings grouping bug
2. Inject `EddyTrackingISContext` instead of manual `new DbContextOptionsBuilder`
3. Filter `VendorWidget.IsEnabled` in queries
4. Add EF Core 8 upgrade + compiled queries for hot paths
5. Move `CampaignRepository` to Core (it's an external service adapter, not data access)
6. Add integration tests with Testcontainers SQL

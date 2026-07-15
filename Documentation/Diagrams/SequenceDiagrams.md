# Sequence Diagrams — Major Workflows

## Widget Package Delivery

```mermaid
sequenceDiagram
    autonumber
    participant C as Client Browser
    participant WPC as WidgetProviderController
    participant WPS as WidgetPackageService
    participant WR as WidgetRepository
    participant MIS as ModelInstantiationService
    participant M as IRenderable Model
    participant VRS as ViewRenderService
    participant API as External Listing API

    C->>WPC: POST GetWidgetPackage
    WPC->>WPC: Set IP, WidgetRequestGuid, TrackId
    WPC->>WPS: GetFullWidgetPackage

    loop Each container in ContainerList
        WPS->>WR: GetWidgetConfig(name, vendorToken)
        WR-->>WPS: VendorWidgetConfig
        WPS->>WPS: AugmentSiteSettingsFromUrlConfig
    end

    par Parallel by WidgetType
        WPS->>MIS: CreateComponent(widgetType)
        MIS-->>WPS: IRenderable
        WPS->>M: Configure(configs)
        M->>API: Fetch ads (if listing widget)
        API-->>M: Ad data
        WPS->>M: RenderAsync(VRS)
        M->>VRS: RenderToStringAsync
        VRS-->>M: HTML/JS
        M-->>WPS: Rendered string
    end

    WPS->>WPS: Optional minify
    WPS--)WR: Task.Run SaveWidgetRequests
    WPS-->>WPC: Full package string
    WPC-->>C: text/html + session cookie script
```

## GP Listing Widget with Forms Engine Session

```mermaid
sequenceDiagram
    participant M as GPListingApiModel
    participant Redis as FESessionRedisService
    participant GP as GPListingApiService
    participant API as GP Listing API

    M->>M: Read fesessionid from site settings
    M->>Redis: GetFormsEngineSession(sessionId)
    Redis-->>M: Filtered form field dictionary
    M->>M: MergeDictionaries(FE, siteSettings)
    M->>M: WidgetSettingsMappingService.Map
    M->>GP: GetApiResponse(filters, placement, track)
    GP->>API: POST JSON
    API-->>GP: GPListingObjectResponse
    GP-->>M: Ads + pixels
    M->>M: MapServiceResponse
```

## Exit Pop Render

```mermaid
sequenceDiagram
    participant JS as Exit Pop Client JS
    participant EP as ExitPopController
    participant WR as WidgetRepository
    participant API as Listing API
    participant VRS as ViewRenderService

    JS->>EP: POST CanRender(trackId)
    EP->>EP: Matching Engine GetCampaignDetail
    EP-->>JS: true/false

    Note over JS: User triggers exit intent

    JS->>EP: POST RenderAd(WidgetRequest)
    EP->>WR: GetWidgetConfig (Exit container)
    alt ExitPop
        EP->>API: AdListingApi GET
    else GPExitPop
        EP->>API: GPListingApi POST
    end
    API-->>EP: Ad data
    EP->>VRS: Render ExitPop/default view
    VRS-->>EP: HTML
    EP-->>JS: HTML for modal
```

## QDF Cascading Dropdown

```mermaid
sequenceDiagram
    participant UI as QDF Light Widget
    participant QC as QDFController
    participant QS as QDFService
    participant ME as Matching Engine

    UI->>QC: POST RetrieveQDFData
    Note over UI,QC: User selected Categories=5

    QC->>QS: GetSubjects(trackId, {Category:5})
    QS->>QS: CreateRequest (DirectoryMatchRequest)
    QS->>ME: GetSubjectsAsync
    ME-->>QS: SubjectResponse
    QS-->>QC: List Subject
    QC->>QC: Map to QDFFieldOptions
    QC-->>UI: JSON { SubCategories: [...] }
```

## Application Startup Cache Warm

```mermaid
sequenceDiagram
    participant Host as ASP.NET Host
    participant S as Startup.Configure
    participant WR as WidgetRepository
    participant DB as Nexus
    participant C as IMemoryCache

    Host->>S: Configure pipeline
    S->>WR: GetVendorUrlConfigurations()
    WR->>DB: SELECT VendorWidgetUrlParameterConfig WHERE IsEnabled
    DB-->>WR: URL config rows
    WR-->>S: Dictionary url → settings
    S->>C: SetCacheItem("URLCONFIGS", data, indefinite)
```

# Data Flow Diagrams

## Request Lifecycle

```mermaid
flowchart LR
    A[Browser loads publisher page] --> B[GET GetWidgetJs]
    B --> C[Client JS initializes]
    C --> D[POST GetWidgetPackage]
    D --> E[WidgetPackageService]
    E --> F{Per container}
    F --> G[WidgetRepository.GetWidgetConfig]
    G --> H[Nexus DB]
    F --> I[ModelInstantiationService]
    I --> J[IRenderable.Configure]
    J --> K[External APIs optional]
    J --> L[IRenderable.RenderAsync]
    L --> M[ViewRenderService + Razor]
    M --> N[HTML/JS response]
    N --> O[Client injects into DOM]
    O --> P[GET SaveWidgetImpression]
    P --> Q[EddyTrackingIS DB]
```

## Database Interactions

```mermaid
flowchart TB
    subgraph Reads
        R1[GetWidgetConfig] --> V1[VW_VendorWidgetConfiguration]
        R2[GetQDFTemplate] --> V2[VW_QDFTemplateConfiguration]
        R3[GetVendorUrlConfigurations] --> T1[VendorWidgetUrlParameterConfig]
    end

    subgraph Writes
        W1[SaveWidgetRequests] --> T2[WidgetRequest]
        W2[SaveWidgetImpression] --> T3[WidgetImpression]
    end

    subgraph Logging
        L1[NLog Error] --> T4[EddyLogging.Exception]
    end

    V1 & V2 & T1 --> Nexus[(Nexus DB)]
    T2 & T3 --> Tracking[(EddyTrackingIS DB)]
    T4 --> LoggingDB[(EddyLogging DB)]
```

## External API Interactions

```mermaid
flowchart LR
    subgraph WidgetProvider
        ALM[AdListingApiModel]
        GPLM[GPListingApiModel]
        QDF[QDFService]
        CR[CampaignRepository]
        EP[ExitPopController]
    end

    ALM -->|HTTP GET| AdAPI[Ad Listing API]
    GPLM -->|HTTP POST| GPAPI[GP Listing API]
    EP --> AdAPI
    EP --> GPAPI
    QDF -->|WCF| ME[Matching Engine]
    CR -->|WCF| ME
    GPLM -->|Redis GET| Redis[(Azure Redis FE Session)]
```

## Authentication Flow

**Not implemented.** No login, JWT, cookies for API auth, or policy-based authorization.

Publisher identification is implicit via `VendorToken` GUID in request payload — not cryptographically verified.

## Background Processing

```mermaid
sequenceDiagram
    participant WPS as WidgetPackageService
    participant Pool as ThreadPool
    participant Repo as WidgetRepository
    participant DB as EddyTrackingIS

    WPS->>WPS: Render complete
    WPS->>Pool: Task.Run(SaveWidgetRequests)
    WPS-->>Client: Return response immediately
    Pool->>Repo: SaveWidgetRequests
    Repo->>DB: INSERT WidgetRequest rows
```

No scheduled jobs, queues, or retry logic.

## Authentication Flow (Target State — Recommended)

```mermaid
sequenceDiagram
    participant Pub as Publisher
    participant API as WidgetProvider
    participant DB as Nexus

    Pub->>API: Request + VendorToken
    API->>DB: Validate token exists and IsEnabled
    alt invalid
        API-->>Pub: 401 Unauthorized
    end
    API-->>Pub: Widget package
```

*This diagram represents a recommended future state, not current behavior.*

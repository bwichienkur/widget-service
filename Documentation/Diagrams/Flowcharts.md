# Flowcharts — Business Processes

## Widget Package Decision Flow

```mermaid
flowchart TD
    A[POST GetWidgetPackage] --> B{ContainerList empty?}
    B -->|Yes| Z[Return WidgetError: No widgets found]
    B -->|No| C[For each container]
    C --> D[GetWidgetConfig from Nexus]
    D --> E{Config found?}
    E -->|No| C
    E -->|Yes| F[Augment site settings from URL + query string]
    F --> G[Group by WidgetType]
    G --> H[Parallel render each type]
    H --> I[ModelInstantiationService.CreateComponent]
    I --> J{ICacheable?}
    J -->|Yes| K{Cache hit?}
    K -->|Yes| L[Use cached HTML]
    K -->|No| M[Configure + Render]
    J -->|No| M
    M --> N[ViewRenderService Razor]
    L --> O[Concatenate output]
    N --> O
    O --> P{MinifyJavascript?}
    P -->|Yes| Q[NUglify]
    P -->|No| R[Save tracking async]
    Q --> R
    R --> S[Return HTML/JS to client]
```

## Widget Type Routing

```mermaid
flowchart TD
    A[WidgetType from DB] --> B{Type?}
    B -->|1 AdStackSolo| C[AdStackSoloModel]
    B -->|2 SmartListing| D[SmartListingModel]
    B -->|3 QDF| E[QDFModel]
    B -->|4,6 ProgramForm/WizardForm| F[WizardFormModel]
    B -->|5 LeaveBehind| G[LeaveBehindModel]
    B -->|7 AdListingApi| H[AdListingApiModel + Ad API]
    B -->|8 QDFLight| I[QDFLightModel + ME]
    B -->|9 ExitPop| J[ExitPopModel + client trackers]
    B -->|10 GPQDFLight| K[GPQDFLightModel]
    B -->|11 GPListingApi| L[GPListingApiModel + GP API + Redis]
    B -->|12 GPExitPop| M[GPExitPopModel]
    C & D & E & F & G & H & I & J & K & L & M --> N[IRenderable.RenderAsync]
```

## Exit Pop Eligibility

```mermaid
flowchart TD
    A[POST CanRender] --> B{Valid GUID?}
    B -->|No| C[Return false]
    B -->|Yes| D[WCF GetCampaignDetailByTrackID]
    D --> E{Success?}
    E -->|No| C
    E -->|Yes| F{campaign.AllowExitPops?}
    F -->|Yes| G[Return true]
    F -->|No| C
```

## QDF Field Population

```mermaid
flowchart TD
    A[GetQDFTemplate] --> B[Load template from VW_QDFTemplateConfiguration]
    B --> C[For each field]
    C --> D{Control type?}
    D -->|Dropdown| E{Field code?}
    E -->|Categories| F[ME GetCategories with predecessors]
    E -->|SubCategories| G[ME GetSubjects]
    E -->|Specialties| H[ME GetSpecialties]
    E -->|Desired_Degree_Level| I[ME GetProgramLevels]
    F & G & H & I --> J[PopulateOptionValues]
    D -->|Textbox/Hidden| K[Set Value from filters]
    J --> L[Return QDFTemplate]
    K --> L
```

## Vendor Token Validation (Current vs Recommended)

```mermaid
flowchart LR
    subgraph Current
        A1[vendorToken] --> B1{Guid.TryParse?}
        B1 -->|No| C1[404 Error]
        B1 -->|Yes| D1[Proceed - any GUID works]
    end

    subgraph Recommended
        A2[vendorToken] --> B2{Guid.TryParse?}
        B2 -->|No| C2[401]
        B2 -->|Yes| E2{Exists in VendorWidget AND IsEnabled?}
        E2 -->|No| C2
        E2 -->|Yes| D2[Proceed]
    end
```

# Widget Provider Service — Enterprise Documentation

Reverse-engineered documentation for **EDDY.IS.WidgetProvider** (.NET 8).

Generated for handoff to engineering teams maintaining, extending, or rewriting the application.

## Quick Start

1. Read [ExecutiveSummary.md](ExecutiveSummary.md) for business context
2. Read [AI_CONTEXT.md](AI_CONTEXT.md) for condensed agent-oriented reference
3. Read [Architecture.md](Architecture.md) for technical structure

## Documentation Map

| # | Topic | Location |
|---|-------|----------|
| 1 | Executive Summary | [ExecutiveSummary.md](ExecutiveSummary.md) |
| 2 | Solution Overview | [Architecture.md](Architecture.md) + [Projects/](Projects/) |
| 3 | Architecture | [Architecture.md](Architecture.md) |
| 4 | Project Documentation | [Projects/](Projects/) |
| 5 | Folder Documentation | [Projects/](Projects/) + [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 6 | Class Documentation | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 7 | Business Logic | [BusinessProcesses.md](BusinessProcesses.md) |
| 8 | API Documentation | [APIs/Endpoints.md](APIs/Endpoints.md) |
| 9 | Database | [Database/Overview.md](Database/Overview.md) |
| 10 | Entities | [Entities/Overview.md](Entities/Overview.md) |
| 11 | Services | [Services/Overview.md](Services/Overview.md) |
| 12 | Dependency Injection | [Diagrams/DependencyInjection.md](Diagrams/DependencyInjection.md) |
| 13 | Configuration | [Deployment/Configuration.md](Deployment/Configuration.md) |
| 14 | Authentication | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 15 | Background Processing | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 16 | Integrations | [ExecutiveSummary.md](ExecutiveSummary.md) + [BusinessProcesses.md](BusinessProcesses.md) |
| 17 | Logging | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 18 | Exception Handling | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 19 | Security Review | [Security/Review.md](Security/Review.md) |
| 20 | Performance Review | [Performance/Review.md](Performance/Review.md) |
| 21 | Code Quality | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 22 | Design Patterns | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 23 | Testing | [Architecture-CrossCutting.md](Architecture-CrossCutting.md) |
| 24 | Deployment | [Deployment/Overview.md](Deployment/Overview.md) |
| 25 | Data Flow | [Diagrams/DataFlow.md](Diagrams/DataFlow.md) |
| 26 | Sequence Diagrams | [Diagrams/SequenceDiagrams.md](Diagrams/SequenceDiagrams.md) |
| 27 | Class Diagrams | [Diagrams/ClassDiagrams.md](Diagrams/ClassDiagrams.md) |
| 28 | Flowcharts | [Diagrams/Flowcharts.md](Diagrams/Flowcharts.md) |
| 29 | Dependency Graphs | [Diagrams/DependencyInjection.md](Diagrams/DependencyInjection.md) |
| 30 | Refactoring | [Refactoring/Recommendations.md](Refactoring/Recommendations.md) |
| 31 | AI Knowledge Base | [AI_CONTEXT.md](AI_CONTEXT.md) |

## Solution Projects

| Project | Documentation |
|---------|---------------|
| EDDY.IS.WidgetProvider.Service | [Projects/EDDY.IS.WidgetProvider.Service.md](Projects/EDDY.IS.WidgetProvider.Service.md) |
| EDDY.IS.WidgetProvider.Core | [Projects/EDDY.IS.WidgetProvider.Core.md](Projects/EDDY.IS.WidgetProvider.Core.md) |
| EDDY.IS.WidgetProvider.Data | [Projects/EDDY.IS.WidgetProvider.Data.md](Projects/EDDY.IS.WidgetProvider.Data.md) |

## Analysis Coverage

All 3 solution projects analyzed:
- ✅ EDDY.IS.WidgetProvider.Service (4 controllers, Startup, templates, assets)
- ✅ EDDY.IS.WidgetProvider.Core (12 widget models, 11 services, interfaces, WCF client)
- ✅ EDDY.IS.WidgetProvider.Data (2 DbContexts, 14 entities, 2 repositories)

## Confidence Notes

| Topic | Confidence |
|-------|------------|
| Code behavior | High — derived from source |
| DB schema beyond EF mappings | Medium — no migrations/DDL in repo |
| Production infra details | Medium — inferred from pipelines + appsettings |
| Campaign table usage | High — mapped but never queried |
| External API contracts | Medium — inferred from DTOs and URL patterns |

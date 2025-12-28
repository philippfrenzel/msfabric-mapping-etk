# Copilot Instructions - Fabric Mapping Service

## Projektübersicht
Microsoft Fabric Extensibility Toolkit Workload für Reference Tables (Lookup Tables) und attributbasiertes Data Mapping. Ermöglicht Datenklassifizierung, Harmonisierung und Transformation via OneLake-Integration.

## Architektur

```
┌─────────────────────────────────────────────────────────────┐
│  Frontend (React + Fluent UI + TypeScript)                  │
├─────────────────────────────────────────────────────────────┤
│  API Layer (ASP.NET Core Controllers)                       │
│  WorkloadController | ReferenceTableController | ItemController
├─────────────────────────────────────────────────────────────┤
│  Core Layer - Zwei Hauptpfade:                              │
│  ┌────────────────────┐  ┌─────────────────────────────────┐│
│  │ Reference Tables   │  │ Attribute Mapping               ││
│  │ (MappingIO/Storage)│  │ (AttributeMappingService)       ││
│  └────────────────────┘  └─────────────────────────────────┘│
│  ┌───────────────────────────────────────────────────────┐  │
│  │ MappingWorkload (IWorkload) - Fabric Orchestrator     │  │
│  └───────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│  Storage: LakehouseStorage | OneLakeStorage | InMemory      │
└─────────────────────────────────────────────────────────────┘
```

## Kritische Patterns

### Workload-Operationen via Enum-Switch
`MappingWorkload.ExecuteAsync()` dispatched via `WorkloadOperationType`:
```csharp
// Füge neue Operation hinzu:
// 1. Enum in WorkloadConfiguration.cs erweitern
// 2. Switch-Case in MappingWorkload.ExecuteAsync() hinzufügen
// 3. Execute*Async Methode implementieren
```

### Storage Abstraktion
Drei Storage-Implementierungen hinter Interfaces:
- `IReferenceMappingStorage` → `InMemoryReferenceMappingStorage` | `LakehouseReferenceMappingStorage`
- `IItemDefinitionStorage` → Fabric Item Definitions
- `IOneLakeStorage` → OneLake-Datenpersistenz

Konfiguration via `LakehouseStorageOptions.UseInMemoryStorage` in `appsettings.json`.

### Attributbasiertes Mapping
```csharp
// Source-Klasse mit MapTo-Attributen dekorieren:
public class SourceModel
{
    [MapTo("TargetPropertyName")]
    public string SourceProp { get; set; }
    
    [IgnoreMapping]
    public string Excluded { get; set; }
}
```

## Commands

```bash
# Build
dotnet build                    # oder scripts/Build/Build.ps1
dotnet test                     # xUnit Tests

# Full-Stack Development
pwsh scripts/Run/StartFullStack.ps1   # API:5001 + Frontend:3000

# Nur Backend
dotnet run --project src/FabricMappingService.Api

# Nur Frontend
cd src/FabricMappingService.Frontend && npm start
```

## Projektstruktur-Konventionen

- **Controllers/**: API-Endpoints, DTOs in `Dtos/` Ordner trennen
- **Services/**: Interface + Implementierung Paare (z.B. `IMappingIO` + `MappingIO`)
- **Workload/**: Fabric-spezifische Orchestrierung, `IWorkload` Interface implementieren
- **Models/**: Domain Models, keine Logik
- **Attributes/**: Custom Mapping Attributes, `AttributeUsage` definieren

## Testing Pattern

Tests folgen Naming: `{Klasse}Tests.cs`, xUnit mit Constructor-Setup:
```csharp
public class MappingWorkloadTests
{
    private readonly MappingWorkload _workload;
    
    public MappingWorkloadTests()
    {
        // Setup mit InMemory-Implementierungen
        _storage = new InMemoryReferenceMappingStorage();
        _workload = new MappingWorkload(...);
    }
    
    [Fact]
    public async Task MethodName_Scenario_ExpectedResult() { }
}
```

## Fabric Manifest
`fabric-manifest/workload-manifest.json` definiert Workload-Metadaten und API-Endpoints für Fabric Registration. Item-Typen in `fabric-manifest/items/`.

## Frontend
React + Fluent UI (@fluentui/react), TypeScript, Webpack bundled:
- `BasicModeEditor.tsx` - Table-basierter CRUD Editor
- `ExpertModeEditor.tsx` - Monaco JSON Editor
- API-Client in `services/apiClient`

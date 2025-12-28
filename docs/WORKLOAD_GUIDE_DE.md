# Microsoft Fabric Workload Anleitung - MappingWorkload

## Übersicht

Der **MappingWorkload** ist eine vollständige Implementierung eines Microsoft Fabric Extensibility Toolkit Workloads für Referenztabellen (KeyMapping) und Daten-Mapping-Operationen. Diese Anleitung beschreibt, wie der Workload gebaut, deployed und in Microsoft Fabric ausgeführt werden kann.

## Architektur

Der MappingWorkload implementiert das `IWorkload`-Interface und orchestriert alle Mapping- und Referenztabellen-Operationen:

```
┌─────────────────────────────────────────────────┐
│           Microsoft Fabric Portal               │
│  ┌────────────────────────────────────────────┐ │
│  │         Workspace                          │ │
│  │  - Reference Tables (KeyMapping)          │ │
│  │  - Mapping Configurations                 │ │
│  │  - Mapping Jobs                           │ │
│  └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
                     ↓ HTTPS
┌─────────────────────────────────────────────────┐
│      REST API Backend (Azure App Service)       │
│  ┌────────────────────────────────────────────┐ │
│  │         WorkloadController                │ │
│  │  - /api/workload/execute                  │ │
│  │  - /api/workload/health                   │ │
│  │  - /api/workload/validate                 │ │
│  └────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────┐ │
│  │         MappingWorkload                   │ │
│  │  - ExecuteAsync()                         │ │
│  │  - ValidateConfigurationAsync()           │ │
│  │  - GetHealthStatusAsync()                 │ │
│  └────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────┐ │
│  │     Core Services                         │ │
│  │  - MappingIO (Reference Tables)           │ │
│  │  - AttributeMappingService                │ │
│  └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│         OneLake Storage                         │
│  - KeyMapping Outports                          │
│  - Mapping Configurations                       │
└─────────────────────────────────────────────────┘
```

## Komponenten

### 1. IWorkload Interface

Das `IWorkload`-Interface definiert den Vertrag für Fabric Workloads:

```csharp
public interface IWorkload
{
    string WorkloadId { get; }
    string DisplayName { get; }
    string Version { get; }
    
    Task<WorkloadExecutionResult> ExecuteAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken);
    
    Task<WorkloadValidationResult> ValidateConfigurationAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken);
    
    Task<WorkloadHealthStatus> GetHealthStatusAsync(
        CancellationToken cancellationToken);
}
```

### 2. MappingWorkload Klasse

Die zentrale Implementierung des Workloads mit folgenden Hauptmethoden:

- **ExecuteAsync**: Führt Workload-Operationen aus (Referenztabellen erstellen, synchronisieren, lesen, etc.)
- **ValidateConfigurationAsync**: Validiert die Workload-Konfiguration vor der Ausführung
- **GetHealthStatusAsync**: Gibt den Gesundheitsstatus des Workloads zurück

### 3. Unterstützte Operationen

Der Workload unterstützt folgende Operationstypen:

- `CreateReferenceTable`: Neue Referenztabelle erstellen
- `SyncReferenceTable`: Referenztabelle mit Quelldaten synchronisieren
- `ReadReferenceTable`: Referenztabellendaten lesen
- `UpdateReferenceTableRow`: Zeile in Referenztabelle aktualisieren
- `DeleteReferenceTable`: Referenztabelle löschen
- `ExecuteMapping`: Daten-Mapping ausführen
- `ValidateMapping`: Mapping-Konfiguration validieren
- `HealthCheck`: Gesundheitsprüfung durchführen
- `CreateMappingItem`: Neues Mapping-Item in einem Fabric-Workspace erstellen
- `UpdateMappingItem`: Bestehendes Mapping-Item aktualisieren
- `DeleteMappingItem`: Mapping-Item löschen
- `StoreToOneLake`: Mapping-Daten in OneLake speichern
- `ReadFromOneLake`: Mapping-Daten aus OneLake lesen

## Build-Anleitung

### Voraussetzungen

- .NET 10.0 SDK oder höher
- Visual Studio 2022, VS Code oder GitHub Codespaces
- PowerShell 7 (für Automatisierungsskripte)

### Projekt bauen

```bash
# Lösung bauen
cd /path/to/msfabric-mapping-etk
dotnet build

# Oder mit dem Build-Skript
pwsh ./scripts/Build/Build.ps1
```

### Tests ausführen

```bash
# Alle Tests ausführen
dotnet test

# Mit Codeabdeckung
dotnet-coverage collect -f cobertura -o coverage.xml dotnet test
```

### Für Produktion veröffentlichen

```bash
# Mit dotnet CLI
dotnet publish -c Release -o ./publish

# Oder mit dem Publish-Skript
pwsh ./scripts/Build/Publish.ps1 -Configuration Release -OutputPath ./publish
```

## Deployment-Anleitung

### Option 1: Azure App Service Deployment

#### Schritt 1: Azure-Ressourcen erstellen

```bash
# Azure CLI verwenden
az login

# Resource Group erstellen
az group create --name fabric-mapping-rg --location westeurope

# App Service Plan erstellen
az appservice plan create \
  --name fabric-mapping-plan \
  --resource-group fabric-mapping-rg \
  --sku B1 \
  --is-linux

# App Service erstellen
az webapp create \
  --name fabric-mapping-service \
  --resource-group fabric-mapping-rg \
  --plan fabric-mapping-plan \
  --runtime "DOTNETCORE:8.0"
```

**Hinweis**: Für Azure Deployment kann es notwendig sein, auf .NET 8.0 oder 9.0 zu wechseln, bis .NET 10 in Azure App Service unterstützt wird.

#### Schritt 2: Anwendung deployen

```powershell
# Mit dem Deploy-Skript
.\scripts\Deploy\DeployToAzure.ps1 `
  -ResourceGroup "fabric-mapping-rg" `
  -AppServiceName "fabric-mapping-service"
```

#### Schritt 3: App-Einstellungen konfigurieren

```bash
# Umgebungsvariablen setzen (falls benötigt)
az webapp config appsettings set \
  --resource-group fabric-mapping-rg \
  --name fabric-mapping-service \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

### Option 2: Azure Container Apps

```bash
# Container bauen
docker build -t fabric-mapping-service:latest .

# In Azure Container Registry pushen
az acr login --name youracr
docker tag fabric-mapping-service youracr.azurecr.io/fabric-mapping-service
docker push youracr.azurecr.io/fabric-mapping-service

# Container App erstellen
az containerapp create \
  --name fabric-mapping-service \
  --resource-group fabric-mapping-rg \
  --environment yourenv \
  --image youracr.azurecr.io/fabric-mapping-service \
  --target-port 8080 \
  --ingress external
```

## Workload in Fabric registrieren

### Voraussetzungen für Registrierung

1. **Azure AD App Registration**
   - Navigieren Sie zu Azure Portal → Microsoft Entra ID → App registrations
   - Erstellen Sie eine neue Registrierung: "Fabric Mapping Service"
   - Notieren Sie: Application (client) ID und Directory (tenant) ID
   - Erstellen Sie ein Client Secret und speichern Sie es sicher

2. **API-Berechtigungen konfigurieren**
   - Fügen Sie folgende Berechtigungen hinzu:
     - Microsoft Graph: `User.Read`
     - Power BI Service: `Workspace.Read.All`, `Workspace.ReadWrite.All`
   - Erteilen Sie Admin-Zustimmung

3. **Workload-Manifest aktualisieren**
   
   Bearbeiten Sie `fabric-manifest/workload-manifest.json`:

   ```json
   {
     "authentication": {
       "aadAppId": "IHRE_APPLICATION_CLIENT_ID"
     },
     "backend": {
       "backendUrl": "https://ihre-api.azurewebsites.net"
     }
   }
   ```

### Workload registrieren

#### Option A: Mit PowerShell-Skript

```powershell
# Workload registrieren
.\scripts\Deploy\RegisterWorkload.ps1 `
  -TenantId "ihre-tenant-id" `
  -BackendUrl "https://fabric-mapping-service.azurewebsites.net" `
  -AadAppId "ihre-app-id"

# Oder mit REST API
.\scripts\Deploy\RegisterWorkload.ps1 `
  -TenantId "ihre-tenant-id" `
  -UseRestApi
```

#### Option B: Manuell mit Azure CLI

```bash
# Access Token erhalten
az account get-access-token --resource "https://analysis.windows.net/powerbi/api"

# Workload registrieren (mit curl)
curl -X POST https://api.fabric.microsoft.com/v1/workloads \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d @fabric-manifest/workload-manifest.json
```

## Workload ausführen

### 1. Health Check durchführen

```bash
# API-Gesundheit prüfen
curl https://ihre-api.azurewebsites.net/api/workload/health

# Antwort:
{
  "isHealthy": true,
  "status": "Healthy",
  "version": "1.0.0",
  "details": {
    "workloadId": "fabric-mapping-service",
    "displayName": "Reference Table & Data Mapping Service"
  }
}
```

### 2. Workload-Informationen abrufen

```bash
curl https://ihre-api.azurewebsites.net/api/workload/info
```

### 3. Workload-Operationen ausführen

#### Referenztabelle erstellen

```bash
curl -X POST https://ihre-api.azurewebsites.net/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "CreateReferenceTable",
    "timeoutSeconds": 60,
    "parameters": {
      "tableName": "produkttyp",
      "columns": "[{\"name\":\"ProductType\",\"dataType\":\"string\",\"order\":1}]",
      "isVisible": true,
      "notifyOnNewMapping": false
    }
  }'
```

#### Referenztabelle synchronisieren

```bash
curl -X POST https://ihre-api.azurewebsites.net/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "SyncReferenceTable",
    "timeoutSeconds": 60,
    "parameters": {
      "tableName": "produkttyp",
      "keyAttributeName": "Produkt",
      "data": "[{\"Produkt\":\"VTP001\",\"Name\":\"Product A\"}]"
    }
  }'
```

#### Referenztabelle lesen

```bash
curl -X POST https://ihre-api.azurewebsites.net/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "ReadReferenceTable",
    "timeoutSeconds": 30,
    "parameters": {
      "tableName": "produkttyp"
    }
  }'
```

### 4. Konfiguration validieren

```bash
curl -X POST https://ihre-api.azurewebsites.net/api/workload/validate \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "CreateReferenceTable",
    "timeoutSeconds": 60,
    "parameters": {
      "tableName": "test_table",
      "columns": "[]"
    }
  }'
```

## Integration in Fabric Workspace

### 1. Workspace erstellen oder öffnen

1. Melden Sie sich bei Microsoft Fabric an
2. Navigieren Sie zu Ihrem Workspace oder erstellen Sie einen neuen
3. Der registrierte Workload sollte nun verfügbar sein

### 2. Reference Table Items erstellen

Im Workspace können Sie jetzt Reference Table Items erstellen:

1. Klicken Sie auf "New" → "Reference Table (KeyMapping)"
2. Geben Sie die Konfiguration ein
3. Die Referenztabelle wird als KeyMapping-Outport bereitgestellt

### 3. Mapping Configurations erstellen

1. Erstellen Sie Mapping Configuration Items für Ihre Daten-Transformationen
2. Konfigurieren Sie Quell- und Zieltypen
3. Führen Sie Mapping-Jobs aus

## Verwendungsbeispiele

### Beispiel 1: Produkttyp-Klassifizierung

```csharp
// Workload konfigurieren
var config = new WorkloadConfiguration
{
    OperationType = WorkloadOperationType.CreateReferenceTable,
    TimeoutSeconds = 60,
    Parameters = new Dictionary<string, object?>
    {
        ["tableName"] = "produkttyp",
        ["columns"] = JsonSerializer.Serialize(new[]
        {
            new { name = "ProductType", dataType = "string", order = 1 },
            new { name = "TargetGroup", dataType = "string", order = 2 }
        }),
        ["isVisible"] = true
    }
};

// Workload ausführen
var result = await workload.ExecuteAsync(config);
```

### Beispiel 2: Daten synchronisieren

```csharp
var syncConfig = new WorkloadConfiguration
{
    OperationType = WorkloadOperationType.SyncReferenceTable,
    TimeoutSeconds = 120,
    Parameters = new Dictionary<string, object?>
    {
        ["tableName"] = "produkttyp",
        ["keyAttributeName"] = "Produkt",
        ["data"] = JsonSerializer.Serialize(productData)
    }
};

var result = await workload.ExecuteAsync(syncConfig);
```

## Fehlerbehebung

### Problem: Workload nicht in Fabric sichtbar

**Lösung**:
1. Überprüfen Sie die Workload-Registrierung
2. Stellen Sie sicher, dass AAD App ID und Backend URL korrekt sind
3. Prüfen Sie die Admin-Berechtigungen für die App Registration

### Problem: API-Aufrufe schlagen fehl

**Lösung**:
1. Überprüfen Sie die Backend-URL-Erreichbarkeit
2. Prüfen Sie die CORS-Konfiguration
3. Validieren Sie die API-Berechtigungen
4. Überprüfen Sie die Application Insights-Logs

### Problem: Health Check gibt Fehler zurück

**Lösung**:
1. Überprüfen Sie die Deployment-Logs in Azure
2. Stellen Sie sicher, dass alle Dependencies korrekt installiert sind
3. Prüfen Sie die App Service-Konfiguration

## Monitoring und Logging

### Application Insights aktivieren

```bash
# Application Insights aktivieren
az monitor app-insights component create \
  --app fabric-mapping-insights \
  --location westeurope \
  --resource-group fabric-mapping-rg

# Instrumentation Key erhalten
az monitor app-insights component show \
  --app fabric-mapping-insights \
  --resource-group fabric-mapping-rg \
  --query instrumentationKey

# In App Settings eintragen
az webapp config appsettings set \
  --resource-group fabric-mapping-rg \
  --name fabric-mapping-service \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=ihr-key"
```

### Log-Abfragen

```kusto
// Workload-Ausführungen anzeigen
traces
| where message contains "Executing workload operation"
| project timestamp, message, customDimensions

// Fehler anzeigen
exceptions
| where timestamp > ago(1h)
| project timestamp, type, outerMessage, innermostMessage
```

## Best Practices

1. **Validierung**: Immer Konfiguration vor Ausführung validieren
2. **Timeouts**: Angemessene Timeouts für Operationen setzen
3. **Fehlerbehandlung**: Detaillierte Fehlermeldungen für Debugging bereitstellen
4. **Logging**: Strukturiertes Logging für alle wichtigen Operationen
5. **Monitoring**: Health Checks und Metriken überwachen
6. **Sicherheit**: Secrets in Azure Key Vault speichern
7. **Testing**: Umfassende Unit- und Integrationstests schreiben
8. **Dokumentation**: Code-Kommentare und API-Dokumentation pflegen

## Weiterführende Ressourcen

- [Microsoft Fabric Documentation](https://learn.microsoft.com/fabric/)
- [Extensibility Toolkit Overview](https://learn.microsoft.com/fabric/extensibility-toolkit/overview)
- [Fabric Backend Implementation](https://learn.microsoft.com/fabric/workload-development-kit/extensibility-back-end)
- [OneLake API Reference](https://learn.microsoft.com/fabric/onelake/onelake-api-reference)
- [Projekt README](../README.md)
- [Fabric Integration Guide](./FABRIC-INTEGRATION.md)
- [API Documentation](./API.md)

## Support

Bei Fragen oder Problemen:
- Öffnen Sie ein Issue auf GitHub
- Siehe [SUPPORT.md](../SUPPORT.md) für weitere Kontaktmöglichkeiten
- Lesen Sie die [FAQ im README](../README.md)

---

**Erstellt für den Microsoft Fabric Extensibility Toolkit Contest**

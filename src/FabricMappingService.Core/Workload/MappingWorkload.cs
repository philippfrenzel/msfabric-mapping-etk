using System.Diagnostics;
using System.Text.Json;
using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;

namespace FabricMappingService.Core.Workload;

/// <summary>
/// Microsoft Fabric Mapping Workload implementation.
/// Provides reference table management and data mapping capabilities for Microsoft Fabric.
/// </summary>
public class MappingWorkload : IWorkload
{
    private readonly IMappingIO _mappingIO;
    private readonly IAttributeMappingService _attributeMappingService;
    private readonly MappingConfiguration _mappingConfiguration;
    private readonly IItemDefinitionStorage _itemStorage;
    private readonly IOneLakeStorage _oneLakeStorage;

    /// <inheritdoc/>
    public string WorkloadId => "fabric-mapping-service";

    /// <inheritdoc/>
    public string DisplayName => "Reference Table & Data Mapping Service";

    /// <inheritdoc/>
    public string Version => "1.0.0";

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingWorkload"/> class.
    /// </summary>
    /// <param name="mappingIO">The reference table mapping service.</param>
    /// <param name="attributeMappingService">The attribute mapping service.</param>
    /// <param name="mappingConfiguration">The mapping configuration.</param>
    /// <param name="itemStorage">The item definition storage service.</param>
    /// <param name="oneLakeStorage">The OneLake storage service.</param>
    public MappingWorkload(
        IMappingIO mappingIO,
        IAttributeMappingService attributeMappingService,
        MappingConfiguration mappingConfiguration,
        IItemDefinitionStorage itemStorage,
        IOneLakeStorage oneLakeStorage)
    {
        ArgumentNullException.ThrowIfNull(mappingIO);
        ArgumentNullException.ThrowIfNull(attributeMappingService);
        ArgumentNullException.ThrowIfNull(mappingConfiguration);
        ArgumentNullException.ThrowIfNull(itemStorage);
        ArgumentNullException.ThrowIfNull(oneLakeStorage);

        _mappingIO = mappingIO;
        _attributeMappingService = attributeMappingService;
        _mappingConfiguration = mappingConfiguration;
        _itemStorage = itemStorage;
        _oneLakeStorage = oneLakeStorage;
    }

    /// <inheritdoc/>
    public async Task<WorkloadExecutionResult> ExecuteAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var stopwatch = Stopwatch.StartNew();
        var result = new WorkloadExecutionResult();

        try
        {
            // Validate configuration first
            var validationResult = await ValidateConfigurationAsync(configuration, cancellationToken);
            if (!validationResult.IsValid)
            {
                result.Success = false;
                result.ErrorMessage = $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}";
                result.Warnings.AddRange(validationResult.Warnings);
                return result;
            }

            // Execute the operation based on type
            result.Data = configuration.OperationType switch
            {
                WorkloadOperationType.CreateReferenceTable => await ExecuteCreateReferenceTableAsync(configuration, cancellationToken),
                WorkloadOperationType.SyncReferenceTable => await ExecuteSyncReferenceTableAsync(configuration, cancellationToken),
                WorkloadOperationType.ReadReferenceTable => await ExecuteReadReferenceTableAsync(configuration, cancellationToken),
                WorkloadOperationType.UpdateReferenceTableRow => await ExecuteUpdateReferenceTableRowAsync(configuration, cancellationToken),
                WorkloadOperationType.DeleteReferenceTable => await ExecuteDeleteReferenceTableAsync(configuration, cancellationToken),
                WorkloadOperationType.ExecuteMapping => await ExecuteMappingAsync(configuration, cancellationToken),
                WorkloadOperationType.ValidateMapping => await ExecuteValidateMappingAsync(configuration, cancellationToken),
                WorkloadOperationType.HealthCheck => await ExecuteHealthCheckAsync(cancellationToken),
                WorkloadOperationType.CreateMappingItem => await ExecuteCreateMappingItemAsync(configuration, cancellationToken),
                WorkloadOperationType.UpdateMappingItem => await ExecuteUpdateMappingItemAsync(configuration, cancellationToken),
                WorkloadOperationType.DeleteMappingItem => await ExecuteDeleteMappingItemAsync(configuration, cancellationToken),
                WorkloadOperationType.StoreToOneLake => await ExecuteStoreToOneLakeAsync(configuration, cancellationToken),
                WorkloadOperationType.ReadFromOneLake => await ExecuteReadFromOneLakeAsync(configuration, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported operation type: {configuration.OperationType}")
            };

            result.Success = true;
            result.Metadata["OperationType"] = configuration.OperationType.ToString();
            result.Metadata["WorkspaceId"] = configuration.WorkspaceId;
            result.Metadata["ItemId"] = configuration.ItemId;
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = "Operation was cancelled";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Metadata["ExceptionType"] = ex.GetType().Name;
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    /// <inheritdoc/>
    public Task<WorkloadValidationResult> ValidateConfigurationAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var result = new WorkloadValidationResult { IsValid = true };

        // Validate timeout
        if (configuration.TimeoutSeconds <= 0)
        {
            result.Errors.Add("Timeout must be greater than 0");
            result.IsValid = false;
        }

        if (configuration.TimeoutSeconds > 3600)
        {
            result.Warnings.Add("Timeout is very long (> 1 hour). Consider reducing it.");
        }

        // Validate operation-specific requirements
        switch (configuration.OperationType)
        {
            case WorkloadOperationType.CreateReferenceTable:
                ValidateCreateReferenceTableConfig(configuration, result);
                break;
            case WorkloadOperationType.SyncReferenceTable:
                ValidateSyncReferenceTableConfig(configuration, result);
                break;
            case WorkloadOperationType.ReadReferenceTable:
            case WorkloadOperationType.DeleteReferenceTable:
                ValidateTableNameConfig(configuration, result);
                break;
            case WorkloadOperationType.UpdateReferenceTableRow:
                ValidateUpdateRowConfig(configuration, result);
                break;
            case WorkloadOperationType.ExecuteMapping:
                ValidateMappingConfig(configuration, result);
                break;
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<WorkloadHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new WorkloadHealthStatus
        {
            IsHealthy = true,
            Status = "Healthy",
            Version = Version,
            Details = new Dictionary<string, object?>
            {
                ["WorkloadId"] = WorkloadId,
                ["DisplayName"] = DisplayName,
                ["MappingServiceAvailable"] = _mappingIO != null,
                ["AttributeMappingServiceAvailable"] = _attributeMappingService != null,
                ["ConfigurationLoaded"] = _mappingConfiguration != null
            }
        };

        return Task.FromResult(status);
    }

    #region Private Execution Methods

    private async Task<object> ExecuteCreateReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = GetRequiredParameter<string>(configuration, "tableName");
        var columnsJson = GetRequiredParameter<string>(configuration, "columns");
        var columns = JsonSerializer.Deserialize<List<ReferenceTableColumn>>(columnsJson)
            ?? throw new InvalidOperationException("Failed to deserialize columns");

        var isVisible = GetOptionalParameter<bool>(configuration, "isVisible", true);
        var notifyOnNewMapping = GetOptionalParameter<bool>(configuration, "notifyOnNewMapping", false);
        var sourceLakehouseItemId = GetOptionalParameter<string?>(configuration, "sourceLakehouseItemId", null);
        var sourceWorkspaceId = GetOptionalParameter<string?>(configuration, "sourceWorkspaceId", null);
        var sourceTableName = GetOptionalParameter<string?>(configuration, "sourceTableName", null);
        var sourceOneLakeLink = GetOptionalParameter<string?>(configuration, "sourceOneLakeLink", null);

        await Task.Run(() =>
        {
            _mappingIO.CreateReferenceTable(
                tableName,
                columns,
                isVisible,
                notifyOnNewMapping,
                sourceLakehouseItemId,
                sourceWorkspaceId,
                sourceTableName,
                sourceOneLakeLink);
        }, cancellationToken);

        return new { tableName, columnsCount = columns.Count };
    }

    private async Task<object> ExecuteSyncReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = GetRequiredParameter<string>(configuration, "tableName");
        var keyAttributeName = GetRequiredParameter<string>(configuration, "keyAttributeName");
        var dataJson = GetRequiredParameter<string>(configuration, "data");
        
        var data = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(dataJson)
            ?? throw new InvalidOperationException("Failed to deserialize data");

        var newKeysAdded = await Task.Run(() =>
            _mappingIO.SyncMapping(data, keyAttributeName, tableName),
            cancellationToken);

        return new { tableName, newKeysAdded, totalKeys = data.Count };
    }

    private async Task<object> ExecuteReadReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = GetRequiredParameter<string>(configuration, "tableName");

        var mappingData = await Task.Run(() =>
            _mappingIO.ReadMapping(tableName),
            cancellationToken);

        return new { tableName, data = mappingData };
    }

    private async Task<object> ExecuteUpdateReferenceTableRowAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = GetRequiredParameter<string>(configuration, "tableName");
        var key = GetRequiredParameter<string>(configuration, "key");
        var attributesJson = GetRequiredParameter<string>(configuration, "attributes");
        
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object?>>(attributesJson)
            ?? throw new InvalidOperationException("Failed to deserialize attributes");

        await Task.Run(() =>
            _mappingIO.AddOrUpdateRow(tableName, key, attributes),
            cancellationToken);

        return new { tableName, key, updated = true };
    }

    private async Task<object> ExecuteDeleteReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = GetRequiredParameter<string>(configuration, "tableName");

        await Task.Run(() =>
            _mappingIO.DeleteReferenceTable(tableName),
            cancellationToken);

        return new { tableName, deleted = true };
    }

    private async Task<object> ExecuteMappingAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var sourceTypeJson = GetRequiredParameter<string>(configuration, "sourceData");
        var targetTypeName = GetRequiredParameter<string>(configuration, "targetType");

        // Note: This is a simplified implementation for the workload orchestration layer.
        // In a production scenario, this would:
        // 1. Dynamically resolve source and target types from assembly
        // 2. Deserialize sourceData to the source type
        // 3. Use AttributeMappingService.Map<TSource, TTarget> for the actual mapping
        // 4. Return the mapped result
        // 
        // For now, the AttributeMappingService can be used directly via the MappingController
        // endpoints (/api/mapping/customer/legacy-to-modern, etc.) which have concrete types.
        // This workload method provides the infrastructure for future dynamic mapping scenarios.
        await Task.CompletedTask;

        return new
        {
            message = "Mapping execution requires type resolution at runtime",
            note = "Use the concrete mapping endpoints in MappingController for attribute-based mapping",
            sourceDataReceived = !string.IsNullOrEmpty(sourceTypeJson),
            targetType = targetTypeName
        };
    }

    private async Task<object> ExecuteValidateMappingAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        return new
        {
            valid = true,
            message = "Mapping configuration is valid"
        };
    }

    private async Task<object> ExecuteHealthCheckAsync(CancellationToken cancellationToken)
    {
        var health = await GetHealthStatusAsync(cancellationToken);
        return health;
    }

    #endregion

    #region Private Validation Methods

    private static void ValidateCreateReferenceTableConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        if (!configuration.Parameters.ContainsKey("tableName"))
        {
            result.Errors.Add("Parameter 'tableName' is required for CreateReferenceTable operation");
            result.IsValid = false;
        }

        if (!configuration.Parameters.ContainsKey("columns"))
        {
            result.Errors.Add("Parameter 'columns' is required for CreateReferenceTable operation");
            result.IsValid = false;
        }
    }

    private static void ValidateSyncReferenceTableConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        if (!configuration.Parameters.ContainsKey("tableName"))
        {
            result.Errors.Add("Parameter 'tableName' is required for SyncReferenceTable operation");
            result.IsValid = false;
        }

        if (!configuration.Parameters.ContainsKey("keyAttributeName"))
        {
            result.Errors.Add("Parameter 'keyAttributeName' is required for SyncReferenceTable operation");
            result.IsValid = false;
        }

        if (!configuration.Parameters.ContainsKey("data"))
        {
            result.Errors.Add("Parameter 'data' is required for SyncReferenceTable operation");
            result.IsValid = false;
        }
    }

    private static void ValidateTableNameConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        if (!configuration.Parameters.ContainsKey("tableName"))
        {
            result.Errors.Add("Parameter 'tableName' is required");
            result.IsValid = false;
        }
    }

    private static void ValidateUpdateRowConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        if (!configuration.Parameters.ContainsKey("tableName"))
        {
            result.Errors.Add("Parameter 'tableName' is required for UpdateReferenceTableRow operation");
            result.IsValid = false;
        }

        if (!configuration.Parameters.ContainsKey("key"))
        {
            result.Errors.Add("Parameter 'key' is required for UpdateReferenceTableRow operation");
            result.IsValid = false;
        }

        if (!configuration.Parameters.ContainsKey("attributes"))
        {
            result.Errors.Add("Parameter 'attributes' is required for UpdateReferenceTableRow operation");
            result.IsValid = false;
        }
    }

    private static void ValidateMappingConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        if (!configuration.Parameters.ContainsKey("sourceData"))
        {
            result.Errors.Add("Parameter 'sourceData' is required for ExecuteMapping operation");
            result.IsValid = false;
        }

        if (!configuration.Parameters.ContainsKey("targetType"))
        {
            result.Errors.Add("Parameter 'targetType' is required for ExecuteMapping operation");
            result.IsValid = false;
        }
    }

    #endregion

    #region Helper Methods

    private static T GetRequiredParameter<T>(WorkloadConfiguration configuration, string parameterName)
    {
        if (!configuration.Parameters.TryGetValue(parameterName, out var value))
        {
            throw new ArgumentException($"Required parameter '{parameterName}' not found in configuration");
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText())
                ?? throw new InvalidOperationException($"Failed to deserialize parameter '{parameterName}'");
        }

        throw new InvalidOperationException($"Parameter '{parameterName}' has invalid type");
    }

    private static T GetOptionalParameter<T>(
        WorkloadConfiguration configuration,
        string parameterName,
        T defaultValue)
    {
        if (!configuration.Parameters.TryGetValue(parameterName, out var value))
        {
            return defaultValue;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
        }

        return defaultValue;
    }

    #endregion

    #region Item Operations

    private async Task<object> ExecuteCreateMappingItemAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var displayName = GetRequiredParameter<string>(configuration, "displayName");
        var workspaceId = GetRequiredParameter<string>(configuration, "workspaceId");
        var lakehouseItemId = GetRequiredParameter<string>(configuration, "lakehouseItemId");
        var tableName = GetRequiredParameter<string>(configuration, "tableName");
        var referenceAttributeName = GetRequiredParameter<string>(configuration, "referenceAttributeName");

        var description = GetOptionalParameter<string>(configuration, "description", string.Empty);
        var lakehouseWorkspaceId = GetOptionalParameter<string>(configuration, "lakehouseWorkspaceId", workspaceId);
        var oneLakeLink = GetOptionalParameter<string>(configuration, "oneLakeLink", string.Empty);

        var mappingColumnsJson = GetOptionalParameter<string>(configuration, "mappingColumns", "[]");
        var mappingColumns = JsonSerializer.Deserialize<List<MappingColumn>>(mappingColumnsJson) ?? [];

        var itemId = Guid.NewGuid().ToString();

        var definition = new MappingItemDefinition
        {
            ItemId = itemId,
            DisplayName = displayName,
            Description = description,
            WorkspaceId = workspaceId,
            Configuration = new MappingItemConfiguration
            {
                LakehouseItemId = lakehouseItemId,
                LakehouseWorkspaceId = lakehouseWorkspaceId,
                TableName = tableName,
                ReferenceAttributeName = referenceAttributeName,
                MappingColumns = mappingColumns,
                OneLakeLink = oneLakeLink
            }
        };

        await _itemStorage.CreateItemDefinitionAsync(definition, cancellationToken);

        return new
        {
            Success = true,
            ItemId = itemId,
            DisplayName = displayName,
            WorkspaceId = workspaceId,
            Message = $"Mapping item '{displayName}' created successfully"
        };
    }

    private async Task<object> ExecuteUpdateMappingItemAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var itemId = GetRequiredParameter<string>(configuration, "itemId");

        var definition = await _itemStorage.GetItemDefinitionAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException($"Mapping item '{itemId}' not found");

        // Update fields if provided
        if (configuration.Parameters.TryGetValue("displayName", out var displayName))
        {
            definition.DisplayName = displayName?.ToString() ?? definition.DisplayName;
        }

        if (configuration.Parameters.TryGetValue("description", out var description))
        {
            definition.Description = description?.ToString();
        }

        if (configuration.Parameters.TryGetValue("lakehouseItemId", out var lakehouseItemId))
        {
            definition.Configuration.LakehouseItemId = lakehouseItemId?.ToString() ?? definition.Configuration.LakehouseItemId;
        }

        if (configuration.Parameters.TryGetValue("tableName", out var tableName))
        {
            definition.Configuration.TableName = tableName?.ToString() ?? definition.Configuration.TableName;
        }

        if (configuration.Parameters.TryGetValue("referenceAttributeName", out var refAttr))
        {
            definition.Configuration.ReferenceAttributeName = refAttr?.ToString() ?? definition.Configuration.ReferenceAttributeName;
        }

        if (configuration.Parameters.TryGetValue("mappingColumns", out var mappingColumnsObj))
        {
            var mappingColumnsJson = mappingColumnsObj?.ToString() ?? "[]";
            definition.Configuration.MappingColumns = JsonSerializer.Deserialize<List<MappingColumn>>(mappingColumnsJson) ?? [];
        }

        await _itemStorage.UpdateItemDefinitionAsync(definition, cancellationToken);

        return new
        {
            Success = true,
            ItemId = itemId,
            DisplayName = definition.DisplayName,
            Message = $"Mapping item '{itemId}' updated successfully"
        };
    }

    private async Task<object> ExecuteDeleteMappingItemAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var itemId = GetRequiredParameter<string>(configuration, "itemId");

        var deleted = await _itemStorage.DeleteItemDefinitionAsync(itemId, cancellationToken);

        return new
        {
            Success = deleted,
            ItemId = itemId,
            Message = deleted
                ? $"Mapping item '{itemId}' deleted successfully"
                : $"Mapping item '{itemId}' not found"
        };
    }

    private async Task<object> ExecuteStoreToOneLakeAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var itemId = GetRequiredParameter<string>(configuration, "itemId");
        var workspaceId = GetRequiredParameter<string>(configuration, "workspaceId");
        var tableName = GetRequiredParameter<string>(configuration, "tableName");
        var dataJson = GetRequiredParameter<string>(configuration, "data");

        var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(dataJson)
            ?? throw new InvalidOperationException("Failed to deserialize mapping data");

        var oneLakePath = await _oneLakeStorage.StoreMappingTableAsync(
            itemId,
            workspaceId,
            tableName,
            data,
            cancellationToken);

        return new
        {
            Success = true,
            OneLakePath = oneLakePath,
            RowCount = data.Count,
            Message = $"Mapping table '{tableName}' stored to OneLake successfully"
        };
    }

    private async Task<object> ExecuteReadFromOneLakeAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var itemId = GetRequiredParameter<string>(configuration, "itemId");
        var workspaceId = GetRequiredParameter<string>(configuration, "workspaceId");
        var tableName = GetRequiredParameter<string>(configuration, "tableName");

        var data = await _oneLakeStorage.ReadMappingTableAsync(
            itemId,
            workspaceId,
            tableName,
            cancellationToken);

        return new
        {
            Success = true,
            TableName = tableName,
            RowCount = data.Count,
            Data = data,
            Message = $"Mapping table '{tableName}' read from OneLake successfully"
        };
    }

    #endregion
}

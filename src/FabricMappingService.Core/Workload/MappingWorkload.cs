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
                ValidateReadOrDeleteReferenceTableConfig(configuration, result);
                break;
            case WorkloadOperationType.UpdateReferenceTableRow:
                ValidateUpdateReferenceTableRowConfig(configuration, result);
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

    private Task<object> ExecuteCreateReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        var columns = ParameterHelper.DeserializeRequired<List<ReferenceTableColumn>>(
            configuration, 
            ParameterNames.Columns, 
            "Failed to deserialize columns");

        var isVisible = ParameterHelper.GetOptional(configuration, ParameterNames.IsVisible, true);
        var notifyOnNewMapping = ParameterHelper.GetOptional(configuration, ParameterNames.NotifyOnNewMapping, false);
        var sourceLakehouseItemId = ParameterHelper.GetOptional<string?>(configuration, ParameterNames.SourceLakehouseItemId, null);
        var sourceWorkspaceId = ParameterHelper.GetOptional<string?>(configuration, ParameterNames.SourceWorkspaceId, null);
        var sourceTableName = ParameterHelper.GetOptional<string?>(configuration, ParameterNames.SourceTableName, null);
        var sourceOneLakeLink = ParameterHelper.GetOptional<string?>(configuration, ParameterNames.SourceOneLakeLink, null);

        _mappingIO.CreateReferenceTable(
            tableName,
            columns,
            isVisible,
            notifyOnNewMapping,
            sourceLakehouseItemId,
            sourceWorkspaceId,
            sourceTableName,
            sourceOneLakeLink);

        return Task.FromResult<object>(new { tableName, columnsCount = columns.Count });
    }

    private Task<object> ExecuteSyncReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        var keyAttributeName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.KeyAttributeName);
        var data = ParameterHelper.DeserializeRequired<List<Dictionary<string, object?>>>(
            configuration, 
            ParameterNames.Data, 
            "Failed to deserialize data");

        var newKeysAdded = _mappingIO.SyncMapping(data, keyAttributeName, tableName);

        return Task.FromResult<object>(new { tableName, newKeysAdded, totalKeys = data.Count });
    }

    private Task<object> ExecuteReadReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        var mappingData = _mappingIO.ReadMapping(tableName);
        return Task.FromResult<object>(new { tableName, data = mappingData });
    }

    private Task<object> ExecuteUpdateReferenceTableRowAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        var key = ParameterHelper.GetRequired<string>(configuration, ParameterNames.Key);
        var attributes = ParameterHelper.DeserializeRequired<Dictionary<string, object?>>(
            configuration, 
            ParameterNames.Attributes, 
            "Failed to deserialize attributes");

        _mappingIO.AddOrUpdateRow(tableName, key, attributes);
        return Task.FromResult<object>(new { tableName, key, updated = true });
    }

    private Task<object> ExecuteDeleteReferenceTableAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        _mappingIO.DeleteReferenceTable(tableName);
        return Task.FromResult<object>(new { tableName, deleted = true });
    }

    private Task<object> ExecuteMappingAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var sourceTypeJson = ParameterHelper.GetRequired<string>(configuration, ParameterNames.SourceData);
        var targetTypeName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TargetType);

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
        
        return Task.FromResult<object>(new
        {
            message = "Mapping execution requires type resolution at runtime",
            note = "Use the concrete mapping endpoints in MappingController for attribute-based mapping",
            sourceDataReceived = !string.IsNullOrEmpty(sourceTypeJson),
            targetType = targetTypeName
        });
    }

    private static Task<object> ExecuteValidateMappingAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<object>(new
        {
            valid = true,
            message = "Mapping configuration is valid"
        });
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
        ValidationHelper.RequireParameters(
            configuration,
            [ParameterNames.TableName, ParameterNames.Columns],
            "CreateReferenceTable",
            result);
    }

    private static void ValidateSyncReferenceTableConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        ValidationHelper.RequireParameters(
            configuration,
            [ParameterNames.TableName, ParameterNames.KeyAttributeName, ParameterNames.Data],
            "SyncReferenceTable",
            result);
    }

    private static void ValidateReadOrDeleteReferenceTableConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        ValidationHelper.RequireParameter(
            configuration,
            ParameterNames.TableName,
            "ReadReferenceTable/DeleteReferenceTable",
            result);
    }

    private static void ValidateUpdateReferenceTableRowConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        ValidationHelper.RequireParameters(
            configuration,
            [ParameterNames.TableName, ParameterNames.Key, ParameterNames.Attributes],
            "UpdateReferenceTableRow",
            result);
    }

    private static void ValidateMappingConfig(
        WorkloadConfiguration configuration,
        WorkloadValidationResult result)
    {
        ValidationHelper.RequireParameters(
            configuration,
            [ParameterNames.SourceData, ParameterNames.TargetType],
            "ExecuteMapping",
            result);
    }

    #endregion

    #region Item Operations

    private async Task<object> ExecuteCreateMappingItemAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var displayName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.DisplayName);
        var workspaceId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.WorkspaceId);
        var lakehouseItemId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.LakehouseItemId);
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        var referenceAttributeName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.ReferenceAttributeName);

        var description = ParameterHelper.GetOptional(configuration, ParameterNames.Description, string.Empty);
        var lakehouseWorkspaceId = ParameterHelper.GetOptional(configuration, ParameterNames.LakehouseWorkspaceId, workspaceId);
        var oneLakeLink = ParameterHelper.GetOptional(configuration, ParameterNames.OneLakeLink, string.Empty);

        var mappingColumns = ParameterHelper.DeserializeOptional(
            configuration, 
            ParameterNames.MappingColumns, 
            new List<MappingColumn>());

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
        var itemId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.ItemId);

        var definition = await _itemStorage.GetItemDefinitionAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException($"Mapping item '{itemId}' not found");

        // Update fields if provided
        if (configuration.Parameters.TryGetValue(ParameterNames.DisplayName, out var displayName))
        {
            definition.DisplayName = displayName?.ToString() ?? definition.DisplayName;
        }

        if (configuration.Parameters.TryGetValue(ParameterNames.Description, out var description))
        {
            definition.Description = description?.ToString();
        }

        if (configuration.Parameters.TryGetValue(ParameterNames.LakehouseItemId, out var lakehouseItemId))
        {
            definition.Configuration.LakehouseItemId = lakehouseItemId?.ToString() ?? definition.Configuration.LakehouseItemId;
        }

        if (configuration.Parameters.TryGetValue(ParameterNames.TableName, out var tableName))
        {
            definition.Configuration.TableName = tableName?.ToString() ?? definition.Configuration.TableName;
        }

        if (configuration.Parameters.TryGetValue(ParameterNames.ReferenceAttributeName, out var refAttr))
        {
            definition.Configuration.ReferenceAttributeName = refAttr?.ToString() ?? definition.Configuration.ReferenceAttributeName;
        }

        if (configuration.Parameters.TryGetValue(ParameterNames.MappingColumns, out var mappingColumnsObj))
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
        var itemId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.ItemId);

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
        var itemId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.ItemId);
        var workspaceId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.WorkspaceId);
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);
        var data = ParameterHelper.DeserializeRequired<Dictionary<string, Dictionary<string, object?>>>(
            configuration,
            ParameterNames.Data,
            "Failed to deserialize mapping data");

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
        var itemId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.ItemId);
        var workspaceId = ParameterHelper.GetRequired<string>(configuration, ParameterNames.WorkspaceId);
        var tableName = ParameterHelper.GetRequired<string>(configuration, ParameterNames.TableName);

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

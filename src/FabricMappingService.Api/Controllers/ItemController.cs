using FabricMappingService.Api.Dtos;
using FabricMappingService.Api.Extensions;
using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FabricMappingService.Api.Controllers;

/// <summary>
/// Controller for managing mapping items in Fabric workspaces.
/// Provides endpoints for creating, configuring, and managing mapping items.
/// </summary>
[ApiController]
[Route("api/items")]
[Produces("application/json")]
public class ItemController : ControllerBase
{
    private readonly IItemDefinitionStorage _itemStorage;
    private readonly IOneLakeStorage _oneLakeStorage;
    private readonly ILogger<ItemController> _logger;

    public ItemController(
        IItemDefinitionStorage itemStorage,
        IOneLakeStorage oneLakeStorage,
        ILogger<ItemController> logger)
    {
        _itemStorage = itemStorage;
        _oneLakeStorage = oneLakeStorage;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new mapping item in a Fabric workspace.
    /// </summary>
    /// <param name="request">The request containing the item configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created mapping item.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MappingItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MappingItemResponse>> CreateMappingItem(
        [FromBody] CreateMappingItemRequest request,
        CancellationToken cancellationToken)
    {
        // Validate all required fields at once
        // Note: Dictionary creation is intentional for readability and completeness of error messages
        // The performance impact is negligible compared to the database/IO operations that follow
        if (!ValidationExtensions.ValidateAllRequired(new Dictionary<string, string?>
        {
            [nameof(request.DisplayName)] = request.DisplayName,
            [nameof(request.WorkspaceId)] = request.WorkspaceId,
            [nameof(request.LakehouseItemId)] = request.LakehouseItemId,
            [nameof(request.TableName)] = request.TableName,
            [nameof(request.ReferenceAttributeName)] = request.ReferenceAttributeName
        }, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            var itemId = Guid.NewGuid().ToString();

            var definition = new MappingItemDefinition
            {
                ItemId = itemId,
                DisplayName = request.DisplayName,
                Description = request.Description,
                WorkspaceId = request.WorkspaceId,
                Configuration = new MappingItemConfiguration
                {
                    LakehouseItemId = request.LakehouseItemId,
                    LakehouseWorkspaceId = request.LakehouseWorkspaceId,
                    TableName = request.TableName,
                    ReferenceAttributeName = request.ReferenceAttributeName,
                    MappingColumns = request.MappingColumns.Select(c => new MappingColumn
                    {
                        ColumnName = c.ColumnName,
                        DataType = c.DataType,
                        Description = c.Description,
                        IsRequired = c.IsRequired,
                        Transformation = c.Transformation
                    }).ToList(),
                    OneLakeLink = request.OneLakeLink
                }
            };

            await _itemStorage.CreateItemDefinitionAsync(definition, cancellationToken);

            _logger.LogInformation("Created mapping item {ItemId} in workspace {WorkspaceId}", itemId, request.WorkspaceId);

            var response = MapToResponse(definition);
            return CreatedAtAction(nameof(GetMappingItem), new { itemId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create mapping item");
            return BadRequest($"Failed to create mapping item: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a mapping item by ID.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapping item.</returns>
    [HttpGet("{itemId}")]
    [ProducesResponseType(typeof(MappingItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MappingItemResponse>> GetMappingItem(
        string itemId,
        CancellationToken cancellationToken)
    {
        var definition = await _itemStorage.GetItemDefinitionAsync(itemId, cancellationToken);
        if (definition == null)
        {
            return NotFound($"Mapping item '{itemId}' not found");
        }

        return Ok(MapToResponse(definition));
    }

    /// <summary>
    /// Lists all mapping items in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of mapping items.</returns>
    [HttpGet("workspace/{workspaceId}")]
    [ProducesResponseType(typeof(IEnumerable<MappingItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MappingItemResponse>>> ListMappingItems(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var definitions = await _itemStorage.ListItemDefinitionsAsync(workspaceId, cancellationToken);
        var responses = definitions.Select(MapToResponse);
        return Ok(responses);
    }

    /// <summary>
    /// Updates an existing mapping item.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated mapping item.</returns>
    [HttpPut("{itemId}")]
    [ProducesResponseType(typeof(MappingItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MappingItemResponse>> UpdateMappingItem(
        string itemId,
        [FromBody] UpdateMappingItemRequest request,
        CancellationToken cancellationToken)
    {
        var definition = await _itemStorage.GetItemDefinitionAsync(itemId, cancellationToken);
        if (definition == null)
        {
            return NotFound($"Mapping item '{itemId}' not found");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            definition.DisplayName = request.DisplayName;
        }

        if (request.Description != null)
        {
            definition.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.LakehouseItemId))
        {
            definition.Configuration.LakehouseItemId = request.LakehouseItemId;
        }

        if (!string.IsNullOrWhiteSpace(request.LakehouseWorkspaceId))
        {
            definition.Configuration.LakehouseWorkspaceId = request.LakehouseWorkspaceId;
        }

        if (!string.IsNullOrWhiteSpace(request.TableName))
        {
            definition.Configuration.TableName = request.TableName;
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceAttributeName))
        {
            definition.Configuration.ReferenceAttributeName = request.ReferenceAttributeName;
        }

        if (request.MappingColumns != null)
        {
            definition.Configuration.MappingColumns = request.MappingColumns.Select(c => new MappingColumn
            {
                ColumnName = c.ColumnName,
                DataType = c.DataType,
                Description = c.Description,
                IsRequired = c.IsRequired,
                Transformation = c.Transformation
            }).ToList();
        }

        if (request.OneLakeLink != null)
        {
            definition.Configuration.OneLakeLink = request.OneLakeLink;
        }

        await _itemStorage.UpdateItemDefinitionAsync(definition, cancellationToken);

        _logger.LogInformation("Updated mapping item {ItemId}", itemId);

        return Ok(MapToResponse(definition));
    }

    /// <summary>
    /// Deletes a mapping item.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{itemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMappingItem(
        string itemId,
        CancellationToken cancellationToken)
    {
        var deleted = await _itemStorage.DeleteItemDefinitionAsync(itemId, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Mapping item '{itemId}' not found");
        }

        _logger.LogInformation("Deleted mapping item {ItemId}", itemId);

        return NoContent();
    }

    /// <summary>
    /// Stores mapping data to OneLake.
    /// </summary>
    /// <param name="request">The request containing the data to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The storage result.</returns>
    [HttpPost("store-to-onelake")]
    [ProducesResponseType(typeof(StoreToOneLakeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StoreToOneLakeResponse>> StoreToOneLake(
        [FromBody] StoreToOneLakeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            return BadRequest("ItemId is required");
        }

        if (string.IsNullOrWhiteSpace(request.WorkspaceId))
        {
            return BadRequest("WorkspaceId is required");
        }

        if (string.IsNullOrWhiteSpace(request.TableName))
        {
            return BadRequest("TableName is required");
        }

        try
        {
            var oneLakePath = await _oneLakeStorage.StoreMappingTableAsync(
                request.ItemId,
                request.WorkspaceId,
                request.TableName,
                request.Data,
                cancellationToken);

            _logger.LogInformation("Stored mapping table {TableName} to OneLake at {Path}", request.TableName, oneLakePath);

            return Ok(new StoreToOneLakeResponse
            {
                Success = true,
                OneLakePath = oneLakePath,
                RowCount = request.Data.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store mapping table to OneLake");
            return Ok(new StoreToOneLakeResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Reads mapping data from OneLake.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapping data.</returns>
    [HttpGet("read-from-onelake/{workspaceId}/{itemId}/{tableName}")]
    [ProducesResponseType(typeof(Dictionary<string, Dictionary<string, object?>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<string, Dictionary<string, object?>>>> ReadFromOneLake(
        string itemId,
        string workspaceId,
        string tableName,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _oneLakeStorage.ReadMappingTableAsync(itemId, workspaceId, tableName, cancellationToken);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private static MappingItemResponse MapToResponse(MappingItemDefinition definition)
    {
        return new MappingItemResponse
        {
            ItemId = definition.ItemId,
            DisplayName = definition.DisplayName,
            Description = definition.Description,
            WorkspaceId = definition.WorkspaceId,
            LakehouseItemId = definition.Configuration.LakehouseItemId,
            LakehouseWorkspaceId = definition.Configuration.LakehouseWorkspaceId,
            TableName = definition.Configuration.TableName,
            ReferenceAttributeName = definition.Configuration.ReferenceAttributeName,
            MappingColumns = definition.Configuration.MappingColumns.Select(c => new MappingColumnDto
            {
                ColumnName = c.ColumnName,
                DataType = c.DataType,
                Description = c.Description,
                IsRequired = c.IsRequired,
                Transformation = c.Transformation
            }).ToList(),
            OneLakeLink = definition.Configuration.OneLakeLink,
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.UpdatedAt
        };
    }
}

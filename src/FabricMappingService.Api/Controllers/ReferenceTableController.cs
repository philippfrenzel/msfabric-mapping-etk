using Microsoft.AspNetCore.Mvc;
using FabricMappingService.Api.Dtos;
using FabricMappingService.Api.Extensions;
using FabricMappingService.Core.Services;
using FabricMappingService.Core.Models;

namespace FabricMappingService.Api.Controllers;

/// <summary>
/// Controller for managing reference mapping tables.
/// Provides endpoints to create, sync, read, and manage reference tables.
/// </summary>
[ApiController]
[Route("api/reference-tables")]
public class ReferenceTableController : ControllerBase
{
    private readonly IMappingIO _mappingIO;
    private readonly ILogger<ReferenceTableController> _logger;

    /// <summary>
    /// Initializes a new instance of the ReferenceTableController class.
    /// </summary>
    /// <param name="mappingIO">The MappingIO service.</param>
    /// <param name="logger">The logger instance.</param>
    public ReferenceTableController(IMappingIO mappingIO, ILogger<ReferenceTableController> logger)
    {
        _mappingIO = mappingIO;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new empty reference table.
    /// </summary>
    /// <param name="request">The request containing table definition.</param>
    /// <returns>A response indicating success or failure.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateReferenceTable([FromBody] CreateReferenceTableRequest request)
    {
        if (!request.TableName.ValidateRequired(nameof(request.TableName), out var validationError))
        {
            return validationError!;
        }

        try
        {
            var columns = request.Columns.Select(c => new ReferenceTableColumn
            {
                Name = c.Name,
                DataType = c.DataType,
                Description = c.Description,
                Order = c.Order
            }).ToList();

            _mappingIO.CreateReferenceTable(
                request.TableName,
                columns,
                request.IsVisible,
                request.NotifyOnNewMapping,
                request.SourceLakehouseItemId,
                request.SourceWorkspaceId,
                request.SourceTableName,
                request.SourceOneLakeLink);

            _logger.LogInformation("Created reference table '{TableName}' with {ColumnCount} columns",
                request.TableName, columns.Count);

            return CreatedAtAction(
                nameof(GetReferenceTable),
                new { tableName = request.TableName },
                new { success = true, tableName = request.TableName });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create reference table '{TableName}'", request.TableName);
            return ValidationExtensions.BadRequestWithSuccess(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reference table '{TableName}'", request.TableName);
            return ValidationExtensions.InternalServerError("Internal server error");
        }
    }

    /// <summary>
    /// Synchronizes a reference table with data from a collection.
    /// Creates the table if it doesn't exist, or adds new keys to existing table.
    /// </summary>
    /// <param name="request">The request containing data and key attribute name.</param>
    /// <returns>A response with the number of new keys added.</returns>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SyncMapping([FromBody] SyncMappingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MappingTableName))
        {
            return BadRequest(new SyncMappingResponse
            {
                Success = false,
                ErrorMessage = "Mapping table name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(request.KeyAttributeName))
        {
            return BadRequest(new SyncMappingResponse
            {
                Success = false,
                ErrorMessage = "Key attribute name is required"
            });
        }

        try
        {
            var newKeysAdded = _mappingIO.SyncMapping(
                request.Data,
                request.KeyAttributeName,
                request.MappingTableName);

            var table = _mappingIO.GetReferenceTable(request.MappingTableName);

            _logger.LogInformation(
                "Synced reference table '{TableName}': {NewKeys} new keys added, {TotalKeys} total keys",
                request.MappingTableName, newKeysAdded, table?.Rows.Count ?? 0);

            return Ok(new SyncMappingResponse
            {
                Success = true,
                TableName = request.MappingTableName,
                NewKeysAdded = newKeysAdded,
                TotalKeys = table?.Rows.Count ?? 0
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for sync mapping '{TableName}'", request.MappingTableName);
            return BadRequest(new SyncMappingResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing reference table '{TableName}'", request.MappingTableName);
            return StatusCode(500, new SyncMappingResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Reads a reference table and returns its data.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <returns>The reference table data as a dictionary.</returns>
    [HttpGet("{tableName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetReferenceTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest(new ReferenceTableResponse
            {
                Success = false,
                ErrorMessage = "Table name is required"
            });
        }

        try
        {
            var data = _mappingIO.ReadMapping(tableName);

            _logger.LogInformation("Retrieved reference table '{TableName}' with {RowCount} rows",
                tableName, data.Count);

            return Ok(new ReferenceTableResponse
            {
                Success = true,
                TableName = tableName,
                Data = data
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reference table '{TableName}' not found", tableName);
            return NotFound(new ReferenceTableResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading reference table '{TableName}'", tableName);
            return StatusCode(500, new ReferenceTableResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Gets all reference table names.
    /// </summary>
    /// <returns>A list of all reference table names.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllTables()
    {
        try
        {
            var tableNames = _mappingIO.GetAllTableNames().ToList();

            _logger.LogInformation("Retrieved {TableCount} reference tables", tableNames.Count);

            return Ok(new
            {
                success = true,
                tables = tableNames,
                count = tableNames.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reference tables");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Deletes a reference table.
    /// </summary>
    /// <param name="tableName">The name of the reference table to delete.</param>
    /// <returns>A response indicating success or failure.</returns>
    [HttpDelete("{tableName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteReferenceTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest(new { success = false, error = "Table name is required" });
        }

        try
        {
            var deleted = _mappingIO.DeleteReferenceTable(tableName);

            if (deleted)
            {
                _logger.LogInformation("Deleted reference table '{TableName}'", tableName);
                return Ok(new { success = true, message = $"Reference table '{tableName}' deleted" });
            }
            else
            {
                _logger.LogWarning("Reference table '{TableName}' not found for deletion", tableName);
                return NotFound(new { success = false, error = $"Reference table '{tableName}' not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reference table '{TableName}'", tableName);
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Adds or updates a row in a reference table.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="request">The request containing the row data.</param>
    /// <returns>A response indicating success or failure.</returns>
    [HttpPut("{tableName}/rows")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddOrUpdateRow(string tableName, [FromBody] AddOrUpdateRowRequest request)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest(new { success = false, error = "Table name is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Key))
        {
            return BadRequest(new { success = false, error = "Key is required" });
        }

        try
        {
            _mappingIO.AddOrUpdateRow(tableName, request.Key, request.Attributes);

            _logger.LogInformation("Updated row with key '{Key}' in reference table '{TableName}'",
                request.Key, tableName);

            return Ok(new
            {
                success = true,
                tableName,
                key = request.Key,
                message = "Row updated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reference table '{TableName}' not found", tableName);
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating row in reference table '{TableName}'", tableName);
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }
}

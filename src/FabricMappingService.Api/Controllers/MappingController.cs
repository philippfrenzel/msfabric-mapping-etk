using Microsoft.AspNetCore.Mvc;
using FabricMappingService.Api.Dtos;
using FabricMappingService.Core.Services;
using FabricMappingService.Core.Examples;
using System.Text.Json;

namespace FabricMappingService.Api.Controllers;

/// <summary>
/// Controller for mapping operations.
/// Provides endpoints for mapping data between different object structures using attribute-based configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MappingController : ControllerBase
{
    private readonly IAttributeMappingService _mappingService;
    private readonly ILogger<MappingController> _logger;

    /// <summary>
    /// Initializes a new instance of the MappingController class.
    /// </summary>
    /// <param name="mappingService">The mapping service instance.</param>
    /// <param name="logger">The logger instance.</param>
    public MappingController(
        IAttributeMappingService mappingService,
        ILogger<MappingController> logger)
    {
        _mappingService = mappingService;
        _logger = logger;
    }

    /// <summary>
    /// Maps customer data from legacy format to modern format.
    /// </summary>
    /// <param name="legacyCustomer">The legacy customer data.</param>
    /// <returns>The mapped modern customer data.</returns>
    [HttpPost("customer/legacy-to-modern")]
    [ProducesResponseType(typeof(MappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult MapLegacyCustomer([FromBody] LegacyCustomerModel legacyCustomer)
    {
        try
        {
            _logger.LogInformation("Mapping legacy customer ID: {CustomerId}", legacyCustomer.Id);

            var result = _mappingService.MapWithResult<LegacyCustomerModel, ModernCustomerModel>(legacyCustomer);

            var response = new MappingResponse
            {
                Success = result.Success,
                Data = result.Result,
                Errors = result.Errors,
                Warnings = result.Warnings,
                MappedPropertiesCount = result.MappedPropertiesCount,
                Metadata = new Dictionary<string, object>
                {
                    { "SourceType", nameof(LegacyCustomerModel) },
                    { "TargetType", nameof(ModernCustomerModel) }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping legacy customer");
            return BadRequest(new MappingResponse
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Maps product data from external format to internal format.
    /// </summary>
    /// <param name="externalProduct">The external product data.</param>
    /// <returns>The mapped internal product data.</returns>
    [HttpPost("product/external-to-internal")]
    [ProducesResponseType(typeof(MappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult MapExternalProduct([FromBody] ExternalProductModel externalProduct)
    {
        try
        {
            _logger.LogInformation("Mapping external product: {ProductCode}", externalProduct.ProductCode);

            var result = _mappingService.MapWithResult<ExternalProductModel, InternalProductModel>(externalProduct);

            var response = new MappingResponse
            {
                Success = result.Success,
                Data = result.Result,
                Errors = result.Errors,
                Warnings = result.Warnings,
                MappedPropertiesCount = result.MappedPropertiesCount,
                Metadata = new Dictionary<string, object>
                {
                    { "SourceType", nameof(ExternalProductModel) },
                    { "TargetType", nameof(InternalProductModel) }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping external product");
            return BadRequest(new MappingResponse
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Maps a batch of legacy customers to modern format.
    /// </summary>
    /// <param name="legacyCustomers">Collection of legacy customer data.</param>
    /// <returns>Batch mapping results.</returns>
    [HttpPost("customer/batch-legacy-to-modern")]
    [ProducesResponseType(typeof(BatchMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult MapLegacyCustomersBatch([FromBody] List<LegacyCustomerModel> legacyCustomers)
    {
        try
        {
            _logger.LogInformation("Batch mapping {Count} legacy customers", legacyCustomers.Count);

            var results = _mappingService.MapCollection<LegacyCustomerModel, ModernCustomerModel>(legacyCustomers);

            var response = new BatchMappingResponse
            {
                Success = true,
                TotalItems = legacyCustomers.Count,
                SuccessCount = legacyCustomers.Count,
                FailureCount = 0,
                Results = results.Select(r => new MappingResponse
                {
                    Success = true,
                    Data = r
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch mapping");
            return BadRequest(new BatchMappingResponse
            {
                Success = false,
                TotalItems = legacyCustomers.Count,
                SuccessCount = 0,
                FailureCount = legacyCustomers.Count,
                Results = new List<MappingResponse>
                {
                    new MappingResponse
                    {
                        Success = false,
                        Errors = new List<string> { ex.Message }
                    }
                }
            });
        }
    }

    /// <summary>
    /// Gets information about available mapping endpoints.
    /// </summary>
    /// <returns>Mapping service information.</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            ServiceName = "Fabric Mapping Service",
            Version = "1.0.0",
            Description = "Attribute-based data mapping service for Microsoft Fabric Extensibility Toolkit",
            AvailableMappings = new[]
            {
                new { Endpoint = "/api/mapping/customer/legacy-to-modern", Method = "POST", Description = "Map legacy customer to modern format" },
                new { Endpoint = "/api/mapping/product/external-to-internal", Method = "POST", Description = "Map external product to internal format" },
                new { Endpoint = "/api/mapping/customer/batch-legacy-to-modern", Method = "POST", Description = "Batch map legacy customers" }
            },
            SupportedFeatures = new[]
            {
                "Attribute-based mapping",
                "Type conversion",
                "Batch operations",
                "Detailed error reporting",
                "Custom converters support"
            }
        });
    }

    /// <summary>
    /// Health check endpoint for the mapping service.
    /// </summary>
    /// <returns>Service health status.</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "FabricMappingService"
        });
    }
}

using Microsoft.AspNetCore.Mvc;

namespace FabricMappingService.Api.Extensions;

/// <summary>
/// Extension methods for request validation in controllers.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a string field is not null or whitespace.
    /// </summary>
    public static bool ValidateRequired(this string? value, string fieldName, out BadRequestObjectResult? errorResult)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errorResult = new BadRequestObjectResult(new { error = $"{fieldName} is required" });
            return false;
        }

        errorResult = null;
        return true;
    }

    /// <summary>
    /// Validates multiple required string fields.
    /// </summary>
    public static bool ValidateAllRequired(
        Dictionary<string, string?> validations,
        out BadRequestObjectResult? errorResult)
    {
        var errors = new List<string>();

        foreach (var (fieldName, value) in validations)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"{fieldName} is required");
            }
        }

        if (errors.Count > 0)
        {
            errorResult = new BadRequestObjectResult(new { errors });
            return false;
        }

        errorResult = null;
        return true;
    }

    /// <summary>
    /// Creates a standardized bad request response with success flag.
    /// </summary>
    public static BadRequestObjectResult BadRequestWithSuccess(string error)
    {
        return new BadRequestObjectResult(new { success = false, error });
    }

    /// <summary>
    /// Creates a standardized internal server error response.
    /// </summary>
    public static ObjectResult InternalServerError(string error)
    {
        return new ObjectResult(new { success = false, error })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}

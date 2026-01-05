namespace FabricMappingService.Core.Workload;

/// <summary>
/// Helper class for validating workload configuration parameters.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates that a required parameter exists in the configuration.
    /// </summary>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterName">The name of the required parameter.</param>
    /// <param name="operationType">The operation type for error message context.</param>
    /// <param name="result">The validation result to update.</param>
    public static void RequireParameter(
        WorkloadConfiguration configuration,
        string parameterName,
        string operationType,
        WorkloadValidationResult result)
    {
        if (!configuration.Parameters.ContainsKey(parameterName))
        {
            result.Errors.Add($"Parameter '{parameterName}' is required for {operationType} operation");
            result.IsValid = false;
        }
    }

    /// <summary>
    /// Validates multiple required parameters at once.
    /// </summary>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterNames">The names of required parameters.</param>
    /// <param name="operationType">The operation type for error message context.</param>
    /// <param name="result">The validation result to update.</param>
    public static void RequireParameters(
        WorkloadConfiguration configuration,
        string[] parameterNames,
        string operationType,
        WorkloadValidationResult result)
    {
        foreach (var parameterName in parameterNames)
        {
            RequireParameter(configuration, parameterName, operationType, result);
        }
    }

    /// <summary>
    /// Validates that at least one of the specified parameters exists.
    /// </summary>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterNames">The names of parameters (at least one required).</param>
    /// <param name="operationType">The operation type for error message context.</param>
    /// <param name="result">The validation result to update.</param>
    public static void RequireAnyParameter(
        WorkloadConfiguration configuration,
        string[] parameterNames,
        string operationType,
        WorkloadValidationResult result)
    {
        var hasAny = parameterNames.Any(name => configuration.Parameters.ContainsKey(name));
        
        if (!hasAny)
        {
            result.Errors.Add($"At least one of the following parameters is required for {operationType} operation: {string.Join(", ", parameterNames)}");
            result.IsValid = false;
        }
    }
}

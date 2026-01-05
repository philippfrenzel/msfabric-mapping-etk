using System.Text.Json;

namespace FabricMappingService.Core.Workload;

/// <summary>
/// Helper class for extracting and converting workload configuration parameters.
/// </summary>
public static class ParameterHelper
{
    /// <summary>
    /// Gets a required parameter from the workload configuration.
    /// </summary>
    /// <typeparam name="T">The expected type of the parameter.</typeparam>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The parameter value.</returns>
    /// <exception cref="ArgumentException">Thrown when the parameter is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the parameter has an invalid type.</exception>
    public static T GetRequired<T>(WorkloadConfiguration configuration, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (!configuration.Parameters.TryGetValue(parameterName, out var value))
        {
            throw new ArgumentException($"Required parameter '{parameterName}' not found in configuration");
        }

        return ConvertParameter<T>(value, parameterName);
    }

    /// <summary>
    /// Gets an optional parameter from the workload configuration with a default value.
    /// </summary>
    /// <typeparam name="T">The expected type of the parameter.</typeparam>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="defaultValue">The default value if the parameter is not found.</param>
    /// <returns>The parameter value or the default value.</returns>
    public static T GetOptional<T>(
        WorkloadConfiguration configuration,
        string parameterName,
        T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (!configuration.Parameters.TryGetValue(parameterName, out var value))
        {
            return defaultValue;
        }

        try
        {
            return ConvertParameter<T>(value, parameterName);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts a parameter value to the specified type.
    /// </summary>
    private static T ConvertParameter<T>(object? value, string parameterName)
    {
        if (value is T typedValue)
        {
            return typedValue;
        }

        if (value is JsonElement jsonElement)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText())
                    ?? throw new InvalidOperationException($"Failed to deserialize parameter '{parameterName}' to type {typeof(T).Name}");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize parameter '{parameterName}': {ex.Message}", ex);
            }
        }

        throw new InvalidOperationException($"Parameter '{parameterName}' has invalid type. Expected {typeof(T).Name} but got {value?.GetType().Name ?? "null"}");
    }

    /// <summary>
    /// Deserializes a JSON string parameter to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterName">The name of the parameter containing JSON.</param>
    /// <param name="errorMessage">Custom error message if deserialization fails.</param>
    /// <returns>The deserialized object.</returns>
    public static T DeserializeRequired<T>(
        WorkloadConfiguration configuration,
        string parameterName,
        string? errorMessage = null)
    {
        var json = GetRequired<string>(configuration, parameterName);
        
        try
        {
            return JsonSerializer.Deserialize<T>(json)
                ?? throw new InvalidOperationException(errorMessage ?? $"Failed to deserialize {parameterName}");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(errorMessage ?? $"Failed to deserialize {parameterName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string parameter to the specified type with a default value.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="parameterName">The name of the parameter containing JSON.</param>
    /// <param name="defaultValue">The default value if deserialization fails.</param>
    /// <returns>The deserialized object or default value.</returns>
    public static T DeserializeOptional<T>(
        WorkloadConfiguration configuration,
        string parameterName,
        T defaultValue)
    {
        var json = GetOptional<string>(configuration, parameterName, string.Empty);
        
        if (string.IsNullOrWhiteSpace(json))
        {
            return defaultValue;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}

namespace FabricMappingService.Core.Converters;

/// <summary>
/// Interface for custom property converters.
/// Implement this interface to create custom conversion logic for property values.
/// </summary>
public interface IPropertyConverter
{
    /// <summary>
    /// Converts a value from source type to target type.
    /// </summary>
    /// <param name="sourceValue">The source value to convert.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <returns>The converted value.</returns>
    object? Convert(object? sourceValue, Type targetType);

    /// <summary>
    /// Determines whether this converter can convert between the specified types.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>True if conversion is supported; otherwise, false.</returns>
    bool CanConvert(Type sourceType, Type targetType);
}

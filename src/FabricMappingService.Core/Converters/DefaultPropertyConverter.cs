namespace FabricMappingService.Core.Converters;

/// <summary>
/// Default property converter that handles basic type conversions.
/// </summary>
public class DefaultPropertyConverter : IPropertyConverter
{
    /// <inheritdoc/>
    public object? Convert(object? sourceValue, Type targetType)
    {
        if (sourceValue == null)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        var sourceType = sourceValue.GetType();

        // Handle same type
        if (targetType.IsAssignableFrom(sourceType))
        {
            return sourceValue;
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        // Try System.Convert for basic types
        try
        {
            return System.Convert.ChangeType(sourceValue, targetType);
        }
        catch (InvalidCastException ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value of type '{sourceType.Name}' to type '{targetType.Name}': Invalid cast.", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value of type '{sourceType.Name}' to type '{targetType.Name}': Format error.", ex);
        }
        catch (OverflowException ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value of type '{sourceType.Name}' to type '{targetType.Name}': Value overflow.", ex);
        }
        catch (ArgumentNullException ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value of type '{sourceType.Name}' to type '{targetType.Name}': Argument null.", ex);
        }
    }

    /// <inheritdoc/>
    public bool CanConvert(Type sourceType, Type targetType)
    {
        if (targetType.IsAssignableFrom(sourceType))
        {
            return true;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Check if both types are primitives or have a TypeConverter
        return underlyingType.IsPrimitive || sourceType.IsPrimitive ||
               underlyingType == typeof(string) || sourceType == typeof(string) ||
               underlyingType == typeof(decimal) || sourceType == typeof(decimal) ||
               underlyingType == typeof(DateTime) || sourceType == typeof(DateTime);
    }
}

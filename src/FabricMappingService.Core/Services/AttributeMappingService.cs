using System.Reflection;
using FabricMappingService.Core.Attributes;
using FabricMappingService.Core.Converters;
using FabricMappingService.Core.Exceptions;
using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Implementation of the attribute-based mapping service.
/// This service uses reflection to map properties between objects based on custom attributes.
/// </summary>
public class AttributeMappingService : IAttributeMappingService
{
    private readonly MappingConfiguration _configuration;
    private readonly IPropertyConverter _defaultConverter;

    /// <summary>
    /// Initializes a new instance of the AttributeMappingService class.
    /// </summary>
    /// <param name="configuration">Optional configuration for mapping behavior.</param>
    public AttributeMappingService(MappingConfiguration? configuration = null)
    {
        _configuration = configuration ?? new MappingConfiguration();
        _defaultConverter = new DefaultPropertyConverter();
    }

    /// <inheritdoc/>
    public TTarget Map<TSource, TTarget>(TSource source) where TTarget : new()
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var target = new TTarget();
        MapToExisting(source, target);
        return target;
    }

    /// <inheritdoc/>
    public MappingResult<TTarget> MapWithResult<TSource, TTarget>(TSource source) where TTarget : new()
    {
        var result = new MappingResult<TTarget>
        {
            Success = true,
            Result = new TTarget()
        };

        if (source == null)
        {
            result.Success = false;
            result.Errors.Add("Source object is null");
            return result;
        }

        try
        {
            MapToExistingInternal(source, result.Result, result.Errors, result.Warnings, 0);
            result.Success = result.Errors.Count == 0;
            result.MappedPropertiesCount = CountMappedProperties<TSource, TTarget>();
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Mapping failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public void MapToExisting<TSource, TTarget>(TSource source, TTarget target)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var errors = new List<string>();
        var warnings = new List<string>();

        MapToExistingInternal(source, target, errors, warnings, 0);

        if (_configuration.ThrowOnError && errors.Count > 0)
        {
            throw new MappingException(
                $"Mapping failed with {errors.Count} error(s): {string.Join(", ", errors)}",
                typeof(TSource),
                typeof(TTarget));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> sources) where TTarget : new()
    {
        if (sources == null)
        {
            throw new ArgumentNullException(nameof(sources));
        }

        return sources.Select(source => Map<TSource, TTarget>(source)).ToList();
    }

    private void MapToExistingInternal<TSource, TTarget>(
        TSource source,
        TTarget target,
        List<string> errors,
        List<string> warnings,
        int depth)
    {
        if (depth > _configuration.MaxDepth)
        {
            warnings.Add($"Maximum mapping depth ({_configuration.MaxDepth}) exceeded");
            return;
        }

        var sourceType = typeof(TSource);
        var targetType = typeof(TTarget);

        // Get class-level mapping profile if exists
        var profileAttr = sourceType.GetCustomAttribute<MappingProfileAttribute>();
        var ignoreUnmapped = profileAttr?.IgnoreUnmapped ?? _configuration.IgnoreUnmapped;
        var caseSensitive = profileAttr?.CaseSensitive ?? _configuration.CaseSensitive;

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        foreach (var sourceProp in sourceProperties)
        {
            // Skip properties marked with IgnoreMapping
            if (sourceProp.GetCustomAttribute<IgnoreMappingAttribute>() != null)
            {
                continue;
            }

            // Check for MapTo attribute
            var mapToAttr = sourceProp.GetCustomAttribute<MapToAttribute>();

            PropertyInfo? targetProp = null;

            if (mapToAttr != null)
            {
                // Use attribute-specified target property name
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                targetProp = targetProperties.FirstOrDefault(p =>
                    string.Equals(p.Name, mapToAttr.TargetPropertyName, comparison));

                if (targetProp == null && !mapToAttr.Optional)
                {
                    errors.Add($"Target property '{mapToAttr.TargetPropertyName}' not found for source property '{sourceProp.Name}'");
                    continue;
                }
            }
            else if (!ignoreUnmapped)
            {
                // Try to match by name if no attribute and not ignoring unmapped
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                targetProp = targetProperties.FirstOrDefault(p =>
                    string.Equals(p.Name, sourceProp.Name, comparison));
            }

            if (targetProp == null)
            {
                continue; // Skip this property
            }

            try
            {
                var sourceValue = sourceProp.GetValue(source);

                // Skip null values if configured
                if (sourceValue == null && !_configuration.MapNullValues)
                {
                    continue;
                }

                // Get converter if specified
                IPropertyConverter? converter = null;
                if (mapToAttr?.ConverterType != null)
                {
                    converter = Activator.CreateInstance(mapToAttr.ConverterType) as IPropertyConverter;
                }

                converter ??= _defaultConverter;

                // Convert and set value
                var convertedValue = converter.Convert(sourceValue, targetProp.PropertyType);
                targetProp.SetValue(target, convertedValue);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error mapping property '{sourceProp.Name}' to '{targetProp.Name}': {ex.Message}";
                errors.Add(errorMsg);

                if (_configuration.ThrowOnError)
                {
                    throw new MappingException(errorMsg, sourceType, targetType, sourceProp.Name);
                }
            }
        }
    }

    private int CountMappedProperties<TSource, TTarget>()
    {
        var sourceType = typeof(TSource);
        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return sourceProperties.Count(p =>
            p.GetCustomAttribute<IgnoreMappingAttribute>() == null);
    }
}

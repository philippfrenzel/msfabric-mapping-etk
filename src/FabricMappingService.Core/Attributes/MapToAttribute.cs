namespace FabricMappingService.Core.Attributes;

/// <summary>
/// Attribute to specify the target property name for mapping.
/// This attribute is used to map a source property to a target property with a different name.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MapToAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the target property to map to.
    /// </summary>
    public string TargetPropertyName { get; }

    /// <summary>
    /// Gets or sets whether this mapping is optional (defaults to false).
    /// If true, missing target properties will not cause mapping errors.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Gets or sets a custom converter type for this property.
    /// The type must implement IPropertyConverter.
    /// </summary>
    public Type? ConverterType { get; set; }

    /// <summary>
    /// Initializes a new instance of the MapToAttribute class.
    /// </summary>
    /// <param name="targetPropertyName">The name of the target property to map to.</param>
    public MapToAttribute(string targetPropertyName)
    {
        if (string.IsNullOrWhiteSpace(targetPropertyName))
        {
            throw new ArgumentException("Target property name cannot be null or empty.", nameof(targetPropertyName));
        }

        TargetPropertyName = targetPropertyName;
    }
}

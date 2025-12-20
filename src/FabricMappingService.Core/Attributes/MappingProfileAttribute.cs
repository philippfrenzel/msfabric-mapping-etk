namespace FabricMappingService.Core.Attributes;

/// <summary>
/// Attribute to define a mapping profile at the class level.
/// This can be used to specify default mapping behavior for all properties in a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MappingProfileAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the profile name for this mapping configuration.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Gets or sets whether to ignore properties without explicit mapping attributes.
    /// If true, only properties with MapTo attribute will be mapped.
    /// </summary>
    public bool IgnoreUnmapped { get; set; }

    /// <summary>
    /// Gets or sets whether to perform case-sensitive property name matching.
    /// </summary>
    public bool CaseSensitive { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the MappingProfileAttribute class.
    /// </summary>
    public MappingProfileAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the MappingProfileAttribute class with a profile name.
    /// </summary>
    /// <param name="profileName">The name of the mapping profile.</param>
    public MappingProfileAttribute(string profileName)
    {
        ProfileName = profileName;
    }
}

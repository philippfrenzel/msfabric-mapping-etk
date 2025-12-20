namespace FabricMappingService.Core.Attributes;

/// <summary>
/// Attribute to mark a property to be ignored during mapping.
/// Properties with this attribute will be skipped by the mapping service.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class IgnoreMappingAttribute : Attribute
{
}

using FabricMappingService.Core.Attributes;

namespace FabricMappingService.Core.Examples;

/// <summary>
/// Example source model representing a customer data structure from a legacy system.
/// </summary>
[MappingProfile("CustomerMapping", IgnoreUnmapped = false)]
public class LegacyCustomerModel
{
    [MapTo("CustomerId")]
    public int Id { get; set; }

    [MapTo("FullName")]
    public string CustomerName { get; set; } = string.Empty;

    [MapTo("EmailAddress")]
    public string Email { get; set; } = string.Empty;

    [MapTo("PhoneNumber")]
    public string Phone { get; set; } = string.Empty;

    [MapTo("RegistrationDate")]
    public DateTime CreatedDate { get; set; }

    [MapTo("IsActive")]
    public bool Status { get; set; }

    [IgnoreMapping]
    public string InternalNotes { get; set; } = string.Empty;

    // This will be mapped by name matching since no MapTo attribute
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Example target model representing a modern customer data structure.
/// </summary>
public class ModernCustomerModel
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; }

    public bool IsActive { get; set; }

    public string Country { get; set; } = string.Empty;
}

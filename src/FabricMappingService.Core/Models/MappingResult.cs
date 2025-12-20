namespace FabricMappingService.Core.Models;

/// <summary>
/// Result of a mapping operation.
/// </summary>
/// <typeparam name="T">The type of the mapped object.</typeparam>
public class MappingResult<T>
{
    /// <summary>
    /// Gets or sets whether the mapping was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the mapped result object.
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred during mapping.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets any warnings that occurred during mapping.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of properties successfully mapped.
    /// </summary>
    public int MappedPropertiesCount { get; set; }
}

using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for the attribute-based mapping service.
/// </summary>
public interface IAttributeMappingService
{
    /// <summary>
    /// Maps a source object to a target object using attribute-based configuration.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <returns>The mapped target object.</returns>
    TTarget Map<TSource, TTarget>(TSource source) where TTarget : new();

    /// <summary>
    /// Maps a source object to a target object using attribute-based configuration
    /// and returns a detailed result including errors and warnings.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <returns>A MappingResult containing the mapped object and any errors or warnings.</returns>
    MappingResult<TTarget> MapWithResult<TSource, TTarget>(TSource source) where TTarget : new();

    /// <summary>
    /// Maps a source object to an existing target object instance.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="target">The existing target object to map to.</param>
    void MapToExisting<TSource, TTarget>(TSource source, TTarget target);

    /// <summary>
    /// Maps a collection of source objects to target objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the source objects.</typeparam>
    /// <typeparam name="TTarget">The type of the target objects.</typeparam>
    /// <param name="sources">The collection of source objects.</param>
    /// <returns>A collection of mapped target objects.</returns>
    IEnumerable<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> sources) where TTarget : new();
}

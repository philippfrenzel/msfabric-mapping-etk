namespace FabricMappingService.Core.Storage;

/// <summary>
/// Interface for lakehouse storage operations.
/// Provides a generic abstraction for reading and writing JSON data to a lakehouse.
/// Implementations can target local file system (development) or OneLake (production).
/// </summary>
public interface ILakehouseStorageProvider
{
    /// <summary>
    /// Reads JSON content from a specified path.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content to.</typeparam>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized object, or null if the file doesn't exist.</returns>
    Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Writes an object as JSON to a specified path.
    /// Creates the directory structure if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="path">The path to write the JSON file to.</param>
    /// <param name="data">The object to serialize and write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task WriteJsonAsync<T>(string path, T data, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the file was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all files in a directory matching a pattern.
    /// </summary>
    /// <param name="directoryPath">The directory path to search.</param>
    /// <param name="pattern">The file pattern to match (e.g., "*.json").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of file paths matching the pattern.</returns>
    Task<IEnumerable<string>> ListFilesAsync(string directoryPath, string pattern = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the specified directory exists.
    /// </summary>
    /// <param name="directoryPath">The directory path to create if it doesn't exist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task EnsureDirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);
}

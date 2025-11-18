using WorkflowAutomation.Core.Entities;

namespace WorkflowAutomation.Core.Interfaces;

/// <summary>
/// Repository interface for Credential entity with credential-specific operations.
/// </summary>
public interface ICredentialRepository : IRepository<Credential>
{
    /// <summary>
    /// Gets credentials by type asynchronously.
    /// </summary>
    /// <param name="type">The credential type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Credentials of the specified type</returns>
    Task<IEnumerable<Credential>> GetByTypeAsync(
        string type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets credentials created by a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Credentials created by the user</returns>
    Task<IEnumerable<Credential>> GetByCreatorAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets credential by name asynchronously.
    /// </summary>
    /// <param name="name">The credential name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Credential with the specified name</returns>
    Task<Credential?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a credential name already exists.
    /// </summary>
    /// <param name="name">The credential name</param>
    /// <param name="excludeId">Optional credential ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> NameExistsAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets credentials with pagination and filtering.
    /// </summary>
    /// <param name="type">Optional type filter</param>
    /// <param name="userId">Optional creator filter</param>
    /// <param name="searchTerm">Optional search term for name</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated and filtered credentials</returns>
    Task<(IEnumerable<Credential> Items, int TotalCount)> GetFilteredPagedAsync(
        string? type = null,
        Guid? userId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unique credential types asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unique credential types</returns>
    Task<IEnumerable<string>> GetCredentialTypesAsync(
        CancellationToken cancellationToken = default);
}

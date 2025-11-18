using WorkflowAutomation.Core.Entities;

namespace WorkflowAutomation.Core.Interfaces;

/// <summary>
/// Repository interface for AIConfiguration entity with AI configuration-specific operations.
/// </summary>
public interface IAIConfigurationRepository : IRepository<AIConfiguration>
{
    /// <summary>
    /// Gets AI configurations for a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI configurations for the user</returns>
    Task<IEnumerable<AIConfiguration>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active AI configurations for a user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active AI configurations for the user</returns>
    Task<IEnumerable<AIConfiguration>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default AI configuration for a user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Default AI configuration for the user</returns>
    Task<AIConfiguration?> GetDefaultByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets AI configurations by provider asynchronously.
    /// </summary>
    /// <param name="provider">The AI provider name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI configurations for the provider</returns>
    Task<IEnumerable<AIConfiguration>> GetByProviderAsync(
        string provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an AI configuration as default for a user asynchronously.
    /// This will unset any existing default for the user.
    /// </summary>
    /// <param name="configurationId">The AI configuration ID</param>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsDefaultAsync(
        Guid configurationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates an AI configuration asynchronously.
    /// </summary>
    /// <param name="configurationId">The AI configuration ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ActivateAsync(Guid configurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates an AI configuration asynchronously.
    /// </summary>
    /// <param name="configurationId">The AI configuration ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateAsync(Guid configurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unique AI providers asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unique provider names</returns>
    Task<IEnumerable<string>> GetProvidersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a display name already exists for a user.
    /// </summary>
    /// <param name="displayName">The display name</param>
    /// <param name="userId">The user ID</param>
    /// <param name="excludeId">Optional configuration ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> DisplayNameExistsAsync(
        string displayName,
        Guid userId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}

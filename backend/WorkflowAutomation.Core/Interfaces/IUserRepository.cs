using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Interfaces;

/// <summary>
/// Repository interface for User entity with user-specific operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by email asynchronously.
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User with the specified email</returns>
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by role asynchronously.
    /// </summary>
    /// <param name="role">The user role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Users with the specified role</returns>
    Task<IEnumerable<User>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active users asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All active users</returns>
    Task<IEnumerable<User>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user with all related data (workflows, credentials) asynchronously.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User with related data</returns>
    Task<User?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email already exists.
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="excludeId">Optional user ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists, false otherwise</returns>
    Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with pagination and filtering.
    /// </summary>
    /// <param name="role">Optional role filter</param>
    /// <param name="activeOnly">Filter for active users only</param>
    /// <param name="searchTerm">Optional search term for name/email</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated and filtered users</returns>
    Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPagedAsync(
        UserRole? role = null,
        bool? activeOnly = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user password hash asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="newPasswordHash">The new password hash</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdatePasswordAsync(
        Guid userId,
        string newPasswordHash,
        CancellationToken cancellationToken = default);
}

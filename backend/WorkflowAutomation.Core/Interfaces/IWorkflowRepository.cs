using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Interfaces;

/// <summary>
/// Repository interface for Workflow entity with workflow-specific operations.
/// </summary>
public interface IWorkflowRepository : IRepository<Workflow>
{
    /// <summary>
    /// Gets workflows by environment asynchronously.
    /// </summary>
    /// <param name="environment">The workflow environment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflows in the specified environment</returns>
    Task<IEnumerable<Workflow>> GetByEnvironmentAsync(
        WorkflowEnvironment environment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active workflows asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All active workflows</returns>
    Task<IEnumerable<Workflow>> GetActiveWorkflowsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflows by creator asynchronously.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflows created by the specified user</returns>
    Task<IEnumerable<Workflow>> GetByCreatorAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow with all related data (executions, versions) asynchronously.
    /// </summary>
    /// <param name="id">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflow with related data</returns>
    Task<Workflow?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflows with pagination and filtering.
    /// </summary>
    /// <param name="environment">Optional environment filter</param>
    /// <param name="activeOnly">Filter for active workflows only</param>
    /// <param name="userId">Optional creator filter</param>
    /// <param name="searchTerm">Optional search term for name/description</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated and filtered workflows</returns>
    Task<(IEnumerable<Workflow> Items, int TotalCount)> GetFilteredPagedAsync(
        WorkflowEnvironment? environment = null,
        bool? activeOnly = null,
        Guid? userId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow versions history asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflow versions ordered by version number descending</returns>
    Task<IEnumerable<WorkflowVersion>> GetVersionsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets derived workflows (promoted to other environments) asynchronously.
    /// </summary>
    /// <param name="workflowId">The parent workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Derived workflows</returns>
    Task<IEnumerable<Workflow>> GetDerivedWorkflowsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ActivateAsync(Guid workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateAsync(Guid workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new version of the workflow asynchronously.
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <param name="changedBy">User who made the changes</param>
    /// <param name="changeNotes">Optional change notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created workflow version</returns>
    Task<WorkflowVersion> CreateVersionAsync(
        Workflow workflow,
        string changedBy,
        string? changeNotes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a workflow name already exists in the same environment.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <param name="environment">The environment</param>
    /// <param name="excludeId">Optional workflow ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> NameExistsAsync(
        string name,
        WorkflowEnvironment environment,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}

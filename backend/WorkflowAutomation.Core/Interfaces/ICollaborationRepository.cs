using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Core.Interfaces;

/// <summary>
/// Repository interface for CollaborationSession entity with collaboration-specific operations.
/// </summary>
public interface ICollaborationRepository : IRepository<CollaborationSession>
{
    /// <summary>
    /// Gets collaboration sessions for a specific workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collaboration sessions for the workflow</returns>
    Task<IEnumerable<CollaborationSession>> GetByWorkflowIdAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active collaboration session for a workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active collaboration session if exists</returns>
    Task<CollaborationSession?> GetActiveSessionAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets collaboration session with all details asynchronously.
    /// </summary>
    /// <param name="id">The session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collaboration session with details</returns>
    Task<CollaborationSession?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a new collaboration session for a workflow asynchronously.
    /// If an active session exists, it will be returned.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The collaboration session</returns>
    Task<CollaborationSession> StartSessionAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a collaboration session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collaborator to a session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="collaborator">The collaborator information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddCollaboratorAsync(
        Guid sessionId,
        CollaboratorInfo collaborator,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a collaborator from a session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="userId">The user ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveCollaboratorAsync(
        Guid sessionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a change in the collaboration session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="change">The workflow change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordChangeAsync(
        Guid sessionId,
        WorkflowChange change,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets changes for a collaboration session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflow changes ordered by timestamp</returns>
    Task<IEnumerable<WorkflowChange>> GetChangesAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active collaborators for a workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active collaborators</returns>
    Task<IEnumerable<CollaboratorInfo>> GetActiveCollaboratorsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old ended sessions asynchronously.
    /// </summary>
    /// <param name="retentionDays">Number of days to retain ended sessions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of deleted sessions</returns>
    Task<int> CleanupOldSessionsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default);
}

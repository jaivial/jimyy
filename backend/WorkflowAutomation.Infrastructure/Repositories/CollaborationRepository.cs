using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CollaborationSession entity.
/// </summary>
public class CollaborationRepository : Repository<CollaborationSession>, ICollaborationRepository
{
    public CollaborationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CollaborationSession>> GetByWorkflowIdAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(s => s.WorkflowId == workflowId)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving collaboration sessions for workflow {workflowId}", ex);
        }
    }

    public async Task<CollaborationSession?> GetActiveSessionAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.WorkflowId == workflowId && s.EndedAt == null, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving active session for workflow {workflowId}", ex);
        }
    }

    public async Task<CollaborationSession?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(s => s.Workflow)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving collaboration session details for ID {id}", ex);
        }
    }

    public async Task<CollaborationSession> StartSessionAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if there's already an active session
            var existingSession = await GetActiveSessionAsync(workflowId, cancellationToken);
            if (existingSession != null)
            {
                return existingSession;
            }

            // Create new session
            var session = new CollaborationSession
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                StartedAt = DateTime.UtcNow,
                Collaborators = new List<CollaboratorInfo>(),
                Changes = new List<WorkflowChange>()
            };

            return await AddAsync(session, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error starting collaboration session for workflow {workflowId}", ex);
        }
    }

    public async Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new KeyNotFoundException($"Collaboration session with ID {sessionId} not found");
            }

            session.EndedAt = DateTime.UtcNow;
            await UpdateAsync(session, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error ending collaboration session {sessionId}", ex);
        }
    }

    public async Task AddCollaboratorAsync(
        Guid sessionId,
        CollaboratorInfo collaborator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new KeyNotFoundException($"Collaboration session with ID {sessionId} not found");
            }

            // Check if collaborator already exists
            var existingCollaborator = session.Collaborators
                .FirstOrDefault(c => c.UserId == collaborator.UserId);

            if (existingCollaborator == null)
            {
                session.Collaborators.Add(collaborator);
            }
            else
            {
                // Update existing collaborator info
                existingCollaborator.UserName = collaborator.UserName;
                existingCollaborator.Color = collaborator.Color;
                existingCollaborator.JoinedAt = collaborator.JoinedAt;
            }

            await UpdateAsync(session, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error adding collaborator to session {sessionId}", ex);
        }
    }

    public async Task RemoveCollaboratorAsync(
        Guid sessionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new KeyNotFoundException($"Collaboration session with ID {sessionId} not found");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException($"Invalid user ID format: {userId}", nameof(userId));
            }

            var collaborator = session.Collaborators.FirstOrDefault(c => c.UserId == userGuid);
            if (collaborator != null)
            {
                session.Collaborators.Remove(collaborator);
                await UpdateAsync(session, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not ArgumentException)
        {
            throw new InvalidOperationException($"Error removing collaborator from session {sessionId}", ex);
        }
    }

    public async Task RecordChangeAsync(
        Guid sessionId,
        WorkflowChange change,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new KeyNotFoundException($"Collaboration session with ID {sessionId} not found");
            }

            session.Changes.Add(change);
            await UpdateAsync(session, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error recording change in session {sessionId}", ex);
        }
    }

    public async Task<IEnumerable<WorkflowChange>> GetChangesAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return Enumerable.Empty<WorkflowChange>();
            }

            return session.Changes.OrderBy(c => c.Timestamp);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving changes for session {sessionId}", ex);
        }
    }

    public async Task<IEnumerable<CollaboratorInfo>> GetActiveCollaboratorsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSession = await GetActiveSessionAsync(workflowId, cancellationToken);
            if (activeSession == null)
            {
                return Enumerable.Empty<CollaboratorInfo>();
            }

            return activeSession.Collaborators;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving active collaborators for workflow {workflowId}", ex);
        }
    }

    public async Task<int> CleanupOldSessionsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var oldSessions = await _dbSet
                .Where(s => s.EndedAt != null && s.EndedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldSessions.Any())
            {
                _dbSet.RemoveRange(oldSessions);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return oldSessions.Count;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error cleaning up old sessions (retention: {retentionDays} days)", ex);
        }
    }
}

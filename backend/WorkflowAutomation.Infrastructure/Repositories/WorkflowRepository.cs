using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Workflow entity.
/// </summary>
public class WorkflowRepository : Repository<Workflow>, IWorkflowRepository
{
    public WorkflowRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Workflow>> GetByEnvironmentAsync(
        WorkflowEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(w => w.Environment == environment)
                .OrderByDescending(w => w.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving workflows for environment {environment}", ex);
        }
    }

    public async Task<IEnumerable<Workflow>> GetActiveWorkflowsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(w => w.Active)
                .OrderByDescending(w => w.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving active workflows", ex);
        }
    }

    public async Task<IEnumerable<Workflow>> GetByCreatorAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(w => w.CreatedBy == userId)
                .OrderByDescending(w => w.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving workflows for user {userId}", ex);
        }
    }

    public async Task<Workflow?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(w => w.Creator)
                .Include(w => w.Executions.OrderByDescending(e => e.StartedAt).Take(10))
                .Include(w => w.Versions.OrderByDescending(v => v.Version).Take(10))
                .Include(w => w.ParentWorkflow)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving workflow details for ID {id}", ex);
        }
    }

    public async Task<(IEnumerable<Workflow> Items, int TotalCount)> GetFilteredPagedAsync(
        WorkflowEnvironment? environment = null,
        bool? activeOnly = null,
        Guid? userId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _dbSet.AsQueryable();

            // Apply filters
            if (environment.HasValue)
            {
                query = query.Where(w => w.Environment == environment.Value);
            }

            if (activeOnly.HasValue)
            {
                query = query.Where(w => w.Active == activeOnly.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(w => w.CreatedBy == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(w =>
                    w.Name.ToLower().Contains(term) ||
                    (w.Description != null && w.Description.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(w => w.UpdatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving filtered paged workflows", ex);
        }
    }

    public async Task<IEnumerable<WorkflowVersion>> GetVersionsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<WorkflowVersion>()
                .Where(v => v.WorkflowId == workflowId)
                .OrderByDescending(v => v.Version)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving versions for workflow {workflowId}", ex);
        }
    }

    public async Task<IEnumerable<Workflow>> GetDerivedWorkflowsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(w => w.ParentWorkflowId == workflowId)
                .OrderBy(w => w.Environment)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving derived workflows for {workflowId}", ex);
        }
    }

    public async Task ActivateAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflow = await GetByIdAsync(workflowId, cancellationToken);
            if (workflow == null)
            {
                throw new KeyNotFoundException($"Workflow with ID {workflowId} not found");
            }

            workflow.Active = true;
            workflow.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(workflow, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error activating workflow {workflowId}", ex);
        }
    }

    public async Task DeactivateAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflow = await GetByIdAsync(workflowId, cancellationToken);
            if (workflow == null)
            {
                throw new KeyNotFoundException($"Workflow with ID {workflowId} not found");
            }

            workflow.Active = false;
            workflow.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(workflow, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error deactivating workflow {workflowId}", ex);
        }
    }

    public async Task<WorkflowVersion> CreateVersionAsync(
        Workflow workflow,
        string changedBy,
        string? changeNotes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var version = new WorkflowVersion
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Version = workflow.Version,
                WorkflowData = workflow.WorkflowData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(changedBy, out var userId) ? userId : Guid.Empty,
                ChangeNotes = changeNotes
            };

            await _context.Set<WorkflowVersion>().AddAsync(version, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return version;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating version for workflow {workflow.Id}", ex);
        }
    }

    public async Task<bool> NameExistsAsync(
        string name,
        WorkflowEnvironment environment,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(w =>
                w.Name.ToLower() == name.ToLower() &&
                w.Environment == environment);

            if (excludeId.HasValue)
            {
                query = query.Where(w => w.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking if workflow name '{name}' exists", ex);
        }
    }
}

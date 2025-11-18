using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for WorkflowExecution entity.
/// </summary>
public class ExecutionRepository : Repository<WorkflowExecution>, IExecutionRepository
{
    public ExecutionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving executions for workflow {workflowId}", ex);
        }
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByStatusAsync(
        ExecutionStatus status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving executions with status {status}", ex);
        }
    }

    public async Task<IEnumerable<WorkflowExecution>> GetRunningExecutionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(e => e.Status == ExecutionStatus.Running)
                .OrderBy(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving running executions", ex);
        }
    }

    public async Task<IEnumerable<WorkflowExecution>> GetFailedExecutionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(e => e.Status == ExecutionStatus.Error)
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving failed executions", ex);
        }
    }

    public async Task<WorkflowExecution?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(e => e.Workflow)
                .Include(e => e.NodeExecutions.OrderBy(ne => ne.StartedAt))
                .Include(e => e.Logs.OrderBy(l => l.Timestamp))
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving execution details for ID {id}", ex);
        }
    }

    public async Task<(IEnumerable<WorkflowExecution> Items, int TotalCount)> GetFilteredPagedAsync(
        Guid? workflowId = null,
        ExecutionStatus? status = null,
        WorkflowEnvironment? environment = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
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
            if (workflowId.HasValue)
            {
                query = query.Where(e => e.WorkflowId == workflowId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            if (environment.HasValue)
            {
                query = query.Where(e => e.Environment == environment.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.StartedAt <= endDate.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(e => e.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving filtered paged executions", ex);
        }
    }

    public async Task<IEnumerable<ExecutionLog>> GetLogsAsync(
        Guid executionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<ExecutionLog>()
                .Where(l => l.ExecutionId == executionId)
                .OrderBy(l => l.Timestamp)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving logs for execution {executionId}", ex);
        }
    }

    public async Task<IEnumerable<ExecutionLog>> GetLogsAsync(
        Guid executionId,
        LogLevel logLevel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<ExecutionLog>()
                .Where(l => l.ExecutionId == executionId && l.Level >= logLevel)
                .OrderBy(l => l.Timestamp)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving filtered logs for execution {executionId}", ex);
        }
    }

    public async Task<IEnumerable<NodeExecution>> GetNodeExecutionsAsync(
        Guid executionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<NodeExecution>()
                .Where(ne => ne.ExecutionId == executionId)
                .OrderBy(ne => ne.StartedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving node executions for execution {executionId}", ex);
        }
    }

    public async Task<IEnumerable<WorkflowExecution>> GetRecentExecutionsAsync(
        Guid workflowId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (count < 1) count = 10;
            if (count > 100) count = 100;

            return await _dbSet
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving recent executions for workflow {workflowId}", ex);
        }
    }

    public async Task<ExecutionStatistics> GetExecutionStatisticsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var executions = await _dbSet
                .Where(e => e.WorkflowId == workflowId)
                .ToListAsync(cancellationToken);

            if (!executions.Any())
            {
                return new ExecutionStatistics();
            }

            var total = executions.Count;
            var successful = executions.Count(e => e.Status == ExecutionStatus.Success);
            var failed = executions.Count(e => e.Status == ExecutionStatus.Error);
            var running = executions.Count(e => e.Status == ExecutionStatus.Running);
            var cancelled = executions.Count(e => e.Status == ExecutionStatus.Canceled);

            var completedExecutions = executions.Where(e => e.FinishedAt.HasValue).ToList();
            var avgDuration = completedExecutions.Any()
                ? completedExecutions.Average(e => e.TotalDurationMs)
                : 0;
            var minDuration = completedExecutions.Any()
                ? completedExecutions.Min(e => e.TotalDurationMs)
                : 0;
            var maxDuration = completedExecutions.Any()
                ? completedExecutions.Max(e => e.TotalDurationMs)
                : 0;

            return new ExecutionStatistics
            {
                TotalExecutions = total,
                SuccessfulExecutions = successful,
                FailedExecutions = failed,
                RunningExecutions = running,
                CancelledExecutions = cancelled,
                AverageDurationMs = avgDuration,
                MinDurationMs = minDuration,
                MaxDurationMs = maxDuration,
                SuccessRate = total > 0 ? (double)successful / total * 100 : 0,
                LastExecutionAt = executions.Max(e => e.StartedAt)
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calculating statistics for workflow {workflowId}", ex);
        }
    }

    public async Task<ExecutionStatistics> GetOverallStatisticsAsync(
        WorkflowEnvironment? environment = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsQueryable();

            if (environment.HasValue)
            {
                query = query.Where(e => e.Environment == environment.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.StartedAt <= endDate.Value);
            }

            var executions = await query.ToListAsync(cancellationToken);

            if (!executions.Any())
            {
                return new ExecutionStatistics();
            }

            var total = executions.Count;
            var successful = executions.Count(e => e.Status == ExecutionStatus.Success);
            var failed = executions.Count(e => e.Status == ExecutionStatus.Error);
            var running = executions.Count(e => e.Status == ExecutionStatus.Running);
            var cancelled = executions.Count(e => e.Status == ExecutionStatus.Canceled);

            var completedExecutions = executions.Where(e => e.FinishedAt.HasValue).ToList();
            var avgDuration = completedExecutions.Any()
                ? completedExecutions.Average(e => e.TotalDurationMs)
                : 0;
            var minDuration = completedExecutions.Any()
                ? completedExecutions.Min(e => e.TotalDurationMs)
                : 0;
            var maxDuration = completedExecutions.Any()
                ? completedExecutions.Max(e => e.TotalDurationMs)
                : 0;

            return new ExecutionStatistics
            {
                TotalExecutions = total,
                SuccessfulExecutions = successful,
                FailedExecutions = failed,
                RunningExecutions = running,
                CancelledExecutions = cancelled,
                AverageDurationMs = avgDuration,
                MinDurationMs = minDuration,
                MaxDurationMs = maxDuration,
                SuccessRate = total > 0 ? (double)successful / total * 100 : 0,
                LastExecutionAt = executions.Any() ? executions.Max(e => e.StartedAt) : (DateTime?)null
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating overall statistics", ex);
        }
    }

    public async Task<int> DeleteOldExecutionsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var oldExecutions = await _dbSet
                .Where(e => e.StartedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldExecutions.Any())
            {
                _dbSet.RemoveRange(oldExecutions);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return oldExecutions.Count;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting old executions (retention: {retentionDays} days)", ex);
        }
    }

    public async Task UpdateNodeExecutionAsync(
        NodeExecution nodeExecution,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Set<NodeExecution>().Update(nodeExecution);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating node execution {nodeExecution.Id}", ex);
        }
    }
}

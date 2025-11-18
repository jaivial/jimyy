using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Interfaces;

/// <summary>
/// Repository interface for WorkflowExecution entity with execution-specific operations.
/// </summary>
public interface IExecutionRepository : IRepository<WorkflowExecution>
{
    /// <summary>
    /// Gets executions for a specific workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Executions for the workflow</returns>
    Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets executions by status asynchronously.
    /// </summary>
    /// <param name="status">The execution status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Executions with the specified status</returns>
    Task<IEnumerable<WorkflowExecution>> GetByStatusAsync(
        ExecutionStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets running executions asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Currently running executions</returns>
    Task<IEnumerable<WorkflowExecution>> GetRunningExecutionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed executions asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Failed executions</returns>
    Task<IEnumerable<WorkflowExecution>> GetFailedExecutionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution with all related data (node executions, logs) asynchronously.
    /// </summary>
    /// <param name="id">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution with related data</returns>
    Task<WorkflowExecution?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets executions with pagination and filtering.
    /// </summary>
    /// <param name="workflowId">Optional workflow filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="environment">Optional environment filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated and filtered executions</returns>
    Task<(IEnumerable<WorkflowExecution> Items, int TotalCount)> GetFilteredPagedAsync(
        Guid? workflowId = null,
        ExecutionStatus? status = null,
        WorkflowEnvironment? environment = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution logs for a specific execution asynchronously.
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution logs ordered by timestamp</returns>
    Task<IEnumerable<ExecutionLog>> GetLogsAsync(
        Guid executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution logs filtered by log level asynchronously.
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="logLevel">The minimum log level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered execution logs</returns>
    Task<IEnumerable<ExecutionLog>> GetLogsAsync(
        Guid executionId,
        LogLevel logLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets node executions for a specific execution asynchronously.
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Node executions ordered by start time</returns>
    Task<IEnumerable<NodeExecution>> GetNodeExecutionsAsync(
        Guid executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent executions for a workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="count">Number of recent executions to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent executions</returns>
    Task<IEnumerable<WorkflowExecution>> GetRecentExecutionsAsync(
        Guid workflowId,
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution statistics for a workflow asynchronously.
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution statistics</returns>
    Task<ExecutionStatistics> GetExecutionStatisticsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overall execution statistics asynchronously.
    /// </summary>
    /// <param name="environment">Optional environment filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Overall execution statistics</returns>
    Task<ExecutionStatistics> GetOverallStatisticsAsync(
        WorkflowEnvironment? environment = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old executions based on retention policy asynchronously.
    /// </summary>
    /// <param name="retentionDays">Number of days to retain executions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of deleted executions</returns>
    Task<int> DeleteOldExecutionsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a node execution asynchronously.
    /// </summary>
    /// <param name="nodeExecution">The node execution to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateNodeExecutionAsync(
        NodeExecution nodeExecution,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Execution statistics data transfer object.
/// </summary>
public class ExecutionStatistics
{
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public int RunningExecutions { get; set; }
    public int CancelledExecutions { get; set; }
    public double AverageDurationMs { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public double SuccessRate { get; set; }
    public DateTime? LastExecutionAt { get; set; }
}

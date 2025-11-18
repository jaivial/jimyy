using System;
using System.Threading.Tasks;
using WorkflowAutomation.Core.Entities;

namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Execution hub interface for broadcasting real-time execution updates via SignalR
    /// </summary>
    public interface IExecutionHub
    {
        /// <summary>
        /// Broadcasts that a workflow execution has started
        /// </summary>
        /// <param name="execution">The workflow execution that started</param>
        Task BroadcastExecutionStarted(WorkflowExecution execution);

        /// <summary>
        /// Broadcasts that a workflow execution has completed
        /// </summary>
        /// <param name="execution">The workflow execution that completed</param>
        Task BroadcastExecutionCompleted(WorkflowExecution execution);

        /// <summary>
        /// Broadcasts that a node execution has started
        /// </summary>
        /// <param name="executionId">The workflow execution ID</param>
        /// <param name="nodeExecution">The node execution that started</param>
        Task BroadcastNodeExecutionStarted(Guid executionId, NodeExecution nodeExecution);

        /// <summary>
        /// Broadcasts that a node execution has completed
        /// </summary>
        /// <param name="executionId">The workflow execution ID</param>
        /// <param name="nodeExecution">The node execution that completed</param>
        Task BroadcastNodeExecutionCompleted(Guid executionId, NodeExecution nodeExecution);

        /// <summary>
        /// Broadcasts an execution log entry
        /// </summary>
        /// <param name="executionId">The workflow execution ID</param>
        /// <param name="log">The execution log entry</param>
        Task BroadcastExecutionLog(Guid executionId, ExecutionLog log);
    }
}

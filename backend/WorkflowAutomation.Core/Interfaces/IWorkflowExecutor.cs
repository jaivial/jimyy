using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkflowAutomation.Core.Entities;

namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Workflow executor interface for executing workflows and managing execution flow
    /// </summary>
    public interface IWorkflowExecutor
    {
        /// <summary>
        /// Executes a workflow asynchronously
        /// </summary>
        /// <param name="workflow">The workflow to execute</param>
        /// <param name="triggerData">Data from the trigger that initiated the execution</param>
        /// <param name="cancellationToken">Cancellation token for aborting execution</param>
        /// <returns>The workflow execution result with logs and status</returns>
        Task<WorkflowExecution> ExecuteAsync(
            Workflow workflow,
            Dictionary<string, object> triggerData,
            CancellationToken cancellationToken = default);
    }
}

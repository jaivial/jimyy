using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Node executor interface for executing individual nodes
    /// This will be implemented in Phase 8: Node System - Base Infrastructure
    /// </summary>
    public interface INodeExecutor
    {
        /// <summary>
        /// Executes a node with the given parameters
        /// </summary>
        /// <param name="parameters">The node parameters</param>
        /// <param name="context">The execution context containing workflow state and data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the node execution</returns>
        Task<object> ExecuteAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken = default);
    }
}

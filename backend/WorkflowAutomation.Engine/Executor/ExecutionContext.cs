using System.Collections.Generic;
using WorkflowAutomation.Core.Entities;

namespace WorkflowAutomation.Engine.Executor
{
    /// <summary>
    /// Execution context that holds workflow state and data during execution
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// The workflow being executed
        /// </summary>
        public Workflow Workflow { get; set; } = null!;

        /// <summary>
        /// The current workflow execution instance
        /// </summary>
        public WorkflowExecution Execution { get; set; } = null!;

        /// <summary>
        /// Data accumulated during execution (trigger data, node results, variables)
        /// Key is either "trigger" or a node ID, value is the data/result
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = null!;
    }
}

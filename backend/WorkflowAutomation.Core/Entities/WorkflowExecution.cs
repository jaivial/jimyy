using System;
using System.Collections.Generic;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Entities
{
    public class WorkflowExecution
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? TriggerMode { get; set; }
        public string? ErrorMessage { get; set; }
        public WorkflowEnvironment Environment { get; set; }

        // Execution metadata
        public Dictionary<string, object> TriggerData { get; set; } = new();
        public string? ExecutionPath { get; set; } // JSON array of node IDs in execution order

        // Execution data (MongoDB for large data)
        public string? ExecutionDataId { get; set; }

        // Performance metrics
        public long TotalDurationMs { get; set; }
        public int NodesExecuted { get; set; }
        public int NodesSkipped { get; set; }
        public int NodesFailed { get; set; }

        // Navigation
        public virtual Workflow? Workflow { get; set; }
        public virtual List<NodeExecution> NodeExecutions { get; set; } = new();
        public virtual List<ExecutionLog> Logs { get; set; } = new();
    }
}

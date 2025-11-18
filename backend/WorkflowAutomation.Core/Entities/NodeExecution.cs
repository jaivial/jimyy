using System;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Core.Entities
{
    public class NodeExecution
    {
        public Guid Id { get; set; }
        public Guid ExecutionId { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public ExecutionStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? InputData { get; set; }
        public string? OutputData { get; set; }
        public string? ErrorMessage { get; set; }
        public int ExecutionOrder { get; set; }
        public int RetryCount { get; set; }
        public long DurationMs { get; set; }

        // Visual tracking
        public Position? NodePosition { get; set; }

        // Navigation
        public virtual WorkflowExecution? Execution { get; set; }
    }
}

using System;
using System.Collections.Generic;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Entities
{
    public class ExecutionLog
    {
        public Guid Id { get; set; }
        public Guid ExecutionId { get; set; }
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NodeId { get; set; }
        public string? NodeName { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        // Navigation
        public virtual WorkflowExecution? Execution { get; set; }
    }
}

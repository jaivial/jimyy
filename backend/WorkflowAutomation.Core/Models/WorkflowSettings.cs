using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Models
{
    public class WorkflowSettings
    {
        public int ErrorWorkflowId { get; set; }
        public int TimeoutMinutes { get; set; }
        public string? Timezone { get; set; }
        public bool SaveExecutionProgress { get; set; }
        public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Sequential;
        public int MaxConcurrentExecutions { get; set; } = 5;
    }
}

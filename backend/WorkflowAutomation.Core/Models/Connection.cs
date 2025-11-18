using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Models
{
    public class Connection
    {
        public string SourceNodeId { get; set; } = string.Empty;
        public string TargetNodeId { get; set; } = string.Empty;
        public string SourceOutput { get; set; } = string.Empty;
        public string TargetInput { get; set; } = string.Empty;
        public ConnectionType Type { get; set; } = ConnectionType.Main;
    }
}

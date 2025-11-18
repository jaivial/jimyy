using System.Collections.Generic;

namespace WorkflowAutomation.Core.Models
{
    public class WorkflowDefinition
    {
        public List<Node> Nodes { get; set; } = new();
        public List<Connection> Connections { get; set; } = new();
        public Dictionary<string, object> Variables { get; set; } = new();
    }
}

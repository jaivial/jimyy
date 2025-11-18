using System.Collections.Generic;

namespace WorkflowAutomation.Core.Models
{
    public class Node
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Position Position { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
        public List<string> Credentials { get; set; } = new();
        public NodeStyle Style { get; set; } = new();
        public string? Notes { get; set; }
        public bool Disabled { get; set; }
        public RetrySettings? RetrySettings { get; set; }
    }
}

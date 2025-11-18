using System;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Models
{
    public class WorkflowChange
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public ChangeType Type { get; set; }
        public string Path { get; set; } = string.Empty; // JSON path to changed element
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}

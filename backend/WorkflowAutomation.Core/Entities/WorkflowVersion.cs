using System;

namespace WorkflowAutomation.Core.Entities
{
    public class WorkflowVersion
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public int Version { get; set; }
        public string WorkflowData { get; set; } = string.Empty; // JSON
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string? ChangeNotes { get; set; }

        // Navigation properties
        public virtual Workflow? Workflow { get; set; }
        public virtual User? Creator { get; set; }
    }
}

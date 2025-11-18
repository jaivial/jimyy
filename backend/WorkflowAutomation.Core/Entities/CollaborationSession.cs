using System;
using System.Collections.Generic;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Core.Entities
{
    public class CollaborationSession
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<CollaboratorInfo> Collaborators { get; set; } = new();
        public List<WorkflowChange> Changes { get; set; } = new();

        public virtual Workflow? Workflow { get; set; }
    }
}

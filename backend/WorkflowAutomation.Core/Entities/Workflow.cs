using System;
using System.Collections.Generic;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Core.Entities
{
    public class Workflow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        // Environment
        public WorkflowEnvironment Environment { get; set; } = WorkflowEnvironment.Testing;

        // Version tracking
        public int Version { get; set; }
        public Guid? ParentWorkflowId { get; set; } // For environment promotion

        // JSON structure of the workflow
        public string WorkflowData { get; set; } = string.Empty;

        // Parsed workflow structure
        public WorkflowDefinition? Definition { get; set; }

        // Settings
        public WorkflowSettings? Settings { get; set; }

        // Collaboration
        public List<string> ActiveEditors { get; set; } = new();
        public DateTime? LastEditedAt { get; set; }
        public string? LastEditedBy { get; set; }

        // Navigation properties
        public virtual User? Creator { get; set; }
        public virtual ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
        public virtual ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
        public virtual Workflow? ParentWorkflow { get; set; }
        public virtual ICollection<Workflow> DerivedWorkflows { get; set; } = new List<Workflow>();
    }
}

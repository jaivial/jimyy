using System;
using System.Collections.Generic;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public UserRole Role { get; set; } = UserRole.Developer;

        // Navigation properties
        public virtual ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
        public virtual ICollection<Credential> Credentials { get; set; } = new List<Credential>();
        public virtual ICollection<AIConfiguration> AIConfigurations { get; set; } = new List<AIConfiguration>();
    }
}

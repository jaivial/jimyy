using System;

namespace WorkflowAutomation.Core.Entities
{
    public class Credential
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string EncryptedData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public virtual User? Creator { get; set; }
    }
}

using System;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Core.Entities
{
    public class AIConfiguration
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Provider { get; set; } = string.Empty; // OpenAI, Anthropic, Custom
        public string ModelName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EncryptedApiKey { get; set; } = string.Empty;
        public string? ApiEndpoint { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public AIModelCapabilities? Capabilities { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User? User { get; set; }
    }
}

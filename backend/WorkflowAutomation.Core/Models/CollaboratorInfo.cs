using System;

namespace WorkflowAutomation.Core.Models
{
    public class CollaboratorInfo
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string? CursorPosition { get; set; }
        public string? Color { get; set; }
    }
}

namespace WorkflowAutomation.Core.Models
{
    public class AIModelCapabilities
    {
        public bool SupportsStreaming { get; set; }
        public bool SupportsVision { get; set; }
        public bool SupportsFunctionCalling { get; set; }
        public int MaxTokens { get; set; }
        public int ContextWindow { get; set; }
    }
}

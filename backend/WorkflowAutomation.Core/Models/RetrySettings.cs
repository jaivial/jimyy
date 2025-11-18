namespace WorkflowAutomation.Core.Models
{
    public class RetrySettings
    {
        public bool Enabled { get; set; }
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
        public bool ExponentialBackoff { get; set; }
    }
}

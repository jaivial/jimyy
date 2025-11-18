namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Defines the capabilities and features supported by a node
    /// </summary>
    public class NodeCapabilities
    {
        /// <summary>
        /// Whether the node supports automatic retry on failure
        /// </summary>
        public bool SupportsRetry { get; set; } = true;

        /// <summary>
        /// Whether the node supports streaming data output
        /// </summary>
        public bool SupportsStreaming { get; set; } = false;

        /// <summary>
        /// Whether the node supports batch processing of items
        /// </summary>
        public bool SupportsBatching { get; set; } = false;

        /// <summary>
        /// Whether the node can be triggered by a webhook
        /// </summary>
        public bool SupportsWebhook { get; set; } = false;

        /// <summary>
        /// Whether the node can be used as a workflow trigger
        /// </summary>
        public bool IsTrigger { get; set; } = false;

        /// <summary>
        /// Maximum execution timeout in seconds (null for no limit)
        /// </summary>
        public int? MaxExecutionTimeSeconds { get; set; }
    }
}

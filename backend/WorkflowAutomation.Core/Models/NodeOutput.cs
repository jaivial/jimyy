namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Defines an output that a node produces
    /// </summary>
    public class NodeOutput
    {
        /// <summary>
        /// Output name (e.g., "main", "error", "true", "false")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Output type for documentation purposes
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Description of the output
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Display name for the output in the UI
        /// </summary>
        public string DisplayName { get; set; }
    }
}

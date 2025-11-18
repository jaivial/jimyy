using System.Collections.Generic;

namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Defines the metadata and schema for a node type
    /// </summary>
    public class NodeDefinition
    {
        /// <summary>
        /// Unique type identifier for the node (e.g., "httpRequest", "if", "set")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Display name shown in the UI
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of what the node does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category for grouping nodes (e.g., "triggers", "actions", "data", "logic")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Icon name or class for UI display
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Color hex code for node styling
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// List of parameters the node accepts
        /// </summary>
        public List<NodeParameter> Parameters { get; set; } = new();

        /// <summary>
        /// List of outputs the node produces
        /// </summary>
        public List<NodeOutput> Outputs { get; set; } = new();

        /// <summary>
        /// List of required credential types
        /// </summary>
        public List<string> RequiredCredentials { get; set; } = new();

        /// <summary>
        /// Capabilities and features supported by this node
        /// </summary>
        public NodeCapabilities Capabilities { get; set; } = new();
    }
}

namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Display options for conditional parameter visibility
    /// </summary>
    public class ParameterDisplayOptions
    {
        /// <summary>
        /// Show this parameter only if condition is met
        /// Format: { "parameterName": "value" }
        /// </summary>
        public object ShowIf { get; set; }

        /// <summary>
        /// Hide this parameter if condition is met
        /// </summary>
        public object HideIf { get; set; }
    }
}

using System.Collections.Generic;

namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Defines a parameter that a node accepts
    /// </summary>
    public class NodeParameter
    {
        /// <summary>
        /// Internal parameter name used in code
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name shown in the UI
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Parameter type: string, number, boolean, json, select, code, multiselect
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Whether this parameter is required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Default value for the parameter
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Description of the parameter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Placeholder text for input fields
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Options for select/multiselect type parameters
        /// </summary>
        public List<ParameterOption> Options { get; set; } = new();

        /// <summary>
        /// Validation rules for the parameter
        /// </summary>
        public ParameterValidation Validation { get; set; }

        /// <summary>
        /// Whether expressions like {{ $node.data }} are supported
        /// </summary>
        public bool SupportsExpressions { get; set; } = true;

        /// <summary>
        /// Display options for conditional parameter visibility
        /// </summary>
        public ParameterDisplayOptions DisplayOptions { get; set; }
    }
}

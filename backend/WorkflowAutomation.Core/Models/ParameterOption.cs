namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Option for select/multiselect parameter types
    /// </summary>
    public class ParameterOption
    {
        /// <summary>
        /// Display name for the option
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Actual value of the option
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Description of the option
        /// </summary>
        public string Description { get; set; }
    }
}

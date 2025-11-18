namespace WorkflowAutomation.Core.Models
{
    /// <summary>
    /// Validation rules for node parameters
    /// </summary>
    public class ParameterValidation
    {
        /// <summary>
        /// Minimum length for string parameters
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Maximum length for string parameters
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Minimum value for numeric parameters
        /// </summary>
        public double? Min { get; set; }

        /// <summary>
        /// Maximum value for numeric parameters
        /// </summary>
        public double? Max { get; set; }

        /// <summary>
        /// Regex pattern for string validation
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Custom validation error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

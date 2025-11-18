using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Utilities
{
    /// <summary>
    /// Utility class for validating node parameters
    /// </summary>
    public static class ParameterValidator
    {
        /// <summary>
        /// Validates all parameters against their definitions
        /// </summary>
        public static ValidationResult ValidateParameters(
            Dictionary<string, object> parameters,
            List<NodeParameter> parameterDefinitions)
        {
            var result = new ValidationResult { IsValid = true };

            if (parameterDefinitions == null || parameterDefinitions.Count == 0)
                return result;

            foreach (var paramDef in parameterDefinitions)
            {
                // Check required parameters
                if (paramDef.Required && !HasValue(parameters, paramDef.Name))
                {
                    result.AddError(paramDef.Name, $"Required parameter '{paramDef.DisplayName ?? paramDef.Name}' is missing");
                    continue;
                }

                // Skip validation if parameter is not provided and not required
                if (!HasValue(parameters, paramDef.Name))
                    continue;

                var value = parameters[paramDef.Name];

                // Validate type
                if (!ValidateType(value, paramDef.Type, out var typeError))
                {
                    result.AddError(paramDef.Name, typeError);
                    continue;
                }

                // Validate rules
                if (paramDef.Validation != null)
                {
                    ValidateRules(paramDef, value, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates a single parameter
        /// </summary>
        public static ValidationResult ValidateParameter(
            string name,
            object value,
            NodeParameter parameterDefinition)
        {
            var result = new ValidationResult { IsValid = true };

            if (parameterDefinition == null)
                return result;

            // Check required
            if (parameterDefinition.Required && value == null)
            {
                result.AddError(name, $"Required parameter '{parameterDefinition.DisplayName ?? name}' is missing");
                return result;
            }

            if (value == null)
                return result;

            // Validate type
            if (!ValidateType(value, parameterDefinition.Type, out var typeError))
            {
                result.AddError(name, typeError);
                return result;
            }

            // Validate rules
            if (parameterDefinition.Validation != null)
            {
                ValidateRules(parameterDefinition, value, result);
            }

            return result;
        }

        private static bool HasValue(Dictionary<string, object> parameters, string name)
        {
            return parameters != null && parameters.ContainsKey(name) && parameters[name] != null;
        }

        private static bool ValidateType(object value, string expectedType, out string error)
        {
            error = null;

            if (value == null)
                return true;

            switch (expectedType?.ToLowerInvariant())
            {
                case "string":
                    if (!(value is string))
                    {
                        error = "Expected string value";
                        return false;
                    }
                    break;

                case "number":
                    if (!IsNumeric(value))
                    {
                        error = "Expected numeric value";
                        return false;
                    }
                    break;

                case "boolean":
                    if (!(value is bool))
                    {
                        error = "Expected boolean value";
                        return false;
                    }
                    break;

                case "json":
                case "object":
                    // Accept any object type for JSON
                    break;

                case "array":
                    if (!IsArray(value))
                    {
                        error = "Expected array value";
                        return false;
                    }
                    break;

                default:
                    // Unknown type, skip validation
                    break;
            }

            return true;
        }

        private static void ValidateRules(NodeParameter paramDef, object value, ValidationResult result)
        {
            var validation = paramDef.Validation;

            // String validation
            if (paramDef.Type == "string" && value is string strValue)
            {
                if (validation.MinLength.HasValue && strValue.Length < validation.MinLength.Value)
                {
                    result.AddError(paramDef.Name,
                        validation.ErrorMessage ?? $"Must be at least {validation.MinLength} characters");
                }

                if (validation.MaxLength.HasValue && strValue.Length > validation.MaxLength.Value)
                {
                    result.AddError(paramDef.Name,
                        validation.ErrorMessage ?? $"Must not exceed {validation.MaxLength} characters");
                }

                if (!string.IsNullOrWhiteSpace(validation.Pattern))
                {
                    try
                    {
                        if (!Regex.IsMatch(strValue, validation.Pattern))
                        {
                            result.AddError(paramDef.Name,
                                validation.ErrorMessage ?? "Value does not match required pattern");
                        }
                    }
                    catch (Exception)
                    {
                        // Invalid regex pattern - skip validation
                    }
                }
            }

            // Numeric validation
            if (paramDef.Type == "number" && IsNumeric(value))
            {
                var numValue = Convert.ToDouble(value);

                if (validation.Min.HasValue && numValue < validation.Min.Value)
                {
                    result.AddError(paramDef.Name,
                        validation.ErrorMessage ?? $"Must be at least {validation.Min}");
                }

                if (validation.Max.HasValue && numValue > validation.Max.Value)
                {
                    result.AddError(paramDef.Name,
                        validation.ErrorMessage ?? $"Must not exceed {validation.Max}");
                }
            }
        }

        private static bool IsNumeric(object value)
        {
            return value is int || value is long || value is float || value is double || value is decimal ||
                   value is short || value is byte || value is uint || value is ulong || value is ushort;
        }

        private static bool IsArray(object value)
        {
            return value is Array || value is System.Collections.IEnumerable && !(value is string);
        }
    }

    /// <summary>
    /// Validation result containing errors by parameter name
    /// </summary>
    public class ValidationResult
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public bool IsValid { get; set; }

        public IReadOnlyDictionary<string, List<string>> Errors => _errors;

        public void AddError(string parameterName, string message)
        {
            IsValid = false;

            if (!_errors.ContainsKey(parameterName))
            {
                _errors[parameterName] = new List<string>();
            }

            _errors[parameterName].Add(message);
        }

        public IEnumerable<string> GetAllErrors()
        {
            return _errors.Values.SelectMany(e => e);
        }

        public string GetErrorSummary()
        {
            return string.Join("; ", GetAllErrors());
        }
    }
}

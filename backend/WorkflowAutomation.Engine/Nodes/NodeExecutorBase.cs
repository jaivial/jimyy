using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Base class for node executors providing common functionality
    /// </summary>
    public abstract class NodeExecutorBase : INodeExecutor
    {
        protected readonly ILogger Logger;

        protected NodeExecutorBase(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// The unique type identifier for this node
        /// </summary>
        public abstract string NodeType { get; }

        /// <summary>
        /// Gets the node definition with metadata
        /// </summary>
        public abstract NodeDefinition GetDefinition();

        /// <summary>
        /// Executes the node with the given parameters and context
        /// </summary>
        public async Task<object> ExecuteAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("Executing node {NodeType} with {ParameterCount} parameters",
                    NodeType, parameters?.Count ?? 0);

                // Validate parameters before execution
                ValidateParameters(parameters);

                // Execute the node-specific logic
                var result = await ExecuteInternalAsync(parameters, context, cancellationToken);

                Logger.LogDebug("Node {NodeType} executed successfully", NodeType);

                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Node {NodeType} execution was cancelled", NodeType);
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing node {NodeType}", NodeType);
                throw;
            }
        }

        /// <summary>
        /// Internal execution method to be implemented by derived classes
        /// </summary>
        protected abstract Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken);

        /// <summary>
        /// Validates the provided parameters against the node definition
        /// </summary>
        protected virtual void ValidateParameters(Dictionary<string, object> parameters)
        {
            var definition = GetDefinition();
            if (definition?.Parameters == null || definition.Parameters.Count == 0)
                return;

            var errors = new List<string>();

            foreach (var paramDef in definition.Parameters)
            {
                // Check required parameters
                if (paramDef.Required && !parameters.ContainsKey(paramDef.Name))
                {
                    errors.Add($"Required parameter '{paramDef.Name}' is missing");
                    continue;
                }

                // Skip validation if parameter is not provided and not required
                if (!parameters.ContainsKey(paramDef.Name))
                    continue;

                var value = parameters[paramDef.Name];

                // Validate based on parameter validation rules
                if (paramDef.Validation != null)
                {
                    ValidateParameterValue(paramDef, value, errors);
                }
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameter validation failed for node {NodeType}: {string.Join(", ", errors)}");
            }
        }

        /// <summary>
        /// Validates a single parameter value
        /// </summary>
        private void ValidateParameterValue(NodeParameter paramDef, object value, List<string> errors)
        {
            var validation = paramDef.Validation;

            if (value == null)
                return;

            // String validation
            if (paramDef.Type == "string" && value is string strValue)
            {
                if (validation.MinLength.HasValue && strValue.Length < validation.MinLength.Value)
                {
                    errors.Add($"Parameter '{paramDef.Name}' must be at least {validation.MinLength} characters");
                }

                if (validation.MaxLength.HasValue && strValue.Length > validation.MaxLength.Value)
                {
                    errors.Add($"Parameter '{paramDef.Name}' must not exceed {validation.MaxLength} characters");
                }

                if (!string.IsNullOrWhiteSpace(validation.Pattern))
                {
                    try
                    {
                        if (!Regex.IsMatch(strValue, validation.Pattern))
                        {
                            var errorMsg = !string.IsNullOrWhiteSpace(validation.ErrorMessage)
                                ? validation.ErrorMessage
                                : $"Parameter '{paramDef.Name}' does not match required pattern";
                            errors.Add(errorMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Invalid regex pattern for parameter {ParameterName}", paramDef.Name);
                    }
                }
            }

            // Numeric validation
            if (paramDef.Type == "number" && IsNumeric(value))
            {
                var numValue = Convert.ToDouble(value);

                if (validation.Min.HasValue && numValue < validation.Min.Value)
                {
                    errors.Add($"Parameter '{paramDef.Name}' must be at least {validation.Min}");
                }

                if (validation.Max.HasValue && numValue > validation.Max.Value)
                {
                    errors.Add($"Parameter '{paramDef.Name}' must not exceed {validation.Max}");
                }
            }
        }

        /// <summary>
        /// Checks if a value is numeric
        /// </summary>
        private bool IsNumeric(object value)
        {
            return value is int || value is long || value is float || value is double || value is decimal;
        }

        /// <summary>
        /// Gets a parameter value with a default fallback
        /// </summary>
        protected T GetParameter<T>(Dictionary<string, object> parameters, string name, T defaultValue = default)
        {
            if (parameters == null || !parameters.ContainsKey(name))
                return defaultValue;

            var value = parameters[name];

            if (value == null)
                return defaultValue;

            try
            {
                if (value is T typedValue)
                    return typedValue;

                // Try to convert
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Logger.LogWarning("Failed to convert parameter {Name} to type {Type}, using default",
                    name, typeof(T).Name);
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a required parameter value or throws an exception
        /// </summary>
        protected T GetRequiredParameter<T>(Dictionary<string, object> parameters, string name)
        {
            if (parameters == null || !parameters.ContainsKey(name))
            {
                throw new ArgumentException($"Required parameter '{name}' is missing");
            }

            var value = parameters[name];

            if (value == null)
            {
                throw new ArgumentException($"Required parameter '{name}' cannot be null");
            }

            try
            {
                if (value is T typedValue)
                    return typedValue;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"Failed to convert parameter '{name}' to type {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Creates a simple success result
        /// </summary>
        protected object CreateSuccessResult(object data = null)
        {
            return new
            {
                success = true,
                data
            };
        }

        /// <summary>
        /// Creates an error result
        /// </summary>
        protected object CreateErrorResult(string message, Exception exception = null)
        {
            return new
            {
                success = false,
                error = message,
                details = exception?.Message
            };
        }
    }
}

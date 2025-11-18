using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Set variables and transform data node
    /// </summary>
    public class SetNode : NodeExecutorBase
    {
        private readonly IExpressionEvaluator _expressionEvaluator;

        public SetNode(
            ILogger<SetNode> logger,
            IExpressionEvaluator expressionEvaluator) : base(logger)
        {
            _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        }

        public override string NodeType => "set";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Set",
                Description = "Set variables and transform data",
                Category = NodeCategories.Data,
                Icon = "edit",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "values",
                        DisplayName = "Values",
                        Type = "collection",
                        Required = true,
                        Description = "Key-value pairs to set"
                    },
                    new NodeParameter
                    {
                        Name = "keepOnlySet",
                        DisplayName = "Keep Only Set Values",
                        Type = "boolean",
                        Required = false,
                        DefaultValue = false,
                        Description = "If true, only set values are kept. If false, values are merged with input"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "data",
                        Type = "object",
                        Description = "The set values"
                    }
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var values = GetParameter<List<Dictionary<string, object>>>(parameters, "values", new List<Dictionary<string, object>>());
            var keepOnlySet = GetParameter<bool>(parameters, "keepOnlySet", false);

            Logger.LogDebug("SetNode: Setting {Count} values (keepOnlySet: {KeepOnlySet})", values.Count, keepOnlySet);

            try
            {
                var result = new Dictionary<string, object>();

                // If not keepOnlySet, start with existing context data
                if (!keepOnlySet && context is Dictionary<string, object> contextDict)
                {
                    foreach (var kvp in contextDict)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }

                // Set each value
                foreach (var valueItem in values)
                {
                    if (!valueItem.TryGetValue("name", out var nameObj) || nameObj == null)
                        continue;

                    var name = nameObj.ToString();

                    if (!valueItem.TryGetValue("value", out var value))
                        continue;

                    // If value is a string starting with {{, treat as expression
                    if (value is string strValue && strValue.Trim().StartsWith("{{"))
                    {
                        try
                        {
                            var evalContext = context as Dictionary<string, object> ?? new Dictionary<string, object>();
                            value = _expressionEvaluator.Evaluate(strValue, evalContext);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "SetNode: Failed to evaluate expression for {Name}", name);
                            // Keep the original value if expression evaluation fails
                        }
                    }

                    result[name] = value;

                    Logger.LogDebug("SetNode: Set {Name} = {Value}", name, value);
                }

                return Task.FromResult(CreateSuccessResult(result));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SetNode: Error setting values");
                return Task.FromResult(CreateErrorResult($"Failed to set values: {ex.Message}", ex));
            }
        }
    }
}

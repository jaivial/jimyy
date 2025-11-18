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
    /// Multi-way branching node based on value matching
    /// </summary>
    public class SwitchNode : NodeExecutorBase
    {
        private readonly IExpressionEvaluator _expressionEvaluator;

        public SwitchNode(
            ILogger<SwitchNode> logger,
            IExpressionEvaluator expressionEvaluator) : base(logger)
        {
            _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        }

        public override string NodeType => "switch";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Switch",
                Description = "Multi-way branching based on value matching",
                Category = NodeCategories.Logic,
                Icon = "random",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "value",
                        DisplayName = "Value",
                        Type = "string",
                        Required = true,
                        Description = "Expression to evaluate and match against cases",
                        Placeholder = "{{ $json.status }}"
                    },
                    new NodeParameter
                    {
                        Name = "cases",
                        DisplayName = "Cases",
                        Type = "collection",
                        Required = true,
                        Description = "List of case values to match against"
                    },
                    new NodeParameter
                    {
                        Name = "fallbackOutput",
                        DisplayName = "Fallback Output",
                        Type = "number",
                        Required = false,
                        DefaultValue = -1,
                        Description = "Output index for default case (-1 for no fallback)"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "output",
                        Type = "object",
                        Description = "Matched case output"
                    }
                },
                Capabilities = new NodeCapabilities
                {
                    // Multi-way branching node
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var valueExpression = GetRequiredParameter<string>(parameters, "value");
            var cases = GetParameter<List<Dictionary<string, object>>>(parameters, "cases", new List<Dictionary<string, object>>());
            var fallbackOutput = GetParameter<int>(parameters, "fallbackOutput", -1);

            Logger.LogDebug("SwitchNode: Evaluating value: {Value}", valueExpression);

            try
            {
                // Evaluate the value expression
                var contextDict = context as Dictionary<string, object> ?? new Dictionary<string, object>();
                var evaluatedValue = _expressionEvaluator.Evaluate(valueExpression, contextDict);
                var valueStr = evaluatedValue?.ToString() ?? "";

                Logger.LogDebug("SwitchNode: Evaluated value: {EvaluatedValue}", valueStr);

                // Find matching case
                for (int i = 0; i < cases.Count; i++)
                {
                    var caseItem = cases[i];
                    if (caseItem.TryGetValue("value", out var caseValue))
                    {
                        var caseStr = caseValue?.ToString() ?? "";
                        if (string.Equals(valueStr, caseStr, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.LogDebug("SwitchNode: Matched case {Index}: {Value}", i, caseStr);

                            var outputIndex = caseItem.TryGetValue("outputIndex", out var idx) ? Convert.ToInt32(idx) : i;

                            return Task.FromResult(CreateSuccessResult(new
                            {
                                matchedCase = caseStr,
                                outputIndex = outputIndex,
                                caseIndex = i
                            }));
                        }
                    }
                }

                // No match found, use fallback
                if (fallbackOutput >= 0)
                {
                    Logger.LogDebug("SwitchNode: No match found, using fallback output {Index}", fallbackOutput);

                    return Task.FromResult(CreateSuccessResult(new
                    {
                        matchedCase = (string?)null,
                        outputIndex = fallbackOutput,
                        isFallback = true
                    }));
                }

                Logger.LogWarning("SwitchNode: No match found and no fallback configured");

                return Task.FromResult(CreateErrorResult("No matching case found and no fallback output configured"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SwitchNode: Error evaluating switch");
                return Task.FromResult(CreateErrorResult($"Failed to evaluate switch: {ex.Message}", ex));
            }
        }
    }
}

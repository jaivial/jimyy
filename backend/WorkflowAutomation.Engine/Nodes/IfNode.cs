using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Conditional branching node that evaluates expressions
    /// </summary>
    public class IfNode : NodeExecutorBase
    {
        private readonly IExpressionEvaluator _expressionEvaluator;

        public IfNode(
            ILogger<IfNode> logger,
            IExpressionEvaluator expressionEvaluator) : base(logger)
        {
            _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        }

        public override string NodeType => "if";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "If",
                Description = "Conditional branching based on expression evaluation",
                Category = NodeCategories.Logic,
                Icon = "git-branch",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "condition",
                        DisplayName = "Condition",
                        Type = "string",
                        Required = true,
                        Description = "Expression to evaluate (must return boolean)",
                        Placeholder = "{{ $json.value > 10 }}"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "true",
                        Type = "boolean",
                        Description = "Executes if condition is true"
                    },
                    new NodeOutput
                    {
                        Name = "false",
                        Type = "boolean",
                        Description = "Executes if condition is false"
                    }
                },
                Capabilities = new NodeCapabilities
                {
                    // Conditional branching node
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var conditionExpression = GetRequiredParameter<string>(parameters, "condition");

            Logger.LogDebug("IfNode: Evaluating condition: {Condition}", conditionExpression);

            try
            {
                // Create context dictionary if needed
                var contextDict = context as Dictionary<string, object> ?? new Dictionary<string, object>();

                // Evaluate the condition expression
                var result = _expressionEvaluator.Evaluate(conditionExpression, contextDict);

                // Convert to boolean
                bool conditionResult;
                if (result is bool boolResult)
                {
                    conditionResult = boolResult;
                }
                else
                {
                    // Try to convert to boolean
                    conditionResult = Convert.ToBoolean(result);
                }

                Logger.LogDebug("IfNode: Condition evaluated to {Result}", conditionResult);

                return Task.FromResult(CreateSuccessResult(new
                {
                    result = conditionResult,
                    branch = conditionResult ? "true" : "false",
                    outputIndex = conditionResult ? 0 : 1
                }));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "IfNode: Error evaluating condition");
                return Task.FromResult(CreateErrorResult($"Failed to evaluate condition: {ex.Message}", ex));
            }
        }
    }
}

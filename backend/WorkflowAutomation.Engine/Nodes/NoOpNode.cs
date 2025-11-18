using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// No operation node - passes data through unchanged (useful for testing)
    /// </summary>
    public class NoOpNode : NodeExecutorBase
    {
        public NoOpNode(ILogger<NoOpNode> logger) : base(logger)
        {
        }

        public override string NodeType => "noop";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "No Operation",
                Description = "Pass data through unchanged (useful for testing and debugging)",
                Category = NodeCategories.Utilities,
                Icon = "arrow-right",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "delay",
                        DisplayName = "Delay (ms)",
                        Type = "number",
                        Required = false,
                        DefaultValue = 0,
                        Description = "Optional delay in milliseconds before passing data through",
                        Validation = new ParameterValidation
                        {
                            Min = 0,
                            Max = 60000
                        }
                    },
                    new NodeParameter
                    {
                        Name = "note",
                        DisplayName = "Note",
                        Type = "string",
                        Required = false,
                        Description = "Optional note for documentation purposes"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "data",
                        Type = "object",
                        Description = "Input data passed through unchanged"
                    }
                }
            };
        }

        protected override async Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var delay = GetParameter<int>(parameters, "delay", 0);
            var note = GetParameter<string>(parameters, "note", null);

            Logger.LogDebug("NoOpNode: Passing data through (delay: {Delay}ms, note: {Note})", delay, note ?? "none");

            // Optional delay
            if (delay > 0)
            {
                await Task.Delay(delay, cancellationToken);
            }

            // Simply pass through the context data
            return CreateSuccessResult(new
            {
                data = context,
                note = note,
                passedThrough = true
            });
        }
    }
}

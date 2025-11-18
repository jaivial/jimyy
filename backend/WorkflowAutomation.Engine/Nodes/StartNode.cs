using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Manual workflow start trigger node
    /// </summary>
    public class StartNode : NodeExecutorBase
    {
        public StartNode(ILogger<StartNode> logger) : base(logger)
        {
        }

        public override string NodeType => "start";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Start",
                Description = "Manual workflow start trigger",
                Category = NodeCategories.Triggers,
                Icon = "play-circle",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "triggerData",
                        DisplayName = "Trigger Data",
                        Type = "object",
                        Required = false,
                        Description = "Optional data to pass to the workflow on start"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "data",
                        Type = "object",
                        Description = "The trigger data passed to the workflow"
                    }
                },
                Capabilities = new NodeCapabilities
                {
                    IsTrigger = true
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            Logger.LogDebug("StartNode: Executing manual workflow start");

            var triggerData = GetParameter<object>(parameters, "triggerData", new { });

            return Task.FromResult(CreateSuccessResult(new
            {
                data = triggerData,
                triggeredAt = System.DateTime.UtcNow
            }));
        }
    }
}

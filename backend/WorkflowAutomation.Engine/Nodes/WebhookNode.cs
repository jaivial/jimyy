using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Webhook trigger node for receiving HTTP webhook requests
    /// </summary>
    public class WebhookNode : NodeExecutorBase
    {
        public WebhookNode(ILogger<WebhookNode> logger) : base(logger)
        {
        }

        public override string NodeType => "webhook";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Webhook",
                Description = "Receive and process webhook HTTP requests",
                Category = NodeCategories.Triggers,
                Icon = "webhook",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "path",
                        DisplayName = "Webhook Path",
                        Type = "string",
                        Required = true,
                        Description = "URL path for the webhook (will be appended to base URL)",
                        Placeholder = "my-webhook",
                        Validation = new ParameterValidation
                        {
                            Pattern = @"^[a-zA-Z0-9\-_/]+$",
                            ErrorMessage = "Path can only contain letters, numbers, hyphens, underscores, and slashes"
                        }
                    },
                    new NodeParameter
                    {
                        Name = "method",
                        DisplayName = "HTTP Method",
                        Type = "string",
                        Required = false,
                        DefaultValue = "POST",
                        Description = "HTTP method to accept",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "GET", Name = "GET" },
                            new ParameterOption { Value = "POST", Name = "POST" },
                            new ParameterOption { Value = "PUT", Name = "PUT" },
                            new ParameterOption { Value = "DELETE", Name = "DELETE" },
                            new ParameterOption { Value = "PATCH", Name = "PATCH" },
                            new ParameterOption { Value = "ANY", Name = "Any Method" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "authentication",
                        DisplayName = "Authentication",
                        Type = "string",
                        Required = false,
                        DefaultValue = "None",
                        Description = "Authentication method for webhook",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "None", Name = "None" },
                            new ParameterOption { Value = "HeaderAuth", Name = "Header Authentication" },
                            new ParameterOption { Value = "BasicAuth", Name = "Basic Authentication" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "responseMode",
                        DisplayName = "Response Mode",
                        Type = "string",
                        Required = false,
                        DefaultValue = "LastNode",
                        Description = "What to return in the webhook response",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "LastNode", Name = "Last Node Output" },
                            new ParameterOption { Value = "FirstNode", Name = "First Node Output" },
                            new ParameterOption { Value = "ResponseCode", Name = "Response Code Only" }
                        }
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "headers",
                        Type = "object",
                        Description = "Request headers"
                    },
                    new NodeOutput
                    {
                        Name = "body",
                        Type = "object",
                        Description = "Request body"
                    },
                    new NodeOutput
                    {
                        Name = "query",
                        Type = "object",
                        Description = "Query parameters"
                    },
                    new NodeOutput
                    {
                        Name = "method",
                        Type = "string",
                        Description = "HTTP method"
                    }
                },
                Capabilities = new NodeCapabilities
                {
                    IsTrigger = true,
                    SupportsWebhook = true
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var path = GetRequiredParameter<string>(parameters, "path");
            var method = GetParameter<string>(parameters, "method", "POST");
            var authentication = GetParameter<string>(parameters, "authentication", "None");
            var responseMode = GetParameter<string>(parameters, "responseMode", "LastNode");

            Logger.LogDebug("WebhookNode: Processing webhook for path: {Path}, method: {Method}", path, method);

            // This node is primarily for configuration - actual webhook data comes from the WebhookController
            // When a webhook is triggered, the controller will pass the request data through context

            Dictionary<string, object> webhookData = new Dictionary<string, object>();

            // Extract webhook data from context if available
            if (context is Dictionary<string, object> contextDict)
            {
                if (contextDict.TryGetValue("webhookData", out var data) && data is Dictionary<string, object> dataDict)
                {
                    webhookData = dataDict;
                }
            }

            // Return the webhook configuration and received data
            return Task.FromResult(CreateSuccessResult(new
            {
                path = path,
                method = method,
                authentication = authentication,
                responseMode = responseMode,
                headers = webhookData.GetValueOrDefault("headers", new Dictionary<string, string>()),
                body = webhookData.GetValueOrDefault("body", new { }),
                query = webhookData.GetValueOrDefault("query", new Dictionary<string, string>()),
                receivedMethod = webhookData.GetValueOrDefault("method", method),
                receivedAt = DateTime.UtcNow
            }));
        }
    }
}

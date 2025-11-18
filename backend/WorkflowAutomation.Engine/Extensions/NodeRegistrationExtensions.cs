using System;
using Microsoft.Extensions.DependencyInjection;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Engine.Nodes;

namespace WorkflowAutomation.Engine.Extensions
{
    /// <summary>
    /// Extension methods for registering built-in nodes
    /// </summary>
    public static class NodeRegistrationExtensions
    {
        /// <summary>
        /// Registers all built-in nodes with the dependency injection container
        /// </summary>
        public static IServiceCollection AddBuiltInNodes(this IServiceCollection services)
        {
            // Register all built-in node executors
            services.AddScoped<StartNode>();
            services.AddScoped<HttpRequestNode>();
            services.AddScoped<IfNode>();
            services.AddScoped<SwitchNode>();
            services.AddScoped<SetNode>();
            services.AddScoped<CodeNode>();
            services.AddScoped<WebhookNode>();
            services.AddScoped<ScheduleNode>();
            services.AddScoped<MergeNode>();
            services.AddScoped<SplitNode>();
            services.AddScoped<FunctionNode>();
            services.AddScoped<NoOpNode>();

            return services;
        }

        /// <summary>
        /// Registers all built-in nodes with the node registry at application startup
        /// </summary>
        public static void RegisterBuiltInNodes(this INodeRegistry registry, IServiceProvider services)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Array of all built-in node types
            var nodeTypes = new[]
            {
                typeof(StartNode),
                typeof(HttpRequestNode),
                typeof(IfNode),
                typeof(SwitchNode),
                typeof(SetNode),
                typeof(CodeNode),
                typeof(WebhookNode),
                typeof(ScheduleNode),
                typeof(MergeNode),
                typeof(SplitNode),
                typeof(FunctionNode),
                typeof(NoOpNode)
            };

            foreach (var nodeType in nodeTypes)
            {
                var executor = (INodeExecutor)services.GetRequiredService(nodeType);
                var nodeExecutor = executor as NodeExecutorBase;

                if (nodeExecutor != null)
                {
                    registry.RegisterNode(executor, nodeExecutor.GetDefinition());
                }
                else
                {
                    registry.RegisterNode(executor);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Models;
using Microsoft.Extensions.Logging;
using CoreLogLevel = WorkflowAutomation.Core.Enums.LogLevel;

namespace WorkflowAutomation.Engine.Executor
{
    /// <summary>
    /// Workflow executor that orchestrates the execution of workflows and nodes
    /// </summary>
    public class WorkflowExecutor : IWorkflowExecutor
    {
        private readonly INodeRegistry _nodeRegistry;
        private readonly IExecutionRepository _executionRepository;
        private readonly ILogger<WorkflowExecutor> _logger;
        private readonly IExpressionEvaluator _expressionEvaluator;
        private readonly IExecutionHub _executionHub;

        public WorkflowExecutor(
            INodeRegistry nodeRegistry,
            IExecutionRepository executionRepository,
            ILogger<WorkflowExecutor> logger,
            IExpressionEvaluator expressionEvaluator,
            IExecutionHub executionHub)
        {
            _nodeRegistry = nodeRegistry;
            _executionRepository = executionRepository;
            _logger = logger;
            _expressionEvaluator = expressionEvaluator;
            _executionHub = executionHub;
        }

        public async Task<WorkflowExecution> ExecuteAsync(
            Workflow workflow,
            Dictionary<string, object> triggerData,
            CancellationToken cancellationToken = default)
        {
            var execution = new WorkflowExecution
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Status = ExecutionStatus.Running,
                StartedAt = DateTime.UtcNow,
                TriggerMode = "manual",
                Environment = workflow.Environment,
                TriggerData = triggerData,
                NodeExecutions = new List<NodeExecution>(),
                Logs = new List<ExecutionLog>()
            };

            await _executionRepository.AddAsync(execution);
            await _executionHub.BroadcastExecutionStarted(execution);

            try
            {
                // Build execution graph from workflow definition
                var executionGraph = BuildExecutionGraph(workflow.Definition);

                // Create execution context
                var context = new ExecutionContext
                {
                    Workflow = workflow,
                    Execution = execution,
                    Data = new Dictionary<string, object> { ["trigger"] = triggerData }
                };

                // Track executed nodes for path visualization
                var executedPath = new List<string>();

                // Execute nodes in dependency order
                await ExecuteNodesAsync(executionGraph, context, executedPath, cancellationToken);

                // Update execution with success status
                execution.Status = ExecutionStatus.Success;
                execution.FinishedAt = DateTime.UtcNow;
                execution.ExecutionPath = System.Text.Json.JsonSerializer.Serialize(executedPath);
                execution.NodesExecuted = executedPath.Count;
                execution.TotalDurationMs = (long)(execution.FinishedAt.Value - execution.StartedAt).TotalMilliseconds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workflow execution failed: {WorkflowId}", workflow.Id);
                execution.Status = ExecutionStatus.Error;
                execution.ErrorMessage = ex.Message;
                execution.FinishedAt = DateTime.UtcNow;

                await LogExecutionEvent(execution, CoreLogLevel.Error, ex.Message);
            }

            await _executionRepository.UpdateAsync(execution);
            await _executionHub.BroadcastExecutionCompleted(execution);

            return execution;
        }

        /// <summary>
        /// Builds an execution graph from the workflow definition
        /// </summary>
        private ExecutionGraph BuildExecutionGraph(WorkflowDefinition definition)
        {
            var graph = new ExecutionGraph();

            // Build dependency tree for each node
            foreach (var node in definition.Nodes)
            {
                // Find all nodes that this node depends on (incoming connections)
                var dependencies = definition.Connections
                    .Where(c => c.TargetNodeId == node.Id)
                    .Select(c => c.SourceNodeId)
                    .ToList();

                graph.AddNode(node.Id, node, dependencies);
            }

            return graph;
        }

        /// <summary>
        /// Executes nodes in the correct order based on the execution graph
        /// </summary>
        private async Task ExecuteNodesAsync(
            ExecutionGraph graph,
            ExecutionContext context,
            List<string> executedPath,
            CancellationToken cancellationToken)
        {
            var executedNodes = new HashSet<string>();
            var nodesToExecute = graph.GetRootNodes().ToList();
            var executionOrder = 0;

            while (nodesToExecute.Any())
            {
                // Execute nodes based on workflow execution mode
                if (context.Workflow.Settings.ExecutionMode == ExecutionMode.Parallel)
                {
                    // Parallel execution mode - execute independent nodes concurrently
                    var tasks = nodesToExecute.Select(async nodeId =>
                    {
                        var node = graph.GetNode(nodeId);
                        if (!node.Disabled)
                        {
                            await ExecuteNodeAsync(node, context, executionOrder++, cancellationToken);
                            lock (executedNodes)
                            {
                                executedNodes.Add(nodeId);
                                executedPath.Add(nodeId);
                            }
                        }
                        else
                        {
                            lock (executedNodes)
                            {
                                executedNodes.Add(nodeId);
                            }
                        }
                    });

                    await Task.WhenAll(tasks);
                }
                else
                {
                    // Sequential execution mode - execute nodes one by one
                    foreach (var nodeId in nodesToExecute)
                    {
                        var node = graph.GetNode(nodeId);
                        if (!node.Disabled)
                        {
                            await ExecuteNodeAsync(node, context, executionOrder++, cancellationToken);
                            executedNodes.Add(nodeId);
                            executedPath.Add(nodeId);
                        }
                        else
                        {
                            executedNodes.Add(nodeId);
                        }
                    }
                }

                // Find next nodes to execute (nodes whose dependencies are all executed)
                nodesToExecute = graph.GetNextNodes(executedNodes).ToList();
            }
        }

        /// <summary>
        /// Executes a single node with retry logic and error handling
        /// </summary>
        private async Task ExecuteNodeAsync(
            Node node,
            ExecutionContext context,
            int executionOrder,
            CancellationToken cancellationToken)
        {
            var nodeExecution = new NodeExecution
            {
                Id = Guid.NewGuid(),
                ExecutionId = context.Execution.Id,
                NodeId = node.Id,
                NodeName = node.Name,
                Status = ExecutionStatus.Running,
                StartedAt = DateTime.UtcNow,
                ExecutionOrder = executionOrder,
                NodePosition = node.Position
            };

            context.Execution.NodeExecutions.Add(nodeExecution);
            await _executionHub.BroadcastNodeExecutionStarted(context.Execution.Id, nodeExecution);

            int retryCount = 0;
            int maxRetries = node.RetrySettings?.Enabled == true ? node.RetrySettings.MaxRetries : 0;

            while (retryCount <= maxRetries)
            {
                try
                {
                    // Get node executor for this node type
                    var executor = _nodeRegistry.GetNodeExecutor(node.Type);

                    // Resolve parameters with expression evaluation
                    var resolvedParameters = ResolveParameters(node.Parameters, context);

                    // Log input data
                    nodeExecution.InputData = System.Text.Json.JsonSerializer.Serialize(resolvedParameters);
                    await LogExecutionEvent(context.Execution, CoreLogLevel.Info,
                        $"Executing node: {node.Name}", node.Id, node.Name);

                    // Execute the node
                    var result = await executor.ExecuteAsync(
                        resolvedParameters,
                        context,
                        cancellationToken);

                    // Store result in context for use by subsequent nodes
                    context.Data[node.Id] = result;

                    // Update node execution with success
                    nodeExecution.OutputData = System.Text.Json.JsonSerializer.Serialize(result);
                    nodeExecution.Status = ExecutionStatus.Success;
                    nodeExecution.FinishedAt = DateTime.UtcNow;
                    nodeExecution.DurationMs = (long)(nodeExecution.FinishedAt.Value - nodeExecution.StartedAt).TotalMilliseconds;

                    await LogExecutionEvent(context.Execution, CoreLogLevel.Info,
                        $"Node completed successfully: {node.Name}", node.Id, node.Name);

                    break; // Success, exit retry loop
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount > maxRetries)
                    {
                        // Max retries exceeded, mark as failed
                        _logger.LogError(ex, "Node execution failed: {NodeId}", node.Id);
                        nodeExecution.Status = ExecutionStatus.Error;
                        nodeExecution.ErrorMessage = ex.Message;
                        nodeExecution.FinishedAt = DateTime.UtcNow;
                        nodeExecution.RetryCount = retryCount - 1;

                        await LogExecutionEvent(context.Execution, CoreLogLevel.Error,
                            $"Node failed: {node.Name} - {ex.Message}", node.Id, node.Name);

                        throw;
                    }
                    else
                    {
                        // Calculate retry delay with exponential backoff
                        int delay = node.RetrySettings.RetryDelayMs;
                        if (node.RetrySettings.ExponentialBackoff)
                        {
                            delay = delay * (int)Math.Pow(2, retryCount - 1);
                        }

                        await LogExecutionEvent(context.Execution, CoreLogLevel.Warning,
                            $"Node failed, retrying ({retryCount}/{maxRetries}): {node.Name}", node.Id, node.Name);

                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            await _executionRepository.UpdateNodeExecutionAsync(nodeExecution);
            await _executionHub.BroadcastNodeExecutionCompleted(context.Execution.Id, nodeExecution);
        }

        /// <summary>
        /// Resolves node parameters by evaluating expressions
        /// </summary>
        private Dictionary<string, object> ResolveParameters(
            Dictionary<string, object> parameters,
            ExecutionContext context)
        {
            var resolved = new Dictionary<string, object>();

            foreach (var param in parameters)
            {
                if (param.Value is string strValue && strValue.StartsWith("={{") && strValue.EndsWith("}}"))
                {
                    // Expression to evaluate - extract expression without {{= and }}
                    var expression = strValue.Substring(3, strValue.Length - 5);
                    resolved[param.Key] = _expressionEvaluator.Evaluate(expression, context.Data);
                }
                else
                {
                    // Regular value, no expression evaluation needed
                    resolved[param.Key] = param.Value;
                }
            }

            return resolved;
        }

        /// <summary>
        /// Logs an execution event and broadcasts it via SignalR
        /// </summary>
        private async Task LogExecutionEvent(
            WorkflowExecution execution,
            CoreLogLevel level,
            string message,
            string? nodeId = null,
            string? nodeName = null)
        {
            var log = new ExecutionLog
            {
                Id = Guid.NewGuid(),
                ExecutionId = execution.Id,
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                NodeId = nodeId,
                NodeName = nodeName,
                Metadata = new Dictionary<string, object>()
            };

            execution.Logs.Add(log);
            await _executionHub.BroadcastExecutionLog(execution.Id, log);
        }
    }
}

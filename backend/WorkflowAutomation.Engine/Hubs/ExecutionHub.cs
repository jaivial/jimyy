using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace WorkflowAutomation.Engine.Hubs
{
    /// <summary>
    /// SignalR hub for broadcasting real-time execution updates to connected clients
    /// </summary>
    public class ExecutionHub : Hub, IExecutionHub
    {
        private readonly ILogger<ExecutionHub> _logger;

        public ExecutionHub(ILogger<ExecutionHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Allows clients to join a specific execution room to receive updates
        /// </summary>
        /// <param name="executionId">The execution ID to monitor</param>
        public async Task JoinExecution(string executionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"execution_{executionId}");
            _logger.LogInformation("Client {ConnectionId} joined execution {ExecutionId}",
                Context.ConnectionId, executionId);
        }

        /// <summary>
        /// Allows clients to leave an execution room
        /// </summary>
        /// <param name="executionId">The execution ID to stop monitoring</param>
        public async Task LeaveExecution(string executionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"execution_{executionId}");
            _logger.LogInformation("Client {ConnectionId} left execution {ExecutionId}",
                Context.ConnectionId, executionId);
        }

        public async Task BroadcastExecutionStarted(WorkflowExecution execution)
        {
            try
            {
                await Clients.All.SendAsync("ExecutionStarted", new
                {
                    execution.Id,
                    execution.WorkflowId,
                    execution.Status,
                    execution.StartedAt,
                    execution.Environment,
                    execution.TriggerMode
                });

                _logger.LogInformation("Broadcasted execution started: {ExecutionId}", execution.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast execution started: {ExecutionId}", execution.Id);
            }
        }

        public async Task BroadcastExecutionCompleted(WorkflowExecution execution)
        {
            try
            {
                await Clients.All.SendAsync("ExecutionCompleted", new
                {
                    execution.Id,
                    execution.WorkflowId,
                    execution.Status,
                    execution.StartedAt,
                    execution.FinishedAt,
                    execution.TotalDurationMs,
                    execution.NodesExecuted,
                    execution.ErrorMessage,
                    execution.ExecutionPath
                });

                _logger.LogInformation("Broadcasted execution completed: {ExecutionId} with status {Status}",
                    execution.Id, execution.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast execution completed: {ExecutionId}", execution.Id);
            }
        }

        public async Task BroadcastNodeExecutionStarted(Guid executionId, NodeExecution nodeExecution)
        {
            try
            {
                await Clients.Group($"execution_{executionId}").SendAsync("NodeExecutionStarted", new
                {
                    ExecutionId = executionId,
                    nodeExecution.Id,
                    nodeExecution.NodeId,
                    nodeExecution.NodeName,
                    nodeExecution.Status,
                    nodeExecution.StartedAt,
                    nodeExecution.ExecutionOrder,
                    nodeExecution.NodePosition
                });

                _logger.LogDebug("Broadcasted node execution started: {NodeId} for execution {ExecutionId}",
                    nodeExecution.NodeId, executionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast node execution started: {NodeId}", nodeExecution.NodeId);
            }
        }

        public async Task BroadcastNodeExecutionCompleted(Guid executionId, NodeExecution nodeExecution)
        {
            try
            {
                await Clients.Group($"execution_{executionId}").SendAsync("NodeExecutionCompleted", new
                {
                    ExecutionId = executionId,
                    nodeExecution.Id,
                    nodeExecution.NodeId,
                    nodeExecution.NodeName,
                    nodeExecution.Status,
                    nodeExecution.StartedAt,
                    nodeExecution.FinishedAt,
                    nodeExecution.DurationMs,
                    nodeExecution.ErrorMessage,
                    nodeExecution.RetryCount
                });

                _logger.LogDebug("Broadcasted node execution completed: {NodeId} for execution {ExecutionId} with status {Status}",
                    nodeExecution.NodeId, executionId, nodeExecution.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast node execution completed: {NodeId}", nodeExecution.NodeId);
            }
        }

        public async Task BroadcastExecutionLog(Guid executionId, ExecutionLog log)
        {
            try
            {
                await Clients.Group($"execution_{executionId}").SendAsync("ExecutionLog", new
                {
                    ExecutionId = executionId,
                    log.Id,
                    log.Level,
                    log.Message,
                    log.Timestamp,
                    log.NodeId,
                    log.NodeName,
                    log.Metadata
                });

                _logger.LogTrace("Broadcasted execution log for execution {ExecutionId}: {Message}",
                    executionId, log.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast execution log for execution {ExecutionId}", executionId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}

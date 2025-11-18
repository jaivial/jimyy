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
    /// Time-based trigger node using cron expressions
    /// </summary>
    public class ScheduleNode : NodeExecutorBase
    {
        public ScheduleNode(ILogger<ScheduleNode> logger) : base(logger)
        {
        }

        public override string NodeType => "schedule";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Schedule",
                Description = "Time-based trigger using cron expressions",
                Category = NodeCategories.Triggers,
                Icon = "clock",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "cronExpression",
                        DisplayName = "Cron Expression",
                        Type = "string",
                        Required = true,
                        Description = "Cron expression for scheduling (e.g., '0 0 * * *' for daily at midnight)",
                        Placeholder = "0 0 * * *",
                        Validation = new ParameterValidation
                        {
                            MinLength = 9,
                            ErrorMessage = "Invalid cron expression format"
                        }
                    },
                    new NodeParameter
                    {
                        Name = "timezone",
                        DisplayName = "Timezone",
                        Type = "string",
                        Required = false,
                        DefaultValue = "UTC",
                        Description = "Timezone for the schedule",
                        Placeholder = "UTC"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "timestamp",
                        Type = "string",
                        Description = "Current execution timestamp"
                    },
                    new NodeOutput
                    {
                        Name = "nextRun",
                        Type = "string",
                        Description = "Next scheduled run time"
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
            var cronExpression = GetRequiredParameter<string>(parameters, "cronExpression");
            var timezone = GetParameter<string>(parameters, "timezone", "UTC");

            Logger.LogDebug("ScheduleNode: Processing schedule with cron: {Cron}, timezone: {Timezone}",
                cronExpression, timezone);

            // Validate cron expression (basic validation)
            if (!IsValidCronExpression(cronExpression))
            {
                var error = "Invalid cron expression format. Expected format: minute hour day month dayOfWeek";
                Logger.LogError("ScheduleNode: {Error}", error);
                return Task.FromResult(CreateErrorResult(error));
            }

            var currentTime = DateTime.UtcNow;

            // Note: Actual scheduling is handled by Hangfire (Phase 13)
            // This node execution represents a scheduled trigger firing
            return Task.FromResult(CreateSuccessResult(new
            {
                cronExpression = cronExpression,
                timezone = timezone,
                timestamp = currentTime,
                nextRun = CalculateNextRun(cronExpression, currentTime),
                triggeredAt = currentTime
            }));
        }

        private bool IsValidCronExpression(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            // Basic validation: cron should have 5 parts (minute hour day month dayOfWeek)
            // or 6 parts if seconds are included
            var parts = cronExpression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 5 && parts.Length <= 6;
        }

        private DateTime? CalculateNextRun(string cronExpression, DateTime currentTime)
        {
            // This is a simplified placeholder
            // In a real implementation, use Cronos or NCrontab library to calculate next run
            // For now, return a future time as placeholder
            try
            {
                // Simple heuristic: add 1 hour for demonstration
                // In production, this should use a proper cron library
                return currentTime.AddHours(1);
            }
            catch
            {
                return null;
            }
        }
    }
}

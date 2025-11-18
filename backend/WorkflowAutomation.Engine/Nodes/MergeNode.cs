using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Merge data from multiple input branches
    /// </summary>
    public class MergeNode : NodeExecutorBase
    {
        public MergeNode(ILogger<MergeNode> logger) : base(logger)
        {
        }

        public override string NodeType => "merge";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Merge",
                Description = "Merge data from multiple input branches",
                Category = NodeCategories.Data,
                Icon = "code-branch",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "mode",
                        DisplayName = "Merge Mode",
                        Type = "string",
                        Required = true,
                        DefaultValue = "append",
                        Description = "How to merge the data",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "append", Name = "Append - Combine all items into array" },
                            new ParameterOption { Value = "merge", Name = "Merge - Merge object properties" },
                            new ParameterOption { Value = "keepKeyMatches", Name = "Keep Key Matches - Match by key" },
                            new ParameterOption { Value = "chooseBranch", Name = "Choose Branch - Select specific input" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "mergeBy",
                        DisplayName = "Merge By Key",
                        Type = "string",
                        Required = false,
                        Description = "Property key to merge by (for keepKeyMatches mode)",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "mode", "keepKeyMatches" }
                            }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "branchIndex",
                        DisplayName = "Branch Index",
                        Type = "number",
                        Required = false,
                        DefaultValue = 0,
                        Description = "Index of branch to choose (for chooseBranch mode)",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "mode", "chooseBranch" }
                            }
                        }
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "merged",
                        Type = "array",
                        Description = "The merged data"
                    }
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var mode = GetRequiredParameter<string>(parameters, "mode");
            var mergeBy = GetParameter<string>(parameters, "mergeBy", null);
            var branchIndex = GetParameter<int>(parameters, "branchIndex", 0);

            Logger.LogDebug("MergeNode: Merging data with mode: {Mode}", mode);

            try
            {
                // Extract input data from context
                // In a real implementation, this would receive data from multiple branches
                // For now, we'll work with the data in context
                var inputData = ExtractInputData(context);

                object mergedData = mode.ToLower() switch
                {
                    "append" => MergeAppend(inputData),
                    "merge" => MergeObjects(inputData),
                    "keepkeymatches" => MergeByKey(inputData, mergeBy),
                    "choosebranch" => ChooseBranch(inputData, branchIndex),
                    _ => throw new ArgumentException($"Unknown merge mode: {mode}")
                };

                Logger.LogDebug("MergeNode: Successfully merged data");

                return Task.FromResult(CreateSuccessResult(new
                {
                    merged = mergedData
                }));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "MergeNode: Error merging data");
                return Task.FromResult(CreateErrorResult($"Failed to merge data: {ex.Message}", ex));
            }
        }

        private List<object> ExtractInputData(object context)
        {
            var result = new List<object>();

            if (context is List<object> list)
            {
                result = list;
            }
            else if (context is Dictionary<string, object> dict)
            {
                // Check if there's an inputs array
                if (dict.TryGetValue("inputs", out var inputs) && inputs is List<object> inputsList)
                {
                    result = inputsList;
                }
                else
                {
                    result.Add(dict);
                }
            }
            else if (context != null)
            {
                result.Add(context);
            }

            return result;
        }

        private List<object> MergeAppend(List<object> inputs)
        {
            var result = new List<object>();

            foreach (var input in inputs)
            {
                if (input is List<object> list)
                {
                    result.AddRange(list);
                }
                else if (input is Array array)
                {
                    result.AddRange(array.Cast<object>());
                }
                else
                {
                    result.Add(input);
                }
            }

            return result;
        }

        private Dictionary<string, object> MergeObjects(List<object> inputs)
        {
            var result = new Dictionary<string, object>();

            foreach (var input in inputs)
            {
                if (input is Dictionary<string, object> dict)
                {
                    foreach (var kvp in dict)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }

            return result;
        }

        private List<object> MergeByKey(List<object> inputs, string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException("Merge key must be specified for keepKeyMatches mode");
            }

            var grouped = new Dictionary<string, Dictionary<string, object>>();

            foreach (var input in inputs)
            {
                if (input is Dictionary<string, object> dict && dict.TryGetValue(keyName, out var keyValue))
                {
                    var key = keyValue?.ToString() ?? "";
                    if (!grouped.ContainsKey(key))
                    {
                        grouped[key] = new Dictionary<string, object>();
                    }

                    foreach (var kvp in dict)
                    {
                        grouped[key][kvp.Key] = kvp.Value;
                    }
                }
            }

            return grouped.Values.Cast<object>().ToList();
        }

        private object ChooseBranch(List<object> inputs, int branchIndex)
        {
            if (branchIndex < 0 || branchIndex >= inputs.Count)
            {
                Logger.LogWarning("MergeNode: Branch index {Index} out of range, returning empty", branchIndex);
                return new List<object>();
            }

            return inputs[branchIndex];
        }
    }
}

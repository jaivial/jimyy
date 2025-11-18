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
    /// Split data into multiple outputs
    /// </summary>
    public class SplitNode : NodeExecutorBase
    {
        public SplitNode(ILogger<SplitNode> logger) : base(logger)
        {
        }

        public override string NodeType => "split";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Split",
                Description = "Split data into multiple outputs",
                Category = NodeCategories.Data,
                Icon = "split",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "mode",
                        DisplayName = "Split Mode",
                        Type = "string",
                        Required = true,
                        DefaultValue = "itemPerOutput",
                        Description = "How to split the data",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "itemPerOutput", Name = "Item Per Output - Each item to separate output" },
                            new ParameterOption { Value = "batchSize", Name = "Batch Size - Split into batches" },
                            new ParameterOption { Value = "byProperty", Name = "By Property - Group by property value" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "batchSize",
                        DisplayName = "Batch Size",
                        Type = "number",
                        Required = false,
                        DefaultValue = 10,
                        Description = "Number of items per batch",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "mode", "batchSize" }
                            }
                        },
                        Validation = new ParameterValidation
                        {
                            Min = 1
                        }
                    },
                    new NodeParameter
                    {
                        Name = "property",
                        DisplayName = "Property",
                        Type = "string",
                        Required = false,
                        Description = "Property name to group by",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "mode", "byProperty" }
                            }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "items",
                        DisplayName = "Items",
                        Type = "array",
                        Required = false,
                        Description = "Array of items to split (uses input data if not specified)"
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "batches",
                        Type = "array",
                        Description = "Array of split data batches"
                    }
                },
                Capabilities = new NodeCapabilities
                {
                    // Data splitting node
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var mode = GetRequiredParameter<string>(parameters, "mode");
            var batchSize = GetParameter<int>(parameters, "batchSize", 10);
            var property = GetParameter<string>(parameters, "property", null);
            var items = GetParameter<List<object>>(parameters, "items", null);

            Logger.LogDebug("SplitNode: Splitting data with mode: {Mode}", mode);

            try
            {
                // Get items from parameters or context
                var dataToSplit = items ?? ExtractItemsFromContext(context);

                if (dataToSplit == null || dataToSplit.Count == 0)
                {
                    Logger.LogWarning("SplitNode: No items to split");
                    return Task.FromResult(CreateSuccessResult(new
                    {
                        batches = new List<object>()
                    }));
                }

                var batches = mode.ToLower() switch
                {
                    "itemperoutput" => SplitItemPerOutput(dataToSplit),
                    "batchsize" => SplitByBatchSize(dataToSplit, batchSize),
                    "byproperty" => SplitByProperty(dataToSplit, property),
                    _ => throw new ArgumentException($"Unknown split mode: {mode}")
                };

                Logger.LogDebug("SplitNode: Successfully split data into {Count} batches", batches.Count);

                return Task.FromResult(CreateSuccessResult(new
                {
                    batches = batches,
                    batchCount = batches.Count
                }));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SplitNode: Error splitting data");
                return Task.FromResult(CreateErrorResult($"Failed to split data: {ex.Message}", ex));
            }
        }

        private List<object> ExtractItemsFromContext(object context)
        {
            if (context is List<object> list)
            {
                return list;
            }

            if (context is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue("items", out var items) && items is List<object> itemsList)
                {
                    return itemsList;
                }

                if (dict.TryGetValue("data", out var data) && data is List<object> dataList)
                {
                    return dataList;
                }

                // Return the dictionary as a single item
                return new List<object> { dict };
            }

            if (context != null)
            {
                return new List<object> { context };
            }

            return new List<object>();
        }

        private List<object> SplitItemPerOutput(List<object> items)
        {
            // Each item becomes its own output
            return items.Select(item => new List<object> { item } as object).ToList();
        }

        private List<object> SplitByBatchSize(List<object> items, int batchSize)
        {
            var batches = new List<object>();

            for (int i = 0; i < items.Count; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();
                batches.Add(batch);
            }

            return batches;
        }

        private List<object> SplitByProperty(List<object> items, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property name must be specified for byProperty mode");
            }

            var groups = new Dictionary<string, List<object>>();

            foreach (var item in items)
            {
                if (item is Dictionary<string, object> dict && dict.TryGetValue(propertyName, out var propValue))
                {
                    var key = propValue?.ToString() ?? "null";

                    if (!groups.ContainsKey(key))
                    {
                        groups[key] = new List<object>();
                    }

                    groups[key].Add(item);
                }
                else
                {
                    // Items without the property go to "undefined" group
                    if (!groups.ContainsKey("undefined"))
                    {
                        groups["undefined"] = new List<object>();
                    }

                    groups["undefined"].Add(item);
                }
            }

            return groups.Values.Cast<object>().ToList();
        }
    }
}

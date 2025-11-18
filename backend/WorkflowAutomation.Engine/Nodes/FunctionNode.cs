using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Transform data using built-in array operations
    /// </summary>
    public class FunctionNode : NodeExecutorBase
    {
        private readonly IExpressionEvaluator _expressionEvaluator;

        public FunctionNode(
            ILogger<FunctionNode> logger,
            IExpressionEvaluator expressionEvaluator) : base(logger)
        {
            _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        }

        public override string NodeType => "function";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Function",
                Description = "Transform data using built-in operations (map, filter, reduce, sort)",
                Category = NodeCategories.Data,
                Icon = "function",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "operation",
                        DisplayName = "Operation",
                        Type = "string",
                        Required = true,
                        Description = "Array operation to perform",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "map", Name = "Map - Transform each item" },
                            new ParameterOption { Value = "filter", Name = "Filter - Keep items matching condition" },
                            new ParameterOption { Value = "reduce", Name = "Reduce - Aggregate to single value" },
                            new ParameterOption { Value = "sort", Name = "Sort - Order items" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "expression",
                        DisplayName = "Expression",
                        Type = "string",
                        Required = true,
                        Description = "Expression to apply",
                        Placeholder = "{{ $item.value * 2 }}"
                    },
                    new NodeParameter
                    {
                        Name = "items",
                        DisplayName = "Items",
                        Type = "array",
                        Required = false,
                        Description = "Array of items to transform (uses input data if not specified)"
                    },
                    new NodeParameter
                    {
                        Name = "sortOrder",
                        DisplayName = "Sort Order",
                        Type = "string",
                        Required = false,
                        DefaultValue = "ascending",
                        Description = "Sort order",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "operation", "sort" }
                            }
                        },
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "ascending", Name = "Ascending" },
                            new ParameterOption { Value = "descending", Name = "Descending" }
                        }
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "result",
                        Type = "object",
                        Description = "Transformed data"
                    }
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var operation = GetRequiredParameter<string>(parameters, "operation").ToLower();
            var expression = GetRequiredParameter<string>(parameters, "expression");
            var items = GetParameter<List<object>>(parameters, "items", null);
            var sortOrder = GetParameter<string>(parameters, "sortOrder", "ascending");

            Logger.LogDebug("FunctionNode: Performing {Operation} operation", operation);

            try
            {
                // Get items from parameters or context
                var dataItems = items ?? ExtractItemsFromContext(context);

                if (dataItems == null || dataItems.Count == 0)
                {
                    Logger.LogWarning("FunctionNode: No items to process");
                    return Task.FromResult(CreateSuccessResult(new
                    {
                        result = new List<object>()
                    }));
                }

                object result = operation switch
                {
                    "map" => MapOperation(dataItems, expression, context),
                    "filter" => FilterOperation(dataItems, expression, context),
                    "reduce" => ReduceOperation(dataItems, expression, context),
                    "sort" => SortOperation(dataItems, expression, sortOrder, context),
                    _ => throw new ArgumentException($"Unknown operation: {operation}")
                };

                Logger.LogDebug("FunctionNode: Successfully performed {Operation}", operation);

                return Task.FromResult(CreateSuccessResult(new
                {
                    result = result
                }));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "FunctionNode: Error performing operation");
                return Task.FromResult(CreateErrorResult($"Failed to perform operation: {ex.Message}", ex));
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

                return new List<object> { dict };
            }

            if (context != null)
            {
                return new List<object> { context };
            }

            return new List<object>();
        }

        private List<object> MapOperation(List<object> items, string expression, object context)
        {
            var result = new List<object>();

            foreach (var item in items)
            {
                var itemContext = CreateItemContext(item, context);
                var mappedValue = _expressionEvaluator.Evaluate(expression, itemContext);
                result.Add(mappedValue);
            }

            return result;
        }

        private List<object> FilterOperation(List<object> items, string expression, object context)
        {
            var result = new List<object>();

            foreach (var item in items)
            {
                var itemContext = CreateItemContext(item, context);
                var filterResult = _expressionEvaluator.Evaluate(expression, itemContext);

                bool keep = filterResult is bool boolResult ? boolResult : Convert.ToBoolean(filterResult);

                if (keep)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private object ReduceOperation(List<object> items, string expression, object context)
        {
            if (items.Count == 0)
                return new object();

            object accumulator = items[0];

            for (int i = 1; i < items.Count; i++)
            {
                var itemContext = new Dictionary<string, object>
                {
                    { "$accumulator", accumulator },
                    { "$item", items[i] },
                    { "$index", i }
                };

                accumulator = _expressionEvaluator.Evaluate(expression, itemContext);
            }

            return accumulator;
        }

        private List<object> SortOperation(List<object> items, string expression, string sortOrder, object context)
        {
            var itemsWithKeys = new List<(object item, object key)>();

            foreach (var item in items)
            {
                var itemContext = CreateItemContext(item, context);
                var key = _expressionEvaluator.Evaluate(expression, itemContext);
                itemsWithKeys.Add((item, key));
            }

            var sorted = sortOrder.ToLower() == "ascending"
                ? itemsWithKeys.OrderBy(x => x.key)
                : itemsWithKeys.OrderByDescending(x => x.key);

            return sorted.Select(x => x.item).ToList();
        }

        private Dictionary<string, object> CreateItemContext(object item, object parentContext)
        {
            var itemContext = new Dictionary<string, object>
            {
                { "$item", item }
            };

            // If item is a dictionary, add its properties to context
            if (item is Dictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    if (!itemContext.ContainsKey(kvp.Key))
                    {
                        itemContext[kvp.Key] = kvp.Value;
                    }
                }
            }

            return itemContext;
        }
    }
}

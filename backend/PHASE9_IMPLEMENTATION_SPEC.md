# Phase 9: Built-in Nodes Implementation Specification

## Overview
Implement 12 essential built-in nodes for the workflow automation platform. All nodes should inherit from `NodeExecutorBase` and be registered with the `NodeRegistry`.

## Project Location
- Backend: `/home/jaime/projects/jimyy/backend`
- Nodes directory: `/home/jaime/projects/jimyy/backend/WorkflowAutomation.Engine/Nodes/`

## Base Infrastructure (Already Implemented)
- `NodeExecutorBase` abstract class at: `WorkflowAutomation.Engine/Nodes/NodeExecutorBase.cs`
- `NodeRegistry` at: `WorkflowAutomation.Engine/Registry/NodeRegistry.cs`
- `INodeExecutor` interface at: `WorkflowAutomation.Core/Interfaces/INodeExecutor.cs`
- `ExecutionContext` at: `WorkflowAutomation.Engine/Executor/ExecutionContext.cs`
- `IExpressionEvaluator` at: `WorkflowAutomation.Core/Interfaces/IExpressionEvaluator.cs`

## Nodes to Implement

### 1. StartNode (Manual Trigger)
**File**: `WorkflowAutomation.Engine/Nodes/StartNode.cs`
**NodeType**: `"start"`
**Category**: `NodeCategories.Triggers`
**Description**: Manual workflow start trigger
**Parameters**:
- `triggerData` (object, optional) - Data to pass to workflow

**Outputs**:
- `data` - The trigger data

**Implementation Notes**:
- Simple trigger node that passes through trigger data
- No complex logic needed
- Should return success result with trigger data

### 2. HttpRequestNode
**File**: `WorkflowAutomation.Engine/Nodes/HttpRequestNode.cs`
**NodeType**: `"httpRequest"`
**Category**: `NodeCategories.Actions`
**Description**: Make HTTP requests (GET, POST, PUT, DELETE, PATCH)
**Parameters**:
- `method` (string, required) - HTTP method (GET, POST, PUT, DELETE, PATCH)
- `url` (string, required) - Target URL
- `headers` (Dictionary<string, string>, optional) - HTTP headers
- `body` (object, optional) - Request body for POST/PUT/PATCH
- `authentication` (string, optional) - Authentication type (None, Basic, Bearer)
- `credentials` (object, optional) - Authentication credentials

**Outputs**:
- `statusCode` (int) - HTTP status code
- `headers` - Response headers
- `body` - Response body (parsed as JSON if possible)

**Implementation Notes**:
- Use `IHttpClientFactory` (inject in constructor)
- Support all HTTP methods
- Handle authentication (Basic, Bearer)
- Parse JSON responses
- Include timeout handling
- Return structured response with status, headers, body

### 3. IfNode (Conditional Branching)
**File**: `WorkflowAutomation.Engine/Nodes/IfNode.cs`
**NodeType**: `"if"`
**Category**: `NodeCategories.Logic`
**Description**: Conditional branching based on expression evaluation
**Parameters**:
- `condition` (string, required) - Expression to evaluate (boolean result)

**Outputs**:
- `result` (bool) - Evaluation result
- `branch` (string) - "true" or "false"

**Implementation Notes**:
- Use `IExpressionEvaluator` (inject in constructor)
- Evaluate the condition expression
- Return true/false branch indicator
- Support accessing previous node data via context

### 4. SwitchNode (Multi-way Branching)
**File**: `WorkflowAutomation.Engine/Nodes/SwitchNode.cs`
**NodeType**: `"switch"`
**Category**: `NodeCategories.Logic`
**Description**: Multi-way branching based on value
**Parameters**:
- `value` (string, required) - Expression to evaluate
- `cases` (List<object>, required) - List of case definitions with `value` and `outputIndex`
- `defaultOutput` (int, optional) - Default output index if no match

**Outputs**:
- `matchedCase` (string) - The matched case value
- `outputIndex` (int) - The output index to route to

**Implementation Notes**:
- Use `IExpressionEvaluator` to evaluate the value expression
- Compare against each case
- Return matching case or default
- Support string, number, and boolean comparisons

### 5. SetNode (Set Variables)
**File**: `WorkflowAutomation.Engine/Nodes/SetNode.cs`
**NodeType**: `"set"`
**Category**: `NodeCategories.DataTransformation`
**Description**: Set variables and transform data
**Parameters**:
- `values` (Dictionary<string, object>, required) - Key-value pairs to set
- `keepOnlySet` (bool, optional, default: false) - Keep only set values or merge with input

**Outputs**:
- All set key-value pairs

**Implementation Notes**:
- Support expression evaluation for values
- Can merge with existing context data or replace
- Return the set values as output

### 6. CodeNode (Execute JavaScript)
**File**: `WorkflowAutomation.Engine/Nodes/CodeNode.cs`
**NodeType**: `"code"`
**Category**: `NodeCategories.DataTransformation`
**Description**: Execute custom JavaScript code using Jint
**Parameters**:
- `code` (string, required) - JavaScript code to execute
- `sandbox` (bool, optional, default: true) - Run in sandboxed environment

**Outputs**:
- `result` - The return value from the code execution

**Implementation Notes**:
- Use Jint library for JavaScript execution
- Install NuGet package: `Jint`
- Inject workflow data into the Jint engine
- Set timeout for execution (prevent infinite loops)
- Handle errors gracefully
- Example from dev guide lines 1178-1216

### 7. WebhookNode (Webhook Trigger)
**File**: `WorkflowAutomation.Engine/Nodes/WebhookNode.cs`
**NodeType**: `"webhook"`
**Category**: `NodeCategories.Triggers`
**Description**: Receive and process webhook data
**Parameters**:
- `path` (string, required) - Webhook path
- `method` (string, optional, default: "POST") - HTTP method to accept
- `authentication` (string, optional) - Authentication type

**Outputs**:
- `headers` - Request headers
- `body` - Request body
- `query` - Query parameters
- `method` - HTTP method

**Implementation Notes**:
- This is a trigger node
- Store webhook configuration
- Return webhook data when triggered
- Actual HTTP endpoint handled by WebhookController (Phase 12)

### 8. ScheduleNode (Time-based Trigger)
**File**: `WorkflowAutomation.Engine/Nodes/ScheduleNode.cs`
**NodeType**: `"schedule"`
**Category**: `NodeCategories.Triggers`
**Description**: Time-based trigger with cron expressions
**Parameters**:
- `cronExpression` (string, required) - Cron expression for scheduling
- `timezone` (string, optional, default: "UTC") - Timezone for schedule

**Outputs**:
- `timestamp` - Execution timestamp
- `nextRun` - Next scheduled run time

**Implementation Notes**:
- Validate cron expression format
- Store schedule configuration
- Return current timestamp when triggered
- Actual scheduling handled by Hangfire (Phase 13)

### 9. MergeNode (Merge Data)
**File**: `WorkflowAutomation.Engine/Nodes/MergeNode.cs`
**NodeType**: `"merge"`
**Category**: `NodeCategories.DataTransformation`
**Description**: Merge data from multiple input branches
**Parameters**:
- `mode` (string, required) - Merge mode: "append", "merge", "keepFirstOnly", "keepLastOnly"
- `mergeBy` (string, optional) - Property to merge by (for object merging)

**Outputs**:
- `merged` - The merged data array/object

**Implementation Notes**:
- Support multiple merge modes
- Handle arrays and objects
- Preserve data structure
- Return merged result

### 10. SplitNode (Split Data)
**File**: `WorkflowAutomation.Engine/Nodes/SplitNode.cs`
**NodeType**: `"split"`
**Category**: `NodeCategories.DataTransformation`
**Description**: Split data into multiple outputs
**Parameters**:
- `mode` (string, required) - Split mode: "itemPerOutput", "batchSize", "byProperty"
- `batchSize` (int, optional) - Items per batch
- `property` (string, optional) - Property to split by

**Outputs**:
- `batches` - Array of split data batches

**Implementation Notes**:
- Support different split modes
- Handle arrays and objects
- Return array of batches

### 11. FunctionNode (Transform Data)
**File**: `WorkflowAutomation.Engine/Nodes/FunctionNode.cs`
**NodeType**: `"function"`
**Category**: `NodeCategories.DataTransformation`
**Description**: Transform data using built-in functions
**Parameters**:
- `operation` (string, required) - Operation: "map", "filter", "reduce", "sort"
- `expression` (string, required) - Expression to apply
- `items` (array, required) - Items to transform

**Outputs**:
- `result` - Transformed data

**Implementation Notes**:
- Use `IExpressionEvaluator` for expressions
- Support map, filter, reduce, sort operations
- Work with arrays of data
- Return transformed result

### 12. NoOpNode (No Operation)
**File**: `WorkflowAutomation.Engine/Nodes/NoOpNode.cs`
**NodeType**: `"noop"`
**Category**: `NodeCategories.Utility`
**Description**: No operation - passes data through (for testing)
**Parameters**:
- `delay` (int, optional, default: 0) - Delay in milliseconds

**Outputs**:
- `data` - Input data passed through

**Implementation Notes**:
- Simple pass-through node
- Optional delay for testing timing
- Return input data unchanged

## Node Implementation Pattern

Each node should follow this pattern:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    public class ExampleNode : NodeExecutorBase
    {
        private readonly IExpressionEvaluator _expressionEvaluator; // if needed

        public ExampleNode(
            ILogger<ExampleNode> logger,
            IExpressionEvaluator expressionEvaluator) // inject dependencies
            : base(logger)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        public override string NodeType => "example";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Example Node",
                Description = "Description of what this node does",
                Category = NodeCategories.Actions,
                Icon = "icon-name",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "paramName",
                        DisplayName = "Parameter Name",
                        Type = "string",
                        Required = true,
                        DefaultValue = "default",
                        Description = "Parameter description",
                        Validation = new ParameterValidation
                        {
                            MinLength = 1,
                            ErrorMessage = "Validation error message"
                        }
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "output",
                        Type = "object",
                        Description = "Output description"
                    }
                }
            };
        }

        protected override async Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            // Get parameters
            var param = GetRequiredParameter<string>(parameters, "paramName");

            // Execute logic
            var result = DoSomething(param);

            // Return result
            return CreateSuccessResult(new
            {
                output = result
            });
        }

        private string DoSomething(string input)
        {
            // Implementation
            return input;
        }
    }
}
```

## Node Registration

After implementing all nodes, update `Program.cs` to register them:

```csharp
// Register Node Executors (Phase 9)
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.StartNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.HttpRequestNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.IfNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.SwitchNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.SetNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.CodeNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.WebhookNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.ScheduleNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.MergeNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.SplitNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.FunctionNode>();
builder.Services.AddScoped<WorkflowAutomation.Engine.Nodes.NoOpNode>();

// Register nodes with NodeRegistry at startup
var serviceProvider = app.Services.CreateScope().ServiceProvider;
var nodeRegistry = serviceProvider.GetRequiredService<INodeRegistry>();

nodeRegistry.RegisterNode(
    serviceProvider.GetRequiredService<WorkflowAutomation.Engine.Nodes.StartNode>(),
    serviceProvider.GetRequiredService<WorkflowAutomation.Engine.Nodes.StartNode>().GetDefinition()
);
// Repeat for all nodes...
```

Or create a helper method to auto-register all nodes:

```csharp
// Add extension method to register all nodes
public static class NodeRegistrationExtensions
{
    public static void RegisterAllNodes(this INodeRegistry registry, IServiceProvider services)
    {
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
            registry.RegisterNode(executor, nodeExecutor.GetDefinition());
        }
    }
}
```

## Required NuGet Packages

Ensure these packages are installed in `WorkflowAutomation.Engine`:
- `Jint` - For JavaScript execution in CodeNode
- `Microsoft.Extensions.Http` - For IHttpClientFactory in HttpRequestNode

## NodeCategories Constants

Ensure `NodeCategories` class exists at `WorkflowAutomation.Core/Constants/NodeCategories.cs`:

```csharp
namespace WorkflowAutomation.Core.Constants
{
    public static class NodeCategories
    {
        public const string Triggers = "Triggers";
        public const string Actions = "Actions";
        public const string Logic = "Logic";
        public const string DataTransformation = "Data Transformation";
        public const string Utility = "Utility";
    }
}
```

## Testing

After implementation:
1. Verify all nodes compile without errors
2. Verify all nodes are registered in NodeRegistry
3. Check that `GetDefinition()` returns proper metadata for each node
4. Test basic execution of each node with sample parameters
5. Verify error handling works correctly

## Success Criteria

- All 12 nodes implemented and compiling
- All nodes properly inherit from NodeExecutorBase
- All nodes registered with NodeRegistry in Program.cs
- NodeCategories constants defined
- Required NuGet packages installed
- Code follows the pattern from dev guide
- No compilation errors

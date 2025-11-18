using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// Execute custom JavaScript code using Jint engine
    /// </summary>
    public class CodeNode : NodeExecutorBase
    {
        private const int DefaultTimeoutMs = 5000;
        private const int MaxTimeoutMs = 30000;

        public CodeNode(ILogger<CodeNode> logger) : base(logger)
        {
        }

        public override string NodeType => "code";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "Code",
                Description = "Execute custom JavaScript code",
                Category = NodeCategories.Data,
                Icon = "code",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "code",
                        DisplayName = "JavaScript Code",
                        Type = "code",
                        Required = true,
                        Description = "JavaScript code to execute. Return value will be the node output.",
                        Placeholder = "return { result: $input.value * 2 };"
                    },
                    new NodeParameter
                    {
                        Name = "sandbox",
                        DisplayName = "Run in Sandbox",
                        Type = "boolean",
                        Required = false,
                        DefaultValue = true,
                        Description = "Run code in sandboxed environment for security"
                    },
                    new NodeParameter
                    {
                        Name = "timeout",
                        DisplayName = "Timeout (ms)",
                        Type = "number",
                        Required = false,
                        DefaultValue = DefaultTimeoutMs,
                        Description = "Maximum execution time in milliseconds",
                        Validation = new ParameterValidation
                        {
                            Min = 100,
                            Max = MaxTimeoutMs
                        }
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "result",
                        Type = "object",
                        Description = "The return value from the code execution"
                    }
                }
            };
        }

        protected override Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var code = GetRequiredParameter<string>(parameters, "code");
            var sandbox = GetParameter<bool>(parameters, "sandbox", true);
            var timeout = GetParameter<int>(parameters, "timeout", DefaultTimeoutMs);

            Logger.LogDebug("CodeNode: Executing JavaScript code (sandbox: {Sandbox}, timeout: {Timeout}ms)", sandbox, timeout);

            try
            {
                var engine = new Jint.Engine(options =>
                {
                    // Configure timeout
                    options.TimeoutInterval(TimeSpan.FromMilliseconds(timeout));

                    // Limit memory if sandbox is enabled
                    if (sandbox)
                    {
                        options.LimitMemory(4_000_000); // 4MB limit
                        options.MaxStatements(10000); // Limit statements to prevent infinite loops
                    }

                    // Strict mode
                    options.Strict();
                });

                // Inject context data
                if (context is Dictionary<string, object> contextDict)
                {
                    foreach (var kvp in contextDict)
                    {
                        try
                        {
                            engine.SetValue(kvp.Key, kvp.Value);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "CodeNode: Failed to inject context variable {Key}", kvp.Key);
                        }
                    }
                }

                // Provide a shorthand for input data
                engine.SetValue("$input", context);

                // Provide utility functions
                engine.SetValue("console", new
                {
                    log = new Action<object>(obj => Logger.LogInformation("CodeNode console.log: {Message}", obj))
                });

                // Execute the code
                var result = engine.Evaluate(code);

                // Convert Jint value to .NET object
                var resultObject = result?.ToObject();

                Logger.LogDebug("CodeNode: Code executed successfully");

                return Task.FromResult(CreateSuccessResult(new
                {
                    result = resultObject
                }));
            }
            catch (Jint.Runtime.JavaScriptException jsEx)
            {
                Logger.LogError(jsEx, "CodeNode: JavaScript error");
                return Task.FromResult(CreateErrorResult($"JavaScript error: {jsEx.Message}", jsEx));
            }
            catch (TimeoutException tex)
            {
                Logger.LogError(tex, "CodeNode: Execution timeout");
                return Task.FromResult(CreateErrorResult("Code execution timeout", tex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "CodeNode: Error executing code");
                return Task.FromResult(CreateErrorResult($"Failed to execute code: {ex.Message}", ex));
            }
        }
    }
}

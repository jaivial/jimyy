using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkflowAutomation.Core.Interfaces;

namespace WorkflowAutomation.Engine.Expressions
{
    /// <summary>
    /// Expression evaluator implementation using Jint for JavaScript expression evaluation
    /// Supports n8n-like expression patterns with safe sandboxed execution
    /// </summary>
    public class ExpressionEvaluator : IExpressionEvaluator
    {
        private readonly ExpressionValidator _validator;
        private const int DefaultTimeoutMs = 5000; // 5 second timeout
        private const int MaxRecursionDepth = 100;

        public ExpressionEvaluator()
        {
            _validator = new ExpressionValidator();
        }

        /// <summary>
        /// Evaluates an expression with the given context data
        /// </summary>
        /// <param name="expression">The expression to evaluate (can include {{ }} delimiters)</param>
        /// <param name="contextData">The context data (previous node results, variables, etc.)</param>
        /// <returns>The evaluated result</returns>
        public object Evaluate(string expression, Dictionary<string, object> contextData)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return expression;
            }

            // Remove {{ }} delimiters if present
            var cleanExpression = _validator.RemoveDelimiters(expression);

            // If no variable references, return as-is (it's just a string)
            if (!_validator.HasVariableReferences(cleanExpression) && !IsJavaScriptExpression(cleanExpression))
            {
                return cleanExpression;
            }

            // Validate expression for security
            var validationResult = _validator.Validate(cleanExpression);
            if (!validationResult.IsValid)
            {
                throw new ExpressionEvaluationException($"Expression validation failed: {validationResult.ErrorMessage}");
            }

            try
            {
                // Create Jint engine with sandboxing
                var engine = CreateSandboxedEngine(contextData);

                // Execute expression
                var result = engine.Evaluate(cleanExpression);

                // Convert Jint result to C# object
                return ConvertJintResult(result);
            }
            catch (JavaScriptException jsEx)
            {
                throw new ExpressionEvaluationException($"JavaScript error in expression: {jsEx.Message}", jsEx);
            }
            catch (Exception ex)
            {
                throw new ExpressionEvaluationException($"Error evaluating expression: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Evaluates multiple expressions in a text string
        /// Replaces all {{expression}} patterns with their evaluated values
        /// </summary>
        public string EvaluateText(string text, Dictionary<string, object> contextData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Find all expressions
            var expressions = _validator.FindExpressions(text);
            if (expressions.Count == 0)
            {
                return text;
            }

            var result = text;
            foreach (var expr in expressions)
            {
                try
                {
                    var value = Evaluate(expr, contextData);
                    var replacement = value?.ToString() ?? string.Empty;
                    result = result.Replace($"{{{{{expr}}}}}", replacement);
                }
                catch (Exception ex)
                {
                    // Replace with error placeholder
                    result = result.Replace($"{{{{{expr}}}}}", $"[ERROR: {ex.Message}]");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates an expression without executing it
        /// </summary>
        public ValidationResult ValidateExpression(string expression)
        {
            var cleanExpression = _validator.RemoveDelimiters(expression);
            return _validator.Validate(cleanExpression);
        }

        /// <summary>
        /// Creates a sandboxed Jint engine with context data and built-in functions
        /// </summary>
        private Jint.Engine CreateSandboxedEngine(Dictionary<string, object> contextData)
        {
            var engine = new Jint.Engine(options =>
            {
                // Security settings
                options.TimeoutInterval(TimeSpan.FromMilliseconds(DefaultTimeoutMs));
                options.LimitRecursion(MaxRecursionDepth);
                options.MaxStatements(10000); // Limit statements to prevent infinite loops

                // Disable dangerous features
                options.Strict(false); // Allow non-strict mode for compatibility
            });

            // Create expression context
            var context = new ExpressionContext(contextData ?? new Dictionary<string, object>());

            // Set up context variables ($node, $workflow, $env, $json)
            engine.SetValue("$node", context.Node);
            engine.SetValue("$workflow", context.Workflow);
            engine.SetValue("$env", context.Env);
            engine.SetValue("$json", context.Json);

            // Register built-in helper functions
            RegisterBuiltInFunctions(engine);

            // Add any custom context data
            foreach (var kvp in contextData ?? new Dictionary<string, object>())
            {
                if (!kvp.Key.StartsWith("$"))
                {
                    engine.SetValue(kvp.Key, kvp.Value);
                }
            }

            return engine;
        }

        /// <summary>
        /// Registers built-in helper functions in the Jint engine
        /// </summary>
        private void RegisterBuiltInFunctions(Jint.Engine engine)
        {
            // Type conversion functions
            engine.SetValue("toNumber", new Func<object, double>(BuiltInFunctions.ToNumber));
            engine.SetValue("toString", new Func<object, string>(BuiltInFunctions.ToString));
            engine.SetValue("toInt", new Func<object, int>(BuiltInFunctions.ToInt));
            engine.SetValue("toBoolean", new Func<object, bool>(BuiltInFunctions.ToBoolean));
            engine.SetValue("toDate", new Func<object, DateTime>(BuiltInFunctions.ToDate));

            // String functions
            engine.SetValue("toUpper", new Func<string, string>(BuiltInFunctions.ToUpper));
            engine.SetValue("toLower", new Func<string, string>(BuiltInFunctions.ToLower));
            engine.SetValue("trim", new Func<string, string>(BuiltInFunctions.Trim));
            engine.SetValue("substring", new Func<string, int, int?, string>(BuiltInFunctions.Substring));
            engine.SetValue("replace", new Func<string, string, string, string>(BuiltInFunctions.Replace));
            engine.SetValue("split", new Func<string, string, string[]>(BuiltInFunctions.Split));
            engine.SetValue("contains", new Func<string, string, bool>(BuiltInFunctions.Contains));
            engine.SetValue("startsWith", new Func<string, string, bool>(BuiltInFunctions.StartsWith));
            engine.SetValue("endsWith", new Func<string, string, bool>(BuiltInFunctions.EndsWith));
            engine.SetValue("length", new Func<string, int>(BuiltInFunctions.Length));
            engine.SetValue("regexMatch", new Func<string, string, bool>(BuiltInFunctions.RegexMatch));

            // Math functions (Jint has built-in Math object, but we add shortcuts)
            engine.SetValue("round", new Func<double, int, double>(BuiltInFunctions.Round));
            engine.SetValue("floor", new Func<double, double>(BuiltInFunctions.Floor));
            engine.SetValue("ceil", new Func<double, double>(BuiltInFunctions.Ceil));
            engine.SetValue("abs", new Func<double, double>(BuiltInFunctions.Abs));
            engine.SetValue("min", new Func<double, double, double>(BuiltInFunctions.Min));
            engine.SetValue("max", new Func<double, double, double>(BuiltInFunctions.Max));
            engine.SetValue("random", new Func<double>(BuiltInFunctions.Random));

            // Date functions
            engine.SetValue("now", new Func<DateTime>(BuiltInFunctions.Now));
            engine.SetValue("utcNow", new Func<DateTime>(BuiltInFunctions.UtcNow));
            engine.SetValue("today", new Func<DateTime>(BuiltInFunctions.Today));
            engine.SetValue("formatDate", new Func<DateTime, string, string>(BuiltInFunctions.FormatDate));
            engine.SetValue("addDays", new Func<DateTime, int, DateTime>(BuiltInFunctions.AddDays));
            engine.SetValue("addHours", new Func<DateTime, int, DateTime>(BuiltInFunctions.AddHours));
            engine.SetValue("addMinutes", new Func<DateTime, int, DateTime>(BuiltInFunctions.AddMinutes));
            engine.SetValue("year", new Func<DateTime, int>(BuiltInFunctions.Year));
            engine.SetValue("month", new Func<DateTime, int>(BuiltInFunctions.Month));
            engine.SetValue("day", new Func<DateTime, int>(BuiltInFunctions.Day));

            // JSON functions
            engine.SetValue("parseJson", new Func<string, object>(BuiltInFunctions.ParseJson));
            engine.SetValue("toJson", new Func<object, string>(BuiltInFunctions.ToJson));
            engine.SetValue("getJsonProperty", new Func<object, string, object>(BuiltInFunctions.GetJsonProperty));

            // Utility functions
            engine.SetValue("isEmpty", new Func<object, bool>(BuiltInFunctions.IsEmpty));
            engine.SetValue("isNull", new Func<object, bool>(BuiltInFunctions.IsNull));
            engine.SetValue("arrayLength", new Func<object, int>(BuiltInFunctions.ArrayLength));
            engine.SetValue("defaultValue", new Func<object, object, object>(BuiltInFunctions.Default));
            engine.SetValue("uuid", new Func<string>(BuiltInFunctions.Uuid));
            engine.SetValue("base64Encode", new Func<string, string>(BuiltInFunctions.Base64Encode));
            engine.SetValue("base64Decode", new Func<string, string>(BuiltInFunctions.Base64Decode));
        }

        /// <summary>
        /// Converts Jint result to C# object
        /// </summary>
        private object ConvertJintResult(JsValue result)
        {
            if (result.IsNull() || result.IsUndefined())
            {
                return null;
            }

            if (result.IsBoolean())
            {
                return result.AsBoolean();
            }

            if (result.IsNumber())
            {
                return result.AsNumber();
            }

            if (result.IsString())
            {
                return result.AsString();
            }

            if (result.IsDate())
            {
                return result.AsDate().ToDateTime();
            }

            if (result.IsArray())
            {
                var array = result.AsArray();
                var list = new List<object>();
                for (uint i = 0; i < array.Length; i++)
                {
                    list.Add(ConvertJintResult(array.Get(i.ToString())));
                }
                return list;
            }

            if (result.IsObject())
            {
                var obj = result.AsObject();
                var dict = new Dictionary<string, object>();
                foreach (var prop in obj.GetOwnProperties())
                {
                    dict[prop.Key.ToString()] = ConvertJintResult(prop.Value.Value);
                }
                return dict;
            }

            // Default: try to convert to string
            return result.ToString();
        }

        /// <summary>
        /// Checks if expression looks like JavaScript code (not just a string literal)
        /// </summary>
        private bool IsJavaScriptExpression(string expression)
        {
            // Check for common JavaScript patterns
            var jsPatterns = new[]
            {
                @"\+",           // Addition/concatenation
                @"-",            // Subtraction
                @"\*",           // Multiplication
                @"\/",           // Division
                @"===",          // Strict equality
                @"==",           // Equality
                @"!==",          // Strict inequality
                @"!=",           // Inequality
                @"&&",           // Logical AND
                @"\|\|",         // Logical OR
                @"\?.*:",        // Ternary operator
                @"\(",           // Function call
                @"\[",           // Array access
                @"Math\.",       // Math operations
                @"new\s",        // Constructor
                @"\.length",     // Property access
                @"\.toString",   // Method call
                @"\.substring",  // Method call
            };

            return jsPatterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(expression, pattern));
        }
    }

    /// <summary>
    /// Exception thrown when expression evaluation fails
    /// </summary>
    public class ExpressionEvaluationException : Exception
    {
        public ExpressionEvaluationException(string message) : base(message)
        {
        }

        public ExpressionEvaluationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

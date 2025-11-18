using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WorkflowAutomation.Engine.Expressions
{
    /// <summary>
    /// Validates expressions before execution to prevent security issues
    /// Checks for forbidden operations and potential security risks
    /// </summary>
    public class ExpressionValidator
    {
        private static readonly HashSet<string> ForbiddenKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eval",
            "Function",
            "setTimeout",
            "setInterval",
            "require",
            "import",
            "process",
            "child_process",
            "fs",
            "net",
            "http",
            "https",
            "__dirname",
            "__filename"
        };

        private static readonly HashSet<string> DangerousPatterns = new HashSet<string>
        {
            @"\.\.\/",                    // Path traversal
            @"\/etc\/",                   // System directories
            @"\/proc\/",                  // Process info
            @"\/sys\/",                   // System info
            @"<script",                   // XSS attempts
            @"javascript:",               // JavaScript protocol
            @"on\w+\s*=",                 // Event handlers
            @"document\.",                // DOM access
            @"window\.",                  // Window object
            @"global\.",                  // Global object
            @"this\.",                    // This context (potentially dangerous)
        };

        /// <summary>
        /// Validates an expression for security and syntax issues
        /// </summary>
        /// <param name="expression">The expression to validate</param>
        /// <returns>Validation result with success flag and error message</returns>
        public ValidationResult Validate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return ValidationResult.Success();
            }

            // Check for forbidden keywords
            foreach (var keyword in ForbiddenKeywords)
            {
                if (Regex.IsMatch(expression, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                {
                    return ValidationResult.Failure($"Forbidden keyword detected: {keyword}");
                }
            }

            // Check for dangerous patterns
            foreach (var pattern in DangerousPatterns)
            {
                if (Regex.IsMatch(expression, pattern, RegexOptions.IgnoreCase))
                {
                    return ValidationResult.Failure($"Potentially dangerous pattern detected: {pattern}");
                }
            }

            // Check for excessive nesting (potential DoS)
            var nestingLevel = CountNestingLevel(expression);
            if (nestingLevel > 10)
            {
                return ValidationResult.Failure($"Expression nesting level too deep: {nestingLevel} (max 10)");
            }

            // Check for excessively long expressions (potential DoS)
            if (expression.Length > 10000)
            {
                return ValidationResult.Failure($"Expression too long: {expression.Length} characters (max 10000)");
            }

            // Check for balanced braces and parentheses
            if (!AreBracesBalanced(expression))
            {
                return ValidationResult.Failure("Unbalanced braces or parentheses in expression");
            }

            // Check for suspicious file system operations
            if (Regex.IsMatch(expression, @"(read|write|delete|unlink|rm)\s*\(", RegexOptions.IgnoreCase))
            {
                return ValidationResult.Failure("File system operations are not allowed in expressions");
            }

            // Check for network operations
            if (Regex.IsMatch(expression, @"(fetch|xhr|ajax|http|https|socket)\s*\(", RegexOptions.IgnoreCase))
            {
                return ValidationResult.Failure("Network operations are not allowed in expressions");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Extracts all variable references from an expression
        /// </summary>
        /// <param name="expression">The expression to analyze</param>
        /// <returns>List of variable references (e.g., $node.NodeName, $workflow.id)</returns>
        public List<string> ExtractVariableReferences(string expression)
        {
            var references = new List<string>();

            // Match patterns like $node.NodeName, $workflow.variables.x, $env.KEY, $json.path
            var pattern = @"\$(?:node|workflow|env|json)(?:\.\w+)+";
            var matches = Regex.Matches(expression, pattern);

            foreach (Match match in matches)
            {
                if (!references.Contains(match.Value))
                {
                    references.Add(match.Value);
                }
            }

            return references;
        }

        /// <summary>
        /// Checks if expression contains any variable references
        /// </summary>
        public bool HasVariableReferences(string expression)
        {
            return Regex.IsMatch(expression, @"\$(?:node|workflow|env|json)\.");
        }

        /// <summary>
        /// Removes expression delimiters ({{ and }}) from a string
        /// </summary>
        public string RemoveDelimiters(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;

            // Remove {{ and }} from start and end
            var trimmed = expression.Trim();
            if (trimmed.StartsWith("{{") && trimmed.EndsWith("}}"))
            {
                return trimmed.Substring(2, trimmed.Length - 4).Trim();
            }

            return expression;
        }

        /// <summary>
        /// Checks if a string contains expression delimiters
        /// </summary>
        public bool IsExpression(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            return trimmed.StartsWith("{{") && trimmed.EndsWith("}}");
        }

        /// <summary>
        /// Finds all expressions in a string (supports multiple expressions)
        /// </summary>
        public List<string> FindExpressions(string text)
        {
            var expressions = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return expressions;

            // Match all {{...}} patterns
            var pattern = @"\{\{([^}]+)\}\}";
            var matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
            {
                expressions.Add(match.Groups[1].Value.Trim());
            }

            return expressions;
        }

        /// <summary>
        /// Counts nesting level of braces/parentheses
        /// </summary>
        private int CountNestingLevel(string expression)
        {
            int maxLevel = 0;
            int currentLevel = 0;

            foreach (char c in expression)
            {
                if (c == '(' || c == '{' || c == '[')
                {
                    currentLevel++;
                    maxLevel = Math.Max(maxLevel, currentLevel);
                }
                else if (c == ')' || c == '}' || c == ']')
                {
                    currentLevel--;
                }
            }

            return maxLevel;
        }

        /// <summary>
        /// Checks if braces and parentheses are balanced
        /// </summary>
        private bool AreBracesBalanced(string expression)
        {
            var stack = new Stack<char>();
            var pairs = new Dictionary<char, char>
            {
                { ')', '(' },
                { '}', '{' },
                { ']', '[' }
            };

            foreach (char c in expression)
            {
                if (c == '(' || c == '{' || c == '[')
                {
                    stack.Push(c);
                }
                else if (c == ')' || c == '}' || c == ']')
                {
                    if (stack.Count == 0 || stack.Pop() != pairs[c])
                    {
                        return false;
                    }
                }
            }

            return stack.Count == 0;
        }
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}

using System.Collections.Generic;

namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Expression evaluator interface for resolving dynamic expressions in workflow parameters
    /// This will be implemented in Phase 7: Expression Evaluator
    /// </summary>
    public interface IExpressionEvaluator
    {
        /// <summary>
        /// Evaluates an expression with the given context data
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <param name="contextData">The context data (previous node results, variables, etc.)</param>
        /// <returns>The evaluated result</returns>
        object Evaluate(string expression, Dictionary<string, object> contextData);
    }
}

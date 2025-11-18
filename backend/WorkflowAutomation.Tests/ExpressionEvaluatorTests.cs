using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using WorkflowAutomation.Engine.Expressions;
using Xunit;

namespace WorkflowAutomation.Tests
{
    public class ExpressionEvaluatorTests
    {
        private readonly ExpressionEvaluator _evaluator;

        public ExpressionEvaluatorTests()
        {
            _evaluator = new ExpressionEvaluator();
        }

        #region Basic Expression Tests

        [Fact]
        public void Evaluate_SimpleString_ReturnsString()
        {
            var result = _evaluator.Evaluate("Hello World", new Dictionary<string, object>());
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Evaluate_NumberExpression_ReturnsNumber()
        {
            var result = _evaluator.Evaluate("{{ 42 }}", new Dictionary<string, object>());
            Assert.Equal(42.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_MathExpression_ReturnsResult()
        {
            var result = _evaluator.Evaluate("{{ 10 + 20 }}", new Dictionary<string, object>());
            Assert.Equal(30.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_StringConcatenation_ReturnsConcatenatedString()
        {
            var result = _evaluator.Evaluate("{{ 'Hello' + ' ' + 'World' }}", new Dictionary<string, object>());
            Assert.Equal("Hello World", result.ToString());
        }

        [Fact]
        public void Evaluate_TernaryOperator_ReturnsCorrectValue()
        {
            var result = _evaluator.Evaluate("{{ 10 > 5 ? 'yes' : 'no' }}", new Dictionary<string, object>());
            Assert.Equal("yes", result.ToString());
        }

        #endregion

        #region Node Data Access Tests

        [Fact]
        public void Evaluate_NodeDataAccess_ReturnsNodeData()
        {
            var jsonData = JObject.Parse("{\"userId\":123}");
            var contextData = new Dictionary<string, object>
            {
                { "$node.HttpRequest", jsonData }
            };

            var result = _evaluator.Evaluate("{{ $node['HttpRequest'].Data.userId }}", contextData);
            Assert.Equal(123.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_NodeDataNestedAccess_ReturnsNestedData()
        {
            var jsonData = JObject.Parse("{\"response\":{\"items\":[{\"id\":456}]}}");
            var contextData = new Dictionary<string, object>
            {
                { "$node.ApiCall", jsonData }
            };

            var result = _evaluator.Evaluate("{{ $node['ApiCall'].Data.response.items[0].id }}", contextData);
            Assert.Equal(456.0, Convert.ToDouble(result));
        }

        #endregion

        #region Workflow Variables Tests

        [Fact]
        public void Evaluate_WorkflowVariable_ReturnsVariable()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$workflow.variables.apiEndpoint", "https://api.example.com" }
            };

            var result = _evaluator.Evaluate("{{ $workflow.variables['apiEndpoint'] }}", contextData);
            Assert.Equal("https://api.example.com", result.ToString());
        }

        [Fact]
        public void Evaluate_WorkflowId_ReturnsId()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$workflow.id", "workflow-123" }
            };

            var result = _evaluator.Evaluate("{{ $workflow.id }}", contextData);
            Assert.Equal("workflow-123", result.ToString());
        }

        #endregion

        #region Environment Variables Tests

        [Fact]
        public void Evaluate_EnvVariable_ReturnsEnvValue()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$env.API_KEY", "secret-key-123" }
            };

            var result = _evaluator.Evaluate("{{ $env['API_KEY'] }}", contextData);
            Assert.Equal("secret-key-123", result.ToString());
        }

        [Fact]
        public void Evaluate_EnvVariableInString_ConcatenatesCorrectly()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$env.AUTH_TOKEN", "token123" }
            };

            var result = _evaluator.Evaluate("{{ 'Bearer ' + $env['AUTH_TOKEN'] }}", contextData);
            Assert.Equal("Bearer token123", result.ToString());
        }

        #endregion

        #region JSON Data Access Tests

        [Fact]
        public void Evaluate_JsonPropertyAccess_ReturnsProperty()
        {
            var jsonData = JObject.Parse("{\"name\":\"John\",\"age\":30}");
            var contextData = new Dictionary<string, object>
            {
                { "$json", jsonData }
            };

            var result = _evaluator.Evaluate("{{ $json.name }}", contextData);
            Assert.Equal("John", result.ToString());
        }

        [Fact]
        public void Evaluate_JsonNestedAccess_ReturnsNestedValue()
        {
            var jsonData = JObject.Parse("{\"user\":{\"profile\":{\"email\":\"test@example.com\"}}}");
            var contextData = new Dictionary<string, object>
            {
                { "$json", jsonData }
            };

            var result = _evaluator.Evaluate("{{ $json.user.profile.email }}", contextData);
            Assert.Equal("test@example.com", result.ToString());
        }

        #endregion

        #region Built-in Functions Tests - Type Conversion

        [Fact]
        public void Evaluate_ToNumber_ConvertsStringToNumber()
        {
            var result = _evaluator.Evaluate("{{ toNumber('123') }}", new Dictionary<string, object>());
            Assert.Equal(123.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_ToString_ConvertsNumberToString()
        {
            var result = _evaluator.Evaluate("{{ toString(456) }}", new Dictionary<string, object>());
            Assert.Equal("456", result.ToString());
        }

        [Fact]
        public void Evaluate_ToInt_RoundsToInteger()
        {
            var result = _evaluator.Evaluate("{{ toInt(3.7) }}", new Dictionary<string, object>());
            Assert.Equal(4, Convert.ToInt32(result));
        }

        [Fact]
        public void Evaluate_ToBoolean_ConvertsToBoolean()
        {
            var result = _evaluator.Evaluate("{{ toBoolean('true') }}", new Dictionary<string, object>());
            Assert.True(Convert.ToBoolean(result));
        }

        #endregion

        #region Built-in Functions Tests - String

        [Fact]
        public void Evaluate_ToUpper_ConvertsToUppercase()
        {
            var result = _evaluator.Evaluate("{{ toUpper('hello') }}", new Dictionary<string, object>());
            Assert.Equal("HELLO", result.ToString());
        }

        [Fact]
        public void Evaluate_ToLower_ConvertsToLowercase()
        {
            var result = _evaluator.Evaluate("{{ toLower('WORLD') }}", new Dictionary<string, object>());
            Assert.Equal("world", result.ToString());
        }

        [Fact]
        public void Evaluate_Trim_RemovesWhitespace()
        {
            var result = _evaluator.Evaluate("{{ trim('  hello  ') }}", new Dictionary<string, object>());
            Assert.Equal("hello", result.ToString());
        }

        [Fact]
        public void Evaluate_Substring_ExtractsSubstring()
        {
            var result = _evaluator.Evaluate("{{ substring('Hello World', 0, 5) }}", new Dictionary<string, object>());
            Assert.Equal("Hello", result.ToString());
        }

        [Fact]
        public void Evaluate_Replace_ReplacesText()
        {
            var result = _evaluator.Evaluate("{{ replace('Hello World', 'World', 'JavaScript') }}", new Dictionary<string, object>());
            Assert.Equal("Hello JavaScript", result.ToString());
        }

        [Fact]
        public void Evaluate_Contains_ChecksForSubstring()
        {
            var result = _evaluator.Evaluate("{{ contains('Hello World', 'World') }}", new Dictionary<string, object>());
            Assert.True(Convert.ToBoolean(result));
        }

        [Fact]
        public void Evaluate_StartsWith_ChecksPrefix()
        {
            var result = _evaluator.Evaluate("{{ startsWith('Hello', 'He') }}", new Dictionary<string, object>());
            Assert.True(Convert.ToBoolean(result));
        }

        [Fact]
        public void Evaluate_EndsWith_ChecksSuffix()
        {
            var result = _evaluator.Evaluate("{{ endsWith('Hello', 'lo') }}", new Dictionary<string, object>());
            Assert.True(Convert.ToBoolean(result));
        }

        [Fact]
        public void Evaluate_Length_ReturnsStringLength()
        {
            var result = _evaluator.Evaluate("{{ length('Hello') }}", new Dictionary<string, object>());
            Assert.Equal(5, Convert.ToInt32(result));
        }

        #endregion

        #region Built-in Functions Tests - Math

        [Fact]
        public void Evaluate_Round_RoundsNumber()
        {
            var result = _evaluator.Evaluate("{{ round(3.14159, 2) }}", new Dictionary<string, object>());
            Assert.Equal(3.14, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_Floor_RoundsDown()
        {
            var result = _evaluator.Evaluate("{{ floor(3.9) }}", new Dictionary<string, object>());
            Assert.Equal(3.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_Ceil_RoundsUp()
        {
            var result = _evaluator.Evaluate("{{ ceil(3.1) }}", new Dictionary<string, object>());
            Assert.Equal(4.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_Abs_ReturnsAbsoluteValue()
        {
            var result = _evaluator.Evaluate("{{ abs(-5) }}", new Dictionary<string, object>());
            Assert.Equal(5.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_Min_ReturnsMinimum()
        {
            var result = _evaluator.Evaluate("{{ min(10, 20) }}", new Dictionary<string, object>());
            Assert.Equal(10.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_Max_ReturnsMaximum()
        {
            var result = _evaluator.Evaluate("{{ max(10, 20) }}", new Dictionary<string, object>());
            Assert.Equal(20.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_MathObject_WorksCorrectly()
        {
            var result = _evaluator.Evaluate("{{ Math.sqrt(16) }}", new Dictionary<string, object>());
            Assert.Equal(4.0, Convert.ToDouble(result));
        }

        #endregion

        #region Built-in Functions Tests - Utility

        [Fact]
        public void Evaluate_IsNull_ChecksForNull()
        {
            var result = _evaluator.Evaluate("{{ isNull(null) }}", new Dictionary<string, object>());
            Assert.True(Convert.ToBoolean(result));
        }

        [Fact]
        public void Evaluate_Default_ReturnsDefaultForNull()
        {
            var result = _evaluator.Evaluate("{{ defaultValue(null, 'default') }}", new Dictionary<string, object>());
            Assert.Equal("default", result.ToString());
        }

        [Fact]
        public void Evaluate_Uuid_GeneratesUuid()
        {
            var result = _evaluator.Evaluate("{{ uuid() }}", new Dictionary<string, object>());
            Assert.NotNull(result);
            Assert.True(Guid.TryParse(result.ToString(), out _));
        }

        [Fact]
        public void Evaluate_Base64Encode_EncodesString()
        {
            var result = _evaluator.Evaluate("{{ base64Encode('Hello') }}", new Dictionary<string, object>());
            Assert.Equal("SGVsbG8=", result.ToString());
        }

        [Fact]
        public void Evaluate_Base64Decode_DecodesString()
        {
            var result = _evaluator.Evaluate("{{ base64Decode('SGVsbG8=') }}", new Dictionary<string, object>());
            Assert.Equal("Hello", result.ToString());
        }

        #endregion

        #region Complex Expression Tests

        [Fact]
        public void Evaluate_ComplexCalculation_ReturnsCorrectResult()
        {
            var jsonData = JObject.Parse("{\"price\":100.0}");
            var contextData = new Dictionary<string, object>
            {
                { "$node.GetProduct", jsonData }
            };

            var result = _evaluator.Evaluate("{{ round($node['GetProduct'].Data.price * 1.1, 2) }}", contextData);
            Assert.Equal(110.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_ComplexStringOperation_WorksCorrectly()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$json", JObject.Parse("{\"firstName\":\"John\",\"lastName\":\"Doe\"}") }
            };

            var result = _evaluator.Evaluate("{{ toUpper($json.firstName) + ' ' + toUpper($json.lastName) }}", contextData);
            Assert.Equal("JOHN DOE", result.ToString());
        }

        [Fact]
        public void Evaluate_ComplexConditional_ReturnsCorrectBranch()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$json", JObject.Parse("{\"age\":25,\"status\":\"active\"}") }
            };

            var result = _evaluator.Evaluate(
                "{{ $json.age >= 18 && $json.status === 'active' ? 'Approved' : 'Denied' }}",
                contextData
            );
            Assert.Equal("Approved", result.ToString());
        }

        #endregion

        #region Multiple Expressions in Text Tests

        [Fact]
        public void EvaluateText_MultipleExpressions_ReplacesAll()
        {
            var contextData = new Dictionary<string, object>
            {
                { "$json", JObject.Parse("{\"name\":\"John\",\"age\":30}") }
            };

            var result = _evaluator.EvaluateText("Hello {{$json.name}}, you are {{$json.age}} years old.", contextData);
            Assert.Equal("Hello John, you are 30 years old.", result);
        }

        [Fact]
        public void EvaluateText_NoExpressions_ReturnsOriginal()
        {
            var result = _evaluator.EvaluateText("Hello World", new Dictionary<string, object>());
            Assert.Equal("Hello World", result);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void ValidateExpression_ValidExpression_ReturnsSuccess()
        {
            var result = _evaluator.ValidateExpression("{{ 10 + 20 }}");
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateExpression_ForbiddenKeyword_ReturnsFail()
        {
            var result = _evaluator.ValidateExpression("{{ eval('dangerous code') }}");
            Assert.False(result.IsValid);
            Assert.Contains("eval", result.ErrorMessage.ToLower());
        }

        [Fact]
        public void ValidateExpression_ExcessiveNesting_ReturnsFail()
        {
            var deeplyNested = "{{ " + new string('(', 15) + "1" + new string(')', 15) + " }}";
            var result = _evaluator.ValidateExpression(deeplyNested);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidateExpression_UnbalancedBraces_ReturnsFail()
        {
            var result = _evaluator.ValidateExpression("{{ (1 + 2 }}");
            Assert.False(result.IsValid);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void Evaluate_InvalidExpression_ThrowsException()
        {
            Assert.Throws<ExpressionEvaluationException>(() =>
            {
                _evaluator.Evaluate("{{ eval('bad') }}", new Dictionary<string, object>());
            });
        }

        [Fact]
        public void Evaluate_NullContextData_HandlesGracefully()
        {
            var result = _evaluator.Evaluate("{{ 10 + 20 }}", null);
            Assert.Equal(30.0, Convert.ToDouble(result));
        }

        [Fact]
        public void Evaluate_EmptyExpression_ReturnsEmpty()
        {
            var result = _evaluator.Evaluate("", new Dictionary<string, object>());
            Assert.Equal("", result);
        }

        #endregion

        #region Security Tests

        [Fact]
        public void Evaluate_FileSystemAccess_IsBlocked()
        {
            Assert.Throws<ExpressionEvaluationException>(() =>
            {
                _evaluator.Evaluate("{{ read('/etc/passwd') }}", new Dictionary<string, object>());
            });
        }

        [Fact]
        public void Evaluate_NetworkAccess_IsBlocked()
        {
            Assert.Throws<ExpressionEvaluationException>(() =>
            {
                _evaluator.Evaluate("{{ fetch('http://evil.com') }}", new Dictionary<string, object>());
            });
        }

        #endregion
    }
}

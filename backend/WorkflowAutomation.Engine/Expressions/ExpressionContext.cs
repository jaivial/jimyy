using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WorkflowAutomation.Engine.Expressions
{
    /// <summary>
    /// Expression context that provides access to workflow data
    /// Supports $node, $workflow, $env, $json accessors
    /// </summary>
    public class ExpressionContext
    {
        private readonly Dictionary<string, object> _contextData;

        public ExpressionContext(Dictionary<string, object> contextData)
        {
            _contextData = contextData ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Access previous node results: $node.NodeName.data.field
        /// </summary>
        public NodeAccessor Node => new NodeAccessor(_contextData);

        /// <summary>
        /// Access workflow variables: $workflow.variables.myVar
        /// </summary>
        public WorkflowAccessor Workflow => new WorkflowAccessor(_contextData);

        /// <summary>
        /// Access environment variables: $env.API_KEY
        /// </summary>
        public EnvironmentAccessor Env => new EnvironmentAccessor(_contextData);

        /// <summary>
        /// Access current item JSON data: $json.nested.path
        /// </summary>
        public JObject Json => GetJsonData();

        private JObject GetJsonData()
        {
            if (_contextData.TryGetValue("$json", out var jsonData))
            {
                if (jsonData is JObject jobj)
                    return jobj;
                if (jsonData is string jsonStr)
                    return JObject.Parse(jsonStr);
                return JObject.FromObject(jsonData);
            }
            return new JObject();
        }

        /// <summary>
        /// Get all context data for expression evaluation
        /// </summary>
        public Dictionary<string, object> GetAllData()
        {
            return new Dictionary<string, object>(_contextData);
        }
    }

    /// <summary>
    /// Accessor for node data
    /// </summary>
    public class NodeAccessor
    {
        private readonly Dictionary<string, object> _contextData;

        public NodeAccessor(Dictionary<string, object> contextData)
        {
            _contextData = contextData;
        }

        /// <summary>
        /// Access node data by node name
        /// </summary>
        public NodeData this[string nodeName]
        {
            get
            {
                var key = $"$node.{nodeName}";
                if (_contextData.TryGetValue(key, out var nodeData))
                {
                    return new NodeData(nodeData);
                }
                return new NodeData(null);
            }
        }
    }

    /// <summary>
    /// Represents data from a specific node
    /// </summary>
    public class NodeData
    {
        private readonly object _data;

        public NodeData(object data)
        {
            _data = data;
        }

        /// <summary>
        /// Access node output data
        /// </summary>
        public JObject Data
        {
            get
            {
                if (_data == null) return new JObject();
                if (_data is JObject jobj) return jobj;
                if (_data is string jsonStr) return JObject.Parse(jsonStr);
                return JObject.FromObject(_data);
            }
        }

        /// <summary>
        /// Get raw data as dictionary
        /// </summary>
        public Dictionary<string, object> AsDict()
        {
            if (_data is Dictionary<string, object> dict)
                return dict;
            return Data.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Accessor for workflow data
    /// </summary>
    public class WorkflowAccessor
    {
        private readonly Dictionary<string, object> _contextData;

        public WorkflowAccessor(Dictionary<string, object> contextData)
        {
            _contextData = contextData;
        }

        /// <summary>
        /// Access workflow variables
        /// </summary>
        public VariableAccessor Variables => new VariableAccessor(_contextData);

        /// <summary>
        /// Workflow ID
        /// </summary>
        public string Id => GetValue("$workflow.id");

        /// <summary>
        /// Workflow name
        /// </summary>
        public string Name => GetValue("$workflow.name");

        private string GetValue(string key)
        {
            if (_contextData.TryGetValue(key, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// Accessor for workflow variables
    /// </summary>
    public class VariableAccessor
    {
        private readonly Dictionary<string, object> _contextData;

        public VariableAccessor(Dictionary<string, object> contextData)
        {
            _contextData = contextData;
        }

        /// <summary>
        /// Access variable by name
        /// </summary>
        public object this[string variableName]
        {
            get
            {
                var key = $"$workflow.variables.{variableName}";
                if (_contextData.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }
        }
    }

    /// <summary>
    /// Accessor for environment variables
    /// </summary>
    public class EnvironmentAccessor
    {
        private readonly Dictionary<string, object> _contextData;

        public EnvironmentAccessor(Dictionary<string, object> contextData)
        {
            _contextData = contextData;
        }

        /// <summary>
        /// Access environment variable by name
        /// </summary>
        public string this[string envVarName]
        {
            get
            {
                // First check context data
                var key = $"$env.{envVarName}";
                if (_contextData.TryGetValue(key, out var value))
                {
                    return value?.ToString() ?? string.Empty;
                }

                // Fall back to system environment variables
                return Environment.GetEnvironmentVariable(envVarName) ?? string.Empty;
            }
        }
    }
}

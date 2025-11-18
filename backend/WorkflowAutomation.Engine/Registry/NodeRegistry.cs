using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Registry
{
    /// <summary>
    /// Thread-safe registry for managing node types and their executors
    /// </summary>
    public class NodeRegistry : INodeRegistry
    {
        private readonly ConcurrentDictionary<string, INodeExecutor> _executors;
        private readonly ConcurrentDictionary<string, NodeDefinition> _definitions;
        private readonly ILogger<NodeRegistry> _logger;

        public NodeRegistry(ILogger<NodeRegistry> logger)
        {
            _logger = logger;
            _executors = new ConcurrentDictionary<string, INodeExecutor>();
            _definitions = new ConcurrentDictionary<string, NodeDefinition>();
        }

        /// <summary>
        /// Registers a node executor with its definition
        /// </summary>
        public void RegisterNode(INodeExecutor executor, NodeDefinition definition)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (string.IsNullOrWhiteSpace(definition.Type))
                throw new ArgumentException("Node type cannot be empty", nameof(definition));

            var nodeType = definition.Type;

            if (_executors.TryAdd(nodeType, executor))
            {
                _definitions.TryAdd(nodeType, definition);
                _logger.LogInformation("Registered node type: {NodeType} - {NodeName}", nodeType, definition.Name);
            }
            else
            {
                _logger.LogWarning("Node type {NodeType} is already registered", nodeType);
            }
        }

        /// <summary>
        /// Registers a node executor (uses reflection to get NodeType property or attribute)
        /// </summary>
        public void RegisterNode(INodeExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            // Try to get node type from NodeType property
            var nodeType = GetNodeType(executor);

            if (string.IsNullOrWhiteSpace(nodeType))
            {
                throw new InvalidOperationException(
                    $"Cannot determine node type for {executor.GetType().Name}. " +
                    "Ensure the executor has a NodeType property or use RegisterNode with NodeDefinition.");
            }

            // Create a basic definition if not provided
            var definition = new NodeDefinition
            {
                Type = nodeType,
                Name = nodeType,
                Description = $"Node executor for {nodeType}",
                Category = "Other"
            };

            RegisterNode(executor, definition);
        }

        /// <summary>
        /// Gets a node executor for the specified node type
        /// </summary>
        public INodeExecutor GetNodeExecutor(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                throw new ArgumentException("Node type cannot be empty", nameof(nodeType));

            if (_executors.TryGetValue(nodeType, out var executor))
            {
                return executor;
            }

            throw new InvalidOperationException($"Node type '{nodeType}' is not registered");
        }

        /// <summary>
        /// Gets the definition for a specific node type
        /// </summary>
        public NodeDefinition GetNodeDefinition(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                throw new ArgumentException("Node type cannot be empty", nameof(nodeType));

            if (_definitions.TryGetValue(nodeType, out var definition))
            {
                return definition;
            }

            return null;
        }

        /// <summary>
        /// Gets all registered node definitions
        /// </summary>
        public IEnumerable<NodeDefinition> GetAllNodeDefinitions()
        {
            return _definitions.Values.ToList();
        }

        /// <summary>
        /// Gets node definitions filtered by category
        /// </summary>
        public IEnumerable<NodeDefinition> GetNodeDefinitionsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return GetAllNodeDefinitions();

            return _definitions.Values
                .Where(d => string.Equals(d.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Checks if a node type is registered
        /// </summary>
        public bool IsNodeTypeRegistered(string nodeType)
        {
            return !string.IsNullOrWhiteSpace(nodeType) && _executors.ContainsKey(nodeType);
        }

        /// <summary>
        /// Gets all registered node types
        /// </summary>
        public IEnumerable<string> GetRegisteredNodeTypes()
        {
            return _executors.Keys.ToList();
        }

        /// <summary>
        /// Gets the count of registered nodes
        /// </summary>
        public int GetRegisteredNodeCount()
        {
            return _executors.Count;
        }

        /// <summary>
        /// Discovers and registers all node executors from the specified assembly
        /// </summary>
        public void DiscoverAndRegisterNodes(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            _logger.LogInformation("Discovering nodes in assembly: {AssemblyName}", assembly.GetName().Name);

            var nodeExecutorType = typeof(INodeExecutor);
            var nodeTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && nodeExecutorType.IsAssignableFrom(t))
                .ToList();

            foreach (var type in nodeTypes)
            {
                try
                {
                    // Try to create an instance (requires parameterless constructor for auto-discovery)
                    if (type.GetConstructor(Type.EmptyTypes) != null)
                    {
                        var executor = (INodeExecutor)Activator.CreateInstance(type);
                        RegisterNode(executor);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Cannot auto-register {TypeName} - no parameterless constructor found",
                            type.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register node type: {TypeName}", type.Name);
                }
            }

            _logger.LogInformation("Discovered and registered {Count} nodes", nodeTypes.Count);
        }

        /// <summary>
        /// Unregisters a node type
        /// </summary>
        public bool UnregisterNode(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                return false;

            var executorRemoved = _executors.TryRemove(nodeType, out _);
            var definitionRemoved = _definitions.TryRemove(nodeType, out _);

            if (executorRemoved || definitionRemoved)
            {
                _logger.LogInformation("Unregistered node type: {NodeType}", nodeType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all registered nodes
        /// </summary>
        public void Clear()
        {
            _executors.Clear();
            _definitions.Clear();
            _logger.LogInformation("Cleared all registered nodes");
        }

        /// <summary>
        /// Gets the node type from an executor using reflection
        /// </summary>
        private string GetNodeType(INodeExecutor executor)
        {
            // Try to get NodeType property
            var nodeTypeProperty = executor.GetType().GetProperty("NodeType");
            if (nodeTypeProperty != null && nodeTypeProperty.PropertyType == typeof(string))
            {
                return nodeTypeProperty.GetValue(executor) as string;
            }

            // Could also check for attributes here if needed
            // var attribute = executor.GetType().GetCustomAttribute<NodeTypeAttribute>();

            return null;
        }
    }
}

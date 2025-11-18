using System.Collections.Generic;
using System.Reflection;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Node registry interface for managing and retrieving node executors
    /// </summary>
    public interface INodeRegistry
    {
        /// <summary>
        /// Registers a node executor with its definition
        /// </summary>
        void RegisterNode(INodeExecutor executor, NodeDefinition definition);

        /// <summary>
        /// Registers a node executor (auto-discovers definition)
        /// </summary>
        void RegisterNode(INodeExecutor executor);

        /// <summary>
        /// Gets a node executor for the specified node type
        /// </summary>
        /// <param name="nodeType">The type of node to execute</param>
        /// <returns>The node executor instance</returns>
        INodeExecutor GetNodeExecutor(string nodeType);

        /// <summary>
        /// Gets the definition for a specific node type
        /// </summary>
        NodeDefinition GetNodeDefinition(string nodeType);

        /// <summary>
        /// Gets all registered node definitions
        /// </summary>
        IEnumerable<NodeDefinition> GetAllNodeDefinitions();

        /// <summary>
        /// Gets node definitions filtered by category
        /// </summary>
        IEnumerable<NodeDefinition> GetNodeDefinitionsByCategory(string category);

        /// <summary>
        /// Checks if a node type is registered
        /// </summary>
        bool IsNodeTypeRegistered(string nodeType);

        /// <summary>
        /// Gets all registered node types
        /// </summary>
        IEnumerable<string> GetRegisteredNodeTypes();

        /// <summary>
        /// Gets the count of registered nodes
        /// </summary>
        int GetRegisteredNodeCount();

        /// <summary>
        /// Discovers and registers all node executors from the specified assembly
        /// </summary>
        void DiscoverAndRegisterNodes(Assembly assembly);

        /// <summary>
        /// Unregisters a node type
        /// </summary>
        bool UnregisterNode(string nodeType);

        /// <summary>
        /// Clears all registered nodes
        /// </summary>
        void Clear();
    }
}

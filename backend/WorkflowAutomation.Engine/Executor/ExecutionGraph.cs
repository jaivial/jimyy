using System.Collections.Generic;
using System.Linq;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Executor
{
    /// <summary>
    /// Execution graph that manages node dependencies and execution order
    /// </summary>
    public class ExecutionGraph
    {
        private readonly Dictionary<string, GraphNode> _nodes = new();

        /// <summary>
        /// Adds a node to the execution graph with its dependencies
        /// </summary>
        /// <param name="id">The node ID</param>
        /// <param name="node">The node entity</param>
        /// <param name="dependencies">List of node IDs that this node depends on</param>
        public void AddNode(string id, Node node, List<string> dependencies)
        {
            _nodes[id] = new GraphNode
            {
                Id = id,
                Node = node,
                Dependencies = dependencies
            };
        }

        /// <summary>
        /// Gets a node by ID
        /// </summary>
        /// <param name="id">The node ID</param>
        /// <returns>The node entity</returns>
        public Node GetNode(string id) => _nodes[id].Node;

        /// <summary>
        /// Gets all root nodes (nodes with no dependencies)
        /// </summary>
        /// <returns>IDs of root nodes</returns>
        public IEnumerable<string> GetRootNodes()
        {
            return _nodes.Where(n => !n.Value.Dependencies.Any()).Select(n => n.Key);
        }

        /// <summary>
        /// Gets the next nodes that can be executed based on already executed nodes
        /// </summary>
        /// <param name="executedNodes">Set of node IDs that have already been executed</param>
        /// <returns>IDs of nodes ready to execute</returns>
        public IEnumerable<string> GetNextNodes(HashSet<string> executedNodes)
        {
            return _nodes
                .Where(n => !executedNodes.Contains(n.Key) &&
                           n.Value.Dependencies.All(d => executedNodes.Contains(d)))
                .Select(n => n.Key);
        }

        /// <summary>
        /// Internal class representing a node in the execution graph
        /// </summary>
        private class GraphNode
        {
            public string Id { get; set; } = null!;
            public Node Node { get; set; } = null!;
            public List<string> Dependencies { get; set; } = null!;
        }
    }
}

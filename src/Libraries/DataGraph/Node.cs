using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataGraph
{
	/// <summary>
	/// Base class for all Node classes.
	/// </summary>
	public abstract class Node
	{
	    private int level = 0;
	    private uint index = 0;

        /// <summary>
		/// Gets the level.
		/// </summary>
		/// <value>The level.</value>
		public int Level
        {
			get { return level; }
		}

		/// <summary>
		/// Gets the Node which is the parent of this node.
		/// </summary>
		/// <value>A node, or null if the node is a root node.</value>
		public Node Parent{ get; internal set; }

        public abstract object Data { get; }

        public uint Index { get { return index; } }

		internal Node(Node parent = null, uint index = 0)
        {
			Parent = parent;
		    level = parent != null ? parent.level + 1 : 0;
		    this.index = index;
        }

        /// <summary>
        /// Returns the depth of the graph from this node.
        /// </summary>
        /// <returns>The depth.</returns>
	    public abstract int Depth();

	    public void UpdateParent(Node newParent)
	    {
	        Parent = newParent;
	        level = Parent.Level + 1;
	    }
	}

    /// <summary>
    /// A node which represents an array.
    /// </summary>
    public class ArrayNode : Node
    {
        private readonly List<Node> children = new List<Node>();

        public ArrayNode(IEnumerable data, Node parent = null, uint index = 0) : base(parent, index)
        {
            uint count = 0;
            foreach (var item in data)
            {
                var enumerable = item as IEnumerable;
                if (enumerable != null && enumerable.GetType() != typeof (string))
                {
                    children.Add(new ArrayNode(enumerable, this, count));
                }
                else
                {
                    children.Add(new LeafNode(item, this, count));
                }
                
                count++;
            }
        }

        /// <summary>
        /// Traverse up the tree to find the root.
        /// </summary>
        public ArrayNode GetRoot()
        {
            if (Parent == null)
            {
                return this;
            }

            var root = Parent;
            while (root.Parent != null)
            {
                root = root.Parent;
            }
            return root as ArrayNode;
        }

        /// <summary>
        /// A collection of Nodes which are children of this node.
        /// </summary>
        public IEnumerable<Node> Children
        {
            get
            {
                return children;
            }
        }

        public override int Depth()
        {
            var nodes = GetAllLeafNodes();
            return nodes.Max(n => n.Level + 1);
        }

        /// <summary>
        /// Gets the data at the specified level from node.
        /// 
        ///        [] 	l0
        ///       / \
        ///      o  [] 	l1
        ///        /|\
        ///       o o o l2
        /// 
        /// If the level specified is less than zero, a node is 
        /// added to the root of the tree.
        /// 
        /// </summary>
        /// <returns>The data at level from node.</returns>
        /// <param name="root">The node from which the data gathering will begin.</param>
        /// <param name="level">The level specified starting at -1(leaves), and continuing -2, -3, etc. </param>
        public IEnumerable<object> GetDataAtLevel(int level)
        {
            if (level >= Depth())
            {
                throw new Exception("You cannot specify a level deeper than the depth of the tree.");
            }

            var nodes = new List<Node>();
            GetNodesAtLevel(this, ref nodes, level);
            return nodes.Select(n => n.Data);
        }

        /// <summary>
        /// Get all the nodes at the specified level, starting at the specified node.
        /// 
        /// If the level specified is a negative number then the root node will be replaced
        /// with an additional root node.
        /// </summary>
        private static void GetNodesAtLevel(Node node, ref List<Node> gatheredNodes, int level)
        {
            while (true)
            {
                if (node.Level == level)
                {
                    gatheredNodes.Add(node);
                    return;
                }

                if (level < 0)
                {
                    var count = 0;
                    var currentRoot = node;
                    while (count > level)
                    {
                        var newNode = new ArrayNode(new[] {currentRoot.Data});
                        currentRoot.Parent = newNode;
                        currentRoot = newNode;
                        count = count - 1;
                    }
                    gatheredNodes.Add(currentRoot);
                    return;
                }

                if (node is ArrayNode)
                {
                    var arrNode = (ArrayNode) node;
                    foreach (var n in arrNode.children)
                    {
                        GetNodesAtLevel(n, ref gatheredNodes, level);
                    }
                }

                break;
            }
        }

        /// <summary>
        /// Get all leaf nodes from the specified node.
        /// </summary>
        public IEnumerable<LeafNode> GetAllLeafNodes()
        {
            var gatheredLeaves = new List<LeafNode>();
            GetAllLeafNodesImpl(this, ref gatheredLeaves);
            return gatheredLeaves;
        }

        private static void GetAllLeafNodesImpl(Node node, ref List<LeafNode> gatheredLeaves)
        {
            var item = node as LeafNode;
            if (item != null)
            {
                gatheredLeaves.Add(item);
            }
            else
            {
                foreach (var c in ((ArrayNode)node).children)
                {
                    GetAllLeafNodesImpl(c, ref gatheredLeaves);
                }
            }
        }

        /// <summary>
        /// Get all data associated with leaf nodes from the specified node.
        /// </summary>
        /// <returns>The all leaf data.</returns>
        public IEnumerable<object> GetAllLeafData()
        {
            var leaves = GetAllLeafNodes();
            return leaves.Select(n => n.Data);
        }

        /// <summary>
        /// Replace a child of this node with a new node.
        /// </summary>
        private void ReplaceChild(Node existingNode, Node newNode)
        {
            var foundNode = children.FirstOrDefault(c => c == existingNode);
            if (foundNode == null)
            {
                throw new Exception("The specified node could not be found in the parent's collection.");
            }
            var index = children.IndexOf(foundNode);
            children.Remove(existingNode);
            children.Insert(index, newNode);
            newNode.UpdateParent(this);
        }

        /// <summary>
        /// Null all nodes at the specified level.
        /// </summary>
        public void NullAtLevel(int level)
        {
            var nodesAtLevel = new List<Node>();
            GetNodesAtLevel(this, ref nodesAtLevel, level);

            foreach (var n in nodesAtLevel)
            {
                var parent = (ArrayNode) n.Parent;
                parent.ReplaceChild(n, new LeafNode(null, parent, n.Index));
            }
        }

        /// <summary>
        /// Overwrite the data at the specified level with the nodes
        /// from the overwriteNode.
        /// </summary>
        public void OverwriteDataAtLevel(Node overwriteNode, int level)
        {
            var nodesAtLevel = new List<Node>();
            GetNodesAtLevel(this, ref nodesAtLevel, level);

            var overwriteNodes = new List<Node>();
            GetNodesAtLevel(overwriteNode, ref overwriteNodes, 1);

            for (var i = 0; i < nodesAtLevel.Count; i++)
            {
                if (i >= overwriteNodes.Count)
                {
                    break;
                }

                var nodeToReplace = nodesAtLevel[i];
                var parentForReplace = (ArrayNode) nodeToReplace.Parent;
                parentForReplace.ReplaceChild(nodeToReplace, overwriteNodes[i]);
            }
        }

        private IEnumerable<object> ToEnumerable()
        {
            var result = new List<object>();

            foreach (var c in children)
            {
                if (c is ArrayNode)
                {
                    result.AddRange(new[] {((ArrayNode) c).ToEnumerable()});
                }
                else { result.Add(((LeafNode)c).Data);}
            }

            return result;
        }

        /// <summary>
        /// Recursively build a data structure representing all 
        /// the data contained in Nodes which are children of this node.
        /// </summary>
        public override object Data
        {
            get
            {
                return ToEnumerable();
            }
        }
    }

    /// <summary>
    /// A node which represents a piece of data.
    /// </summary>
    public class LeafNode : Node
    {
        /// <summary>
        /// Gets the data associated with this node.
        /// </summary>
        public override object Data { get; }

        public LeafNode(object data, Node parent = null, uint index = 0) : base(parent, index)
        {
            Data = data;
        }

        public override int Depth()
        {
            return 1;
        }
    }
}


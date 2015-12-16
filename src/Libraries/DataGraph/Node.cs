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
            set { level = value; }
		}

		/// <summary>
		/// Gets the Node which is the parent of this node.
		/// </summary>
		/// <value>A node, or null if the node is a root node.</value>
		public ArrayNode Parent{ get; internal set; }

        public abstract object Data { get; }

        public uint Index { get { return index; } }

		internal Node(ArrayNode parent = null, uint index = 0)
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
	}

    /// <summary>
    /// A node which represents an array.
    /// </summary>
    public class ArrayNode : Node
    {
        private readonly List<Node> children = new List<Node>();

        public ArrayNode(IEnumerable data, ArrayNode parent = null, uint index = 0) : base(parent, index)
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
            children[index] = newNode;

            newNode.Parent = this;
            newNode.Level = newNode.Parent.Level + 1;
        }

        private void ReplaceChildAtIndex(int index, Node newNode)
        {
            var childToReplace = children[index];
            ReplaceChild(childToReplace, newNode);
        }

        internal void RemoveChild(Node nodeToRemove)
        {
            children.Remove(nodeToRemove);
        }

        internal void AddChild(Node node)
        {
            children.Add(node);
            node.Parent = this;
            node.Level = Level + 1;
        }

        /// <summary>
        /// Sets any leaf node at the specified level to null
        /// then traverses down the tree setting all leaf
        /// nodes to null.
        /// </summary>
        public void NullAtLevelAndBelow(int level)
        {
            var nodesAtLevel = new List<Node>();
            GetNodesAtLevel(this, ref nodesAtLevel, level);

            foreach (var n in nodesAtLevel)
            {
                TraverseDownAndSetDataAtLeaf(n, null);
            }
        }

        internal static void TraverseDownAndSetDataAtLeaf(Node n, object data)
        {
            var leafNode = n as LeafNode;
            if (leafNode != null)
            {
                leafNode.SetData(data);
                return;
            }

            var arrayNode = n as ArrayNode;
            if (arrayNode != null)
            {
                foreach (var c in arrayNode.Children)
                {
                    TraverseDownAndSetDataAtLeaf(c, data);
                }
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

            // Get a collection of the parents to nodes at this level.
            // We do this lookup from the leaves to ensure that we only get
            // the parent nodes which correspond with the nodes at this level
            // and not all of the nodes at the level above.
            var nodeParents = nodesAtLevel.Select(n => n.Parent).Distinct().ToList();

            var overwriteNodes = new List<Node>();
            GetNodesAtLevel(overwriteNode, ref overwriteNodes, overwriteNode.Depth()-1);
            var overwriteParents = overwriteNodes.Select(n => n.Parent).Distinct().ToList();

            for (var i = 0; i < nodeParents.Count(); i++)
            {
                if (i >= overwriteParents.Count)
                {
                    break;
                }

                var replaceNode = nodeParents[i];
                for (var j = 0; j < overwriteNodes.Count(); j++)
                {
                    var newNode = overwriteNodes[j];
                    if (j >= replaceNode.Children.Count())
                    {
                        replaceNode.AddChild(newNode);
                    }
                    else
                    {
                        replaceNode.ReplaceChildAtIndex(j, newNode);
                    }
                }
            }
        }

        public static void SuperimposeFromNodeDown(Node startingNode, Node nodeToSuperimpose)
        {
            var leafNode = startingNode as LeafNode;
            var superimposeLeaf = nodeToSuperimpose as LeafNode;

            if (leafNode != null)
            {
                if (superimposeLeaf != null)
                {
                    // If the superimpose node is a leaf node too
                    // then just take its data. 
                    leafNode.SetData(superimposeLeaf.Data);
                    return;
                }
                else
                {
                    // We've got an array node in the superimposition.
                    // We need to swap out the leaf node in the original
                    // tree for an array node.
                    leafNode.Parent.ReplaceChild(leafNode, nodeToSuperimpose);
                    return;
                }
            }

            var startingArrNode = startingNode as ArrayNode;

            // If the starting node is an array node and the superimpose
            // node is a leaf - throw an exception
            if (superimposeLeaf != null)
            {
                throw new Exception("We can't superimpose a leaf onto an array.");
            }

            var superimposeArrNode = nodeToSuperimpose as ArrayNode;

            for (var i = 0; i < startingArrNode.Children.Count(); i++)
            {
                if (i >= superimposeArrNode.Children.Count())
                {
                    // The superimpose node doesn't have any 
                    // more data.
                    return;
                }

                // Set the cursor at the new starting location
                var newStartingNode = startingArrNode.Children.ElementAt(i);
                var newSuperimposeNode = superimposeArrNode.Children.ElementAt(i);

                // Recurse down the tree superimposing as you go!
                SuperimposeFromNodeDown(newStartingNode, newSuperimposeNode);
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
        private object leafData;

        /// <summary>
        /// Gets the data associated with this node.
        /// </summary>
        public override object Data { get { return leafData; } }

        public LeafNode(object data, ArrayNode parent = null, uint index = 0) : base(parent, index)
        {
            leafData = data;
        }

        public override int Depth()
        {
            return 1;
        }

        public void SetData(object newValue)
        {
            leafData = newValue;
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DataGraph
{
    /// <summary>
    /// Base class for all Node classes.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
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
    [IsVisibleInDynamoLibrary(false)]
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
        internal void ReplaceChild(Node existingNode, Node newNode)
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

        internal void AddChild(Node node)
        {
            children.Add(node);
            node.Parent = this;
            node.Level = Level + 1;
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
    [IsVisibleInDynamoLibrary(false)]
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

    public static class DataGraph
    {
        /// <summary>
        /// Given a collection of data, a tree is built. The tree is overwritten with nulls from the specified level down. 
        /// Then the tree is then superimposed with data from the second collection, starting at the specified level.
        /// </summary>
        /// <param name="data">The base data which will be overwritten.</param>
        /// <param name="superimposeData">The data which will overwrite.</param>
        /// <param name="level">The level at which to begin the overwrite, specified from the leaves, starting at -1.</param>
        /// <param name="overwriteLowerLevelsWithNulls">If True, data at the specified level and below will be overwritten with nulls.</param>
        public static object SuperimposeDataAtLevel([ArbitraryDimensionArrayImport]object data, [ArbitraryDimensionArrayImport]object superimposeData, 
            int level, bool overwriteLowerLevelsWithNulls = true)
        {
            // Clone the node.
            var baseNode = FromData(data);
            var convertedLevel = baseNode.Depth() + level;

            if (overwriteLowerLevelsWithNulls)
            {
                // Null the data at the level and below.
                NullAtLevelAndBelow(baseNode, convertedLevel);
            }

            // Superimpose new data on the node.
            var superimposeNode = FromData(superimposeData);
            OverwriteDataAtLevel(baseNode, superimposeNode, convertedLevel);

            return baseNode.Data;
        }

        /// <summary>
        /// Constructs a tree of the specified data, then gets a collection 
        /// containing all the data at the specified level.
        /// </summary>
        /// <param name="data">An object representing the data.</param>
        /// <param name="level">A level specified from the leaves starting at -1.</param>
        /// <returns></returns>
        public static IList GetDataAtLevel([ArbitraryDimensionArrayImport]object data, int level)
        {
            var node = FromData(data);
            return GetDataAtLevel(node, node.Depth() + level);
        }

        #region internal methods

        /// <summary>
        /// Create a Data Graph Node from data.
        /// </summary>
        /// <param name="data">The data to construct the node.</param>
        /// <returns>A LeafNode if the object is singular and an ArrayNode if the object is a collection.</returns>
        private static Node FromData([ArbitraryDimensionArrayImport] object data)
        {
            Node node;
            var enumerable = data as IEnumerable;
            if (enumerable != null && enumerable.GetType() != typeof(string))
            {
                node = new ArrayNode(enumerable);
            }
            else
            {
                node = new LeafNode(data);
            }

            return node;
        }

        /// <summary>
        /// Clone a node.
        /// </summary>
        /// <param name="node">The node to clone.</param>
        /// <returns>A Node.</returns>
        private static Node FromNode(Node node)
        {
            return FromData(node.Data);
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
        internal static IList GetDataAtLevel(Node node, int level)
        {
            if (level >= node.Depth())
            {
                throw new Exception("You cannot specify a level deeper than the depth of the tree.");
            }

            var nodes = new List<Node>();
            GetNodesAtLevel(node, ref nodes, level);
            return nodes.Select(n => n.Data).ToList();
        }

        /// <summary>
        /// Get all the nodes at the specified level, starting at the specified node.
        /// 
        /// If the level specified is a negative number then the root node will be replaced
        /// with an additional root node.
        /// </summary>
        internal static void GetNodesAtLevel(Node node, ref List<Node> gatheredNodes, int level)
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
                        var newNode = new ArrayNode(new[] { currentRoot.Data });
                        currentRoot.Parent = newNode;
                        currentRoot = newNode;
                        count = count - 1;
                    }
                    gatheredNodes.Add(currentRoot);
                    return;
                }

                if (node is ArrayNode)
                {
                    var arrNode = (ArrayNode)node;
                    foreach (var n in arrNode.Children)
                    {
                        GetNodesAtLevel(n, ref gatheredNodes, level);
                    }
                }

                break;
            }
        }

        /// <summary>
        /// Sets any leaf node at the specified 
        /// level to null, then traverses down the tree setting all leaf
        /// nodes to null.
        /// </summary>
        internal static void NullAtLevelAndBelow(Node node, int level)
        {
            var nodesAtLevel = new List<Node>();
            GetNodesAtLevel(node, ref nodesAtLevel, level);
            foreach (var n in nodesAtLevel)
            {
                ArrayNode.TraverseDownAndSetDataAtLeaf(n, null);
            }
        }

        /// <summary>
        /// Overwrite the data at the specified level with the nodes
        /// from the overwriteNode.
        /// </summary>
        internal static void OverwriteDataAtLevel(Node node, Node overwriteNode, int level)
        {
            var nodesAtLevel = new List<Node>();
            GetNodesAtLevel(node, ref nodesAtLevel, level);

            // Get a collection of the parents to nodes at this level.
            // We do this lookup from the leaves to ensure that we only get
            // the parent nodes which correspond with the nodes at this level
            // and not all of the nodes at the level above.
            var nodeParents = nodesAtLevel.Select(n => n.Parent).Distinct().ToList();

            var overwriteNodes = new List<Node>();
            GetNodesAtLevel(overwriteNode, ref overwriteNodes, (int) 1);
            var overwriteParents = overwriteNodes.Select(n => n.Parent).Distinct().ToList();

            for (var i = 0; i < nodeParents.Count(); i++)
            {
                if (i >= overwriteParents.Count())
                {
                    break;
                }

                DataGraph.SuperimposeFromNodeDown(nodeParents[i], overwriteParents.ElementAt(i));
            }
        }

        internal static void SuperimposeFromNodeDown(Node startingNode, Node nodeToSuperimpose)
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
                throw new Exception("The requested operation would cause the replacement of an array node with a leaf node. This is not supported.");
            }

            var superimposeArrNode = nodeToSuperimpose as ArrayNode;

            var width = Math.Max(startingArrNode.Children.Count(), superimposeArrNode.Children.Count());

            for (var i = 0; i < width; i++)
            {
                if (i >= superimposeArrNode.Children.Count())
                {
                    // The superimpose node doesn't have any 
                    // more data.
                    return;
                }

                var newSuperimposeNode = superimposeArrNode.Children.ElementAt(i);
                if (i >= startingArrNode.Children.Count())
                {
                    // If we have more to superimpose than we
                    // have slots allowed, add the node.
                    startingArrNode.AddChild(newSuperimposeNode);
                }
                else
                {
                    // Set the cursor at the new starting location
                    var newStartingNode = startingArrNode.Children.ElementAt(i);

                    // Recurse down the tree superimposing as you go!
                    SuperimposeFromNodeDown(newStartingNode, newSuperimposeNode);
                }
            }
        }

        #endregion
    }
}


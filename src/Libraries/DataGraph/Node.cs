using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataGraph
{
	/// <summary>
	/// The Node class represents a nested array structure as
	/// a tree where the nodes are arrays and the leaves are single objects.
	/// </summary>
	public class Node{

		/// <summary>
		/// Gets the address of the node in the tree as an array of 
		/// ints representing the location of the node in each sub-array.
		/// 
		/// For example, 
		/// </summary>
		/// <value>The address.</value>
		public List<uint> Address{ get;}

		/// <summary>
		/// Gets the level.
		/// </summary>
		/// <value>The level.</value>
		public int Level{
			get{return Address.Count;}
		}

		/// <summary>
		/// Gets the Node which is the parent of this node.
		/// </summary>
		/// <value>A node, or null if the node is a root node.</value>
		public Node Parent{ get; internal set;}

		/// <summary>
		/// Gets nodes which are children of this node.
		/// </summary>
		/// <value>The children.</value>
		public List<Node> Children { get;}

		/// <summary>
		/// Gets a value indicating whether this instance is root.
		/// </summary>
		/// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
		public bool IsRoot{
			get{return Parent == null;}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has children.
		/// </summary>
		/// <value><c>true</c> if this instance has children; otherwise, <c>false</c>.</value>
		public bool IsLeaf{
			get{return Children.Count == 0;}
		}

		/// <summary>
		/// The number of levels from this node to the leaves.
		/// </summary>
		/// <value>The depth.</value>
		public int Depth{ get;}

		/// <summary>
		/// Gets the data associated with this node.
		/// </summary>
		/// <value>A reference to the original object or IEnumerable
		/// stored at this position.</value>
		public object Data{ get; private set;}

		internal Node(object data, Node parent = null, uint index = 0){

			Parent = parent;
			Data = data;

			Children = new List<Node> ();

			Address = new List<uint> ();
			if(parent != null){
				Address.AddRange (parent.Address);
			}
			Address.Add (index);

			if(data is IEnumerable && data.GetType() != typeof(string)){
				uint count = 0;
				foreach(var item in (IEnumerable)data){
					Children.Add (new Node(item, this, count));
					count++;
				}
			}

			int depth = 0;
			GetMaxDepth (ref depth,0);
			Depth = depth;
		}

		public static Node FromData(object data)
		{
			if(data == null){
				throw new Exception ("A node cannot be created with null data.");
			}

			return new Node (data);
		}
		
		/// <summary>
		/// Returns the depth of the graph from this node.
		/// </summary>
		/// <returns>The depth.</returns>
		private void GetMaxDepth(ref int maxDepth, int currentDepth){
			var newDepth = currentDepth+1;

			if(Children.Count == 0){
				maxDepth = Math.Max (newDepth, maxDepth);
			}
			foreach(var node in Children){
				node.GetMaxDepth (ref maxDepth, newDepth);
			}

		}

		/// <summary>
		/// Traverse up the tree to find the root.
		/// </summary>
		/// <returns>The root.</returns>
		public Node GetRoot(){
			if(IsRoot){
				return this;
			}
			else{
				return Parent.GetRoot ();
			}
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
        /// If the Depth - level specified is less than zero, a node is 
        /// added to the root of the tree.
        /// 
        /// </summary>
        /// <returns>The data at level from node.</returns>
        /// <param name="root">The node from which the data gathering will begin.</param>
        /// <param name="level">The level specified starting at -1(leaves), and continuing -2, -3, etc. </param>
        public IEnumerable<object> GetDataAtLevel(int level)
		{
            if (level > -1)
            {
                throw new Exception("Levels are specified starting from the leaves (-1) and continuing -2, -3, etc.");
            }
			var nodes = new List<Node> ();
			GetNodesAtLevel (this, ref nodes, Depth + 1 + level);
			return nodes.Select (n => n.Data);
		}
			
		/// <summary>
		/// Get all the nodes at the specified level, starting at the specified node.
		/// 
		/// If the level specified is a negative number then the root node will be replaced
		/// with an additional root node.
		/// </summary>
		private static void GetNodesAtLevel(Node node, ref List<Node> gatheredNodes, int level){
			if(level < 0){
				var count = 0;
				Node currentRoot = node;
				while(count > level)
				{
					var newNode = new Node (new[]{currentRoot.Data});
					currentRoot.Parent = newNode;
					currentRoot = newNode;
					count=count-1;
				}
				gatheredNodes.Add (currentRoot);
				return;
			}

			if(node.Level == level){
				gatheredNodes.Add (node);
				return;
			}else{
				foreach (var n in node.Children) {
					GetNodesAtLevel (n, ref gatheredNodes, level);
				}
			}
		}

		/// <summary>
		/// Get all leaf nodes from the specified node.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="gatheredLeaves">Gathered leaves.</param>
		public List<Node> GetAllLeafNodes(){
			var gatheredLeaves = new List<Node>();
			GetAllLeafNodesImpl (this, ref gatheredLeaves);
			return gatheredLeaves;
		}

		private static void GetAllLeafNodesImpl(Node node, ref List<Node> gatheredLeaves){
			if(node.IsLeaf){
				gatheredLeaves.Add (node);
			}

			foreach(var c in node.Children){
				GetAllLeafNodesImpl (c, ref gatheredLeaves);
			}
		}

		/// <summary>
		/// Get all data associated with leaf nodes from the specified node.
		/// </summary>
		/// <returns>The all leaf data.</returns>
		/// <param name="node">Node.</param>
		public static IEnumerable<object> GetAllLeafData(Node node){
			var leaves = node.GetAllLeafNodes ();
			return leaves.Select (n => n.Data);
		}

		/// <summary>
		/// Clones the node and returns a node with the same topology, 
		/// but full of nulls.
		/// </summary>
		public Node CloneWithNulls()
        {
			var newNode = (Node)this.MemberwiseClone ();
			newNode.NullData();
			return newNode;
		}

        /// <summary>
        /// Clones the node and returns a node with the same topology,
        /// with nulls at all positions on a given level.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
	    public Node CloneWithNullsAtLevel(int level)
	    {
	        var newNode = (Node) this.MemberwiseClone();
            newNode.NullDataAtLevel(Depth + 1 + level);
	        return newNode;
	    }

		private void NullData()
		{
		    Data = Children.Count == 0 ? null : 
                Enumerable.Repeat<object> (null, Children.Count);

		    foreach(var c in Children){
				c.NullData ();
			}
		}

	    private void NullDataAtLevel(int level)
	    {
	        if (Level == level)
	        {
	            Data = Children.Count == 0 ? null : 
                    Enumerable.Repeat<object>(null, Children.Count);
            }

	        foreach (var c in Children)
	        {
	            c.NullDataAtLevel(level);
	        }
	    }

		/// <summary>
		/// Give a node with a different structure, update the structure of 
		/// this node to match, filling in previously non-existing branches with nulls.
		/// 
		/// If the structure to superimpose is a subset of this node's graph,
		/// then no changes will be made.
		/// </summary>
		public void Superimpose(Node structure){


		}
			
		public override string ToString ()
		{
			return string.Join (".",Address.Select (x => x.ToString ()));
		}


	}
}


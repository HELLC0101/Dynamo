using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using DataGraph;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace DataGraphTests
{
	public class DataGraphTests
	{
		private string[] arr1 = new[]{"A","B","C"}; 
		private int[] arr2 = new[]{0,1,2}; 
		private string[] arr3 = new[]{"a","b","c"};

		[Test]
		public void Level_SingleDimension(){
			var node = new ArrayNode(SingleDimensionalTestArray ());
			Assert.AreEqual (node.Level, 0);
			Assert.AreEqual (node.Children.First ().Level, 1);
		}

		[Test]
		public void Level_Jagged(){
			var node = new ArrayNode(JaggedTestArray ());
			Assert.AreEqual (node.Children.First ().Level, 1);
			Assert.AreEqual (node.Children.Last ().Level, 1);
		}

		[Test]
		public void Depth_SingleDimension(){
			var node1 = new ArrayNode(SingleDimensionalTestArray ());
			Assert.AreEqual (2, node1.Depth());
		}

		[Test]
		public void Depth_TwoDimension(){
			var node2 = new ArrayNode(TwoDimensionalTestArray ());
			Assert.AreEqual (3, node2.Depth());
		}

		[Test]
		public void Depth_Jagged(){
			var node2 = new ArrayNode(JaggedTestArray ());
			Assert.AreEqual (3, node2.Depth());
		}

		[Test]
		public void GetAllLeafData_SingleDimension()
		{
			var node = new ArrayNode(SingleDimensionalTestArray ());
			var data = node.GetAllLeafData ();
			Assert.AreEqual(data.Count (), 3);
		}

		[Test]
		public void GetAllLeafData_TwoDimension(){
			var node = new ArrayNode(TwoDimensionalTestArray ());
			var data = node.GetAllLeafData ();
			Assert.AreEqual(data.Count (), 6);
		}

		[Test]
		public void GetAllLeafData_Jagged(){
			var node = new ArrayNode(JaggedTestArray ());
			var data = node.GetAllLeafData ();
			Assert.AreEqual(data.Count (), 7);
		}

		[Test]
		public void GetDataAtLevel_Jagged(){
			var node = new ArrayNode(JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = Node.GetDataAtLevel (node, 1);
			Console.WriteLine ("@-2");
			Console.WriteLine (PrintData (data));
            Assert.AreEqual(data.First(), "foo");
		}

		[Test]
		public void GetDataAtLevel_ThreeDimension(){
			var node = new ArrayNode(ThreeDimensionalTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = Node.GetDataAtLevel(node, 3);
			Console.WriteLine ("@-1");
			Assert.AreEqual (data.First (), "a");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data.Last (), "c");
		}

		[Test]
		public void GetDataAtLevel_Jagged_AtLevel(){
			var node = new ArrayNode(JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = Node.GetDataAtLevel(node, 2);
			Console.WriteLine ("@-1");
			Assert.AreEqual (data.First (), arr1.First ());
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data.Last (), arr2.Last ());
		}

		[Test]
		public void GetDataAtLevel_Jagged_Leaves(){
			var node = new ArrayNode(JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = node.GetAllLeafData ();
			Console.WriteLine ("@leaves");
			Assert.AreEqual (data.First (), "foo");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data.Last (), arr2.Last ());
		}

		[Test]
		public void GetDataAtLevel_NegativeIndex(){
			var node = new ArrayNode(JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = Node.GetDataAtLevel(node, -1);
			Console.WriteLine ("@-1");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (4, node.GetRoot ().Depth());
		}

		[Test]
		public void GetDataAtLevel_DepthGreaterThanSize_ThrowsException(){
			var node = new ArrayNode(JaggedTestArray ());
			Assert.Throws<Exception> (()=> Node.GetDataAtLevel(node, 10));
		}

		[Test]
		public void GetDataAtLevel_Case1(){
			var node = new ArrayNode (Phase2 ());

			var data = Node.GetDataAtLevel(node, 0);
			Assert.AreEqual (data.Count (), 1);

			data = Node.GetDataAtLevel(node, 1);
			Assert.AreEqual (data.Count (), 2);

            data = Node.GetDataAtLevel(node, 2);
            Assert.AreEqual(data.Count(), 4);

            data = Node.GetDataAtLevel(node, 3);
			Assert.AreEqual (data.Count (), 12);
		}

		[Test]
		public void GetAllLeafData_IsLeaves_CorrectCount(){
			var node = new ArrayNode(JaggedTestArray ());
			var data = node.GetAllLeafData ();
			Assert.AreEqual(7, data.Count ());
		}

	    [Test]
	    public void NullAtLevel_1_JaggedArray()
	    {
	        var node = new ArrayNode(JaggedTestArray());
            Console.WriteLine(PrintData(node.Data));
            node.NullAtLevelAndBelow(1);
            Console.WriteLine("Nulled at level 1...");
            Console.WriteLine(PrintData(node.Data));
	        var leafData = node.GetAllLeafNodes().Select(n => n.Data);
            Assert.True(leafData.All(d=>d==null));
	    }

        [Test]
        public void NullAtLevel_2_JaggedArray()
        {
            var node = new ArrayNode(JaggedTestArray());
            Console.WriteLine(PrintData(node.Data));
            node.NullAtLevelAndBelow(2);
            Console.WriteLine("Nulled at level 2...");
            Console.WriteLine(PrintData(node.Data));
            var data = Node.GetDataAtLevel(node, 2);
            Assert.True(data.All(d=>d==null));
        }

        [Test]
        public void NullAtLevel_3_ThreeDimensionalArray()
        {
            var node = new ArrayNode(ThreeDimensionalTestArray());
            Console.WriteLine(PrintData(node.Data));
            node.NullAtLevelAndBelow(3);
            Console.WriteLine("Nulled at level 3...");
            Console.WriteLine(PrintData(node.Data));
            var data = Node.GetDataAtLevel(node, 3);
            Assert.True(data.All(d => d == null));
        }

        [Test]
	    public void OverwriteNodesAtLevel_JaggedArray_OverwriteWithSmallerData()
	    {
	        var node = new ArrayNode(JaggedTestArray());
            Console.WriteLine(PrintData(node.Data));
            node.NullAtLevelAndBelow(2);
	        var data = new[] { "foobar", "foobuzz"};
            var newNode = new ArrayNode(data);
            node.OverwriteDataAtLevel(newNode,2);
            Console.WriteLine(PrintData(node.Data));
	        Assert.AreEqual(Node.GetDataAtLevel(node, 2).First(), "foobar");
	    }

        [Test]
        public void OverwriteNodesAtLevel_JaggedArray_OverwriteWithLargerData()
        {
            var node = new ArrayNode(JaggedTestArray());
            Console.WriteLine(PrintData(node.Data));
            node.NullAtLevelAndBelow(2);
            var data = new[] { "foobar", "foobuzz", "foobarbuzz", "foobuzzbar" };
            var newNode = new ArrayNode(data);
            node.OverwriteDataAtLevel(newNode, 2);
            Console.WriteLine(PrintData(node.Data));
            Assert.AreEqual(Node.GetDataAtLevel(node, 2).First(), "foobar");
        }

	    [Test]
	    public void Phase5()
	    {
	        var data1 = new[]
	        {
	            new[]
	            {
	                new[] {"A", "B", "C"},
	                new[] {"D", "E", "F"}
	            },
	            new[]
	            {
	                new[] {"a", "b", "c"},
	                new[] {"d", "e", "f"}
	            }
	        };

            var node1 = new ArrayNode(data1);

            var data2 = new[]
            {
                new[]{"A0", "B1"},
                new[] {"D5", "E6"}
            };

            var node2 = new ArrayNode(data2);

            Console.WriteLine(PrintData(node1.Data));
            node1.NullAtLevelAndBelow(2);
            node1.OverwriteDataAtLevel(node2, 2);
            Console.WriteLine(PrintData(node1.Data));
            Assert.Pass();
        }

	    [Test]
	    public void SuperImposeFromNodeDown_AllGood()
	    {
	        var data1 = new[] {arr1, arr1};
	        var data2 = new[] {0, 1, 2};
            var node1 = new ArrayNode(data1);
            Console.WriteLine(PrintData(node1.Data));
            var node2 = new ArrayNode(data2);
            Node.SuperimposeFromNodeDown(node1.Children.First(), node2);
            Console.WriteLine(PrintData(node1.Data));
	        var data = Node.GetDataAtLevel(node1, node1.Depth() - 1);
            Assert.AreEqual(data.First(), 0);
            Assert.AreEqual(data.Last(), "C");
        }

        [Test]
        public void SuperImposeFromNodeDown_SuperimposeStartsWithLeaf_ThrowsException()
        {
            var data1 = new[] { arr1, arr1 };
            var data2 = 0;
            var node1 = new ArrayNode(data1);
            Console.WriteLine(PrintData(node1.Data));
            var node2 = new LeafNode(data2);
            Assert.Throws<Exception>(() => Node.SuperimposeFromNodeDown(node1.Children.First(), node2));
        }

        [Test]
        public void SuperImposeFromNodeDown_SuperimposeReplacesLeafWithArray()
        {
            var data1 = new[] { arr1, arr1 };
            var data2 = new ArrayList(){ 0, 1, new [] {2,3} };
            var node1 = new ArrayNode(data1);
            Console.WriteLine(PrintData(node1.Data));
            var node2 = new ArrayNode(data2);
            Node.SuperimposeFromNodeDown(node1.Children.First(), node2);
            Console.WriteLine(PrintData(node1.Data));
            var data = Node.GetDataAtLevel(node1, node1.Depth() - 1);
            Assert.AreEqual(data.First(), 0);
            Assert.AreEqual(data.Last(), "C");
        }

        private IEnumerable SingleDimensionalTestArray(){
			return arr1;
		}

		private IEnumerable TwoDimensionalTestArray(){
			return new List<object>(){arr1,arr2};
		}

		private IEnumerable ThreeDimensionalTestArray(){
			return new List<object>{arr1,arr2,new[]{arr3, arr3}};
		}
			
		private IEnumerable JaggedTestArray(){
			return new List<object>{ "foo", arr1, arr2 };
		}

		private IEnumerable Phase2(){
			return new [] {
				new[] {
					new[]{ "A", "B", "C" },
					new[]{ "D", "E", "F" },
				},
				new[] {
					new[]{ "a", "b", "c" },
					new[]{ "d", "e", "f" },
				},
			};
		}

		private string PrintData(object data){
			var jsonString = JsonConvert.SerializeObject (
				data, Formatting.Indented, new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
			return jsonString;
		}
	}
}


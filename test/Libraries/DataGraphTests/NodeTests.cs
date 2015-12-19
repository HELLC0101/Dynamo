using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using DataGraph;
using System.Linq;
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
			var data = DataGraph.DataGraph.GetDataAtLevel (node, 1);
			Console.WriteLine ("@-2");
			Console.WriteLine (PrintData (data));
            Assert.AreEqual(data[0], "foo");
		}

		[Test]
		public void GetDataAtLevel_ThreeDimension(){
			var node = new ArrayNode(ThreeDimensionalTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = DataGraph.DataGraph.GetDataAtLevel(node, 3);
			Console.WriteLine ("@-1");
			Assert.AreEqual (data[0], "a");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data[data.Count-1], "c");
		}

		[Test]
		public void GetDataAtLevel_Jagged_AtLevel(){
			var node = new ArrayNode(JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = DataGraph.DataGraph.GetDataAtLevel(node, 2);
			Console.WriteLine ("@-1");
			Assert.AreEqual (data[0], arr1.First ());
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data[data.Count-1], arr2.Last ());
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
			var data = DataGraph.DataGraph.GetDataAtLevel(node, -1);
			Console.WriteLine ("@-1");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (4, node.GetRoot ().Depth());
		}

		[Test]
		public void GetDataAtLevel_DepthGreaterThanSize_ThrowsException(){
			var node = new ArrayNode(JaggedTestArray ());
			Assert.Throws<Exception> (()=> DataGraph.DataGraph.GetDataAtLevel(node, 10));
		}

		[Test]
		public void GetDataAtLevel_Case1(){
			var node = new ArrayNode (Phase2 ());

			var data = DataGraph.DataGraph.GetDataAtLevel(node, 0);
			Assert.AreEqual (data.Count, 1);
            
			data = DataGraph.DataGraph.GetDataAtLevel(node, 1);
			Assert.AreEqual (data.Count, 2);

            data = DataGraph.DataGraph.GetDataAtLevel(node, 2);
            Assert.AreEqual(data.Count, 4);

            data = DataGraph.DataGraph.GetDataAtLevel(node, 3);
			Assert.AreEqual (data.Count, 12);
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
            DataGraph.DataGraph.NullAtLevelAndBelow(node, 1);
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
            DataGraph.DataGraph.NullAtLevelAndBelow(node, 2);
            Console.WriteLine("Nulled at level 2...");
            Console.WriteLine(PrintData(node.Data));
            var data = DataGraph.DataGraph.GetDataAtLevel((Node)node, (int) 2);
            foreach (var d in data)
            {
                Assert.True(d == null);
            }
        }

        [Test]
        public void NullAtLevel_3_ThreeDimensionalArray()
        {
            var node = new ArrayNode(ThreeDimensionalTestArray());
            Console.WriteLine(PrintData(node.Data));
            DataGraph.DataGraph.NullAtLevelAndBelow(node, 3);
            Console.WriteLine("Nulled at level 3...");
            Console.WriteLine(PrintData(node.Data));
            var data = DataGraph.DataGraph.GetDataAtLevel((Node)node, (int) 3);
            foreach (var d in data)
            {
                Assert.True(d == null);
            }
        }

        [Test]
	    public void OverwriteNodesAtLevel_JaggedArray_OverwriteWithSmallerData()
	    {
	        var node = new ArrayNode(JaggedTestArray());
            Console.WriteLine(PrintData(node.Data));
            DataGraph.DataGraph.NullAtLevelAndBelow(node, 2);
	        var data = new[] { "foobar", "foobuzz"};
            var overwriteNode = new ArrayNode(data);
            DataGraph.DataGraph.OverwriteDataAtLevel(node, overwriteNode, 2);
            Console.WriteLine(PrintData(node.Data));
	        Assert.AreEqual(DataGraph.DataGraph.GetDataAtLevel((Node)node, (int) 2)[0], "foobar");
	    }

        [Test]
        public void OverwriteNodesAtLevel_JaggedArray_OverwriteWithLargerData()
        {
            var node = new ArrayNode(JaggedTestArray());
            Console.WriteLine(PrintData(node.Data));
            DataGraph.DataGraph.NullAtLevelAndBelow(node, 2);
            var data = new[] { "foobar", "foobuzz", "foobarbuzz", "foobuzzbar" };
            var overwriteNode = new ArrayNode(data);
            DataGraph.DataGraph.OverwriteDataAtLevel(node, overwriteNode, 2);
            Console.WriteLine(PrintData(node.Data));
            Assert.AreEqual(DataGraph.DataGraph.GetDataAtLevel((Node)node, (int) 2)[0], "foobar");
        }

        [Test]
        public void OverwriteNodesAtLevel_JaggedArray_OverwriteTwoDimensionalData()
        {
            var node = new ArrayNode(JaggedTestArray());
            DataGraph.DataGraph.NullAtLevelAndBelow(node, 2);
            var data = new[] { new[] { "foobar", "foobuzz"}, new[] {"foobarbuzz", "foobuzzbar" }};
            var overwriteNode = new ArrayNode(data);
            DataGraph.DataGraph.OverwriteDataAtLevel(node, overwriteNode, 2);
            Console.WriteLine(PrintData(node.Data));
            var result = DataGraph.DataGraph.GetDataAtAddress(node.Data, new[] {0, 1, 0, 0});
            Assert.AreEqual(result , "foobar");
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
            DataGraph.DataGraph.NullAtLevelAndBelow(node1, 2);
            DataGraph.DataGraph.OverwriteDataAtLevel(node1, node2, 2);
            Console.WriteLine(PrintData(node1.Data));
            Assert.Pass();
        }

	    [Test]
	    public void SuperimposeDataAtLevel_StartsWithLeaf_ThrowsException()
	    {
            // Provide a square array.
            // Attempt to overwrite with a leaf node.
            // This would result in data loss.
            var data1 = new[] { arr1, arr1 };
            var data2 = new []{0};
            Assert.Throws<Exception>(() => DataGraph.DataGraph.SuperimposeDataAtLevel(data1, data2, -2));
        }

        [Test]
        public void SuperimposeDataAtLevel_ReplacesLeafWithArray()
        {
            var data1 = new[] { arr1, arr1 };
            var data2 = new ArrayList() { 0, 1, new[] { 2, 3 } };
            Console.WriteLine(PrintData(data1));

            var data = DataGraph.DataGraph.SuperimposeDataAtLevel(data1, data2, -1, false);
            Console.WriteLine(PrintData(data));

            var result1 = DataGraph.DataGraph.GetDataAtAddress(data, new []{ 0, 0, 0});
            var result2 = DataGraph.DataGraph.GetDataAtAddress(data, new []{ 0, 1, 2 });

            Assert.AreEqual(result1, 0);
            Assert.AreEqual(result2, "C");
        }

        [Test]
	    public void SuperimposeDataAtLevel_LargerData_OverwritesExistingAndExpandsIntoNextCollection()
	    {
            var data = new[] {"foobar", "foobuzz", "foobarbuzz", "foobuzzbar" };
            var result = DataGraph.DataGraph.SuperimposeDataAtLevel(JaggedTestArray(), data, -1);

            Console.WriteLine(PrintData(result));

            var result1 = DataGraph.DataGraph.GetDataAtAddress(result, new[] { 0, 1,0 });
            Assert.AreEqual(result1, "foobar");

            var result2 = DataGraph.DataGraph.GetDataAtAddress(result, new[] { 0, 1, 2 });
            Assert.AreEqual(result2, "foobarbuzz");

            var result3 = DataGraph.DataGraph.GetDataAtAddress(result, new[] { 0, 2, 0 });
            Assert.AreEqual(result3, "foobuzzbar");
        }

        [Test]
        public void SuperimposeDataAtLevel_SmallerData_OverwritesExistingData()
        {
            var data = new[] { "foobar", "foobuzz"};
            var result = DataGraph.DataGraph.SuperimposeDataAtLevel(JaggedTestArray(), data, -1);

            Console.WriteLine(PrintData(result));

            var result1 = DataGraph.DataGraph.GetDataAtAddress(result, new[] {0, 1, 0});
            Assert.AreEqual(result1, "foobar");

            var result2 = DataGraph.DataGraph.GetDataAtAddress(result, new[] { 0, 2, 0 });
            Assert.AreEqual(result2, null);
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


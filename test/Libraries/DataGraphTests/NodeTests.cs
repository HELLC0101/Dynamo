using System;
using System.Diagnostics;
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
			var node = new Node (SingleDimensionalTestArray ());
			Assert.AreEqual (node.Level, 1);
			Assert.AreEqual (node.Children.First ().Level, 2);
		}

		[Test]
		public void Level_Jagged(){
			var node = new Node (JaggedTestArray ());
			Assert.AreEqual (node.Children.First ().Level, 2);
			Assert.AreEqual (node.Children.Last ().Level, 2);
		}

		[Test]
		public void ToString_SingleDimension(){

			var node = new Node (SingleDimensionalTestArray ());
			Assert.AreEqual (node.ToString (), "0");
		}

		[Test]
		public void ToString_TwoDimension(){
			var node = new Node (TwoDimensionalTestArray ());
			var data = node.GetAllLeafNodes ();
			Assert.AreEqual (data.First ().ToString (), "0.0.0");
			Assert.AreEqual (data.Last ().ToString (), "0.1.2");
		}

		[Test]
		public void ToString_Jagged(){
			var node = new Node (JaggedTestArray ());
			var data = node.GetAllLeafNodes ();
			Assert.AreEqual (data.First ().ToString (), "0.0");
			Assert.AreEqual (data.Last ().ToString (), "0.2.2");
		}

		[Test]
		public void Depth_SingleDimension(){
			var node1 = new Node (SingleDimensionalTestArray ());
			Assert.AreEqual (2, node1.Depth);
		}

		[Test]
		public void Depth_TwoDimension(){
			var node2 = new Node (TwoDimensionalTestArray ());
			Assert.AreEqual (3, node2.Depth);
		}

		[Test]
		public void Depth_Jagged(){
			var node2 = new Node (JaggedTestArray ());
			Assert.AreEqual (3, node2.Depth);
		}

		[Test]
		public void GetAllLeafData_SingleDimension()
		{
			var node = new Node (SingleDimensionalTestArray ());
			var data = Node.GetAllLeafData (node);
			Assert.AreEqual(data.Count (), 3);
		}

		[Test]
		public void GetAllLeafData_TwoDimension(){
			var node = new Node (TwoDimensionalTestArray ());
			var data = Node.GetAllLeafData (node);
			Assert.AreEqual(data.Count (), 6);
		}

		[Test]
		public void GetAllLeafData_Jagged(){
			var node = new Node (JaggedTestArray ());
			var data = Node.GetAllLeafData (node);
			Assert.AreEqual(data.Count (), 7);
		}

		[Test]
		public void GetDataAtLevel_Jagged(){
			var node = new Node (JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = node.GetDataAtLevel (-2);
			Console.WriteLine ("@-2");
			Console.WriteLine (PrintData (data));
            Assert.AreEqual(data.First(), "foo");
		}

		[Test]
		public void GetDataAtLevel_ThreeDimension(){
			var node = new Node (ThreeDimensionalTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = node.GetDataAtLevel (-1);
			Console.WriteLine ("@-1");
			Assert.AreEqual (data.First (), "a");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data.Last (), "c");
		}

		[Test]
		public void GetDataAtLevel_Jagged_AtLevel(){
			var node = new Node (JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = node.GetDataAtLevel (-1);
			Console.WriteLine ("@-1");
			Assert.AreEqual (data.First (), arr1.First ());
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data.Last (), arr2.Last ());
		}

		[Test]
		public void GetDataAtLevel_Jagged_Leaves(){
			var node = new Node (JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = Node.GetAllLeafData (node);
			Console.WriteLine ("@leaves");
			Assert.AreEqual (data.First (), "foo");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (data.Last (), arr2.Last ());
		}

		[Test]
		public void GetDataAtLevel_NegativeIndex(){
			var node = new Node (JaggedTestArray ());
			Console.WriteLine (PrintData (node.Data));
			var data = node.GetDataAtLevel (-5);
			Console.WriteLine ("@-5");
			Console.WriteLine (PrintData (data));
			Assert.AreEqual (4, node.GetRoot ().Depth);
		}

		[Test]
		public void GetDataAtLevel_DepthGreaterThanSize_ThrowsException(){
			var node = new Node (JaggedTestArray ());
			Assert.Throws<Exception> (()=>node.GetDataAtLevel (10));
		}

		[Test]
		public void GetDataAtLevel_Case1(){
			var node = new Node (Phase2 ());

			var data = node.GetDataAtLevel (-3);
			Assert.AreEqual (data.Count (), 2);

			data = node.GetDataAtLevel (-2);
			Assert.AreEqual (data.Count (), 4);

			data = node.GetDataAtLevel (-1);
			Assert.AreEqual (data.Count (), 12);
		}

		[Test]
		public void GetAllLeafData_IsLeaves_CorrectCount(){
			var node = new Node (JaggedTestArray ());
			var data = Node.GetAllLeafData (node);
			Assert.AreEqual(7, data.Count ());
		}

		[Test]
		public void CloneWithNulls_Jagged(){
			var node = new Node (JaggedTestArray ());
		    var nodeWithNulls = node.CloneWithNulls();
			var leaves = nodeWithNulls.GetAllLeafNodes ();
			foreach(var l in leaves){
				Assert.AreEqual(null, l.Data);
			}
		}

        [Test]
        public void CloneWithNullsAtLevel_Jagged()
        {
            var node = new Node(JaggedTestArray());
            var nodeWithNulls = node.CloneWithNullsAtLevel(node.Depth - 2);
            var leafData = nodeWithNulls.GetDataAtLevel(node.Depth-2);
            foreach (var l in leafData)
            {
                if (l is IEnumerable)
                {
                    Assert.AreEqual(l, Enumerable.Repeat<object>(null, 3));
                }
                else
                {
                    Assert.AreEqual(null, l);
                }
            }
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
				data, Formatting.Indented);
			return jsonString;
		}
	}
}


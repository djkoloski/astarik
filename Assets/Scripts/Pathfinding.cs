using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding
{
	public class IHDAStar
	{
		public const int NODE_INDEX_NONE = -1;

		public class Node
		{
			public int[] position;
			public int parent;
			public object data;
			public float g;
			public float h;

			public float f
			{
				get { return g + h; }
			}

			public Node(int dimension, int[] newPosition, int newParent, object newData, float newG, float newH)
			{
				position = new int[dimension];
				System.Array.Copy(newPosition, position, position.Length);
				parent = newParent;
				data = newData;
				g = newG;
				h = newH;
			}
		}

		public class PositionEqualityComparer :
			IEqualityComparer<int[]>
		{
			bool IEqualityComparer<int[]>.Equals(int[] x, int[] y)
			{
				if (x.Length != y.Length)
					return false;

				for (int i = 0; i < x.Length; ++i)
					if (x[i] != y[i])
						return false;

				return true;
			}
			int IEqualityComparer<int[]>.GetHashCode(int[] obj)
			{
				int result = 17;
				for (int i = 0; i < obj.Length; ++i)
					result = unchecked(result * 23 + obj[i]);
				return result;
			}
		}

		public class NodeComparer :
			IComparer<int>
		{
			List<Node> nodes_;

			public NodeComparer(List<Node> nodes)
			{
				nodes_ = nodes;
			}

			int IComparer<int>.Compare(int x, int y)
			{
				if (Mathf.Approximately(nodes_[x].f, nodes_[y].f))
					return 0;
				else if (nodes_[x].f < nodes_[y].f)
					return -1;
				return 1;
			}
		}

		public delegate object DataDelegate(float[] values);
		public delegate float CostDelegate(object fromData, float[] fromValues, object toData, float[] toValues);
		public delegate float HeuristicDelegate(object data, float[] values);
		public delegate bool TraversibleDelegate(object data, float[] values);
		public delegate bool GoalDelegate(object data, float[] values);

		private int dimensions_;
		private int subdivisions_;
		private bool[] dimensionWrapping_;
		private DataDelegate dataDelegate_;
		private CostDelegate costDelegate_;
		private HeuristicDelegate heuristicDelegate_;
		private TraversibleDelegate traversibleDelegate_;
		private GoalDelegate goalDelegate_;

		private List<Node> nodes_;
		private Dictionary<int[], int> nodeMap_;
		private MinHeap<int> nodeQueue_;
		private List<float[]> path_;

		private int[] tempPosition;
		private float[] tempValues0;
		private float[] tempValues1;

		public bool Searching
		{
			get { return path_ == null && nodeQueue_.Count != 0; }
		}
		public List<float[]> Path
		{
			get { return path_; }
		}

		private static string PositionToString(int[] position)
		{
			string str = "(";
			for (int i = 0; i < position.Length; ++i)
				str += position[i] + (i < position.Length - 1 ? "," : ")");
			return str;
		}
		private bool TryGetChildPosition(int[] parentPosition, int direction, int[] childPosition)
		{
			int moveDimension = direction / 2;
			int moveAmount = ((direction & 1) == 0 ? -1 : 1);
			int movedValue = parentPosition[moveDimension] + moveAmount;

			if (movedValue < 0 || movedValue >= subdivisions_)
			{
				if (dimensionWrapping_[moveDimension])
					movedValue = (movedValue + subdivisions_) % subdivisions_;
				else
					return false;
			}

			System.Array.Copy(parentPosition, childPosition, dimensions_);
			childPosition[moveDimension] = movedValue;

			return true;
		}
		private void GetPositionValues(int[] position, float[] values)
		{
			for (int i = 0; i < dimensions_; ++i)
				values[i] = (position[i] + 0.5f) / subdivisions_;
		}
		private void GetValuesPosition(float[] values, int[] position)
		{
			for (int i = 0; i < dimensions_; ++i)
				position[i] = Mathf.FloorToInt(values[i] * subdivisions_);
		}
		private bool CalculateIsTraversible(int[] position)
		{
			// TODO: HACK: XXX: this should provide the proper data
			// This will result in a lot of boxing/unboxing though
			GetPositionValues(position, tempValues0);
			return traversibleDelegate_(null, tempValues0);
		}
		private int AllocateNode(int[] position, int parent, float g)
		{
			GetPositionValues(position, tempValues0);
			object data = dataDelegate_(tempValues0);
			Node newNode = new Node(dimensions_, position, parent, data, g, heuristicDelegate_(data, tempValues0));
			nodes_.Add(newNode);
			nodeMap_[newNode.position] = nodes_.Count - 1;
			return nodes_.Count - 1;
		}
		private int TryGetNodeIndexByPosition(int[] position)
		{
			int nodeIndex;
			if (nodeMap_.TryGetValue(position, out nodeIndex))
				return nodeIndex;
			return NODE_INDEX_NONE;
		}

		public IHDAStar(int dimensions, int subdivisions, bool[] dimensionWrapping, DataDelegate dataDelegate, CostDelegate costDelegate, HeuristicDelegate heuristicDelegate, TraversibleDelegate traversibleDelegate, GoalDelegate goalDelegate)
		{
			dimensions_ = dimensions;
			subdivisions_ = subdivisions;
			dimensionWrapping_ = new bool[dimensions_];
			System.Array.Copy(dimensionWrapping, dimensionWrapping_, dimensions_);
			dataDelegate_ = dataDelegate;
			costDelegate_ = costDelegate;
			heuristicDelegate_ = heuristicDelegate;
			traversibleDelegate_ = traversibleDelegate;
			goalDelegate_ = goalDelegate;
		}
		public void BeginSearch(float[] startValues)
		{
			nodes_ = new List<Node>();
			nodeMap_ = new Dictionary<int[], int>(new PositionEqualityComparer());
			nodeQueue_ = new MinHeap<int>(new NodeComparer(nodes_));
			path_ = null;

			tempPosition = new int[dimensions_];
			tempValues0 = new float[dimensions_];
			tempValues1 = new float[dimensions_];

			GetValuesPosition(startValues, tempPosition);
			int startingNodeIndex = AllocateNode(tempPosition, NODE_INDEX_NONE, 0.0f);

			nodeQueue_.Insert(startingNodeIndex);
		}
		public void StepSearch()
		{
			if (!Searching)
				return;

			int nodeIndex = nodeQueue_.ExtractMin();
			Node node = nodes_[nodeIndex];

			// TODO: HACK: XXX: this should provide an array of values
			if (goalDelegate_(node.data, null))
			{
				BuildPath(nodeIndex);
				return;
			}

			// Explore all children
			for (int i = 0; i < dimensions_ * 2; ++i)
			{
				if (!TryGetChildPosition(node.position, i, tempPosition))
					continue;

				if (!CalculateIsTraversible(tempPosition))
					continue;

				GetPositionValues(node.position, tempValues0);
				GetPositionValues(tempPosition, tempValues1);
				float newG = node.g + costDelegate_(node.data, tempValues0, dataDelegate_(tempValues1), tempValues1);
				int childIndex = TryGetNodeIndexByPosition(tempPosition);

				if (childIndex == NODE_INDEX_NONE)
				{
					childIndex = AllocateNode(tempPosition, nodeIndex, newG);
					nodeQueue_.Insert(childIndex);
				}
				else
				{
					Node child = nodes_[childIndex];
					if (newG < child.g)
					{
						child.g = newG;
						nodeQueue_.Insert(childIndex);
					}
				}
			}
		}
		public void BuildPath(int nodeIndex)
		{
			path_ = new List<float[]>();

			while (nodeIndex != -1)
			{
				float[] values = new float[dimensions_];
				Node node = nodes_[nodeIndex];
				GetPositionValues(node.position, values);
				path_.Add(values);
				nodeIndex = node.parent;
			}

			path_.Reverse();
		}
	}
}
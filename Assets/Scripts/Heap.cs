using UnityEngine;
using System.Collections.Generic;

public class MinHeap<T>
{
	protected List<T> heap_;
	protected IComparer<T> comparer_;

	public int Count
	{
		get { return heap_.Count; }
	}
	public T Min
	{
		get { return heap_[0]; }
	}

	public MinHeap(IComparer<T> comparer)
	{
		heap_ = new List<T>();
		comparer_ = comparer;
	}

	protected void HeapifyUp(int index)
	{
		T temp;

		int parent = (index - 1) >> 1;
		while (parent >= 0)
		{
			if (comparer_.Compare(heap_[index], heap_[parent]) >= 0)
				return;

			// Swap
			temp = heap_[index];
			heap_[index] = heap_[parent];
			heap_[parent] = temp;

			index = parent;
			parent = (index - 1) >> 1;
		}
	}
	protected void HeapifyDown(int index)
	{
		T temp;

		int left = (index << 1) + 1;
		int right = (index << 1) + 2;
		int swap;

		while (left < Count)
		{
			if (right < Count)
			{
				if (comparer_.Compare(heap_[left], heap_[right]) <= 0)
					swap = left;
				else
					swap = right;
			}
			else
				swap = left;

			if (comparer_.Compare(heap_[index], heap_[swap]) <= 0)
				return;

			// Swap
			temp = heap_[index];
			heap_[index] = heap_[swap];
			heap_[swap] = temp;

			index = swap;
			left = (index << 1) + 1;
			right = (index << 1) + 2;
		}
	}

	public void Insert(T item)
	{
		int index = Count;
		heap_.Add(item);
		HeapifyUp(index);
	}
	public T ExtractMin()
	{
		T result = heap_[0];
		heap_[0] = heap_[Count - 1];
		heap_.RemoveAt(Count - 1);
		HeapifyDown(0);
		return result;
	}
	public bool Validate()
	{
		for (int i = 1; i < Count; ++i)
		{
			int parent = (i - 1) >> 1;
			if (comparer_.Compare(heap_[parent], heap_[i]) > 0)
				return false;
		}
		return true;
	}
}

public class IndexedMinHeap<T>
{
	protected List<T> heap_;
	protected Dictionary<T, int> heapIndex_;
	protected IComparer<T> comparer_;

	public int Count
	{
		get { return heap_.Count; }
	}
	public T Min
	{
		get { return heap_[0]; }
	}

	public IndexedMinHeap(IComparer<T> comparer)
	{
		heap_ = new List<T>();
		heapIndex_ = new Dictionary<T, int>();
		comparer_ = comparer;
	}

	protected void HeapifyUp(int index)
	{
		T temp;

		int parent = (index - 1) >> 1;
		while (parent >= 0)
		{
			if (comparer_.Compare(heap_[index], heap_[parent]) >= 0)
				return;

			// Swap
			temp = heap_[index];
			heap_[index] = heap_[parent];
			heap_[parent] = temp;
			heapIndex_[heap_[index]] = index;
			heapIndex_[heap_[parent]] = parent;

			index = parent;
			parent = (index - 1) >> 1;
		}
	}
	protected void HeapifyDown(int index)
	{
		T temp;

		int left = (index << 1) + 1;
		int right = (index << 1) + 2;
		int swap;

		while (left < Count)
		{
			if (right < Count)
			{
				if (comparer_.Compare(heap_[left], heap_[right]) <= 0)
					swap = left;
				else
					swap = right;
			}
			else
				swap = left;

			if (comparer_.Compare(heap_[index], heap_[swap]) <= 0)
				return;

			// Swap
			temp = heap_[index];
			heap_[index] = heap_[swap];
			heap_[swap] = temp;
			heapIndex_[heap_[index]] = index;
			heapIndex_[heap_[swap]] = swap;

			index = swap;
			left = (index << 1) + 1;
			right = (index << 1) + 2;
		}
	}

	public void Insert(T item)
	{
		int index = Count;
		heap_.Add(item);
		heapIndex_.Add(item, index);

		HeapifyUp(index);
	}
	public T ExtractMin()
	{
		T result = heap_[0];
		heap_[0] = heap_[Count - 1];
		heapIndex_[heap_[0]] = 0;
		heapIndex_.Remove(result);
		heap_.RemoveAt(Count - 1);

		HeapifyDown(0);
		return result;
	}
	public bool Contains(T item)
	{
		return heapIndex_.ContainsKey(item);
	}
	public void DecreaseKey(T item)
	{
		HeapifyUp(heapIndex_[item]);
	}
	public void IncreaseKey(T item)
	{
		HeapifyDown(heapIndex_[item]);
	}
	public void ModifyKey(T item, T newValue)
	{
		int index = heapIndex_[item];
		heap_[index] = newValue;
		heapIndex_.Remove(item);
		heapIndex_[newValue] = index;

		int comp = comparer_.Compare(newValue, item);
		if (comp < 0)
			HeapifyUp(index);
		else if (comp > 0)
			HeapifyDown(index);
	}
	public void Remove(T item)
	{
		int index = heapIndex_[item];

		heap_[index] = heap_[Count - 1];
		heapIndex_[heap_[index]] = index;

		heap_.RemoveAt(Count - 1);
		heapIndex_.Remove(item);

		if (index < heap_.Count)
		{
			int comp = comparer_.Compare(heap_[index], item);
			if (comp < 0)
				HeapifyUp(index);
			else if (comp > 0)
				HeapifyDown(index);
		}
	}
	public bool Validate()
	{
		for (int i = 1; i < Count; ++i)
		{
			int parent = (i - 1) >> 1;
			if (comparer_.Compare(heap_[parent], heap_[i]) > 0)
				return false;
		}
		return true;
	}
}

public class IntervalHeap<T>
{
	protected List<T> heap_;
	protected IComparer<T> comparer_;

	public int Count
	{
		get { return heap_.Count; }
	}
	public T Min
	{
		get { return heap_[0]; }
	}
	public T Max
	{
		get { return heap_[heap_.Count == 1 ? 0 : 1]; }
	}

	public IntervalHeap(IComparer<T> comparer)
	{
		heap_ = new List<T>();
		comparer_ = comparer;
	}

	protected int FixupInterval(int index)
	{
		T temp;

		if ((index & 1) == 0)
		{
			// FixupParentUpper
			int parent = (((index >> 1) - 1) | 1);
			if (parent >= 0 && comparer_.Compare(heap_[index], heap_[parent]) > 0)
			{
				// Swap
				temp = heap_[parent];
				heap_[parent] = heap_[index];
				heap_[index] = temp;

				index = parent;
			}
		}
		else
		{
			// FixupInterval
			int lower = (index & ~1);
			int upper = (index | 1);
			if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
			{
				// Swap
				temp = heap_[lower];
				heap_[lower] = heap_[upper];
				heap_[upper] = temp;

				index = (index ^ 1);
			}
		}

		return index;
	}
	protected void HeapifyDown(int index)
	{
		if ((index & 1) == 0)
			HeapifyLowerDown(index);
		else
			HeapifyUpperDown(index);
	}
	protected void HeapifyLowerDown(int index)
	{
		T temp;

		while (index < heap_.Count)
		{
			// FixupChildLower
			int left = (((index | 1) << 1) & ~1);
			int right = ((((index | 1) << 1) + 2) & ~1);
			int least;

			if (right < heap_.Count)
			{
				if (comparer_.Compare(heap_[left], heap_[right]) <= 0)
					least = left;
				else
					least = right;
			}
			else
			{
				if (left < heap_.Count)
					least = left;
				else
					break;
			}

			if (comparer_.Compare(heap_[least], heap_[index]) < 0)
			{
				// Swap
				temp = heap_[least];
				heap_[least] = heap_[index];
				heap_[index] = temp;

				index = least;
			}
			else
				break;

			// FixupInterval
			int lower = (index & ~1);
			int upper = (index | 1);
			if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
			{
				// Swap
				temp = heap_[lower];
				heap_[lower] = heap_[upper];
				heap_[upper] = temp;
			}
		}
	}
	protected void HeapifyUpperDown(int index)
	{
		T temp;

		while (index < heap_.Count)
		{
			// FixupChildUpper
			int left = (((index | 1) << 1) | 1);
			int right = ((((index | 1) << 1) + 2) | 1);
			int greatest;
			if (right < heap_.Count)
			{
				if (comparer_.Compare(heap_[left], heap_[right]) >= 0)
					greatest = left;
				else
					greatest = right;
			}
			else
			{
				if (left < heap_.Count)
					greatest = left;
				else
					break;
			}

			if (comparer_.Compare(heap_[greatest], heap_[index]) > 0)
			{
				// Swap
				temp = heap_[greatest];
				heap_[greatest] = heap_[index];
				heap_[index] = temp;

				index = greatest;
			}
			else
				break;

			// FixupInterval
			int lower = (index & ~1);
			int upper = (index | 1);
			if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
			{
				// Swap
				temp = heap_[lower];
				heap_[lower] = heap_[upper];
				heap_[upper] = temp;
			}
		}
	}
	protected void HeapifyUp(int index)
	{
		if ((index & 1) == 0)
			HeapifyLowerUp(index);
		else
			HeapifyUpperUp(index);
	}
	protected void HeapifyLowerUp(int index)
	{
		T temp;

		while (true)
		{
			// FixupParentLower
			int parent = (((index >> 1) - 1) & ~1);
			if (parent >= 0 && comparer_.Compare(heap_[parent], heap_[index]) > 0)
			{
				// Swap
				temp = heap_[parent];
				heap_[parent] = heap_[index];
				heap_[index] = temp;

				index = parent;
			}
			else
				break;
		}
	}
	protected void HeapifyUpperUp(int index)
	{
		T temp;

		while (true)
		{
			// FixupParentUpper
			int parent = (((index >> 1) - 1) | 1);
			if (parent >= 0 && comparer_.Compare(heap_[index], heap_[parent]) > 0)
			{
				// Swap
				temp = heap_[parent];
				heap_[parent] = heap_[index];
				heap_[index] = temp;

				index = parent;
			}
			else
				break;
		}
	}

	public void Insert(T item)
	{
		heap_.Add(item);

		HeapifyUp(FixupInterval(heap_.Count - 1));
	}
	public T ExtractMin()
	{
		T result = heap_[0];
		heap_[0] = heap_[heap_.Count - 1];

		heap_.RemoveAt(heap_.Count - 1);

		HeapifyDown(0);

		return result;
	}
	public T ExtractMax()
	{
		int max = (heap_.Count == 1 ? 0 : 1);
		T result = heap_[max];
		heap_[max] = heap_[heap_.Count - 1];
		heap_.RemoveAt(heap_.Count - 1);

		HeapifyDown(max);

		return result;
	}
	public bool Validate()
	{
		for (int i = 2; i < heap_.Count; ++i)
		{
			int parent = (i >> 1) - 1;

			if (comparer_.Compare(heap_[(i & ~1)], heap_[(i | 1)]) > 0)
				return false;
			if (comparer_.Compare(heap_[(parent & ~1)], heap_[i]) > 0)
				return false;
			if (comparer_.Compare(heap_[i], heap_[(parent | 1)]) > 0)
				return false;
		}
		return true;
	}
}

public class IndexedIntervalHeap<T>
{
	protected List<T> heap_;
	protected Dictionary<T, int> heapIndex_;
	protected IComparer<T> comparer_;

	public int Count
	{
		get { return heap_.Count; }
	}
	public T Min
	{
		get { return heap_[0]; }
	}
	public T Max
	{
		get { return heap_[heap_.Count == 1 ? 0 : 1]; }
	}

	public IndexedIntervalHeap(IComparer<T> comparer)
	{
		heap_ = new List<T>();
		heapIndex_ = new Dictionary<T, int>();
		comparer_ = comparer;
	}

	protected void HeapifyDown(int index)
	{
		if ((index & 1) == 0)
			HeapifyLowerDown(index);
		else
			HeapifyUpperDown(index);
	}
	protected void HeapifyLowerDown(int index)
	{
		T temp;

		while (index < heap_.Count)
		{
			// FixupChildLower
			int left = (((index | 1) << 1) & ~1);
			int right = ((((index | 1) << 1) + 2) & ~1);
			int least;

			if (right < heap_.Count)
			{
				if (comparer_.Compare(heap_[left], heap_[right]) <= 0)
					least = left;
				else
					least = right;
			}
			else
			{
				if (left < heap_.Count)
					least = left;
				else
					break;
			}

			if (comparer_.Compare(heap_[least], heap_[index]) < 0)
			{
				// Swap
				temp = heap_[least];
				heap_[least] = heap_[index];
				heap_[index] = temp;
				heapIndex_[heap_[least]] = least;
				heapIndex_[heap_[index]] = index;

				index = least;
			}
			else
				break;

			// FixupInterval
			int lower = (index & ~1);
			int upper = (index | 1);
			if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
			{
				// Swap
				temp = heap_[lower];
				heap_[lower] = heap_[upper];
				heap_[upper] = temp;
				heapIndex_[heap_[lower]] = lower;
				heapIndex_[heap_[upper]] = upper;
			}
		}
	}
	protected void HeapifyUpperDown(int index)
	{
		T temp;

		while (index < heap_.Count)
		{
			// FixupChildUpper
			int left = (((index | 1) << 1) | 1);
			int right = ((((index | 1) << 1) + 2) | 1);
			int greatest;
			if (right < heap_.Count)
			{
				if (comparer_.Compare(heap_[left], heap_[right]) >= 0)
					greatest = left;
				else
					greatest = right;
			}
			else
			{
				if (left < heap_.Count)
					greatest = left;
				else
					break;
			}

			if (comparer_.Compare(heap_[greatest], heap_[index]) > 0)
			{
				// Swap
				temp = heap_[greatest];
				heap_[greatest] = heap_[index];
				heap_[index] = temp;
				heapIndex_[heap_[greatest]] = greatest;
				heapIndex_[heap_[index]] = index;

				index = greatest;
			}
			else
				break;

			// FixupInterval
			int lower = (index & ~1);
			int upper = (index | 1);
			if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
			{
				// Swap
				temp = heap_[lower];
				heap_[lower] = heap_[upper];
				heap_[upper] = temp;
				heapIndex_[heap_[lower]] = lower;
				heapIndex_[heap_[upper]] = upper;
			}
		}
	}
	protected void HeapifyUp(int index)
	{
		if ((index & 1) == 0)
			HeapifyLowerUp(index);
		else
			HeapifyUpperUp(index);
	}
	protected void HeapifyLowerUp(int index)
	{
		T temp;

		while (true)
		{
			// FixupParentLower
			int parent = (((index >> 1) - 1) & ~1);
			if (parent >= 0 && comparer_.Compare(heap_[parent], heap_[index]) > 0)
			{
				// Swap
				temp = heap_[parent];
				heap_[parent] = heap_[index];
				heap_[index] = temp;
				heapIndex_[heap_[parent]] = parent;
				heapIndex_[heap_[index]] = index;

				index = parent;
			}
			else
				break;
		}
	}
	protected void HeapifyUpperUp(int index)
	{
		T temp;

		while (true)
		{
			// FixupParentUpper
			int parent = (((index >> 1) - 1) | 1);
			if (parent >= 0 && comparer_.Compare(heap_[index], heap_[parent]) > 0)
			{
				// Swap
				temp = heap_[parent];
				heap_[parent] = heap_[index];
				heap_[index] = temp;
				heapIndex_[heap_[parent]] = parent;
				heapIndex_[heap_[index]] = index;

				index = parent;
			}
			else
				break;
		}
	}

	public void Insert(T item)
	{
		T temp;

		int index = heap_.Count;
		heap_.Add(item);
		heapIndex_.Add(item, index);

		if ((index & 1) == 0)
		{
			// FixupParentUpper
			int parent = (((index >> 1) - 1) | 1);
			if (parent >= 0 && comparer_.Compare(heap_[index], heap_[parent]) > 0)
			{
				// Swap
				temp = heap_[parent];
				heap_[parent] = heap_[index];
				heap_[index] = temp;
				heapIndex_[heap_[parent]] = parent;
				heapIndex_[heap_[index]] = index;

				index = parent;
			}
		}
		else
		{
			// FixupInterval
			int lower = (index & ~1);
			int upper = (index | 1);
			if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
			{
				// Swap
				temp = heap_[lower];
				heap_[lower] = heap_[upper];
				heap_[upper] = temp;
				heapIndex_[heap_[lower]] = lower;
				heapIndex_[heap_[upper]] = upper;

				index = (index ^ 1);
			}
		}

		HeapifyUp(index);
	}
	public T ExtractMin()
	{
		T result = heap_[0];
		heap_[0] = heap_[heap_.Count - 1];
		heap_.RemoveAt(heap_.Count - 1);
		heapIndex_.Remove(result);

		HeapifyDown(0);
		return result;
	}
	public T ExtractMax()
	{
		int index = (heap_.Count == 1 ? 0 : 1);
		T result = heap_[index];
		heap_[index] = heap_[heap_.Count - 1];
		heap_.RemoveAt(heap_.Count - 1);
		heapIndex_.Remove(result);

		HeapifyDown(index);
		return result;
	}
	public bool Contains(T item)
	{
		return heapIndex_.ContainsKey(item);
	}
	public void DecreaseKey(T item)
	{
		T temp;

		int index = heapIndex_[item];

		// FixupInterval
		int lower = (index & ~1);
		int upper = (index | 1);
		if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
		{
			// Swap
			temp = heap_[lower];
			heap_[lower] = heap_[upper];
			heap_[upper] = temp;
			heapIndex_[heap_[lower]] = lower;
			heapIndex_[heap_[upper]] = upper;

			index = (index ^ 1);
		}

		HeapifyUp(lower);
		HeapifyDown(upper);
	}
	public void IncreaseKey(T item)
	{
		T temp;

		int index = heapIndex_[item];

		// FixupInterval
		int lower = (index & ~1);
		int upper = (index | 1);
		if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
		{
			// Swap
			temp = heap_[lower];
			heap_[lower] = heap_[upper];
			heap_[upper] = temp;
			heapIndex_[heap_[lower]] = lower;
			heapIndex_[heap_[upper]] = upper;

			index = (index ^ 1);
		}

		HeapifyDown(lower);
		HeapifyUp(upper);
	}
	public void ModifyKey(T item, T newValue)
	{
		T temp;

		int index = heapIndex_[item];
		heap_[index] = newValue;
		heapIndex_.Remove(item);
		heapIndex_.Add(newValue, index);

		// FixupInterval
		int lower = (index & ~1);
		int upper = (index | 1);
		if (upper < heap_.Count && comparer_.Compare(heap_[lower], heap_[upper]) > 0)
		{
			// Swap
			temp = heap_[lower];
			heap_[lower] = heap_[upper];
			heap_[upper] = temp;
			heapIndex_[heap_[lower]] = lower;
			heapIndex_[heap_[upper]] = upper;

			index = (index ^ 1);
		}

		int comp = comparer_.Compare(newValue, item);
		if (comp < 0)
		{
			HeapifyUp(lower);
			HeapifyDown(upper);
		}
		else if (comp > 0)
		{
			HeapifyDown(lower);
			HeapifyUp(upper);
		}
	}
	public bool Validate()
	{
		for (int i = 2; i < heap_.Count; ++i)
		{
			int parent = (i >> 1) - 1;

			if (comparer_.Compare(heap_[(i & ~1)], heap_[(i | 1)]) > 0)
				return false;
			if (comparer_.Compare(heap_[(parent & ~1)], heap_[i]) > 0)
				return false;
			if (comparer_.Compare(heap_[i], heap_[(parent | 1)]) > 0)
				return false;
		}
		return true;
	}
}
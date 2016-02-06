using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class HeapUnitTests
{
	public const int kHeapTestSize = 100000;

	[Test]
	public void SortingBaseline()
	{
		List<int> list = new List<int>();

		for (int i = 0; i < kHeapTestSize; ++i)
		{
			int value = Random.Range(int.MinValue + 1, int.MaxValue);
			int pos = list.BinarySearch(value);
			list.Insert((pos < 0 ? ~pos : pos), value);
		}

		float greatest = int.MaxValue;
		while (list.Count > 0)
		{
			float next = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			Assert.IsTrue(next <= greatest);
			greatest = next;
		}
	}

	[Test]
	public void MinHeap()
	{
		MinHeap<int> heap = new MinHeap<int>(Comparer<int>.Default);

		for (int i = 0; i < kHeapTestSize; ++i)
			heap.Insert(Random.Range(int.MinValue + 1, int.MaxValue));

		Assert.IsTrue(heap.Validate());

		int least = int.MinValue;
		while (heap.Count > 0)
		{
			int next = heap.ExtractMin();
			Assert.IsTrue(next >= least);
			least = next;
		}
	}

	[Test]
	public void IndexedMinHeap()
	{
		IndexedMinHeap<int> heap = new IndexedMinHeap<int>(Comparer<int>.Default);

		List<int> modifyKeys = new List<int>();

		for (int i = 0; i < kHeapTestSize; ++i)
		{
			int value = Random.Range(int.MinValue + 1, int.MaxValue);
			while (heap.Contains(value))
				value = Random.Range(int.MinValue + 1, int.MaxValue);

			heap.Insert(value);
			if (Random.value < 0.1f)
				modifyKeys.Add(value);
		}

		Assert.IsTrue(heap.Validate());

		for (int i = 0; i < modifyKeys.Count; ++i)
		{
			int newValue = modifyKeys[i];

			if (Random.value < 0.5f)
			{
				heap.Remove(newValue);
			}
			else
			{
				while (heap.Contains(newValue))
					newValue += Random.Range(-1000, 1000);

				heap.ModifyKey(modifyKeys[i], newValue);
			}
		}

		Assert.IsTrue(heap.Validate());

		float least = int.MinValue;
		while (heap.Count > 0)
		{
			float next = heap.ExtractMin();
			Assert.IsTrue(next >= least);
			least = next;
		}
	}

	[Test]
	public void IntervalHeap()
	{
		IntervalHeap<int> heap = new IntervalHeap<int>(Comparer<int>.Default);

		for (int i = 0; i < kHeapTestSize; ++i)
			heap.Insert(Random.Range(int.MinValue + 1, int.MaxValue));

		Assert.IsTrue(heap.Validate());

		int least = int.MinValue;
		int greatest = int.MaxValue;
		while (heap.Count > 0)
		{
			int next = heap.ExtractMin();
			Assert.IsTrue(next >= least);
			least = next;

			next = heap.ExtractMax();
			Assert.IsTrue(next <= greatest);
			greatest = next;
		}
	}

	[Test]
	public void IndexedIntervalHeap()
	{
		IndexedIntervalHeap<int> heap = new IndexedIntervalHeap<int>(Comparer<int>.Default);

		List<int> modifyKeys = new List<int>();

		for (int i = 0; i < kHeapTestSize; ++i)
		{
			int value = Random.Range(int.MinValue + 1, int.MaxValue);
			while (heap.Contains(value))
				value = Random.Range(int.MinValue + 1, int.MaxValue);

			heap.Insert(value);
			if (Random.value < 0.1f)
				modifyKeys.Add(value);
		}

		Assert.IsTrue(heap.Validate());

		for (int i = 0; i < modifyKeys.Count; ++i)
		{
			int newValue = modifyKeys[i];
			while (heap.Contains(newValue))
				newValue += Random.Range(-1000, 1000);

			heap.ModifyKey(modifyKeys[i], newValue);
		}

		Assert.IsTrue(heap.Validate());

		int least = int.MinValue;
		int greatest = int.MaxValue;
		while (heap.Count > 0)
		{
			int next = heap.ExtractMin();
			Assert.IsTrue(next >= least);
			least = next;

			next = heap.ExtractMax();
			Assert.IsTrue(next <= greatest);
			greatest = next;
		}
	}
}
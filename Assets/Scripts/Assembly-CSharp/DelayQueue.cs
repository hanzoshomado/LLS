using System.Collections.Generic;
using UnityEngine;

public class DelayQueue<T>
{
	private readonly List<DelayQueueItem<T>> _items = new List<DelayQueueItem<T>>();

	public void Enqueue(T item)
	{
		Enqueue(item, Time.unscaledTime);
	}

	public void Enqueue(T item, float time)
	{
		_items.Add(new DelayQueueItem<T>
		{
			Value = item,
			ReadyTime = time
		});
	}

	public T TryDequeue()
	{
		float unscaledTime = Time.unscaledTime;
		for (int i = 0; i < _items.Count; i++)
		{
			DelayQueueItem<T> delayQueueItem = _items[i];
			if (delayQueueItem.ReadyTime <= unscaledTime)
			{
				_items.Remove(delayQueueItem);
				return delayQueueItem.Value;
			}
		}
		return default(T);
	}

	public int GetCount()
	{
		return _items.Count;
	}

	public void Clear()
	{
		_items.Clear();
	}
}

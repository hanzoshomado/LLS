using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrottledQueue<T>
{
	private readonly DelayQueue<T> _delayQueue;

	private readonly LinkedList<float> _messageSendTimes;

	private readonly int _itemsPer;

	private readonly float _numSeconds;

	private bool _hasEverThrottled;

	private int _maxItemsWithinWindow;

	public ThrottledQueue(int itemsPer, float seconds)
	{
		_delayQueue = new DelayQueue<T>();
		_messageSendTimes = new LinkedList<float>();
		_itemsPer = itemsPer;
		_numSeconds = seconds;
	}

	public int MaxItemsWithinWindow()
	{
		return _maxItemsWithinWindow;
	}

	public bool HasEverThrottled()
	{
		return _hasEverThrottled;
	}

	public void Enqueue(T item)
	{
		ClearSendTimesBeforeLimitWindow();
		_maxItemsWithinWindow = Math.Max(_maxItemsWithinWindow, _messageSendTimes.Count);
		if (_messageSendTimes.Count < _itemsPer)
		{
			_messageSendTimes.AddLast(Time.unscaledTime);
			_delayQueue.Enqueue(item);
			return;
		}
		_hasEverThrottled = true;
		LinkedListNode<float> linkedListNode = _messageSendTimes.Last;
		int num = _itemsPer - 1;
		while (linkedListNode.Previous != null && num > 0)
		{
			num--;
			linkedListNode = linkedListNode.Previous;
		}
		float num2 = linkedListNode.Value + _numSeconds;
		_messageSendTimes.AddLast(num2);
		_delayQueue.Enqueue(item, num2);
	}

	public T TryDequeue()
	{
		return _delayQueue.TryDequeue();
	}

	private void ClearSendTimesBeforeLimitWindow()
	{
		float num = Time.unscaledTime - _numSeconds;
		while (_messageSendTimes.First != null && _messageSendTimes.First.Value < num)
		{
			_messageSendTimes.RemoveFirst();
		}
	}

	public void ClearFutureItems()
	{
		_delayQueue.Clear();
		while (_messageSendTimes.Last != null && _messageSendTimes.Last.Value > Time.unscaledTime)
		{
			_messageSendTimes.RemoveLast();
		}
	}

	public int GetCount()
	{
		return _delayQueue.GetCount();
	}
}

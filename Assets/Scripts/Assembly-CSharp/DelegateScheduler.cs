using System;
using System.Collections.Generic;
using UnityEngine;

public class DelegateScheduler : Singleton<DelegateScheduler>
{
	private List<TimedDelegate> _timedCallbacks;

	private List<TimedDelegate> _timedCallbacksToAdd;

	private List<TimedDelegate> _timedCallbacksToDelete;

	public override void Awake()
	{
		base.Awake();
		_timedCallbacks = new List<TimedDelegate>();
		_timedCallbacksToAdd = new List<TimedDelegate>();
		_timedCallbacksToDelete = new List<TimedDelegate>();
	}

	public void ClearAll()
	{
		_timedCallbacks.Clear();
		_timedCallbacksToAdd.Clear();
		_timedCallbacksToDelete.Clear();
	}

	public void Schedule(Action del, float secondsToWait)
	{
		_timedCallbacksToAdd.Add(new TimedDelegate(del, Time.time + secondsToWait));
	}

	private void Update()
	{
		if (_timedCallbacksToAdd.Count > 0)
		{
			foreach (TimedDelegate item in _timedCallbacksToAdd)
			{
				_timedCallbacks.Add(item);
			}
			_timedCallbacksToAdd.Clear();
		}
		foreach (TimedDelegate timedCallback in _timedCallbacks)
		{
			if (Time.time > timedCallback.TimeToTrigger)
			{
				timedCallback.DelegateToCall();
				_timedCallbacksToDelete.Add(timedCallback);
			}
		}
		if (_timedCallbacksToDelete.Count <= 0)
		{
			return;
		}
		foreach (TimedDelegate item2 in _timedCallbacksToDelete)
		{
			_timedCallbacks.Remove(item2);
		}
		_timedCallbacksToDelete.Clear();
	}
}

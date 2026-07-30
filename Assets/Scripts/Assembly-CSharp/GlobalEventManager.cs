using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventManager : Singleton<GlobalEventManager>
{
	private Dictionary<string, List<object>> _eventListeners;

	private Dictionary<string, List<object>> _onceListeners;

	public override void Awake()
	{
		base.Awake();
		_eventListeners = new Dictionary<string, List<object>>();
		_onceListeners = new Dictionary<string, List<object>>();
	}

	public void AddEventListenerOnce<T>(string eventName, Action<T> callback)
	{
		List<object> onceCallbackList = getOnceCallbackList(eventName);
		onceCallbackList.Add(callback);
	}

	public void AddEventListenerOnce(string eventName, Action callback)
	{
		List<object> onceCallbackList = getOnceCallbackList(eventName);
		onceCallbackList.Add(callback);
	}

	public void AddEventListener<T>(string eventName, Action<T> callback)
	{
		List<object> callbackList = getCallbackList(eventName);
		if (callbackList.Contains(callback))
		{
			Debug.LogError("AddEventListener - Already contains callback for event: " + eventName);
		}
		callbackList.Add(callback);
	}

	public void AddEventListener(string eventName, Action callback)
	{
		List<object> callbackList = getCallbackList(eventName);
		if (callbackList.Contains(callback))
		{
			Debug.LogError("AddEventListener - Already contains callback for event: " + eventName);
		}
		callbackList.Add(callback);
	}

	public void RemoveEventListener(string eventName, Action callback)
	{
		List<object> callbackList = getCallbackList(eventName);
		if (!callbackList.Contains(callback))
		{
			Debug.LogWarning("RemoveEventListener - No such listener! EventName: " + eventName);
		}
		callbackList.Remove(callback);
	}

	public void RemoveEventListener<T>(string eventName, Action<T> callback)
	{
		List<object> callbackList = getCallbackList(eventName);
		if (!callbackList.Contains(callback))
		{
			Debug.LogWarning("RemoveEventListener - No such listener! EventName: " + eventName);
		}
		callbackList.Remove(callback);
	}

	public void Dispatch(string eventName)
	{
		List<object> callbackList = getCallbackList(eventName);
		foreach (Action item in callbackList)
		{
			item();
		}
		List<object> onceCallbackList = getOnceCallbackList(eventName);
		foreach (Action item2 in onceCallbackList)
		{
			item2();
		}
		onceCallbackList.Clear();
	}

	public void Dispatch<T>(string eventName, T argument)
	{
		List<object> callbackList = getCallbackList(eventName);
		for (int i = 0; i < callbackList.Count; i++)
		{
			Action<T> action = callbackList[i] as Action<T>;
			if (action != null)
			{
				action(argument);
			}
		}
		List<object> onceCallbackList = getOnceCallbackList(eventName);
		for (int j = 0; j < onceCallbackList.Count; j++)
		{
			Action<T> action2 = onceCallbackList[j] as Action<T>;
			if (action2 != null)
			{
				action2(argument);
			}
		}
		onceCallbackList.Clear();
	}

	private List<object> getCallbackList(string eventName)
	{
		if (!_eventListeners.ContainsKey(eventName))
		{
			_eventListeners[eventName] = new List<object>();
		}
		return _eventListeners[eventName];
	}

	private List<object> getOnceCallbackList(string eventName)
	{
		if (!_onceListeners.ContainsKey(eventName))
		{
			_onceListeners[eventName] = new List<object>();
		}
		return _onceListeners[eventName];
	}
}

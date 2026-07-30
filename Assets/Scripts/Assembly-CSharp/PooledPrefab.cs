using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PooledPrefab
{
	public Transform Prefab;

	public bool AddPoolReference;

	public int MaxCount = -1;

	private List<Transform> _instantiatedObjects = new List<Transform>();

	public Transform InstantiateNewObject(bool forceNewInstance = false)
	{
		if (!forceNewInstance)
		{
			for (int i = 0; i < _instantiatedObjects.Count; i++)
			{
				if (_instantiatedObjects[i] != null && !_instantiatedObjects[i].gameObject.activeSelf)
				{
					_instantiatedObjects[i].gameObject.SetActive(true);
					return _instantiatedObjects[i];
				}
			}
		}
		Transform transform = UnityEngine.Object.Instantiate(Prefab);
		if (AddPoolReference)
		{
			PooledPrefabReference pooledPrefabReference = transform.gameObject.AddComponent<PooledPrefabReference>();
			pooledPrefabReference.OwnerPool = this;
		}
		_instantiatedObjects.Add(transform);
		return transform;
	}

	public void DestroyAllObjects()
	{
		for (int i = 0; i < _instantiatedObjects.Count; i++)
		{
			if (_instantiatedObjects[i] != null && _instantiatedObjects[i].gameObject.activeSelf)
			{
				_instantiatedObjects[i].gameObject.SetActive(false);
			}
		}
	}

	public bool HasMaxedOutObjectsInPool()
	{
		if (MaxCount <= 0)
		{
			return false;
		}
		return _instantiatedObjects.Count >= MaxCount;
	}

	public void DestroyObject(GameObject obj, bool addToPool = true)
	{
		if (addToPool && !HasMaxedOutObjectsInPool())
		{
			obj.SetActive(false);
			return;
		}
		if (_instantiatedObjects.Contains(obj.transform))
		{
			_instantiatedObjects.Remove(obj.transform);
		}
		else
		{
			Debug.LogError("Trying to remove pooled object not created by us! " + obj.name);
		}
		obj.transform.SetParent(null, false);
		UnityEngine.Object.Destroy(obj);
	}

	public int GetNumActiveObjects()
	{
		int num = 0;
		for (int i = 0; i < _instantiatedObjects.Count; i++)
		{
			if (_instantiatedObjects[i] != null && _instantiatedObjects[i].gameObject.activeSelf)
			{
				num++;
			}
		}
		return num;
	}
}

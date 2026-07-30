using System.Collections.Generic;
using UnityEngine;

public class TransformUtils
{
	public static void ChangeLayersRecursively(Transform transform, int layerMask)
	{
		List<Transform> allChildrenRecursive = GetAllChildrenRecursive(transform);
		for (int i = 0; i < allChildrenRecursive.Count; i++)
		{
			allChildrenRecursive[i].gameObject.layer = layerMask;
		}
	}

	public static List<Transform> GetAllChildrenRecursive(Transform root)
	{
		List<Transform> list = new List<Transform>();
		addSelfAndChildrenToList(root, list);
		return list;
	}

	private static void addSelfAndChildrenToList(Transform transform, List<Transform> childList)
	{
		childList.Add(transform);
		for (int i = 0; i < transform.childCount; i++)
		{
			addSelfAndChildrenToList(transform.GetChild(i), childList);
		}
	}

	public static Transform FindChildRecursive(Transform root, string childName)
	{
		if (root.gameObject.name == childName)
		{
			return root;
		}
		for (int i = 0; i < root.childCount; i++)
		{
			Transform child = root.GetChild(i);
			child = FindChildRecursive(child, childName);
			if (child != null)
			{
				return child;
			}
		}
		return null;
	}

	public static void DestroyAllChildren(Transform parentTransform)
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < parentTransform.childCount; i++)
		{
			list.Add(parentTransform.GetChild(i));
		}
		for (int j = 0; j < list.Count; j++)
		{
			Object.Destroy(list[j].gameObject);
		}
	}

	public static void HideAllChildren(Transform parentTransform)
	{
		for (int i = 0; i < parentTransform.childCount; i++)
		{
			parentTransform.GetChild(i).gameObject.SetActive(false);
		}
	}

	public static void ShowAllChildren(Transform parentTransform)
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < parentTransform.childCount; i++)
		{
			parentTransform.GetChild(i).gameObject.SetActive(true);
		}
	}
}

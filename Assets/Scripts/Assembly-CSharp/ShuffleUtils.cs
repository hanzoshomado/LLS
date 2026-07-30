using System.Collections.Generic;
using UnityEngine;

public class ShuffleUtils
{
	public static T PickRandom<T>(T[] list)
	{
		return list[Random.Range(0, list.Length)];
	}

	public static void ShuffleList<T>(List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int index = Random.Range(0, list.Count);
			T value = list[index];
			list[index] = list[i];
			list[i] = value;
		}
	}
}

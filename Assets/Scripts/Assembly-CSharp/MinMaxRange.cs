using System;
using UnityEngine;

[Serializable]
public class MinMaxRange
{
	public float Min;

	public float Max;

	public float GetRandomValue()
	{
		return UnityEngine.Random.Range(Min, Max);
	}
}

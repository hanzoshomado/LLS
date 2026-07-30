using System.Collections.Generic;
using UnityEngine;

public class WeightedRandomPicker
{
	public static T PickFromList<T>(List<T> weightedAlternatives) where T : Weighted
	{
		int num = 0;
		foreach (T weightedAlternative in weightedAlternatives)
		{
			Weighted weighted = weightedAlternative;
			num += weighted.GetWeight();
		}
		float num2 = Random.Range(0f, 1f) * (float)num;
		int num3 = 0;
		foreach (T weightedAlternative2 in weightedAlternatives)
		{
			Weighted weighted2 = weightedAlternative2;
			num3 += weighted2.GetWeight();
			if (num2 <= (float)num3)
			{
				return (T)weighted2;
			}
		}
		return default(T);
	}
}

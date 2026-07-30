using UnityEngine;

public class Asserter
{
	public static void Assert(bool condition, string errorMessage = "")
	{
		if (!condition)
		{
			Debug.LogError("Assert Failed: " + errorMessage);
		}
	}
}

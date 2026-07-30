using System;
using System.Collections;

[Serializable]
public class ITweenHash
{
	public iTween.EaseType EaseType;

	public float TimeToTravel = 1f;

	public float Speed = -1f;

	public float Delay;

	public bool IsLocal;

	public float GetTotalTime()
	{
		return TimeToTravel + Delay;
	}

	public Hashtable GetHash()
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("easeType", EaseType);
		hashtable.Add("delay", Delay);
		hashtable.Add("islocal", IsLocal);
		if (Speed > 0f)
		{
			hashtable.Add("speed", Speed);
		}
		else if (TimeToTravel > 0f)
		{
			hashtable.Add("time", TimeToTravel);
		}
		return hashtable;
	}
}

using System;

public struct TimedDelegate
{
	public float TimeToTrigger;

	public Action DelegateToCall;

	public TimedDelegate(Action delegateToCall, float timeToTrigger)
	{
		DelegateToCall = delegateToCall;
		TimeToTrigger = timeToTrigger;
	}
}

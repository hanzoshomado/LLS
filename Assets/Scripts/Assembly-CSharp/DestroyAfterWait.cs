using UnityEngine;

public class DestroyAfterWait : MonoBehaviour
{
	public float WaitTime;

	private float _timeToDestroy;

	private void Start()
	{
		_timeToDestroy = Time.time + WaitTime;
	}

	public void SetWaitTime(float waitTime)
	{
		WaitTime = waitTime;
		_timeToDestroy = Time.time + WaitTime;
	}

	private void Update()
	{
		if (Time.time > _timeToDestroy)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

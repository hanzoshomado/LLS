using UnityEngine;

public class DestroyPooledAfterWait : MonoBehaviour
{
	private float _timeToDestroy = -1f;

	public void StartCountdown(float countdown)
	{
		_timeToDestroy = Time.time + countdown;
	}

	public void StopCountdown()
	{
		_timeToDestroy = -1f;
	}

	private void Update()
	{
		if (_timeToDestroy > 0f && Time.time > _timeToDestroy)
		{
			_timeToDestroy = -1f;
			GetComponent<PooledPrefabReference>().OwnerPool.DestroyObject(base.gameObject);
		}
	}
}

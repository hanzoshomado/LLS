using UnityEngine;

public class SetAnimatorRandomValue : MonoBehaviour
{
	private Animator _animator;

	private float _timeForNextUpdate;

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_timeForNextUpdate = Time.time + 1f;
	}

	private void Update()
	{
		if (_animator != null && _animator.isActiveAndEnabled && Time.time > _timeForNextUpdate)
		{
			_animator.SetFloat("RandomValue", Random.Range(0f, 1f));
			_timeForNextUpdate = Time.time + 0.2f;
		}
	}
}

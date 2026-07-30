using UnityEngine;

public class Rotator : MonoBehaviour
{
	public Vector3 RandomStartRotation;

	public Vector3 RotationSpeed;

	public bool IsRandom;

	private Vector3 _rotationSpeed;

	public void SetRotationSpeed(Vector3 newSpeed)
	{
		_rotationSpeed = newSpeed;
	}

	public void Awake()
	{
		if (IsRandom)
		{
			_rotationSpeed = new Vector3(Random.Range(0f - RotationSpeed.x, RotationSpeed.x), Random.Range(0f - RotationSpeed.y, RotationSpeed.y), Random.Range(0f - RotationSpeed.z, RotationSpeed.z));
		}
		else
		{
			_rotationSpeed = RotationSpeed;
		}
		if (RandomStartRotation.magnitude > 1f)
		{
			base.transform.localEulerAngles = new Vector3(Random.Range(0f - RandomStartRotation.x, RandomStartRotation.x), Random.Range(0f - RandomStartRotation.y, RandomStartRotation.y), Random.Range(0f - RandomStartRotation.z, RandomStartRotation.z));
		}
	}

	public void Update()
	{
		base.transform.Rotate(_rotationSpeed * Time.deltaTime);
	}
}

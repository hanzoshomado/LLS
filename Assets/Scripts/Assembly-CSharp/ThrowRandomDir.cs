using UnityEngine;

public class ThrowRandomDir : MonoBehaviour
{
	private Rigidbody rigidBody;

	public bool throwObject = true;

	public float distanceX = 1f;

	public float distanceY = 1f;

	public float scaleThrow = 1f;

	public float torqueScale = 1f;

	private void OnEnable()
	{
		rigidBody = GetComponent<Rigidbody>();
		if (throwObject)
		{
			rigidBody.Sleep();
			rigidBody.AddForce(new Vector3(Random.Range(0f - distanceX, distanceX), Random.Range(0f - distanceY, distanceY), Random.Range(scaleThrow * 0.5f, scaleThrow)));
		}
		rigidBody.AddTorque(new Vector3(Random.Range(5f, 20f), Random.Range(5f, 20f), Random.Range(5f, 20f)) * Random.Range(10f * torqueScale, 30f * torqueScale));
	}
}

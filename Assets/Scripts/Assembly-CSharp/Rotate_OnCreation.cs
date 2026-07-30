using UnityEngine;

[ExecuteInEditMode]
public class Rotate_OnCreation : MonoBehaviour
{
	public float min = -7f;

	public float max = 7f;

	private void Start()
	{
		if (base.transform.localRotation == Quaternion.identity)
		{
			base.transform.Rotate(new Vector3(Random.Range(min, max), Random.Range(-180f, 180f), Random.Range(min, max)));
		}
	}
}

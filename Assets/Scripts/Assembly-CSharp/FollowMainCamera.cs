using UnityEngine;

public class FollowMainCamera : MonoBehaviour
{
	public Vector3 PositionModifier = new Vector3(0f, 30f, 0f);

	private void Update()
	{
		if (Camera.main != null)
		{
			base.transform.position = Camera.main.transform.position + PositionModifier;
		}
	}
}

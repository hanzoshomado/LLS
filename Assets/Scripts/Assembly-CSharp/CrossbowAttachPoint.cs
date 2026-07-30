using UnityEngine;

public class CrossbowAttachPoint : MonoBehaviour
{
	public Vector3 Size;

	public Vector3 GetClosestLocalPosition(Vector3 impactPositionWorld)
	{
		Vector3 result = base.transform.InverseTransformPoint(impactPositionWorld);
		result.x = Mathf.Clamp(result.x, (0f - Size.x) * 0.5f, Size.x * 0.5f);
		result.y = Mathf.Clamp(result.y, (0f - Size.y) * 0.5f, Size.y * 0.5f);
		result.z = Mathf.Clamp(result.z, (0f - Size.z) * 0.5f, Size.z * 0.5f);
		return result;
	}
}

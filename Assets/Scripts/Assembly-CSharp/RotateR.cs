using UnityEngine;

[ExecuteInEditMode]
public class RotateR : MonoBehaviour
{
	public bool Rotate90;

	private void Update()
	{
		if (Rotate90)
		{
			base.transform.Rotate(0f, -90f, 0f, Space.World);
			Rotate90 = false;
		}
	}
}

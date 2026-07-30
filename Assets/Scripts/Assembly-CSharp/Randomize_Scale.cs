using UnityEngine;

public class Randomize_Scale : MonoBehaviour
{
	public bool Maintain_AspectRatio;

	public int Scale_Min;

	public int Scale_Max;

	public bool Enable_X;

	public int X_Scale_Min;

	public int X_Scale_Max;

	public bool Enable_Y;

	public int Y_Scale_Min;

	public int Y_Scale_Max;

	public bool Enable_Z;

	public int Z_Scale_Min;

	public int Z_Scale_Max;

	public bool I_haveBeenCreated;

	private void Start()
	{
		Vector3 one = Vector3.one;
		if (Maintain_AspectRatio)
		{
			float num = Random.Range(Scale_Min, Scale_Max);
			if (Enable_X)
			{
				one.x = num;
			}
			if (Enable_Y)
			{
				one.y = num;
			}
			if (Enable_Z)
			{
				one.z = num;
			}
		}
		else
		{
			if (Enable_X)
			{
				one.x = Random.Range(X_Scale_Min, X_Scale_Max);
			}
			if (Enable_Y)
			{
				one.y = Random.Range(Y_Scale_Min, Y_Scale_Max);
			}
			if (Enable_Z)
			{
				one.z = Random.Range(Z_Scale_Min, Z_Scale_Max);
			}
		}
		if (!I_haveBeenCreated && base.transform.localScale == Vector3.one)
		{
			base.transform.localScale = one;
			I_haveBeenCreated = true;
		}
	}
}

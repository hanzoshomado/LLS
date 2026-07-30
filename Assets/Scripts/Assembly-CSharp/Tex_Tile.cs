using UnityEngine;

[ExecuteInEditMode]
public class Tex_Tile : MonoBehaviour
{
	public float InitialScaleX = 1f;

	public float InitialScaleZ = 1f;

	public bool LockX;

	public bool LockZ;

	public bool Z_is_Up;

	public bool Flip_U_and_V_values;

	private float TileX;

	private float TileZ;

	private Vector3 PreviousScale;

	public Renderer rend;

	private void Start()
	{
		rend = GetComponent<Renderer>();
		PreviousScale = base.transform.localScale;
	}

	private void Update()
	{
		if (base.transform.localScale != PreviousScale)
		{
			TileX = base.transform.localScale.x / InitialScaleX;
			TileZ = base.transform.localScale.z / InitialScaleZ;
			if (Z_is_Up)
			{
				TileZ = base.transform.localScale.y / InitialScaleZ;
			}
			if (LockX)
			{
				TileX = 1f;
			}
			if (LockZ)
			{
				TileZ = 1f;
			}
			if (Flip_U_and_V_values)
			{
				rend.material.SetTextureScale("_MainTex", new Vector2(TileZ, TileX));
			}
			else
			{
				rend.material.SetTextureScale("_MainTex", new Vector2(TileX, TileZ));
			}
			PreviousScale = base.transform.localScale;
		}
	}
}

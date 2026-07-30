using UnityEngine;

public class Tex_TileY : MonoBehaviour
{
	public float InitialScaleX = 1f;

	public float InitialScaleZ = 1f;

	public Transform ScaleObject;

	private Renderer _renderer;

	private Vector3 _lastLocalScale;

	private Material _lastSharedMaterial;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		if (ScaleObject.localScale.x != _lastLocalScale.x || ScaleObject.localScale.y != _lastLocalScale.y || ScaleObject.localScale.z != _lastLocalScale.z || _lastSharedMaterial != _renderer.sharedMaterial)
		{
			_renderer.material.SetTextureScale("_MainTex", new Vector2(ScaleObject.localScale.x / InitialScaleX, ScaleObject.localScale.z / InitialScaleZ));
			_lastLocalScale = ScaleObject.localScale;
			_lastSharedMaterial = _renderer.sharedMaterial;
		}
	}
}

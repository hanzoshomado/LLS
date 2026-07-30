using UnityEngine;

public class ScrollMaterial : MonoBehaviour
{
	public Vector2 ScrollSpeed;

	private Material _material;

	private Vector2 _offset;

	private float _speedMultiplier;

	public void SetSpeedMultiplier(float speedMultiplier)
	{
		_speedMultiplier = speedMultiplier;
	}

	private void Awake()
	{
		_speedMultiplier = 1f;
		_material = GetComponent<Renderer>().material;
	}

	private void FixedUpdate()
	{
		_offset += _speedMultiplier * ScrollSpeed * Time.fixedDeltaTime;
		_material.SetTextureOffset("_MainTex", _offset);
	}
}

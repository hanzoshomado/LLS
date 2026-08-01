using UnityEngine;
using UnityEngine.Rendering;

// The rail shot is hitscan, so there is nothing travelling through the world to look at. This
// draws the line the shot took and fades it out over a fraction of a second.
//
// Owned and driven by SantaCharacterController, which fires it from tryRenderAttackID - the one
// path every client runs - so other players' shots are visible too, not just your own. Builds its
// own renderer at runtime, so there is nothing to wire onto the player prefab.
public class ShockRifleTracer
{
	// Pushed in from the controller's inspector fields on every shot so live tweaks take effect.
	public Material TracerMaterial;
	public Color StartColor = new Color(0.55f, 0.85f, 1f, 1f);
	public Color EndColor = new Color(0.25f, 0.6f, 1f, 0.9f);
	public float StartWidth = 0.14f;
	public float EndWidth = 0.05f;
	public float FadeTime = 0.2f;

	private readonly Transform _parent;
	private LineRenderer _line;
	private float _firedTime;

	public ShockRifleTracer(Transform parent)
	{
		_parent = parent;
	}

	public void Show(Vector3 from, Vector3 to)
	{
		ensureLine();
		_line.SetPosition(0, from);
		_line.SetPosition(1, to);
		_firedTime = Time.time;
		applyFade(0f);
		_line.enabled = true;
	}

	// Cheap no-op once the shot has faded, so the character can call this every frame.
	public void Tick()
	{
		if (_line == null || !_line.enabled)
		{
			return;
		}
		float t = ((FadeTime > 0f) ? ((Time.time - _firedTime) / FadeTime) : 1f);
		if (t >= 1f)
		{
			_line.enabled = false;
			return;
		}
		applyFade(t);
	}

	// Thins out as well as fading, so the rail looks like it collapses rather than just dimming.
	private void applyFade(float t)
	{
		float alpha = 1f - t;
		Color startColor = StartColor;
		Color endColor = EndColor;
		startColor.a *= alpha;
		endColor.a *= alpha;
		_line.startColor = startColor;
		_line.endColor = endColor;

		float widthScale = Mathf.Lerp(1f, 0.35f, t);
		_line.startWidth = StartWidth * widthScale;
		_line.endWidth = EndWidth * widthScale;
	}

	private void ensureLine()
	{
		if (_line != null)
		{
			// The material is an inspector field, so it can change between shots.
			applyMaterial();
			return;
		}
		GameObject gameObject = new GameObject("ShockRifleTracer");
		gameObject.transform.SetParent(_parent, false);
		_line = gameObject.AddComponent<LineRenderer>();
		_line.useWorldSpace = true;
		_line.receiveShadows = false;
		_line.shadowCastingMode = ShadowCastingMode.Off;
		_line.lightProbeUsage = LightProbeUsage.Off;
		_line.reflectionProbeUsage = ReflectionProbeUsage.Off;
		_line.alignment = LineAlignment.View;
		_line.textureMode = LineTextureMode.Stretch;
		_line.numCapVertices = 2;
		_line.positionCount = 2;
		_line.enabled = false;
		applyMaterial();
	}

	private void applyMaterial()
	{
		Material material = ((TracerMaterial != null) ? TracerMaterial : UnlitLineMaterial.Resolve());
		if (material != null && _line.sharedMaterial != material)
		{
			_line.sharedMaterial = material;
		}
	}
}

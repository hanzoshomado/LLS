using System.Collections.Generic;
using UnityEngine;

// Ghosts a character's renderers by swapping their materials for a transparent stand-in, then
// swaps the originals straight back once it is solid again.
//
// A swap rather than an alpha tween because the character's shaders (Custom/ColorSwap, Toon/Lit)
// are opaque - no blend mode, no alpha output - so turning _Color.a down on them does nothing at
// all. The replacement lives in Resources/CharacterFade.shader.
//
// Driven by SantaCharacterController for the copy you control and nothing else, so other players
// always see you solid.
public class CharacterFader
{
	private const string FadeShaderResource = "CharacterFade";

	// One ghost material per source material, shared across every character that ever fades.
	// Only the local character fades, so there is never more than one user of a given entry, and
	// this keeps respawns from leaking a fresh set of materials each time.
	private static readonly Dictionary<Material, Material> _fadeMaterials = new Dictionary<Material, Material>();

	private static Shader _fadeShader;

	private static bool _fadeShaderMissing;

	// Only the parts that actually block the view - the head and the weapons. Ghosting the torso
	// and legs too made the whole character disappear, which reads worse than it sounds.
	private readonly Transform[] _roots;

	private Renderer[] _renderers;

	private Material[][] _originalMaterials;

	private Material[][] _ghostMaterials;

	private bool _searched;

	private bool _isGhosted;

	private float _alpha = 1f;

	public CharacterFader(Transform[] roots)
	{
		_roots = roots;
	}

	public void Tick(bool shouldFade, float fadedAlpha, float fadeSpeed)
	{
		float target = (shouldFade ? Mathf.Clamp01(fadedAlpha) : 1f);
		_alpha = ((fadeSpeed > 0f) ? Mathf.MoveTowards(_alpha, target, fadeSpeed * Time.deltaTime) : target);

		if (_alpha >= 0.999f)
		{
			restoreOriginals();
			return;
		}
		if (!cacheRenderers())
		{
			return;
		}
		applyGhost();
	}

	private void applyGhost()
	{
		for (int i = 0; i < _ghostMaterials.Length; i++)
		{
			Material[] materials = _ghostMaterials[i];
			for (int j = 0; j < materials.Length; j++)
			{
				if (materials[j] == null)
				{
					continue;
				}
				Color color = materials[j].color;
				color.a = _alpha;
				materials[j].color = color;
			}
		}
		if (_isGhosted)
		{
			return;
		}
		_isGhosted = true;
		for (int k = 0; k < _renderers.Length; k++)
		{
			if (_renderers[k] != null)
			{
				_renderers[k].sharedMaterials = _ghostMaterials[k];
			}
		}
	}

	private void restoreOriginals()
	{
		if (!_isGhosted)
		{
			return;
		}
		_isGhosted = false;
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				_renderers[i].sharedMaterials = _originalMaterials[i];
			}
		}
	}

	private bool cacheRenderers()
	{
		if (_searched)
		{
			return _renderers != null;
		}
		_searched = true;

		Shader shader = resolveFadeShader();
		if (shader == null)
		{
			return false;
		}

		List<Renderer> kept = new List<Renderer>();
		for (int r = 0; r < _roots.Length; r++)
		{
			if (_roots[r] == null)
			{
				continue;
			}
			// Inactive included, so the weapons you aren't holding are cached too and swapping
			// weapons mid-session doesn't need a rebuild.
			Renderer[] found = _roots[r].GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < found.Length; i++)
			{
				// Particles, tracers and the grenade arc draw themselves and have their own
				// materials; ghosting them would fade the aiming aids along with the thing they
				// exist to compensate for.
				if (found[i] is ParticleSystemRenderer || found[i] is LineRenderer || found[i] is TrailRenderer)
				{
					continue;
				}
				if (found[i].sharedMaterials.Length == 0)
				{
					continue;
				}
				kept.Add(found[i]);
			}
		}
		if (kept.Count == 0)
		{
			_renderers = null;
			return false;
		}

		_renderers = kept.ToArray();
		_originalMaterials = new Material[_renderers.Length][];
		_ghostMaterials = new Material[_renderers.Length][];
		for (int j = 0; j < _renderers.Length; j++)
		{
			Material[] originals = _renderers[j].sharedMaterials;
			_originalMaterials[j] = originals;
			Material[] ghosts = new Material[originals.Length];
			for (int k = 0; k < originals.Length; k++)
			{
				ghosts[k] = getGhostMaterial(originals[k], shader);
			}
			_ghostMaterials[j] = ghosts;
		}
		return true;
	}

	private static Material getGhostMaterial(Material source, Shader shader)
	{
		if (source == null)
		{
			return null;
		}
		Material ghost;
		if (_fadeMaterials.TryGetValue(source, out ghost) && ghost != null)
		{
			return ghost;
		}
		ghost = new Material(shader);
		// Carry the look across so the ghost still reads as the same character rather than a
		// white silhouette.
		if (source.HasProperty("_MainTex"))
		{
			ghost.mainTexture = source.mainTexture;
		}
		if (source.HasProperty("_Color"))
		{
			ghost.color = source.color;
		}
		_fadeMaterials[source] = ghost;
		return ghost;
	}

	private static Shader resolveFadeShader()
	{
		if (_fadeShader != null || _fadeShaderMissing)
		{
			return _fadeShader;
		}
		_fadeShader = Resources.Load<Shader>(FadeShaderResource);
		if (_fadeShader == null)
		{
			_fadeShaderMissing = true;
			Debug.LogWarning("CharacterFader: Resources/" + FadeShaderResource + ".shader is missing, so aiming won't fade the character.");
		}
		return _fadeShader;
	}
}

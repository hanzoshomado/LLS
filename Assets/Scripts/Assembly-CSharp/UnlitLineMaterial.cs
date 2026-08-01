using UnityEngine;

// Shared fallback for effects that build a LineRenderer at runtime and so have no material to
// assign. Finds whatever unlit, vertex-coloured shader is actually present in the build and keeps
// one material for all of them.
public static class UnlitLineMaterial
{
	private static Material _material;

	private static readonly string[] Candidates = new string[]
	{
		"Sprites/Default",
		"Particles/Standard Unlit",
		"Legacy Shaders/Particles/Alpha Blended",
		"Unlit/Color"
	};

	// Null if none of them exist, in which case the caller should leave the LineRenderer on its
	// own default material rather than blanking it.
	public static Material Resolve()
	{
		if (_material != null)
		{
			return _material;
		}
		for (int i = 0; i < Candidates.Length; i++)
		{
			Shader shader = Shader.Find(Candidates[i]);
			if (shader != null)
			{
				_material = new Material(shader);
				return _material;
			}
		}
		return null;
	}
}

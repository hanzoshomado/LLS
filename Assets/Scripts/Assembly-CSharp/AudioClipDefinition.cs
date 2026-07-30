using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioClipDefinition
{
	public AudioClip Clip;

	public AudioMixerGroup MixerGroup;

	public float PitchVariation;

	public bool HasPitchVariation()
	{
		return Math.Abs(PitchVariation) > 0.0001f;
	}
}

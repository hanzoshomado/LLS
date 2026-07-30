using System;
using UnityEngine;

public class WorldAudioSource : MonoBehaviour
{
	public float MaxDistance = 500f;

	private Transform _targetTransform;

	private AudioSource _audioSource;

	private Action _doneCallback;

	private bool _isFadingOut;

	private float _fadeOutTimeLeft;

	private float _fadeOutTargetVolume;

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
	}

	public void Initialize(Transform targetTransform, AudioClipDefinition audioClip, float delay, bool loop, float maxDistanceMultiplier = 1f)
	{
		_targetTransform = targetTransform;
		_audioSource.clip = audioClip.Clip;
		_audioSource.outputAudioMixerGroup = audioClip.MixerGroup;
		_audioSource.loop = loop;
		_audioSource.maxDistance = MaxDistance * maxDistanceMultiplier;
		_audioSource.volume = 1f;
		_audioSource.time = 0f;
		_audioSource.pitch = 1f;
		if (audioClip.HasPitchVariation())
		{
			VaryPitch(audioClip.PitchVariation);
		}
		_isFadingOut = false;
		if (delay > 0f)
		{
			_audioSource.PlayDelayed(delay);
		}
		else
		{
			_audioSource.Play();
		}
	}

	public void SetVolume(float volume)
	{
		if ((bool)_audioSource)
		{
			_audioSource.volume = volume;
		}
	}

	public void SetDopplerLevel(float dopplerLevel)
	{
		if ((bool)_audioSource)
		{
			_audioSource.dopplerLevel = dopplerLevel;
		}
	}

	public void SetMinDistance(float minDistance)
	{
		if ((bool)_audioSource)
		{
			_audioSource.dopplerLevel = minDistance;
		}
	}

	public void VaryPitch(float variation)
	{
		if ((bool)_audioSource)
		{
			_audioSource.pitch = UnityEngine.Random.Range(1f - variation, 1f + variation);
		}
	}

	public void SetRandomTime(float min, float max)
	{
		_audioSource.time = UnityEngine.Random.Range(min, max);
	}

	public void FadeOut(float duration)
	{
		_isFadingOut = true;
		_fadeOutTargetVolume = 0f;
		_fadeOutTimeLeft = duration;
	}

	public void StopPlaying()
	{
		_audioSource.Stop();
		_doneCallback = null;
		GetComponent<PooledPrefabReference>().OwnerPool.DestroyObject(base.gameObject);
	}

	public void SetDoneCallback(Action doneCallback)
	{
		_doneCallback = doneCallback;
	}

	private void Update()
	{
		if (_targetTransform != null)
		{
			base.transform.position = _targetTransform.position;
		}
		if (!_audioSource.isPlaying)
		{
			if (_doneCallback != null)
			{
				_doneCallback();
				_doneCallback = null;
			}
			GetComponent<PooledPrefabReference>().OwnerPool.DestroyObject(base.gameObject);
		}
		if (_isFadingOut && _audioSource.isPlaying)
		{
			UpdateFadeOut();
		}
	}

	private void UpdateFadeOut()
	{
		if (_fadeOutTimeLeft < Time.deltaTime)
		{
			_isFadingOut = false;
			StopPlaying();
			return;
		}
		float num = _audioSource.volume - _fadeOutTargetVolume;
		float num2 = Mathf.Max(Time.deltaTime / _fadeOutTimeLeft, 0f);
		float num3 = num2 * num;
		_audioSource.volume -= num3;
		_fadeOutTimeLeft -= Time.deltaTime;
	}
}

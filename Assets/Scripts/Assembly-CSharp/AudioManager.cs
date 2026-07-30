using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
	public AudioSource MusicAudioSource;

	public PooledPrefab WorldAudioSourcePool;

	public PooledPrefab GlobalAudioSourcePool;

	public AudioMixerGroup DefaultMusicMixerGroup;

	private AudioClip _musicSavedFromOverride;

	private float _musicSavedFromOverrideTime;

	private void Start()
	{
		refreshVolume();
		Singleton<GlobalEventManager>.Instance.AddEventListener("SoundVolumeChanged", refreshVolume);
		Singleton<GlobalEventManager>.Instance.AddEventListener("MusicVolumeChanged", refreshVolume);
		Singleton<GlobalEventManager>.Instance.AddEventListener("CharacterWithControlSpawned", refreshVolume);
	}

	private void refreshVolume()
	{
		AudioListener.volume = Singleton<GameSettingsManger>.Instance.GetSoundVolume();
		MusicAudioSource.volume = Singleton<GameSettingsManger>.Instance.GetMusicVolume();
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("SoundVolumeChanged", refreshVolume);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("MusicVolumeChanged", refreshVolume);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("CharacterWithControlSpawned", refreshVolume);
	}

	public void PlayClipGlobal(AudioClipDefinition[] clips, float delay = 0f)
	{
		PlayClipGlobal(clips[Random.Range(0, clips.Length)]);
	}

	public void PlayClipGlobal(AudioClipDefinition clip, float delay = 0f)
	{
		Transform transform = GlobalAudioSourcePool.InstantiateNewObject();
		WorldAudioSource component = transform.GetComponent<WorldAudioSource>();
		component.Initialize(null, clip, delay, false);
	}

	public void PlayMusicClip(AudioClipDefinition[] clips)
	{
		PlayMusicClip(clips[Random.Range(0, clips.Length)]);
	}

	public void PlayMusicClip(AudioClipDefinition clip, bool clearSavedFromOverride = true)
	{
		PlayMusicClip(clip.Clip, clip.MixerGroup, clearSavedFromOverride);
	}

	public void PlayMusicClip(AudioClip clip, AudioMixerGroup mixerGroup = null, bool clearSavedFromOverride = true)
	{
		if (clip != null && (clip != MusicAudioSource.clip || MusicAudioSource.mute))
		{
			MusicAudioSource.clip = clip;
			MusicAudioSource.time = 0f;
			MusicAudioSource.mute = false;
			if (mixerGroup != null)
			{
				MusicAudioSource.outputAudioMixerGroup = mixerGroup;
			}
			else
			{
				MusicAudioSource.outputAudioMixerGroup = DefaultMusicMixerGroup;
			}
			MusicAudioSource.Play();
			if (clearSavedFromOverride)
			{
				_musicSavedFromOverride = null;
			}
		}
	}

	public bool IsPlayingMusic(AudioClipDefinition musicClip)
	{
		return MusicAudioSource.clip == musicClip.Clip;
	}

	public void SetMusicClipPosition(float secondsFromStart)
	{
		MusicAudioSource.time = secondsFromStart;
	}

	public void OverrideMusicClip(AudioClipDefinition audioClipDefinition)
	{
		if (!MusicAudioSource.mute)
		{
			_musicSavedFromOverride = MusicAudioSource.clip;
			_musicSavedFromOverrideTime = MusicAudioSource.time;
		}
		PlayMusicClip(audioClipDefinition, false);
	}

	public void RestoreMusic()
	{
		if (_musicSavedFromOverride != null)
		{
			MusicAudioSource.clip = _musicSavedFromOverride;
			MusicAudioSource.time = Mathf.Min(_musicSavedFromOverrideTime, MusicAudioSource.clip.length - 1f);
			MusicAudioSource.Play();
		}
		_musicSavedFromOverride = null;
	}

	public void StopMusic()
	{
		MusicAudioSource.mute = true;
	}

	public void SetMusicVolume(float volume)
	{
		MusicAudioSource.volume = volume;
	}

	public WorldAudioSource PlayClipAtTransform(AudioClipDefinition[] clips, Transform targetTransform, float delay = 0f, bool loop = false, float maxDistanceMultiplier = 1f)
	{
		return PlayClipAtTransform(clips[Random.Range(0, clips.Length)], targetTransform, delay, loop, maxDistanceMultiplier);
	}

	public WorldAudioSource PlayClipAtTransform(AudioClipDefinition clip, Transform targetTransform, float delay = 0f, bool loop = false, float maxDistanceMultiplier = 1f)
	{
		Transform transform = WorldAudioSourcePool.InstantiateNewObject();
		transform.position = targetTransform.position;
		WorldAudioSource component = transform.GetComponent<WorldAudioSource>();
		component.Initialize(targetTransform, clip, delay, loop, maxDistanceMultiplier);
		return component;
	}

	public WorldAudioSource PlayClipAtPosition(AudioClipDefinition[] clips, Vector3 position, float delay = 0f, bool loop = false, float maxDistanceMultiplier = 1f)
	{
		return PlayClipAtPosition(clips[Random.Range(0, clips.Length)], position, delay, loop, maxDistanceMultiplier);
	}

	public WorldAudioSource PlayClipAtPosition(AudioClipDefinition clip, Vector3 position, float delay = 0f, bool loop = false, float maxDistanceMultiplier = 1f)
	{
		Transform transform = WorldAudioSourcePool.InstantiateNewObject();
		transform.position = position;
		WorldAudioSource component = transform.GetComponent<WorldAudioSource>();
		component.Initialize(null, clip, delay, loop, maxDistanceMultiplier);
		return component;
	}

	private void Update()
	{
		if (Camera.main != null)
		{
			base.transform.position = Camera.main.transform.position;
		}
	}
}

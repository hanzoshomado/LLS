using UnityEngine;

public class GameSettingsManger : Singleton<GameSettingsManger>
{
	private bool _serverStartsInSpectatorMode;

	public void SetServerStartsInSpectatorMode(bool value)
	{
		_serverStartsInSpectatorMode = value;
	}

	public bool ShouldServerStartInSpectatorMode()
	{
		return _serverStartsInSpectatorMode;
	}

	public float GetSoundVolume()
	{
		return PlayerPrefs.GetFloat("SoundVolume", 1f);
	}

	public void SetSoundVolume(float value)
	{
		PlayerPrefs.SetFloat("SoundVolume", value);
		Singleton<GlobalEventManager>.Instance.Dispatch("SoundVolumeChanged");
	}

	public float GetMusicVolume()
	{
		return PlayerPrefs.GetFloat("MusicVolume", 0.75f);
	}

	public void SetMusicVolume(float value)
	{
		PlayerPrefs.SetFloat("MusicVolume", value);
		Singleton<GlobalEventManager>.Instance.Dispatch("MusicVolumeChanged");
	}
}

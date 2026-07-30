using UnityEngine;
using UnityEngine.UI;

public class EscMenu : MonoBehaviour
{
	public Slider SoundSlider;

	public Slider MusicSlider;

	public Text GameNameLabel;

	private bool _hasInitializedDisplays;

	public void Show()
	{
		base.gameObject.SetActive(true);
		GameNameLabel.text = GameModeManager.Instance.GetGameName();
		_hasInitializedDisplays = false;
		SoundSlider.value = Singleton<GameSettingsManger>.Instance.GetSoundVolume();
		MusicSlider.value = Singleton<GameSettingsManger>.Instance.GetMusicVolume();
		_hasInitializedDisplays = true;
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void OnSoundSliderChanged()
	{
		if (_hasInitializedDisplays)
		{
			Singleton<GameSettingsManger>.Instance.SetSoundVolume(SoundSlider.value);
		}
	}

	public void OnMusicSliderChanged()
	{
		if (_hasInitializedDisplays)
		{
			Singleton<GameSettingsManger>.Instance.SetMusicVolume(MusicSlider.value);
		}
	}

	public void OnClickExitGame()
	{
		Application.Quit();
	}

	public void OnClickMainMenu()
	{
		Singleton<GameplaySceneManager>.Instance.DisconnectAndExitToMainMenu();
	}
}

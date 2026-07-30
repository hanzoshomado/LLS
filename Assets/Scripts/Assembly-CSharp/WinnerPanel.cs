using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinnerPanel : MonoBehaviour
{
	public Text WinningUserNameLabel;

	public Text NumberToNextRoundLabel;

	public GameObject Container;

	private void Start()
	{
		Container.SetActive(GameModeManager.Instance.IsGameModeInfoLoaded() && GameModeManager.Instance.HasRoundEnded());
		Singleton<GlobalEventManager>.Instance.AddEventListener("SecondsToGameStartedUpdated", onSecondsUpdated);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameStartedWaitingToStart", onGameStartedWaitingToStart);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameEnded", onGameEnded);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameInfoLoaded", onGameInfoLoaded);
		Singleton<GlobalEventManager>.Instance.AddEventListener("RoundWinnerNameChanged", onRoundWinnerChanged);
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("SecondsToGameStartedUpdated", onSecondsUpdated);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameStartedWaitingToStart", onGameStartedWaitingToStart);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameEnded", onGameEnded);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameInfoLoaded", onGameInfoLoaded);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("RoundWinnerNameChanged", onRoundWinnerChanged);
	}

	private void onGameInfoLoaded()
	{
		Container.SetActive(GameModeManager.Instance.HasRoundEnded());
	}

	private void onGameEnded()
	{
		Container.SetActive(true);
		Singleton<AudioManager>.Instance.PlayClipGlobal(Singleton<AudioLibrary>.Instance.GameOver);
		List<SantaCharacterController> allLivingCharacters = CharacterTracker.Instance.GetAllLivingCharacters();
		if (allLivingCharacters.Count > 0)
		{
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.GameOverHoHo, allLivingCharacters[0].transform, 3f);
		}
	}

	private void onRoundWinnerChanged()
	{
		WinningUserNameLabel.text = GameModeManager.Instance.GetRoundWinnerName();
	}

	private void onGameStartedWaitingToStart()
	{
		Container.SetActive(false);
	}

	private void onSecondsUpdated()
	{
		if (GameModeManager.Instance.HasRoundEnded())
		{
			NumberToNextRoundLabel.text = "(" + GameModeManager.Instance.GetVisualSecondsToGameStart() + "s)";
		}
	}
}

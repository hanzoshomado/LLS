using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoundStartCountdownDisplay : MonoBehaviour
{
	public GameObject TitleLabel;

	public GameObject WaitingLabel;

	public Text NumberLabel;

	public GameObject FightLabel;

	public float SecondsToDisplayFight;

	public ITweenHash ScaleHash;

	public float ScaleFrom;

	private void Start()
	{
		WaitingLabel.gameObject.SetActive(GameModeManager.Instance.IsGameModeInfoLoaded() && GameModeManager.Instance.IsWaitingForRoundToStart());
		TitleLabel.SetActive(false);
		NumberLabel.gameObject.SetActive(false);
		FightLabel.SetActive(false);
		Singleton<GlobalEventManager>.Instance.AddEventListener("SecondsToGameStartedUpdated", onSecondsUpdated);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameStarted", onGameStarted);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameInfoLoaded", onGameInfoLoaded);
		Singleton<GlobalEventManager>.Instance.AddEventListener("CountDownAborted", onCountDownAborted);
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("SecondsToGameStartedUpdated", onSecondsUpdated);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameStarted", onGameStarted);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameInfoLoaded", onGameInfoLoaded);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("CountDownAborted", onCountDownAborted);
	}

	private void onGameInfoLoaded()
	{
		WaitingLabel.gameObject.SetActive(GameModeManager.Instance.IsWaitingForRoundToStart());
	}

	private void onSecondsUpdated()
	{
		if (GameModeManager.Instance.IsGameModeInfoLoaded() && !GameModeManager.Instance.HasRoundEnded())
		{
			int visualSecondsToGameStart = GameModeManager.Instance.GetVisualSecondsToGameStart();
			WaitingLabel.gameObject.SetActive(false);
			TitleLabel.gameObject.SetActive(true);
			NumberLabel.gameObject.SetActive(true);
			if (visualSecondsToGameStart == 8 || visualSecondsToGameStart == 6 || visualSecondsToGameStart == 4)
			{
				Singleton<AudioManager>.Instance.PlayClipGlobal(Singleton<AudioLibrary>.Instance.CountdownPunch);
			}
			if (visualSecondsToGameStart == 4)
			{
				Singleton<AudioManager>.Instance.PlayClipGlobal(Singleton<AudioLibrary>.Instance.GameStartDrop);
			}
			if (visualSecondsToGameStart == 1)
			{
				Singleton<AudioManager>.Instance.PlayClipGlobal(Singleton<AudioLibrary>.Instance.ReadyFightVoice, 1f);
				Singleton<AudioManager>.Instance.PlayClipGlobal(Singleton<AudioLibrary>.Instance.GameStart, 1f);
			}
			Singleton<AudioManager>.Instance.PlayClipGlobal(Singleton<AudioLibrary>.Instance.Jingle);
			NumberLabel.text = visualSecondsToGameStart.ToString();
			NumberLabel.transform.localScale = ScaleFrom * Vector3.one;
			NumberLabel.GetComponent<ITweenMover>().ScaleTo(Vector3.one, ScaleHash);
		}
	}

	private void onCountDownAborted()
	{
		WaitingLabel.gameObject.SetActive(true);
		TitleLabel.gameObject.SetActive(false);
		NumberLabel.gameObject.SetActive(false);
	}

	private void onGameStarted()
	{
		WaitingLabel.gameObject.SetActive(false);
		TitleLabel.gameObject.SetActive(false);
		NumberLabel.gameObject.SetActive(false);
		FightLabel.SetActive(true);
		FightLabel.transform.localScale = ScaleFrom * Vector3.one;
		FightLabel.GetComponent<ITweenMover>().ScaleTo(Vector3.one, ScaleHash);
		StartCoroutine(waitThenHideFight());
	}

	private IEnumerator waitThenHideFight()
	{
		yield return new WaitForSeconds(SecondsToDisplayFight);
		FightLabel.SetActive(false);
	}
}

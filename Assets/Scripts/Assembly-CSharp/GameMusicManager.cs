public class GameMusicManager : Singleton<GameMusicManager>
{
	private void OnPostStart()
	{
		Singleton<AudioManager>.Instance.PlayMusicClip(Singleton<AudioLibrary>.Instance.MainMenuMusic);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameStarted", onGameStarted);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameStartedWaitingToStart", onGameStartedWaitingToStart);
		Singleton<GlobalEventManager>.Instance.AddEventListener("SecondsToGameStartedUpdated", onSecondsToGameStartedUpdated);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameEnded", onGameEnded);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameInfoLoaded", onGameInfoLoaded);
		Singleton<GlobalEventManager>.Instance.AddEventListener("MainMenuLoaded", onMainMenuLoaded);
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameStarted", onGameStarted);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameStartedWaitingToStart", onGameStartedWaitingToStart);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("SecondsToGameStartedUpdated", onSecondsToGameStartedUpdated);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameEnded", onGameEnded);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameInfoLoaded", onGameInfoLoaded);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("MainMenuLoaded", onMainMenuLoaded);
	}

	private void onMainMenuLoaded()
	{
		Singleton<AudioManager>.Instance.PlayMusicClip(Singleton<AudioLibrary>.Instance.MainMenuMusic);
	}

	private void onGameStarted()
	{
		Singleton<AudioManager>.Instance.PlayMusicClip(Singleton<AudioLibrary>.Instance.CombatMusic);
	}

	private void onGameStartedWaitingToStart()
	{
	}

	private void onSecondsToGameStartedUpdated()
	{
		if (GameModeManager.Instance.IsWaitingForRoundToStart() && !Singleton<AudioManager>.Instance.IsPlayingMusic(Singleton<AudioLibrary>.Instance.CountdownMusic))
		{
			int visualSecondsToGameStart = GameModeManager.Instance.GetVisualSecondsToGameStart();
			int startGameMaxCountdown = GameModeManager.Instance.GetStartGameMaxCountdown();
			float num = startGameMaxCountdown - visualSecondsToGameStart;
			Singleton<AudioManager>.Instance.PlayMusicClip(Singleton<AudioLibrary>.Instance.CountdownMusic);
			Singleton<AudioManager>.Instance.SetMusicClipPosition(num + 1f);
		}
	}

	private void onGameEnded()
	{
		Singleton<AudioManager>.Instance.PlayMusicClip(Singleton<AudioLibrary>.Instance.WinMusic);
	}

	private void onGameInfoLoaded()
	{
		if (!GameModeManager.Instance.IsWaitingForRoundToStart() && !GameModeManager.Instance.HasRoundEnded() && GameModeManager.Instance.IsRoundActive())
		{
			Singleton<AudioManager>.Instance.PlayMusicClip(Singleton<AudioLibrary>.Instance.CombatMusic);
		}
	}
}
